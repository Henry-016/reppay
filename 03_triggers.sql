-- ============================================================
-- TRIGGER 1: Sincronizar status_despesa com base nas parcelas
-- Dispara após qualquer INSERT ou UPDATE de status em parcela
-- ============================================================

CREATE OR REPLACE FUNCTION fn_sync_status_despesa()
RETURNS TRIGGER AS $$
DECLARE
    v_id_despesa INT;
    v_total      INT;
    v_pagas      INT;
BEGIN
    -- Se for DELETE, usa o OLD, senão usa o NEW
    v_id_despesa := COALESCE(NEW.id_despesa, OLD.id_despesa);

    SELECT
        COUNT(*),
        COUNT(*) FILTER (WHERE status = 'PAGO')
    INTO v_total, v_pagas
    FROM parcela
    WHERE id_despesa = v_id_despesa;

    UPDATE despesa
    SET status = CASE
        -- Garante que se total for 0 (apagou todas as parcelas), não fique QUITADA
        WHEN v_total > 0 AND v_pagas = v_total THEN 'QUITADA'
        ELSE 'ATIVA'
    END
    WHERE id_despesa = v_id_despesa
      AND status <> 'CANCELADA';

    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_sync_status_despesa
AFTER INSERT OR UPDATE OF status OR DELETE ON parcela
FOR EACH ROW
EXECUTE FUNCTION fn_sync_status_despesa();


-- ============================================================
-- TRIGGER 2A: Ao criar um grupo, inserir o admin em pertence
-- Garante que o admin sempre é membro do próprio grupo
-- ============================================================

CREATE OR REPLACE FUNCTION fn_admin_vira_membro()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO pertence (id_usuario, id_grupo)
    VALUES (NEW.id_admin, NEW.id_grupo)
    ON CONFLICT DO NOTHING;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_admin_vira_membro
AFTER INSERT ON grupo
FOR EACH ROW
EXECUTE FUNCTION fn_admin_vira_membro();

-- ============================================================
-- TRIGGER 3: Bloquear alteração em despesa com parcela paga
-- ============================================================
CREATE OR REPLACE FUNCTION fn_bloquear_alteracao_despesa_paga()
RETURNS TRIGGER AS $$
BEGIN
    -- Se tem parcela paga, bloqueia a exclusão da despesa ou alteração de dados sensíveis
    IF EXISTS (
        SELECT 1 FROM parcela
        WHERE id_despesa = OLD.id_despesa
          AND status = 'PAGO'
    ) THEN
        RAISE EXCEPTION
            'Não é permitido alterar dados financeiros ou excluir uma despesa que já possui parcelas pagas.';
    END IF;

    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

-- 3.A: Gatilho apenas para UPDATE nas colunas restritas
CREATE TRIGGER trg_bloquear_update_despesa_paga
BEFORE UPDATE OF valor, vencimento, id_grupo ON despesa
FOR EACH ROW
EXECUTE FUNCTION fn_bloquear_alteracao_despesa_paga();

-- 3.B: Gatilho separado para DELETE
CREATE TRIGGER trg_bloquear_delete_despesa_paga
BEFORE DELETE ON despesa
FOR EACH ROW
EXECUTE FUNCTION fn_bloquear_alteracao_despesa_paga();


-- ============================================================
-- TRIGGER 4: Bloquear saída de membro com pendência
-- ============================================================
CREATE OR REPLACE FUNCTION fn_bloquear_saida_pertence()
RETURNS TRIGGER AS $$
BEGIN
    -- Verifica dívidas ativas
    IF EXISTS (
        SELECT 1
        FROM parcela p
        JOIN despesa d ON p.id_despesa = d.id_despesa
        WHERE p.id_usuario = OLD.id_usuario
          AND d.id_grupo   = OLD.id_grupo
          AND p.status IN ('PENDENTE', 'ATRASADO')
    ) THEN
        RAISE EXCEPTION
            'Usuário possui dívidas pendentes neste grupo e não pode ser removido.';
    END IF;

    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_bloquear_saida_pertence
BEFORE DELETE ON pertence
FOR EACH ROW
EXECUTE FUNCTION fn_bloquear_saida_pertence();