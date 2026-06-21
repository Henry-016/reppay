CREATE TABLE usuario (
    id_usuario SERIAL PRIMARY KEY,
    foto_perfil text,
    nome VARCHAR(100) NOT NULL,
    senha VARCHAR(255) NOT NULL,
    email VARCHAR(254) UNIQUE NOT NULL,
    ativo boolean not null default true
);

CREATE TABLE codigo_recuperacao (
    id_codigo SERIAL PRIMARY KEY,
    codigo VARCHAR(255) NOT NULL,
    data_expiracao TIMESTAMPTZ NOT NULL,
    codigo_usado BOOLEAN NOT NULL DEFAULT FALSE,
    tentativas INT NOT NULL DEFAULT 0,
    id_usuario INT NOT NULL REFERENCES usuario(id_usuario) ON DELETE CASCADE
);
CREATE INDEX idx_codigo_recuperacao_usuario ON codigo_recuperacao(id_usuario);

CREATE TABLE grupo (
    id_grupo SERIAL PRIMARY KEY,
    codigo_acesso VARCHAR(20) UNIQUE NOT NULL,
    nome VARCHAR(100) NOT NULL,
    imagem_banner text,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    id_admin INT NOT NULL REFERENCES usuario(id_usuario) ON DELETE NO ACTION
);

CREATE TABLE despesa (
    id_despesa SERIAL PRIMARY KEY,
    data_cadastro DATE NOT NULL DEFAULT CURRENT_DATE,
    vencimento DATE NOT NULL, 
    nome VARCHAR(255) NOT NULL,
    valor DECIMAL(10,2) NOT NULL CHECK (valor > 0),
    icone text,
    status VARCHAR(20) NOT NULL DEFAULT 'ATIVA',
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    id_grupo INT NOT NULL REFERENCES grupo(id_grupo) ON DELETE CASCADE,
    CONSTRAINT chk_status_despesa CHECK (status IN ('ATIVA', 'QUITADA'))
);

CREATE TABLE pertence (
    id_usuario INT NOT NULL REFERENCES usuario(id_usuario) ON DELETE RESTRICT,
    id_grupo INT NOT NULL REFERENCES grupo(id_grupo) ON DELETE CASCADE,
    PRIMARY KEY (id_usuario, id_grupo)
);

CREATE TABLE parcela (
    id_parcela SERIAL PRIMARY KEY,
    valor DECIMAL(10,2) NOT NULL CHECK (valor > 0),
    status VARCHAR(20) NOT NULL DEFAULT 'PENDENTE',
    data_pagamento DATE,
    id_usuario INT NOT NULL REFERENCES usuario(id_usuario),
    id_despesa INT NOT NULL REFERENCES despesa(id_despesa) ON DELETE CASCADE,
    UNIQUE(id_usuario, id_despesa),
    CONSTRAINT chk_status_parcela CHECK (status IN ('PENDENTE', 'PAGO', 'ATRASADO', 'EM_ANALISE')),
    CONSTRAINT parcela_check CHECK (
    (status IN ('PAGO', 'EM_ANALISE') AND data_pagamento IS NOT NULL) OR
    (status IN ('PENDENTE', 'ATRASADO') AND data_pagamento IS NULL)
    )
);

CREATE TABLE refresh_token (
    id_refresh SERIAL PRIMARY KEY,

    token_hash text UNIQUE NOT NULL,

    data_criacao TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    data_expiracao TIMESTAMPTZ NOT NULL,

    revogado BOOLEAN NOT NULL DEFAULT FALSE,

    id_usuario INT NOT NULL
        REFERENCES usuario(id_usuario)
        ON DELETE CASCADE
);


CREATE INDEX idx_despesa_grupo
ON despesa(id_grupo);

CREATE INDEX idx_parcela_usuario
ON parcela(id_usuario);

CREATE INDEX idx_pertence_grupo
ON pertence(id_grupo);

CREATE INDEX idx_grupo_admin ON grupo(id_admin);

CREATE INDEX idx_parcela_despesa_status
ON parcela(id_despesa, status);

CREATE INDEX idx_refresh_token_usuario_revogado
ON refresh_token(id_usuario, revogado);