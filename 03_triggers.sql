CREATE OR REPLACE FUNCTION fn_sync_status_despesa()
RETURNS TRIGGER AS $$
DECLARE
    v_id_despesa INT;
    v_total      BIGINT;
    v_pagas      BIGINT;
BEGIN
    v_id_despesa := COALESCE(NEW.id_despesa, OLD.id_despesa);

    SELECT
        COUNT(*),
        COUNT(*) FILTER (WHERE status = 'PAGO')
    INTO v_total, v_pagas
    FROM parcela
    WHERE id_despesa = v_id_despesa;

    UPDATE despesa
    SET status = CASE
        WHEN v_total > 0 AND v_pagas = v_total THEN 'QUITADA'
        ELSE 'ATIVA'
    END
    WHERE id_despesa = v_id_despesa
        AND status IS DISTINCT FROM ( 
      CASE
        WHEN v_total > 0 AND v_pagas = v_total THEN 'QUITADA'
        ELSE 'ATIVA'
      END
    );

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
FOR EACH ROW
EXECUTE FUNCTION fn_admin_vira_membro();


CREATE OR REPLACE FUNCTION fn_bloquear_alteracao_despesa_paga()
RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM parcela
        WHERE id_despesa = OLD.id_despesa
          AND status in ('PAGO', 'EM_ANALISE')
    ) THEN
        RAISE EXCEPTION 
            'Não é permitido alterar dados financeiros ou excluir uma despesa que já possui parcelas pagas ou em análise.';
    END IF;

    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_bloquear_update_despesa_paga
BEFORE UPDATE OF valor, vencimento, id_grupo ON despesa
FOR EACH ROW
EXECUTE FUNCTION fn_bloquear_alteracao_despesa_paga();

CREATE TRIGGER trg_bloquear_delete_despesa_paga
BEFORE DELETE ON despesa
FOR EACH ROW
EXECUTE FUNCTION fn_bloquear_alteracao_despesa_paga();


CREATE OR REPLACE FUNCTION fn_validar_usuario_no_grupo()
RETURNS TRIGGER AS $$
BEGIN

    IF NOT EXISTS (
        SELECT 1
        FROM pertence p
        JOIN despesa d
          ON d.id_grupo = p.id_grupo
        WHERE p.id_usuario = NEW.id_usuario
          AND d.id_despesa = NEW.id_despesa
    ) THEN

        RAISE EXCEPTION
        'Usuário não pertence ao grupo da despesa';

    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validar_usuario_no_grupo
BEFORE INSERT OR UPDATE OF id_usuario, id_despesa
ON parcela
FOR EACH ROW
EXECUTE FUNCTION fn_validar_usuario_no_grupo();


CREATE OR REPLACE FUNCTION fn_validar_admin_no_grupo()
RETURNS TRIGGER AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pertence p
        JOIN usuario u ON u.id_usuario = p.id_usuario
        WHERE p.id_usuario = NEW.id_admin
          AND p.id_grupo = NEW.id_grupo
          AND u.ativo = TRUE
    ) THEN
        RAISE EXCEPTION
            'O administrador deve pertencer ao grupo e possuir uma conta ativa.';
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validar_admin_no_grupo
BEFORE UPDATE OF id_admin ON grupo
FOR EACH ROW
EXECUTE FUNCTION fn_validar_admin_no_grupo();


CREATE OR REPLACE FUNCTION fn_check_move_despesa()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.id_grupo = OLD.id_grupo THEN
        RETURN NEW;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM parcela p
        WHERE p.id_despesa = NEW.id_despesa
          AND NOT EXISTS (
              SELECT 1
              FROM pertence pt
              WHERE pt.id_usuario = p.id_usuario
                AND pt.id_grupo = NEW.id_grupo
          )
    ) THEN
        RAISE EXCEPTION
            'Não é possível mover a despesa para outro grupo: existem parcelas de usuários que não pertencem ao grupo de destino.';
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_check_move_despesa
BEFORE UPDATE OF id_grupo ON despesa
FOR EACH ROW
EXECUTE FUNCTION fn_check_move_despesa();


CREATE OR REPLACE FUNCTION fn_proteger_parcela_paga()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.status IN ('PAGO', 'EM_ANALISE') THEN
        RAISE EXCEPTION 'Não é possível excluir uma parcela que já foi paga ou está em análise. Estorne o valor primeiro.';
    END IF;

    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_proteger_parcela_paga
BEFORE DELETE ON parcela
FOR EACH ROW
EXECUTE FUNCTION fn_proteger_parcela_paga();

CREATE OR REPLACE FUNCTION fn_bloquear_mutacao_parcela_paga()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.status IN ('PAGO', 'EM_ANALISE') THEN
        
        IF NEW.valor IS DISTINCT FROM OLD.valor
           OR NEW.id_usuario IS DISTINCT FROM OLD.id_usuario
           OR NEW.id_despesa IS DISTINCT FROM OLD.id_despesa THEN
             RAISE EXCEPTION 'Não é possível alterar valor, titularidade ou despesa de uma parcela paga ou em análise.';
        END IF;
        IF NEW.data_pagamento IS DISTINCT FROM OLD.data_pagamento THEN
            IF NOT (OLD.status = 'EM_ANALISE'
                    AND NEW.status IN ('PENDENTE', 'ATRASADO')
                    AND NEW.data_pagamento IS NULL) THEN
                RAISE EXCEPTION 'Não é permitido alterar a data de pagamento de uma parcela paga ou em análise (estorno requer status pendente e remoção da data).';
            END IF;
        END IF;

    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE TRIGGER trg_bloquear_mutacao_parcela_paga
BEFORE UPDATE OF valor, id_usuario, id_despesa, data_pagamento ON parcela
FOR EACH ROW
EXECUTE FUNCTION fn_bloquear_mutacao_parcela_paga();

CREATE OR REPLACE FUNCTION fn_bloquear_arquivamento_despesa()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.ativo = FALSE AND OLD.ativo = TRUE THEN

        IF current_setting('app.arquivando_grupo', true) = 'true' THEN
            RETURN NEW;
        END IF;

        IF EXISTS (
            SELECT 1 FROM parcela
            WHERE id_despesa = NEW.id_despesa
              AND status IN ('PENDENTE', 'ATRASADO', 'EM_ANALISE')
        ) THEN
            RAISE EXCEPTION
                'Não é possível arquivar uma despesa com parcelas pendentes, atrasadas ou em análise.';
        END IF;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_bloquear_arquivamento_despesa
BEFORE UPDATE OF ativo ON despesa
FOR EACH ROW
EXECUTE FUNCTION fn_bloquear_arquivamento_despesa();

CREATE OR REPLACE FUNCTION fn_bloquear_arquivamento_grupo()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.ativo = FALSE AND OLD.ativo = TRUE THEN
        
        IF EXISTS (
            SELECT 1
            FROM parcela p
            JOIN despesa d ON p.id_despesa = d.id_despesa
            WHERE d.id_grupo = NEW.id_grupo
              AND p.status IN ('PENDENTE', 'ATRASADO', 'EM_ANALISE')
        ) THEN
            RAISE EXCEPTION 'Não é possível arquivar um grupo que possui despesas com parcelas pendentes ou em análise. Quite todas as dívidas primeiro.';
        END IF;

    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_bloquear_arquivamento_grupo
BEFORE UPDATE OF ativo ON grupo
FOR EACH ROW
EXECUTE FUNCTION fn_bloquear_arquivamento_grupo();


CREATE OR REPLACE FUNCTION fn_validar_ativo_parcela() RETURNS TRIGGER AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM usuario WHERE id_usuario = NEW.id_usuario AND ativo = TRUE) THEN
        RAISE EXCEPTION 'Usuário inativo não pode receber parcelas.';
    END IF;
    IF NOT EXISTS (SELECT 1 FROM despesa WHERE id_despesa = NEW.id_despesa AND ativo = TRUE) THEN
        RAISE EXCEPTION 'Não é possível adicionar parcelas a uma despesa inativa.';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
CREATE TRIGGER trg_valida_ativo_parcela BEFORE INSERT OR UPDATE OF id_usuario, id_despesa ON parcela FOR EACH ROW EXECUTE FUNCTION fn_validar_ativo_parcela();

CREATE OR REPLACE FUNCTION fn_bloquear_desativacao_admin() RETURNS TRIGGER AS $$
BEGIN
    IF NEW.ativo = FALSE AND OLD.ativo = TRUE THEN
        IF EXISTS (SELECT 1 FROM grupo WHERE id_admin = NEW.id_usuario AND ativo = TRUE) THEN
            RAISE EXCEPTION 'Usuário é administrador de um grupo ativo e não pode ser desativado.';
        END IF;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
CREATE TRIGGER trg_bloquear_desativacao_admin BEFORE UPDATE OF ativo ON usuario FOR EACH ROW EXECUTE FUNCTION fn_bloquear_desativacao_admin();

CREATE OR REPLACE FUNCTION fn_bloquear_reversao_parcela() RETURNS TRIGGER AS $$
BEGIN
    IF OLD.status = 'PAGO' AND NEW.status <> 'PAGO' THEN
        RAISE EXCEPTION 'Não é permitido alterar o status de uma parcela paga sem o devido estorno.';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_bloquear_reversao_parcela 
BEFORE UPDATE OF status ON parcela 
FOR EACH ROW 
EXECUTE FUNCTION fn_bloquear_reversao_parcela();

CREATE OR REPLACE FUNCTION fn_propagar_inativacao_grupo() RETURNS TRIGGER AS $$
BEGIN
    IF NEW.ativo = FALSE AND OLD.ativo = TRUE THEN
        PERFORM set_config('app.arquivando_grupo', 'true', true); 
        UPDATE despesa SET ativo = FALSE WHERE id_grupo = NEW.id_grupo;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_propagar_inativacao_grupo 
AFTER UPDATE OF ativo ON grupo 
FOR EACH ROW 
EXECUTE FUNCTION fn_propagar_inativacao_grupo();

CREATE OR REPLACE FUNCTION fn_validar_ativo_despesa() RETURNS TRIGGER AS $$
BEGIN
    IF NEW.ativo = TRUE THEN
        IF NOT EXISTS (SELECT 1 FROM grupo WHERE id_grupo = NEW.id_grupo AND ativo = TRUE) THEN
            RAISE EXCEPTION 'Não é possível manter ou adicionar uma despesa ativa em um grupo inativo.';
        END IF;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_valida_ativo_despesa 
BEFORE INSERT OR UPDATE OF id_grupo, ativo ON despesa 
FOR EACH ROW EXECUTE FUNCTION fn_validar_ativo_despesa();

CREATE OR REPLACE FUNCTION fn_valida_status_despesa()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.status = 'QUITADA' AND (
        NOT EXISTS (SELECT 1 FROM parcela WHERE id_despesa = NEW.id_despesa)
        OR EXISTS (SELECT 1 FROM parcela WHERE id_despesa = NEW.id_despesa AND status <> 'PAGO')
    ) THEN
        RAISE EXCEPTION 'Não é possível marcar a despesa como QUITADA enquanto existirem parcelas não pagas ou a despesa não tiver parcelas.';
    END IF;

    IF NEW.status = 'ATIVA' AND NOT EXISTS (
        SELECT 1 FROM parcela WHERE id_despesa = NEW.id_despesa AND status <> 'PAGO'
    ) AND EXISTS (
        SELECT 1 FROM parcela WHERE id_despesa = NEW.id_despesa
    ) THEN
        RAISE EXCEPTION 'Não é possível reativar uma despesa totalmente quitada sem reverter alguma parcela antes.';
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_valida_status_despesa
BEFORE UPDATE OF status ON despesa
FOR EACH ROW
EXECUTE FUNCTION fn_valida_status_despesa();

CREATE OR REPLACE FUNCTION fn_bloquear_parcela_em_despesa_quitada()
RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM despesa
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
FOR EACH ROW
EXECUTE FUNCTION fn_bloquear_parcela_em_despesa_quitada();

CREATE OR REPLACE FUNCTION fn_validar_saida_pertence()
RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM grupo
        WHERE id_grupo = OLD.id_grupo
          AND id_admin  = OLD.id_usuario
    ) THEN
        RAISE EXCEPTION
            'O administrador não pode ser removido do grupo. Transfira a liderança antes de sair.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM parcela p
        JOIN despesa d ON p.id_despesa = d.id_despesa
        WHERE p.id_usuario = OLD.id_usuario
          AND d.id_grupo   = OLD.id_grupo
          AND p.status IN ('PENDENTE', 'ATRASADO', 'EM_ANALISE')
    ) THEN
        RAISE EXCEPTION
            'Usuário possui dívidas pendentes neste grupo e não pode ser removido.';
    END IF;

    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validar_saida_pertence
BEFORE DELETE ON pertence
FOR EACH ROW
EXECUTE FUNCTION fn_validar_saida_pertence();