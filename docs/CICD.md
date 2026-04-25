# CI/CD Pipeline — Documentação Completa

Este documento descreve em detalhes o pipeline de CI/CD implementado para o projeto **MeetupApi**, usando **GitHub Actions**, **Azure Container Registry (ACR)** e **Azure Container Apps**, com infraestrutura provisionada via **Terraform**.

---

## Visão Geral

```
┌──────────────────────────────────────────────────────────────────┐
│                        GitHub Actions                            │
│                                                                  │
│  push/PR → [test] → [docker build]                               │
│  push main only:                                                 │
│    → [docker build + push to ACR] → [deploy to Container Apps]  │
└──────────────────────────────────────────────────────────────────┘
```

O pipeline é disparado em dois cenários:

| Evento | Jobs executados |
|---|---|
| Pull Request → `main` | `test` + `docker` (build only) |
| Push → `main` | `test` + `docker` (build + push) + `deploy` |

---

## Jobs do Pipeline

### Job 1 — `test`: Build & Test (.NET)

Valida que o código compila e todos os testes passam.

**Passos:**
1. Checkout do código-fonte
2. Instala o .NET 10 SDK
3. `dotnet restore` — restaura pacotes NuGet
4. `dotnet build --configuration Release` — compila em modo Release
5. `dotnet test` — executa testes unitários e de integração

**Testes incluídos:**
- `WeatherForecastTests` — testes unitários de conversão de temperatura e validações do record
- `WeatherForecastIntegrationTests` — testes de integração usando `WebApplicationFactory`, validando respostas HTTP reais

---

### Job 2 — `docker`: Docker Build & Push

Valida o Dockerfile em PRs e faz push para o ACR na `main`.

**Passos (PRs e main):**
1. Checkout do código-fonte
2. Define a tag da imagem como o SHA do commit (`github.sha`)
3. `docker build` — constrói a imagem a partir do `Dockerfile` multi-stage

**Passos adicionais (apenas na `main`):**
4. Login no Azure via Service Principal (`AZURE_CREDENTIALS`)
5. `az acr login` — autentica no Azure Container Registry
6. Tagueia e faz push de duas tags:
   - `<sha>` — tag imutável para rastreabilidade
   - `latest` — tag atualizada para sempre apontar para a versão mais recente

---

### Job 3 — `deploy`: Deploy to Azure Container Apps

Atualiza o Container App com a nova imagem. Só executa em push para `main`.

**Passos:**
1. Login no Azure
2. `az containerapp update` — atualiza a revisão ativa com a nova imagem (tag SHA)
3. Exibe a URL pública do Container App

---

## Dockerfile (Multi-stage Build)

O `Dockerfile` usa build multi-stage para minimizar o tamanho da imagem final:

| Stage | Base image | Função |
|---|---|---|
| `base` | `mcr.microsoft.com/dotnet/aspnet:10.0` | Runtime mínimo |
| `build` | `mcr.microsoft.com/dotnet/sdk:10.0` | Compila o projeto |
| `publish` | `build` | Gera artefatos de publicação |
| `final` | `base` | Imagem final com apenas o runtime |

A porta exposta é `8080` (HTTP), compatível com o `target_port` configurado no Container App.

---

## Infraestrutura com Terraform

Os recursos Azure são provisionados com Terraform na pasta `infra/terraform/`.

### Recursos criados

| Recurso | Nome padrão | Descrição |
|---|---|---|
| Resource Group | `meetup-2504-rg` | Agrupa todos os recursos |
| Container Registry | `meetup2504acr` | Armazena as imagens Docker (SKU Basic) |
| Log Analytics Workspace | `meetup2504-law` | Coleta logs do Container Apps |
| Container Apps Environment | `meetup2504-cae` | Ambiente de execução dos containers |
| Container App | `meetup2504-app` | API publicada publicamente |

### Como provisionar a infraestrutura

**Pré-requisitos:**
- [Terraform >= 1.9](https://www.terraform.io/downloads)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) autenticado (`az login`)

```bash
cd infra/terraform

# Inicializar o Terraform
terraform init

# Revisar o plano de execução
terraform plan -var="subscription_id=<SEU_SUBSCRIPTION_ID>"

# Aplicar (criar recursos)
terraform apply -var="subscription_id=<SEU_SUBSCRIPTION_ID>"
```

Para sobrescrever outros valores padrão, use variáveis adicionais:

```bash
terraform apply \
  -var="subscription_id=<ID>" \
  -var="resource_group_name=meu-rg" \
  -var="location=brazilsouth" \
  -var="acr_name=meunomeunico"
```

### Destruir a infraestrutura

```bash
terraform destroy -var="subscription_id=<SEU_SUBSCRIPTION_ID>"
```

---

## Secrets do GitHub Actions

Configure os seguintes secrets no repositório (`Settings → Secrets and variables → Actions`):

| Secret | Descrição | Como obter |
|---|---|---|
| `AZURE_CREDENTIALS` | JSON do Service Principal | `az ad sp create-for-rbac --sdk-auth` |
| `ACR_NAME` | Nome do ACR (ex: `meetup2504acr`) | Output do Terraform: `acr_name` |
| `ACR_LOGIN_SERVER` | Login server do ACR (ex: `meetup2504acr.azurecr.io`) | Output do Terraform: `acr_login_server` |

### Criar o Service Principal

```bash
az ad sp create-for-rbac \
  --name "meetup2504-sp" \
  --role Contributor \
  --scopes /subscriptions/<SUBSCRIPTION_ID>/resourceGroups/meetup-2504-rg \
  --sdk-auth
```

Cole o JSON retornado no secret `AZURE_CREDENTIALS`.

---

## Variáveis de Ambiente no Workflow

As seguintes variáveis estão definidas no topo do arquivo `.github/workflows/ci-cd.yml` e podem ser ajustadas conforme necessário:

| Variável | Valor padrão | Descrição |
|---|---|---|
| `IMAGE_NAME` | `meetupapi` | Nome da imagem Docker |
| `CONTAINER_APP_NAME` | `meetup2504-app` | Nome do Container App no Azure |
| `RESOURCE_GROUP` | `meetup-2504-rg` | Nome do Resource Group |

---

## Fluxo Completo: Do Código ao Azure

```
1. Developer faz push para main
         │
         ▼
2. GitHub Actions dispara o workflow
         │
         ▼
3. [test] dotnet build + dotnet test
         │  (falha aqui = pipeline para)
         ▼
4. [docker] docker build -t meetupapi:<sha> .
         │
         ▼
5. [docker] az acr login
         │
         ▼
6. [docker] docker push para ACR
         │  meetup2504acr.azurecr.io/meetupapi:<sha>
         │  meetup2504acr.azurecr.io/meetupapi:latest
         ▼
7. [deploy] az containerapp update --image meetupapi:<sha>
         │
         ▼
8. API disponível em https://<fqdn>.azurecontainerapps.io
```

---

## Monitoramento e Logs

Após o deploy, você pode acessar os logs do Container App via:

```bash
# Logs em tempo real
az containerapp logs show \
  --name meetup2504-app \
  --resource-group meetup-2504-rg \
  --follow

# Logs via Log Analytics (portal Azure ou CLI)
az monitor log-analytics query \
  --workspace <WORKSPACE_ID> \
  --analytics-query "ContainerAppConsoleLogs_CL | order by TimeGenerated desc | limit 50"
```

---

## Referências

- [GitHub Actions — azure/login](https://github.com/azure/login)
- [Azure Container Apps — az containerapp](https://learn.microsoft.com/cli/azure/containerapp)
- [Terraform — azurerm_container_app](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/container_app)
- [.NET 10 — dotnet test](https://learn.microsoft.com/dotnet/core/tools/dotnet-test)
