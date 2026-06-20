<h1 align="center">RepPay</h1>

<p align="center">
  Uma plataforma web moderna e desacoplada, construída para simplificar a criação, gestão financeira e o fluxo de tarefas em repúblicas e grupos estudantis.
</p>

---

## 📖 Sobre

O **RepPay** foi criado para eliminar o atrito na divisão de despesas, controle de inadimplência e administração de moradias estudantis, permitindo que os usuários gerenciem seus grupos com facilidade. A plataforma conta com uma API REST robusta construída em C# (.NET 9.0) no back-end, e um front-end rápido e responsivo desenvolvido em React.

## 💻 Demonstração

> *(Dica: Adicione aqui um GIF ou link para um vídeo curto demonstrando o login e a criação de uma despesa na plataforma).*

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

## 👨‍💻 Guia de Apresentação: Como alterar o código ao vivo

O ambiente foi configurado para que não seja necessária nenhuma IDE pesada (como Visual Studio) para demonstrar alterações de código durante a apresentação. Qualquer editor de texto simples (Bloco de Notas, Nano, Gedit) é suficiente, pois **a compilação ocorre dentro do Docker**.

Com a stack rodando (`docker compose up -d db api frontend-dev`), siga os fluxos abaixo de acordo com a camada que precisa ser alterada:

### 🎨 Alterações no Front-end (React)
1. Abra o arquivo desejado (ex: `.tsx` ou `.scss`) em qualquer editor de texto.
2. Faça a alteração (ex: mudar a cor de um botão ou alterar um texto).
3. **Salve o arquivo.**
4. *Resultado:* O Vite detectará a mudança e recarregará a página no navegador em frações de segundo, refletindo a alteração instantaneamente.

### ⚙️ Alterações no Back-end (C#)
1. Abra o arquivo `.cs` desejado e modifique a lógica ou regra de negócio.
2. Salve o arquivo.
3. No terminal, execute o comando abaixo para recompilar exclusivamente a API:
   ```bash
   docker compose up -d --build api
   ```
4. *Resultado:* O Docker irá gerar a nova `.dll` e substituir o container da API de forma transparente, mantendo o banco de dados e o front-end intactos.

### 🗄️ Alterações no Banco de Dados (PostgreSQL)
1. Abra os arquivos `.sql` localizados na pasta `scripts_sql/` e faça a alteração (ex: adicionar uma nova Trigger ou alterar o limite de uma coluna).
2. Salve o arquivo.
3. No terminal, destrua o volume antigo do banco para forçar a leitura do novo script:
   ```bash
   docker compose down -v
   ```
4. Suba a stack novamente:
   ```bash
   docker compose up -d db api frontend-dev
   ```
5. *Resultado:* O PostgreSQL nascerá do zero e executará as novas regras estruturais imediatamente.

*(Dica de Apresentação: Mantenha um terminal rodando `docker compose logs -f api` em um monitor secundário para acompanhar possíveis erros de sintaxe no C# em tempo real).*

---

## 🔐 Variáveis de Ambiente & Segurança

* As configurações de portas e variáveis de ambiente estão orquestradas no arquivo `docker-compose.yml`.
* **Criptografia de Senhas:** O back-end utiliza a biblioteca `BCrypt.Net-Next` para garantir a proteção (hash) das senhas dos usuários.
* **Autenticação JWT:** A segurança das rotas da API é garantida através de tokens JWT baseados em chaves simétricas.

---

## 📂 Estrutura do Repositório

```text
reppay/
├── react/                   # Código-fonte do Front-end (React/Vite)
├── RepPay.API/              # Código-fonte do Back-end (Controllers, Models, Services)
├── scripts_sql/             # Scripts automáticos do BD (DDL e Triggers)
├── docker-compose.yml       # Orquestração da infraestrutura Docker
└── README.md                # Documentação do projeto
```

---

## ✍️ Autores

* **[Davison / Seu Nome]** - *Desenvolvimento Back-end / Front-end / Infraestrutura*
* **[Nome do Integrante 2]** - *Responsabilidade no projeto*
* **[Nome do Integrante 3]** - *Responsabilidade no projeto*
* **[Nome do Integrante 4]** - *Responsabilidade no projeto*
