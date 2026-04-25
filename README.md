# meetup-2504 — MeetupApi

API REST em .NET 10 com pipeline de CI/CD completo usando **GitHub Actions**, **Azure Container Registry (ACR)** e **Azure Container Apps**, com infraestrutura provisionada via **Terraform**.

---

## Sobre o Projeto

O **MeetupApi** é uma ASP.NET Core Minimal API que expõe um endpoint de previsão do tempo (`/weatherforecast`). O projeto inclui:

- API com .NET 10 (Minimal API)
- Testes unitários e de integração com xUnit
- Dockerfile multi-stage otimizado
- Infraestrutura como código com Terraform (Azure)
- Pipeline de CI/CD com GitHub Actions

---

## Endpoints da API

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/weatherforecast` | Retorna 5 previsões do tempo aleatórias |

**Exemplo de resposta:**
```json
[
  {
    "date": "2025-04-26",
    "temperatureC": 22,
    "temperatureF": 71,
    "summary": "Warm"
  }
]
```

---

## Estrutura do Repositório

```
meetup-2504/
├── .github/
│   └── workflows/
│       └── ci-cd.yml          # Pipeline GitHub Actions
├── MeetupApi/                 # Código-fonte da API
│   ├── Program.cs
│   ├── MeetupApi.csproj
│   └── appsettings.json
├── MeetupApi.Tests/           # Testes unitários e de integração
│   ├── WeatherForecastTests.cs
│   ├── WeatherForecastIntegrationTests.cs
│   └── MeetupApi.Tests.csproj
├── infra/
│   └── terraform/             # Infraestrutura como código
│       ├── main.tf
│       ├── variables.tf
│       └── outputs.tf
├── docs/
│   └── CICD.md                # Documentação detalhada do CI/CD
├── Dockerfile                 # Build multi-stage
└── meetup-2504.slnx           # Solution file
```

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
- [Terraform >= 1.9](https://www.terraform.io/downloads)

---

## Desenvolvimento Local

### Executar a API

```bash
cd MeetupApi
dotnet run
```

A API estará disponível em `http://localhost:5000` (ou conforme `launchSettings.json`).

### Executar os Testes

```bash
dotnet test meetup-2504.slnx
```

### Build com Docker

```bash
# Build da imagem
docker build -t meetupapi:local .

# Executar o container
docker run -p 8080:8080 meetupapi:local
```

Acesse: `http://localhost:8080/weatherforecast`

---

## Infraestrutura no Azure (Terraform)

Os recursos Azure são provisionados com Terraform na pasta `infra/terraform/`.

**Recursos criados:**
- Resource Group
- Azure Container Registry (ACR) — SKU Basic
- Log Analytics Workspace
- Azure Container Apps Environment
- Azure Container App (API pública)

```bash
cd infra/terraform

terraform init
terraform plan -var="subscription_id=<SEU_SUBSCRIPTION_ID>"
terraform apply -var="subscription_id=<SEU_SUBSCRIPTION_ID>"
```

Consulte [`docs/CICD.md`](docs/CICD.md) para detalhes completos.

---

## CI/CD com GitHub Actions

O pipeline `.github/workflows/ci-cd.yml` executa automaticamente:

| Evento | Ação |
|---|---|
| Pull Request → `main` | Build + Testes + Validação do Dockerfile |
| Push → `main` | Build + Testes + Push ACR + Deploy Container Apps |

### Secrets necessários no repositório

| Secret | Descrição |
|---|---|
| `AZURE_CREDENTIALS` | JSON do Service Principal Azure |
| `ACR_NAME` | Nome do Azure Container Registry |
| `ACR_LOGIN_SERVER` | Login server do ACR (ex: `meuacr.azurecr.io`) |

Consulte [`docs/CICD.md`](docs/CICD.md) para o passo a passo completo de configuração.

---

## Documentação

- [**docs/CICD.md**](docs/CICD.md) — Documentação completa do pipeline CI/CD, Terraform e configuração de secrets
