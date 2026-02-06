# Quick Start Guide

Comece a usar o Outline MCP Integration em 5-10 minutos!

## Escolha Seu Cenário

<table>
<tr>
<th>Executável (.exe/.bin)</th>
<th>Docker (SSE Remoto)</th>
<th>Desenvolvimento (.NET SDK)</th>
</tr>
<tr>
<td>Não requer .NET SDK<br>Setup mais rápido<br>Ideal para usuários</td>
<td>Servidor centralizado<br>Múltiplos clientes<br>Ideal para equipes</td>
<td>Hot reload<br>Debug fácil<br>Ideal para devs</td>
</tr>
<tr>
<td><a href="#-cenário-1-executável-recomendado">Ver Cenário 1 ↓</a></td>
<td><a href="#-cenário-2-docker-sse">Ver Cenário 2 ↓</a></td>
<td><a href="#-cenário-3-desenvolvimento-net-sdk">Ver Cenário 3 ↓</a></td>
</tr>
</table>

---

## Cenário 1: Executável (Recomendado)

### Passo 1: Gerar Executável

```bash
# Clone o repositório
git clone <repository-url>
cd Outline

# Windows (PowerShell)
.\scripts\publish-win-x64.ps1

# Linux
./scripts/publish-linux-x64.sh

# macOS (Apple Silicon)
./scripts/publish-osx-arm64.sh
```

**Saída:** `publish/<platform>/Outline.Mcp.Server.exe` (~70-100MB)

### Passo 2: Configurar Variáveis

```bash
# Windows (PowerShell)
$env:OUTLINE_BASE_URL="http://localhost:3000"
$env:OUTLINE_API_KEY="your-api-key-here"

# Linux/macOS
export OUTLINE_BASE_URL="http://localhost:3000"
export OUTLINE_API_KEY="your-api-key-here"
```

**Obter API Key:** http://localhost:3000 → Settings → API & Apps → New API Key

### Passo 3: Configurar no Cursor/Claude Desktop

**Windows - Edite `%APPDATA%\Cursor\User\globalStorage\mcp.json`:**

```json
{
  "mcpServers": {
    "outline": {
      "command": "C:\\Users\\seu-usuario\\repos\\Outline\\publish\\win-x64\\Outline.Mcp.Server.exe",
      "args": [],
      "env": {
        "OUTLINE_API_KEY": "your-api-key",
        "OUTLINE_BASE_URL": "http://localhost:3000"
      }
    }
  }
}
```

**Linux/macOS - Edite `~/.config/Cursor/User/globalStorage/mcp.json`:**

```json
{
  "mcpServers": {
    "outline": {
      "command": "/Users/seu-usuario/repos/Outline/publish/osx-arm64/Outline.Mcp.Server",
      "args": [],
      "env": {
        "OUTLINE_API_KEY": "your-api-key",
        "OUTLINE_BASE_URL": "http://localhost:3000"
      }
    }
  }
}
```

**Importante:** Use caminho **absoluto** para o executável.

### Passo 4: Reiniciar e Testar

1. Reinicie o Cursor completamente
2. Use Agent Mode
3. Digite: "Liste as collections disponíveis no Outline"

**Pronto!** Veja o [guia de uso](./docs/usage/mcp-tools.md).

**Detalhes:** [docs/setup/executable.md](./docs/setup/executable.md)

---

## Cenário 2: Docker (SSE)

### Passo 1: Configurar API Key

```bash
# Configure a API Key do Outline
export OUTLINE_API_KEY="your-api-key-here"
```

**Obter API Key:** http://localhost:3000 → Settings → API & Apps → New API Key

### Passo 2: Executar Stack Completo

```bash
# Clone o repositório
git clone <repository-url>
cd Outline

# Execute todos os serviços (Outline + MCP Server SSE)
docker-compose up -d

# Acompanhe os logs
docker-compose logs -f outline-mcp-server
```

**Serviços iniciados:**
- **MCP Server SSE:** http://localhost:8080
- **Outline:** http://localhost:3000
- PostgreSQL, Redis

### Passo 3: Configurar Cliente MCP

**Cursor - Edite `mcp.json`:**

```json
{
  "mcpServers": {
    "outline-remote": {
      "url": "http://localhost:8080/sse"
    }
  }
}
```

**Claude Desktop - Edite `claude_desktop_config.json` (mesmo formato acima)**

### Passo 4: Reiniciar e Testar

1. Reinicie o Cursor/Claude Desktop
2. Use Agent Mode
3. Digite: "Liste as collections do Outline"

**Pronto!** Servidor SSE está rodando.

**Swagger UI:** http://localhost:8080/swagger

**Detalhes:** [docs/setup/sse-remote.md](./docs/setup/sse-remote.md)

---

## Cenário 3: Desenvolvimento (.NET SDK)

### Passo 1: Clone e Build

```bash
# Clone o repositório
git clone <repository-url>
cd Outline

# Restaure e compile
dotnet restore
dotnet build
```

**Requisito:** .NET SDK 8.0 ou superior ([Download](https://dotnet.microsoft.com/download))

### Passo 2: Configurar Variáveis

```bash
# Windows (PowerShell)
$env:OUTLINE_BASE_URL="http://localhost:3000"
$env:OUTLINE_API_KEY="your-api-key-here"

# Linux/macOS
export OUTLINE_BASE_URL="http://localhost:3000"
export OUTLINE_API_KEY="your-api-key-here"
```

### Passo 3: Configurar no Cursor

**Edite `mcp.json`:**

```json
{
  "mcpServers": {
    "outline": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\Users\\seu-usuario\\repos\\Outline\\src\\Outline.Mcp.Server\\Outline.Mcp.Server.csproj",
        "--no-build"
      ],
      "env": {
        "OUTLINE_API_KEY": "your-api-key",
        "OUTLINE_BASE_URL": "http://localhost:3000"
      }
    }
  }
}
```

**Importante:** Use caminho **absoluto** para o `.csproj`.

### Passo 4: Reiniciar e Testar

1. Compile antes: `dotnet build src/Outline.Mcp.Server`
2. Reinicie o Cursor
3. Use Agent Mode
4. Digite: "Liste as collections do Outline"

**Pronto!** Para hot reload, use `dotnet watch`.

**Detalhes:** [docs/setup/development.md](./docs/setup/development.md)

---

## Próximos Passos

### 1. Executar Bootstrap

Cria documentação inicial e valida instalação:

```bash
# CLI
dotnet run --project src/Outline.Mcp.Client -- bootstrap \
  --collection-id "your-collection-id"

# Via Cursor/Claude Desktop
"Use /quick-start para executar o bootstrap completo"
```

### 2. Testar Ferramentas

```
"Liste as collections disponíveis no Outline"
"Busque documentos sobre API no Outline"
"Crie documentação da classe UserService no Outline"
```

### 3. Explorar Prompts MCP

- `/doc-project` - Documentar projeto completo
- `/doc-feature` - Documentar funcionalidade
- `/get-budget` - Criar proposta comercial
- `/quick-start` - Bootstrap completo

Ver: [docs/usage/mcp-prompts.md](./docs/usage/mcp-prompts.md)

---

## Documentação Completa

- **Setup Detalhado:**
  - [Docker Local](./docs/setup/docker.md)
  - [Executável](./docs/setup/executable.md)
  - [Desenvolvimento](./docs/setup/development.md)
  - [SSE Remoto](./docs/setup/sse-remote.md)

- **Guias de Uso:**
  - [CLI](./docs/usage/cli.md)
  - [MCP Tools](./docs/usage/mcp-tools.md)
  - [MCP Prompts](./docs/usage/mcp-prompts.md)
  - [Revisões](./docs/usage/revisions.md)

- **Referência:**
  - [Arquitetura](./docs/architecture.md)
  - [Troubleshooting](./docs/troubleshooting.md)

---

## Precisa de Ajuda?

### Outline não está rodando

```bash
# Execute Outline com Docker
docker-compose up -d

# Aguarde ~2 minutos
docker-compose logs -f outline

# Acesse: http://localhost:3000
```

### Erro "API Key not configured"

1. Acesse http://localhost:3000
2. Settings → API & Apps → New API Key
3. Configure variável: `export OUTLINE_API_KEY="ol_api_..."`

### Tools não aparecem no Cursor

1. Verifique `mcp.json` (sintaxe válida, caminho correto)
2. Use caminho **absoluto**
3. Compile antes (para .csproj): `dotnet build src/Outline.Mcp.Server`
4. Reinicie o Cursor **completamente**

**Mais problemas:** [docs/troubleshooting.md](./docs/troubleshooting.md)
