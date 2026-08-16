# catalog-api

Serviço responsável pelo catálogo de jogos e pelo fluxo de compra da plataforma FIAP Cloud Games (FCG).

## Finalidade

- CRUD de jogos (somente administradores podem criar/editar/remover).
- Listagem e consulta de jogos (usuários autenticados).
- Registro de compras: publica `OrderPlacedEvent` e aguarda processamento pelo payments-api.
- Adiciona o jogo à biblioteca do usuário ao receber `PaymentProcessedEvent` com status `Approved`.

## Eventos

| Direção  | Evento                   | Descrição                                                        |
|----------|--------------------------|------------------------------------------------------------------|
| Publica  | `OrderPlacedEvent`       | Disparado quando o usuário inicia uma compra via `POST /api/purchases` |
| Consome  | `PaymentProcessedEvent`  | Adiciona jogo à biblioteca do usuário se status = `Approved`    |

## Variáveis de Ambiente

| Variável                                | Obrigatório | Valor padrão (dev local)                          | Descrição                          |
|-----------------------------------------|-------------|----------------------------------------------------|------------------------------------|
| `ConnectionStrings__DefaultConnection` | Sim         | `(localdb)\MSSQLLocalDB;Database=FcgCatalog;...`  | Connection string SQL Server       |
| `JwtSettings__SecretKey`               | Sim         | `fiap-cloud-games-secret-key-minimo-32-chars!`    | Chave secreta do JWT (≥32 chars)   |
| `JwtSettings__Issuer`                  | Sim         | `FiapCloudGames`                                  | Issuer do token JWT (deve ser o mesmo do users-api) |
| `JwtSettings__Audience`               | Sim         | `FiapCloudGames.Client`                           | Audience do token JWT              |
| `RabbitMq__Host`                       | Sim         | `localhost`                                       | Host do RabbitMQ                   |
| `RabbitMq__User`                       | Não         | `guest`                                           | Usuário do RabbitMQ                |
| `RabbitMq__Pass`                       | Não         | `guest`                                           | Senha do RabbitMQ                  |
| `MongoSettings__ConnectionString`      | Sim         | `mongodb://localhost:27017`                       | Connection string do MongoDB (avaliações) |
| `MongoSettings__Database`              | Sim         | `fcg_catalog`                                     | Nome do database Mongo             |
| `RedisSettings__ConnectionString`      | Sim         | `localhost:6379`                                  | Connection string do Redis (cache) |

> No Docker Compose e K8s, `JwtSettings__Issuer` = `FiapCloudGames` e `ConnectionStrings__DefaultConnection` aponta para `catalog-db`.

## Endpoints

| Método   | Rota                        | Auth           | Descrição                                    |
|----------|-----------------------------|----------------|----------------------------------------------|
| `GET`    | `/api/Game`                 | Bearer (user)  | Lista todos os jogos do catálogo             |
| `GET`    | `/api/Game/{id}`            | Bearer (user)  | Retorna um jogo pelo id                      |
| `POST`   | `/api/Game`                 | Bearer (Admin) | Cadastra novo jogo                           |
| `PUT`    | `/api/Game/{id}`            | Bearer (Admin) | Atualiza dados de um jogo                    |
| `DELETE` | `/api/Game/{id}`            | Bearer (Admin) | Remove um jogo                               |
| `POST`   | `/api/purchases`            | Bearer (user)  | Inicia a compra de um jogo                   |
| `POST`   | `/api/Review`               | Bearer (user)  | Registra avaliação de um jogo                |
| `GET`    | `/api/Review/game/{gameId}` | Bearer (user)  | Lista avaliações de um jogo                  |

## Observabilidade

| Rota       | Descrição                                              |
|------------|----------------------------------------------------------|
| `/metrics` | Métricas Prometheus (contagem de requisições, latência, status code) — raspado pelo Prometheus da orchestration |

### Exemplos de uso

```bash
# Listar jogos (precisa de token JWT)
curl http://localhost:8000/api/Game \
  -H "Authorization: Bearer <token>"

# Comprar jogo
curl -X POST http://localhost:8000/api/purchases \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"gameId":"<guid-do-jogo>"}'
```

## Como rodar localmente

**Pré-requisitos:** .NET 8 SDK, SQL Server (LocalDB ou instância local), RabbitMQ.

```bash
cd src/Catalog.API
dotnet run
# Swagger disponível em http://localhost:<porta>/ (modo Development)
```

## Como rodar via Docker

A partir da raiz do repositório:

```bash
docker compose -f ../fcg-orchestration/docker-compose.yml up --build catalog-api catalog-db rabbitmq
```

A partir da Fase 3, o serviço não é mais acessível direto — todo tráfego externo passa pelo Kong (`fcg-orchestration`), em **http://localhost:8000/api/Game**, **http://localhost:8000/api/purchases** e **http://localhost:8000/api/Review**. Ver o README do `fcg-orchestration` para detalhes do gateway.

## Repositório

> Repositório: https://github.com/YuriLucka/fcg-catalog-api
