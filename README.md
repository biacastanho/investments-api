# Investments API

API REST desenvolvida para a atividade substitutiva da Fase 1 da Pós-Tech em Arquitetura de Sistemas .NET com Azure.

O projeto permite cadastrar usuários, realizar autenticação por JWT e gerenciar investimentos. Todas as operações com investimentos são protegidas, e cada usuário pode consultar ou alterar somente os próprios registros.

## Tecnologias utilizadas

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server 2022
- Docker
- JWT Bearer
- BCrypt
- Swagger / OpenAPI
- xUnit, Moq e FluentAssertions

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

- Cadastro de usuário;
- autenticação com e-mail e senha;
- geração de token JWT;
- cadastro, consulta, atualização e exclusão de investimentos;
- controle de acesso aos investimentos pelo usuário autenticado;
- armazenamento de senhas com hash BCrypt;
- cadastro e manutenção dos tipos de investimento;
- validação dos dados recebidos;
- documentação e testes dos endpoints pelo Swagger.

## Pré-requisitos

Para executar o projeto, é necessário ter instalado:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/);
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0);
- Visual Studio 2022 com a carga de trabalho **Desenvolvimento ASP.NET e Web**, ou outro editor compatível com .NET.

Confira as instalações no PowerShell:

```powershell
docker --version
docker compose version
dotnet --version
```

O Docker Desktop deve estar aberto antes de iniciar o banco de dados.

## Banco de dados com Docker

O arquivo `docker-compose.yml`, localizado na raiz do projeto, cria uma instância do SQL Server 2022 na porta `1433`.

Abra o PowerShell na pasta que contém `InvestmentsApi.sln` e execute:

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

## Configuração da conexão

A configuração padrão da aplicação utiliza SQL Server LocalDB. Para usar o SQL Server do Docker, defina a connection string no PowerShell antes de iniciar a API:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=InvestmentsDb;User Id=sa;Password=Investments@2026;Encrypt=True;TrustServerCertificate=True"
```

Também é possível definir uma chave JWT própria para o ambiente local:

```powershell
$env:Jwt__Secret = "Investments-local-jwt-secret-with-at-least-32-characters"
```

Essas variáveis permanecem disponíveis somente na janela atual do PowerShell. Se abrir outro terminal para executar a API, será necessário configurá-las novamente.

As migrations são aplicadas automaticamente na primeira inicialização. Dessa forma, o banco `InvestmentsDb`, suas tabelas e os tipos iniciais de investimento serão criados sem a execução manual de scripts SQL.

## Executando o projeto

Na raiz do projeto, restaure as dependências e faça o build:

```powershell
dotnet restore InvestmentsApi.sln
dotnet build InvestmentsApi.sln
```

Depois, execute a API:

```powershell
dotnet run --project .\src\Investments.Api --launch-profile http
```

A documentação ficará disponível em:

```text
http://localhost:5012/swagger
```

O terminal deve permanecer aberto enquanto a API estiver em execução.

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

## Utilizando a autenticação no Swagger

1. Execute `POST /users` para cadastrar um usuário.
2. Execute `POST /auth` com o e-mail e a senha cadastrados.
3. Copie o token retornado pela API.
4. Clique em **Authorize**, na parte superior do Swagger.
5. Informe o token e confirme a autorização.
6. Execute os endpoints protegidos de investimentos.

Exemplo de cadastro:

```json
{
  "name": "Usuario Teste",
  "email": "usuario@teste.com",
  "password": "Senha@123"
}
```

Exemplo de investimento:

```json
{
  "type": "Acoes",
  "amount": 1500.75,
  "investedAt": "2026-09-01T00:00:00Z",
  "description": "PETR4"
}
```

Os tipos criados inicialmente são `Acoes`, `RendaFixa`, `Fundos`, `Tesouro` e `Cripto`.

## Testes

Para executar todos os testes:

```powershell
dotnet test InvestmentsApi.sln
```

Os testes cobrem as principais regras de domínio e o fluxo de cadastro, autenticação, proteção dos endpoints, CRUD de investimentos e isolamento dos dados entre usuários.

## Verificando o banco de dados

Depois de executar a API, é possível confirmar a criação do banco pelo próprio container:

```powershell
docker exec -it investments-sqlserver /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "Investments@2026" -C `
  -Q "SELECT name FROM sys.databases"
```

Para listar as tabelas:

```powershell
docker exec -it investments-sqlserver /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "Investments@2026" -C `
  -d InvestmentsDb `
  -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME"
```

## Encerrando o ambiente

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

> O comando `docker compose down -v` também remove o volume e apaga o banco de dados. Use-o somente quando quiser recriar o ambiente do zero.

## Solução de problemas

### A API tenta acessar o LocalDB

Defina novamente a variável `ConnectionStrings__DefaultConnection` no mesmo PowerShell em que executará `dotnet run`.

### A porta 1433 já está em uso

Verifique se existe outra instância do SQL Server ou outro container utilizando essa porta:

```powershell
docker ps
```

### O Swagger não abre

Confirme se o terminal da API mostra que ela está escutando em `http://localhost:5012`. Se a execução foi encerrada, rode novamente o comando `dotnet run`.

### Erro de conexão logo após subir o container

O SQL Server pode levar alguns segundos para concluir a inicialização. Confira os logs com `docker compose logs -f sqlserver`, aguarde a mensagem de disponibilidade e tente iniciar a API novamente.
