CREATE OR REPLACE FUNCTION fn_sync_status_despesa()
RETURNS TRIGGER AS $$
DECLARE
    v_id_despesa INT;
    v_total      BIGINT;
    v_pagas      BIGINT;
    v_novo_status VARCHAR(20);
BEGIN
    IF TG_OP = 'UPDATE'
        AND NEW.status IS NOT DISTINCT FROM OLD.status THEN
            RETURN NEW;
    END IF;
    
    v_id_despesa := COALESCE(NEW.id_despesa, OLD.id_despesa);

    SELECT
        COUNT(*),
        COUNT(*) FILTER (WHERE status = 'PAGO')
    INTO v_total, v_pagas
    FROM parcela
    WHERE id_despesa = v_id_despesa;

    v_novo_status := CASE
        WHEN v_total > 0 AND v_pagas = v_total THEN 'QUITADA'
        ELSE 'ATIVA'
    END;

    UPDATE despesa
    SET status = v_novo_status
    WHERE id_despesa = v_id_despesa
      AND status IS DISTINCT FROM v_novo_status;

    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_sync_status_despesa
AFTER INSERT OR UPDATE OF status OR DELETE ON parcela
FOR EACH ROW
EXECUTE FUNCTION fn_sync_status_despesa();

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
FOR EACH ROW EXECUTE FUNCTION fn_admin_vira_membro();

CREATE OR REPLACE FUNCTION fn_validar_admin_no_grupo()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'UPDATE' THEN
        IF NOT EXISTS (
            SELECT 1 FROM pertence p
            JOIN usuario u ON u.id_usuario = p.id_usuario
            WHERE p.id_usuario = NEW.id_admin
              AND p.id_grupo = NEW.id_grupo
              AND u.ativo = TRUE
        ) THEN
            RAISE EXCEPTION 'O administrador deve pertencer ao grupo e possuir uma conta ativa.';
        END IF;
    END IF;

    IF TG_OP = 'INSERT' THEN
        IF NOT EXISTS (
            SELECT 1 FROM usuario
            WHERE id_usuario = NEW.id_admin AND ativo = TRUE
        ) THEN
            RAISE EXCEPTION 'O administrador de um novo grupo deve possuir uma conta ativa.';
        END IF;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validar_admin_no_grupo
BEFORE INSERT OR UPDATE OF id_admin ON grupo
FOR EACH ROW EXECUTE FUNCTION fn_validar_admin_no_grupo();

CREATE OR REPLACE FUNCTION fn_validar_usuario_no_grupo()
RETURNS TRIGGER AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pertence p
        JOIN despesa d ON d.id_grupo = p.id_grupo
        WHERE p.id_usuario = NEW.id_usuario
          AND d.id_despesa = NEW.id_despesa
    ) THEN
        RAISE EXCEPTION 'Usuário não pertence ao grupo da despesa.';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validar_usuario_no_grupo
BEFORE INSERT OR UPDATE OF id_usuario, id_despesa ON parcela
FOR EACH ROW EXECUTE FUNCTION fn_validar_usuario_no_grupo();

CREATE OR REPLACE FUNCTION fn_bloquear_alteracao_despesa_paga()
RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM parcela
        WHERE id_despesa = OLD.id_despesa
          AND status in ('PAGO', 'EM_ANALISE')
    ) THEN
        RAISE EXCEPTION 'Não é permitido alterar dados financeiros ou excluir uma despesa que já possui parcelas pagas ou em análise.';
    END IF;
    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_bloquear_update_despesa_paga
BEFORE UPDATE OF valor, vencimento, id_grupo ON despesa
FOR EACH ROW EXECUTE FUNCTION fn_bloquear_alteracao_despesa_paga();

CREATE TRIGGER trg_bloquear_delete_despesa_paga
BEFORE DELETE ON despesa
FOR EACH ROW EXECUTE FUNCTION fn_bloquear_alteracao_despesa_paga();

CREATE OR REPLACE FUNCTION fn_bloquear_parcela_em_despesa_quitada()
RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
    SELECT 1
    FROM despesa
    WHERE id_despesa = NEW.id_despesa
      AND status = 'QUITADA'
) THEN
        RAISE EXCEPTION 'Não é possível adicionar parcelas a uma despesa já quitada.';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_bloquear_parcela_em_despesa_quitada
BEFORE INSERT ON parcela
FOR EACH ROW EXECUTE FUNCTION fn_bloquear_parcela_em_despesa_quitada();

CREATE OR REPLACE FUNCTION fn_validar_saida_pertence()
RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM grupo
        WHERE id_grupo = OLD.id_grupo AND id_admin = OLD.id_usuario
    ) THEN
        RAISE EXCEPTION 'O administrador não pode ser removido do grupo. Transfira a liderança antes de sair.';
    END IF;
    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validar_saida_pertence
BEFORE DELETE ON pertence
FOR EACH ROW EXECUTE FUNCTION fn_validar_saida_pertence();

CREATE OR REPLACE FUNCTION fn_bloquear_update_status_despesa()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.status IS DISTINCT FROM OLD.status THEN
        RAISE EXCEPTION
        'O status da despesa é derivado das parcelas e não pode ser alterado manualmente.';
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_bloquear_update_status_despesa
BEFORE UPDATE OF status ON despesa
FOR EACH ROW
WHEN (pg_trigger_depth() = 0)
EXECUTE FUNCTION fn_bloquear_update_status_despesa();

CREATE OR REPLACE FUNCTION fn_revogar_sessao_usuario_inativo() 
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.ativo = FALSE AND OLD.ativo = TRUE THEN
        UPDATE refresh_token 
        SET revogado = TRUE 
        WHERE id_usuario = NEW.id_usuario 
          AND revogado = FALSE;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_revogar_sessao_inativo 
AFTER UPDATE OF ativo ON usuario 
FOR EACH ROW 
EXECUTE FUNCTION fn_revogar_sessao_usuario_inativo();