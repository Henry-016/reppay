<h1 align="center">RepPay</h1>

<p align="center">
  Uma plataforma web moderna e desacoplada, construída para simplificar a criação, gestão financeira e o fluxo de tarefas em repúblicas e grupos estudantis.
</p>

---

## 📖 Sobre

O **RepPay** foi criado para eliminar o atrito na divisão de despesas, controle de inadimplência e administração de moradias estudantis, permitindo que os usuários gerenciem seus grupos com facilidade. A plataforma conta com uma API REST robusta construída em C# (.NET 9.0) no back-end, e um front-end rápido e responsivo desenvolvido em React.

## 💻 Demonstração

> <img width="1908" height="1019" alt="Cadastro" src="https://github.com/user-attachments/assets/b50b230c-0381-485c-8426-977f133d2b42" />


  <img width="1908" height="1019" alt="Login" src="https://github.com/user-attachments/assets/9ba93e88-c802-4a23-893c-c1d917b100ac" />

  
  <img width="1908" height="1019" alt="Criar_Republica" src="https://github.com/user-attachments/assets/a7283366-3b92-45e5-b4cc-7fc5ea569aa6" />

  
  <img width="1908" height="1019" alt="Criar_Despesa" src="https://github.com/user-attachments/assets/b272b660-6ee3-4322-bb88-988a25a197c2" />



## 🛠️ Tecnologias Utilizadas

Este projeto foi construído utilizando os seguintes frameworks, bibliotecas e ferramentas principais:

* ![React](https://img.shields.io/badge/react-%2320232a.svg?style=for-the-badge&logo=react&logoColor=%2361DAFB)
* ![Vite](https://img.shields.io/badge/vite-%23646CFF.svg?style=for-the-badge&logo=vite&logoColor=white)
* ![TypeScript](https://img.shields.io/badge/typescript-%23007ACC.svg?style=for-the-badge&logo=typescript&logoColor=white)
* ![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
* ![.NET](https://img.shields.io/badge/.NET_9.0-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
* ![Postgres](https://img.shields.io/badge/postgres-%23316192.svg?style=for-the-badge&logo=postgresql&logoColor=white)
* ![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)

---

## 🚀 Começando (Getting Started)

A maneira mais fácil e segura de rodar toda a stack (Front-end, Back-end, Banco de Dados e migrações) é utilizando o Docker Compose.

### Pré-requisitos

Certifique-se de ter os seguintes programas instalados na sua máquina:
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Ou Docker Engine + Compose no Linux)
* [Git](https://git-scm.com/)
* *Atenção:* As portas **5432**, **8080**, **5173** e **3000** devem estar livres na sua máquina.

### Passos de Execução

**1. Clone o Repositório:**
```bash
$ git clone [https://github.com/Henry-016/reppay.git](https://github.com/Henry-016/reppay.git)
$ cd reppay
```

**2. Suba os containers (Modo de Desenvolvimento):** Execute o comando abaixo para construir a aplicação e iniciar os serviços com *Hot Reload* no Front-end:
```bash
$ docker compose up -d db api frontend-dev
```

**3. Acessando as Aplicações:** Assim que a inicialização for concluída, você poderá acessar os serviços em:
* **Frontend (Aplicação Web):** http://localhost:5173
* **Backend (API REST Swagger):** http://localhost:5149/swagger

---


## 🔐 Variáveis de Ambiente & Segurança

* As configurações de portas e variáveis de ambiente estão orquestradas no arquivo `docker-compose.yml`.
* **Criptografia de Senhas:** O back-end utiliza a biblioteca `BCrypt.Net-Next` para garantir a proteção (hash) das senhas dos usuários.
* **Autenticação JWT:** A segurança das rotas da API é garantida através de tokens JWT baseados em chaves simétricas.

---

## 📂 Estrutura do Repositório

```text
📦 RepPay
├── 📂 .vscode/               # Configurações de ambiente do VS Code
├── 📂 Artefatos/             # Documentação, diagramas de banco e imagens do projeto
├── 📂 react/                 # Aplicação Front-end (Interface com o usuário)
├── 📂 RepPay.API/            # Aplicação Back-end (API em C# .NET)
├── 📂 RepPay.API.Tests/      # Testes unitários para garantir a qualidade da API
├── 📂 scripts_sql/           # Scripts de criação de tabelas, triggers e índices do PostgreSQL
├── 📄 .gitignore             # Arquivos e pastas ignorados pelo controle de versão
├── 📄 docker-compose.yml     # Orquestração dos containers (Banco, API e Front)
├── 📄 README.md              # Documentação principal do repositório
└── 📄 RepPay.sln             # Solução do Visual Studio que agrupa o back-end e os testes
```

---

## ✍️ Autores

* **[DANIEL MENDES DA SILVA]** - *Front-end*
* **[ENRIQUE FERREIRA DA SILVA]** - *Back-end*
* **[DAVISON GABRIEL MONTEIRO DE FARIAS]** - *Banco de dados*
