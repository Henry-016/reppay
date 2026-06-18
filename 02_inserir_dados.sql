-- ==============================================================================
-- SCRIPT DE POPULAÇÃO DO BANCO (SEED)
-- Desenvolvido para testar rotinas, triggers e restrições de negócio.
-- ==============================================================================

-- ATENÇÃO: As senhas abaixo são placeholders para desenvolvimento.
-- Em produção, NUNCA insira senhas em texto puro. O back-end deve 
-- utilizar algoritmos de hash seguros (como bcrypt, argon2).
INSERT INTO usuario (nome, senha, email) VALUES
('Alice Silva', 'hash_senha_123', 'alice@email.com'),
('Bob Santos', 'hash_senha_123', 'bob@email.com'),
('Charlie Mendes', 'hash_senha_123', 'charlie@email.com'),
('Diana Costa', 'hash_senha_123', 'diana@email.com');


-- 2. Inserindo Grupos (Trigger 2A adicionará os admins em 'pertence' automaticamente)
INSERT INTO grupo (codigo_acesso, nome, imagem_banner, id_admin) VALUES
('REP2026', 'República', 'banner_rep.png', 
    (SELECT id_usuario FROM usuario WHERE email = 'alice@email.com')),
('PRAIA26', 'Viagem Praia', 'banner_praia.png', 
    (SELECT id_usuario FROM usuario WHERE email = 'diana@email.com'));


-- 3. Adicionando os outros membros aos grupos (Tabela Pertence)
INSERT INTO pertence (id_grupo, id_usuario) VALUES
-- Membros da República
((SELECT id_grupo FROM grupo WHERE codigo_acesso = 'REP2026'), (SELECT id_usuario FROM usuario WHERE email = 'bob@email.com')),
((SELECT id_grupo FROM grupo WHERE codigo_acesso = 'REP2026'), (SELECT id_usuario FROM usuario WHERE email = 'charlie@email.com')),
-- Membros da Viagem Praia
((SELECT id_grupo FROM grupo WHERE codigo_acesso = 'PRAIA26'), (SELECT id_usuario FROM usuario WHERE email = 'bob@email.com'));


-- 4. Criando Despesas
INSERT INTO despesa (vencimento, nome, valor, icone, id_grupo, status) VALUES
(CURRENT_DATE + INTERVAL '5 days', 'Aluguel', 1500.00, 'icone_casa.png', (SELECT id_grupo FROM grupo WHERE codigo_acesso = 'REP2026'), 'ATIVA'),
(CURRENT_DATE - INTERVAL '2 days', 'Internet', 120.00, 'icone_wifi.png', (SELECT id_grupo FROM grupo WHERE codigo_acesso = 'REP2026'), 'ATIVA'),
(CURRENT_DATE - INTERVAL '10 days', 'Mercado', 300.00, 'icone_carrinho.png', (SELECT id_grupo FROM grupo WHERE codigo_acesso = 'PRAIA26'), 'ATIVA'),
-- Cenário D: Despesa já nasce cancelada para testar o bloqueio da Trigger 1
(CURRENT_DATE + INTERVAL '30 days', 'Academia', 200.00, 'icone_peso.png', (SELECT id_grupo FROM grupo WHERE codigo_acesso = 'REP2026'), 'CANCELADA');


-- 5. Criando as Parcelas

-- ==============================================================================
-- Cenário A: Aluguel (Despesa ATIVA)
-- Alice pagou, os outros não. A despesa deve continuar 'ATIVA'.
-- ==============================================================================
INSERT INTO parcela (valor, status, data_pagamento, id_usuario, id_despesa) VALUES
(500.00, 'PAGO', CURRENT_DATE, 
    (SELECT id_usuario FROM usuario WHERE email = 'alice@email.com'), 
    (SELECT id_despesa FROM despesa WHERE nome = 'Aluguel' AND id_grupo = (SELECT id_grupo FROM grupo WHERE codigo_acesso = 'REP2026'))),
(500.00, 'PENDENTE', NULL, 
    (SELECT id_usuario FROM usuario WHERE email = 'bob@email.com'), 
    (SELECT id_despesa FROM despesa WHERE nome = 'Aluguel' AND id_grupo = (SELECT id_grupo FROM grupo WHERE codigo_acesso = 'REP2026'))),
(500.00, 'PENDENTE', NULL, 
    (SELECT id_usuario FROM usuario WHERE email = 'charlie@email.com'), 
    (SELECT id_despesa FROM despesa WHERE nome = 'Aluguel' AND id_grupo = (SELECT id_grupo FROM grupo WHERE codigo_acesso = 'REP2026')));


-- ==============================================================================
-- Cenário B: Internet (Despesa vai para QUITADA)
-- Nota: A trg_sync_status_despesa roda após cada um destes 3 inserts. 
-- O status da despesa só mudará efetivamente para 'QUITADA' no terceiro insert, 
-- quando a condição (v_pagas = v_total) for verdadeira.
-- ==============================================================================
INSERT INTO parcela (valor, status, data_pagamento, id_usuario, id_despesa) VALUES
(40.00, 'PAGO', CURRENT_DATE - INTERVAL '1 day', 
    (SELECT id_usuario FROM usuario WHERE email = 'alice@email.com'), 
    (SELECT id_despesa FROM despesa WHERE nome = 'Internet' AND id_grupo = (SELECT id_grupo FROM grupo WHERE codigo_acesso = 'REP2026'))),
(40.00, 'PAGO', CURRENT_DATE - INTERVAL '1 day', 
    (SELECT id_usuario FROM usuario WHERE email = 'bob@email.com'), 
    (SELECT id_despesa FROM despesa WHERE nome = 'Internet' AND id_grupo = (SELECT id_grupo FROM grupo WHERE codigo_acesso = 'REP2026'))),
(40.00, 'PAGO', CURRENT_DATE - INTERVAL '1 day', 
    (SELECT id_usuario FROM usuario WHERE email = 'charlie@email.com'), 
    (SELECT id_despesa FROM despesa WHERE nome = 'Internet' AND id_grupo = (SELECT id_grupo FROM grupo WHERE codigo_acesso = 'REP2026')));


-- ==============================================================================
-- Cenário C: Mercado (Despesa ATIVA, bloqueio de saída de grupo)
-- Diana pagou, Bob atrasou. Bob ficará travado pela Trigger 4 se tentar sair.
-- ==============================================================================
INSERT INTO parcela (valor, status, data_pagamento, id_usuario, id_despesa) VALUES
(150.00, 'PAGO', CURRENT_DATE - INTERVAL '5 days', 
    (SELECT id_usuario FROM usuario WHERE email = 'diana@email.com'), 
    (SELECT id_despesa FROM despesa WHERE nome = 'Mercado' AND id_grupo = (SELECT id_grupo FROM grupo WHERE codigo_acesso = 'PRAIA26'))),
(150.00, 'ATRASADO', NULL, 
    (SELECT id_usuario FROM usuario WHERE email = 'bob@email.com'), 
    (SELECT id_despesa FROM despesa WHERE nome = 'Mercado' AND id_grupo = (SELECT id_grupo FROM grupo WHERE codigo_acesso = 'PRAIA26')));


-- ==============================================================================
-- Cenário D: Academia (Despesa CANCELADA)
-- Testa a restrição da Trigger 1: o status da despesa não pode mudar para QUITADA
-- ou ATIVA se ela foi cancelada manualmente.
-- ==============================================================================
INSERT INTO parcela (valor, status, data_pagamento, id_usuario, id_despesa) VALUES
(200.00, 'PAGO', CURRENT_DATE, 
    (SELECT id_usuario FROM usuario WHERE email = 'alice@email.com'), 
    (SELECT id_despesa FROM despesa WHERE nome = 'Academia' AND id_grupo = (SELECT id_grupo FROM grupo WHERE codigo_acesso = 'REP2026')));

    -- Cenário E: Despesa recém-criada (0 parcelas)
-- Testa o estado inicial. O status deve permanecer 'ATIVA'.
INSERT INTO despesa (vencimento, nome, valor, icone, id_grupo, status) VALUES
(CURRENT_DATE + INTERVAL '15 days', 'Água', 90.00, 'icone_gota.png', 
    (SELECT id_grupo FROM grupo WHERE codigo_acesso = 'REP2026'), 'ATIVA');