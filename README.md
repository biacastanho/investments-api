# Investments API

[![CI](https://github.com/biacastanho/investments-api/actions/workflows/ci.yml/badge.svg)](https://github.com/biacastanho/investments-api/actions/workflows/ci.yml)
[![CD](https://github.com/biacastanho/investments-api/actions/workflows/cd.yml/badge.svg)](https://github.com/biacastanho/investments-api/actions/workflows/cd.yml)

API REST desenvolvida para as atividades substitutivas da Pós-Tech em Arquitetura de Sistemas .NET com Azure:

- **Fase 1:** construção da API;
- **Fase 2:** esteira de CI/CD com GitHub Actions e implantação no Azure App Service (veja [CI/CD](#cicd)).

O projeto permite cadastrar usuários, realizar autenticação por JWT e gerenciar investimentos. Os endpoints de investimentos são protegidos, e cada usuário pode consultar ou alterar somente os próprios registros.

## Tecnologias utilizadas

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server LocalDB
- SQL Server 2022 com Docker (opcional)
- JWT Bearer
- BCrypt
- Swagger / OpenAPI
- xUnit, Moq e FluentAssertions
- GitHub Actions
- Azure App Service e Azure SQL Database

## Estrutura do projeto

```text
src/
  Investments.Api             Controllers, configuração da API e middlewares
  Investments.Application     Serviços, casos de uso e DTOs
  Investments.Domain          Entidades e contratos de repositório
  Investments.Infrastructure  Entity Framework, SQL Server, migrations e JWT

tests/
  Investments.Tests           Testes de unidade e integração
```

## Funcionalidades

- Cadastro de usuários;
- autenticação com e-mail e senha;
- geração de token JWT;
- cadastro, consulta, atualização e exclusão de investimentos;
- controle de acesso aos investimentos pelo usuário autenticado;
- armazenamento de senhas com hash BCrypt;
- cadastro e manutenção dos tipos de investimento;
- validação dos dados recebidos;
- documentação e execução dos endpoints pelo Swagger;
- criação e atualização automática do banco por migrations.

## Pré-requisitos

Para executar o projeto com a configuração padrão, é necessário ter instalado:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0);
- Visual Studio 2022 com a carga de trabalho **Desenvolvimento ASP.NET e Web**;
- SQL Server Express LocalDB.

Confira a versão instalada do .NET no PowerShell:

```powershell
dotnet --version
```

O projeto também pode ser executado com SQL Server em um container Docker. Nesse caso, é necessário ter o [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado e em execução.

## Banco de dados

A aplicação utiliza SQL Server e está configurada por padrão para executar com SQL Server LocalDB.

### Opção A — SQL Server LocalDB

A connection string padrão está configurada no arquivo `appsettings.json`:

```text
Server=(localdb)\MSSQLLocalDB;Database=InvestmentsDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

Não é necessário criar o banco ou executar scripts SQL manualmente.

Ao iniciar a API pela primeira vez, as migrations do Entity Framework são aplicadas automaticamente, criando:

- o banco `InvestmentsDb`;
- as tabelas da aplicação;
- os relacionamentos e índices;
- os tipos iniciais de investimento.

Para visualizar o banco no Visual Studio:

1. Abra **View → SQL Server Object Explorer**;
2. expanda **SQL Server → (localdb)\MSSQLLocalDB**;
3. atualize a pasta **Databases**;
4. abra o banco `InvestmentsDb`.

### Opção B — SQL Server com Docker

Para quem não possui LocalDB, o arquivo `docker-compose.yml` cria uma instância do SQL Server 2022 na porta `1433`.

Confira se o Docker está disponível:

```powershell
docker --version
docker compose version
```

Na raiz do projeto, execute:

```powershell
docker compose up -d
```

Confira se o container foi iniciado:

```powershell
docker compose ps
```

Para acompanhar a inicialização do SQL Server:

```powershell
docker compose logs -f sqlserver
```

Quando aparecer a mensagem indicando que o SQL Server está pronto para receber conexões, pressione `Ctrl+C`. Isso encerra somente a exibição dos logs; o container continuará em execução.

Para usar o banco do Docker, defina a connection string no mesmo PowerShell em que executará a API:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=InvestmentsDb;User Id=sa;Password=Investments@2026;Encrypt=True;TrustServerCertificate=True"
```

Essa variável permanece disponível somente na janela atual do PowerShell.

## Executando o projeto

Abra o PowerShell na pasta que contém o arquivo `InvestmentsApi.sln`.

Restaure as dependências:

```powershell
dotnet restore InvestmentsApi.sln
```

Faça o build da solução:

```powershell
dotnet build InvestmentsApi.sln
```

A configuração padrão utiliza o LocalDB, portanto não é necessário informar uma connection string manualmente.

Execute a API:

```powershell
dotnet run --project .\src\Investments.Api --launch-profile http
```

A documentação da API ficará disponível em:

```text
http://localhost:5012/swagger
```

O terminal deve permanecer aberto enquanto a API estiver em execução.

### Executando pelo Visual Studio

Também é possível executar diretamente pelo Visual Studio:

1. Abra `InvestmentsApi.sln`;
2. clique com o botão direito no projeto `Investments.Api`;
3. selecione **Set as Startup Project**;
4. escolha o perfil `http`;
5. pressione `F5` ou clique no botão de execução.

## Endpoints

| Método | Rota | Autenticação | Descrição |
|---|---|---|---|
| POST | `/users` | Não | Cadastra um usuário |
| POST | `/auth` | Não | Autentica o usuário e retorna um JWT |
| GET | `/investments` | Bearer JWT | Lista os investimentos do usuário |
| GET | `/investments/{id}` | Bearer JWT | Consulta um investimento |
| POST | `/investments` | Bearer JWT | Cadastra um investimento |
| PUT | `/investments/{id}` | Bearer JWT | Atualiza um investimento |
| DELETE | `/investments/{id}` | Bearer JWT | Exclui um investimento |
| GET | `/investment-types` | Bearer JWT | Lista os tipos de investimento |
| GET | `/investment-types/{id}` | Bearer JWT | Consulta um tipo de investimento |
| POST | `/investment-types` | Bearer JWT | Cadastra um tipo de investimento |
| PUT | `/investment-types/{id}` | Bearer JWT | Atualiza um tipo de investimento |
| DELETE | `/investment-types/{id}` | Bearer JWT | Exclui um tipo que não esteja em uso |
| GET | `/health` | Não | Status da API e do banco, com a versão publicada |

## Autenticação no Swagger

Para acessar os endpoints protegidos:

1. Execute `POST /users` para cadastrar um usuário;
2. execute `POST /auth` com o e-mail e a senha cadastrados;
3. copie o token retornado pela API;
4. clique em **Authorize**, na parte superior do Swagger;
5. informe o token e confirme;
6. execute os endpoints protegidos.

Exemplo de cadastro:

```json
{
  "name": "Usuario Teste",
  "email": "usuario@teste.com",
  "password": "Senha@123"
}
```

Exemplo de autenticação:

```json
{
  "email": "usuario@teste.com",
  "password": "Senha@123"
}
```

Exemplo de cadastro de investimento:

```json
{
  "type": "Acoes",
  "amount": 1500.75,
  "investedAt": "2026-09-01T00:00:00Z",
  "description": "PETR4"
}
```

Os tipos de investimento criados inicialmente são:

- `Acoes`;
- `RendaFixa`;
- `Fundos`;
- `Tesouro`;
- `Cripto`.

Novos tipos podem ser cadastrados pelo endpoint `POST /investment-types`.

## Validações e segurança

A API possui validações para:

- campos obrigatórios;
- formato do e-mail;
- e-mail já cadastrado;
- valor investido maior que zero;
- data de investimento obrigatória e não futura;
- tipo de investimento existente;
- tamanho máximo dos campos;
- autenticação nos endpoints protegidos;
- acesso somente aos investimentos do usuário autenticado.

As senhas não são armazenadas em texto puro. Antes de serem gravadas no banco, elas são processadas com BCrypt.

O token JWT contém o identificador do usuário. Esse identificador é utilizado para filtrar as consultas e impedir que um usuário consulte, atualize ou exclua investimentos pertencentes a outro usuário.

## Testes

Para executar todos os testes:

```powershell
dotnet test InvestmentsApi.sln
```

Os testes cobrem:

- regras das entidades;
- validações dos investimentos;
- hash e verificação de senha;
- cadastro de usuário;
- autenticação;
- proteção dos endpoints;
- fluxo completo de investimentos;
- isolamento dos dados entre usuários;
- manutenção dos tipos de investimento.

Ao final da execução, confirme que não existem testes com falha.

## CI/CD

A esteira foi montada com GitHub Actions e está dividida em dois workflows:

```text
push na main ──> CI (.github/workflows/ci.yml) ──sucesso──> CD (.github/workflows/cd.yml) ──> Azure App Service
                 restore                                    baixa o artefato do CI
                 build                                      deploy no App Service
                 testes                                     verifica GET /health
                 publish + artefato
```

### CI

Arquivo: [`.github/workflows/ci.yml`](.github/workflows/ci.yml)

É executado automaticamente a cada `push` e `pull request` na branch `main` (e também manualmente, pela aba **Actions**). Os passos são:

1. checkout do código;
2. instalação do .NET 8;
3. `dotnet restore`;
4. `dotnet build` em `Release`, com a versão `1.0.<número da execução>`;
5. `dotnet test`, com os testes de unidade e integração;
6. upload do resultado dos testes (`test-results`, arquivo `.trx`);
7. `dotnet publish` da API;
8. upload do pacote de publicação como artefato (`investments-api`).

Se a compilação ou qualquer teste falhar, o workflow é interrompido e nenhum artefato é gerado.

### CD

Arquivo: [`.github/workflows/cd.yml`](.github/workflows/cd.yml)

É disparado pelo evento `workflow_run`, sempre que o CI termina. O deploy só acontece quando o CI terminou com **sucesso** e foi originado por um push na `main`; pull requests não são implantados. Os passos são:

1. download do artefato `investments-api` gerado pela execução do CI que disparou o CD;
2. deploy no Azure App Service com a action `azure/webapps-deploy`;
3. verificação da aplicação publicada, chamando `GET /health` até a API responder `Healthy`;
4. resumo do deploy com o commit implantado e o link do Swagger.

O job usa o ambiente `production` do GitHub, então cada implantação fica registrada em **Deployments**, com o link da aplicação.

Enquanto a variável `AZURE_WEBAPP_NAME` não estiver configurada no repositório, o job de deploy aparece como *skipped*.

### Configuração do ambiente de produção no Azure

Todos os recursos abaixo utilizam camadas gratuitas do Azure.

**1. Banco de dados (Azure SQL Database — oferta gratuita)**

1. No portal do Azure, crie um **SQL Database**;
2. no topo da tela de criação, clique em **Apply offer** para usar a oferta gratuita;
3. crie um servidor novo com **SQL authentication** (anote usuário e senha);
4. em **Networking**, marque **Allow Azure services and resources to access this server**;
5. após a criação, abra **Connection strings** e copie a string **ADO.NET**, substituindo `{your_password}` pela senha.

**2. Aplicação (Azure App Service — plano F1 gratuito)**

1. Crie um **Web App** com:
   - **Publish:** Code;
   - **Runtime stack:** .NET 8 (LTS);
   - **Operating System:** Windows;
   - **Pricing plan:** Free F1;
2. após a criação, em **Settings → Configuration → General settings**, ative **SCM Basic Auth Publishing Credentials** e salve;
3. em **Settings → Environment variables**:
   - na aba **Connection strings**, adicione `DefaultConnection`, do tipo `SQLAzure`, com a string do banco;
   - na aba **App settings**, adicione `Jwt__Secret` com um valor aleatório de pelo menos 32 caracteres;
4. em **Overview**, clique em **Download publish profile**.

As migrations são aplicadas automaticamente quando a API inicia, então não é necessário executar scripts no banco.

**3. Repositório no GitHub**

Em **Settings → Secrets and variables → Actions**:

| Tipo | Nome | Valor |
|---|---|---|
| Secret | `AZURE_WEBAPP_PUBLISH_PROFILE` | Conteúdo completo do arquivo `.PublishSettings` baixado |
| Variable | `AZURE_WEBAPP_NAME` | Nome do Web App criado no Azure |

Depois disso, qualquer push na `main` executa a esteira completa. Para disparar sem alterar código, execute o CI manualmente em **Actions → CI → Run workflow**.

### Verificando a implantação

Ao final do CD, a aplicação fica disponível no endereço do App Service:

```text
https://<nome-do-app>.azurewebsites.net/swagger
https://<nome-do-app>.azurewebsites.net/health
```

O `/health` retorna a versão implantada, que corresponde ao número da execução do CI e ao commit:

```json
{"status":"Healthy","version":"1.0.12+3e9a1aa...","environment":"Production","checks":{"database":"Healthy"}}
```

## Verificando o banco no Visual Studio

Quando a aplicação estiver usando LocalDB, abra o **SQL Server Object Explorer** e expanda:

```text
SQL Server
  (localdb)\MSSQLLocalDB
    Databases
      InvestmentsDb
        Tables
```

As principais tabelas são:

- `dbo.Users`;
- `dbo.Investments`;
- `dbo.InvestmentTypes`;
- `dbo.__EFMigrationsHistory`.

Para conferir os tipos de investimento cadastrados, abra uma nova consulta no banco `InvestmentsDb` e execute:

```sql
SELECT *
FROM InvestmentTypes;
```

Para conferir os usuários e verificar que as senhas foram armazenadas como hash:

```sql
SELECT Id, Name, Email, PasswordHash
FROM Users;
```

Para visualizar os investimentos:

```sql
SELECT *
FROM Investments;
```

Para verificar as migrations aplicadas:

```sql
SELECT *
FROM __EFMigrationsHistory;
```

## Encerrando o ambiente Docker

Esta seção se aplica somente a quem escolheu executar o SQL Server com Docker.

Para parar o SQL Server sem apagar os dados:

```powershell
docker compose stop
```

Para iniciar novamente:

```powershell
docker compose start
```

Para remover o container e manter o volume com os dados:

```powershell
docker compose down
```

O comando abaixo também remove o volume e apaga o banco de dados:

```powershell
docker compose down -v
```

Utilize-o somente quando quiser recriar o ambiente do zero.

## Solução de problemas

### O banco não aparece no SQL Server Object Explorer

Inicie a API pelo menos uma vez para que as migrations sejam aplicadas. Depois, clique em **Refresh** na pasta `Databases`.

### A API continua usando o LocalDB quando deveria usar o Docker

Defina `ConnectionStrings__DefaultConnection` no mesmo PowerShell em que executará o comando `dotnet run`.

### A porta 1433 já está em uso

Verifique se existe outra instância ou outro container utilizando a porta:

```powershell
docker ps
```

### O Swagger não abre

Confirme se a API está em execução e se o terminal mostra:

```text
http://localhost:5012
```

Depois, acesse:

```text
http://localhost:5012/swagger
```

### Erro de conexão logo após iniciar o Docker

O SQL Server pode levar alguns segundos para concluir a inicialização. Confira os logs:

```powershell
docker compose logs -f sqlserver
```

Aguarde a mensagem de disponibilidade e tente iniciar a API novamente.