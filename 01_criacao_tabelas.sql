
/*

Caso precisa rodar mais de uma vez só descomentar;

DROP TABLE IF EXISTS parcela CASCADE;
DROP TABLE IF EXISTS pertence CASCADE;
DROP TABLE IF EXISTS despesa CASCADE;
DROP TABLE IF EXISTS grupo CASCADE;
DROP TABLE IF EXISTS usuario CASCADE;
DROP TYPE IF EXISTS status_parcela CASCADE;
DROP TYPE IF EXISTS status_despesa CASCADE;
*/

CREATE TYPE status_parcela AS ENUM ('PENDENTE', 'PAGO', 'ATRASADO');
create type status_despesa as ENUM ('ATIVA','QUITADA','CANCELADA');

create table usuario (
    id_usuario Serial primary key,
    nome VARCHAR (100) not NULL,
    senha VARCHAR (255) not NULL,
    email VARCHAR (254) UNIQUE not NULL
);

create TABLE grupo(
    id_grupo Serial primary key,
    codigo_acesso VARCHAR (20) UNIQUE not null,
    nome VARCHAR (100) not null,
    imagem_banner VARCHAR(500),
    id_admin int not null,
    CONSTRAINT fk_admin_grupo Foreign key (id_admin) REFERENCES usuario(id_usuario) ON DELETE NO ACTION
);
create TABLE despesa(
    id_despesa Serial primary key,
    data_cadastro date NOT NULL DEFAULT CURRENT_DATE,
    vencimento date not null,
    nome VARCHAR (255) not null,
    valor DECIMAL(10,2) not null check (valor > 0),
    icone VARCHAR(500),
    status status_despesa not null  default 'ATIVA',
    id_grupo INT NOT NULL,
    CONSTRAINT fk_despesa_grupo Foreign key (id_grupo) REFERENCES grupo(id_grupo)
);

create table pertence(
    primary key (id_usuario, id_grupo),
    id_grupo INT not null,
    id_usuario int not null,
    
    CONSTRAINT fk_pertence_usuario FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario) ON DELETE RESTRICT,
    CONSTRAINT fk_pertence_grupo FOREIGN KEY (id_grupo) REFERENCES grupo(id_grupo) ON DELETE CASCADE
);


CREATE TABLE parcela (
    
    id_parcela SERIAL PRIMARY KEY,
    
    valor DECIMAL(10,2) NOT NULL check (valor > 0),
    
    status status_parcela not null DEFAULT 'PENDENTE',

    data_pagamento date,
    
    id_usuario INT NOT NULL,
    id_despesa INT NOT NULL,
    
    
    CONSTRAINT fk_parcela_usuario FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario),
    CONSTRAINT fk_parcela_despesa FOREIGN KEY (id_despesa) REFERENCES despesa(id_despesa) ON DELETE CASCADE,

    UNIQUE(id_usuario, id_despesa),
    CHECK (
    (status = 'PAGO' AND data_pagamento IS NOT NULL)
    OR
    (status IN ('PENDENTE','ATRASADO') AND data_pagamento IS NULL))
);

CREATE INDEX idx_parcela_despesa ON parcela(id_despesa);
