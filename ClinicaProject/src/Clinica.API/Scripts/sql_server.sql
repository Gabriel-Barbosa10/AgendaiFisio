CREATE DATABASE AgendaiFisioDB

GO

CREATE TABLE usuario (
    id_usuario INT PRIMARY KEY IDENTITY (1, 1),
    nome VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    cpf VARCHAR(14) NOT NULL UNIQUE,
    senha VARCHAR(20) NOT NULL,
    crefito VARCHAR(9) UNIQUE,
    tipo_perfil VARCHAR(10) NOT NULL CHECK (tipo_perfil IN ('PACIENTE', 'TERAPEUTA')),
    aceite_lgpd BIT NOT NULL DEFAULT 0,
    CONSTRAINT CK_Senha_Min CHECK (LEN (senha) >= 8)
);

CREATE TABLE agendamento (
    id_agendamento INT PRIMARY KEY IDENTITY (1, 1),
    id_paciente INT NOT NULL,
    id_terapeuta INT NOT NULL,
    data_hora DATETIME NOT NULL,
    descricao_sintomas VARCHAR(1000) NULL,
    status VARCHAR(15) NOT NULL DEFAULT 'PENDENTE' CHECK (
        status IN (
            'PENDENTE',
            'CONFIRMADO',
            'CANCELADO',
            'REALIZADO',
            'NO_SHOW'
        )
    ),
    CONSTRAINT FK_Agendamento_Paciente FOREIGN KEY (id_paciente) REFERENCES usuario (id_usuario),
    CONSTRAINT FK_Agendamento_Terapeuta FOREIGN KEY (id_terapeuta) REFERENCES usuario (id_usuario)
);

CREATE TABLE prontuario (
    id_prontuario INT PRIMARY KEY IDENTITY (1, 1),
    id_paciente INT NOT NULL,
    id_terapeuta INT NOT NULL,
    versao INT NOT NULL DEFAULT 1,
    descricao VARCHAR(MAX),
    CONSTRAINT FK_Prontuario_Paciente FOREIGN KEY (id_paciente) REFERENCES usuario (id_usuario),
    CONSTRAINT FK_Prontuario_Terapeuta FOREIGN KEY (id_terapeuta) REFERENCES usuario (id_usuario)
);

CREATE TABLE nota_evolucao (
    id_nota INT PRIMARY KEY IDENTITY (1, 1),
    id_prontuario INT NOT NULL,
    id_terapeuta INT NOT NULL,
    id_agendamento INT NULL,
    texto_evolucao VARCHAR(5000) NOT NULL,
    data_registro DATETIME DEFAULT GETDATE (),
    CONSTRAINT FK_Nota_Prontuario FOREIGN KEY (id_prontuario) REFERENCES prontuario (id_prontuario),
    CONSTRAINT FK_Nota_Terapeuta FOREIGN KEY (id_terapeuta) REFERENCES usuario (id_usuario),
    CONSTRAINT FK_Nota_Agendamento FOREIGN KEY (id_agendamento) REFERENCES agendamento (id_agendamento)
);

CREATE TABLE arquivo_exame (
    id_arquivo INT PRIMARY KEY IDENTITY (1, 1),
    caminho_storage VARCHAR(500) NOT NULL,
    tamanho_bytes BIGINT NULL,
    id_prontuario INT NOT NULL,
    CONSTRAINT FK_Arquivo_Prontuario FOREIGN KEY (id_prontuario) REFERENCES prontuario (id_prontuario)
);