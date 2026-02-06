# Docker Setup - Outline MCP Integration

Guia para instalar o Outline usando Docker Compose.

## Pré-requisitos

- Docker (versão recente)
- Docker Compose
- Portas disponíveis: 3000, 5432, 6379

### Verificar Instalação

```bash
docker --version
docker-compose --version
```

Se não estiver instalado, baixe em: https://docs.docker.com/get-docker/

## Instalação do Outline

### 1. Clone o Repositório

```bash
git clone <repository-url>
cd Outline
```

### 2. Configure as Variáveis de Ambiente

```bash
# Copie o arquivo de exemplo
cp .env.example .env

# Windows (PowerShell)
Copy-Item .env.example .env

# Edite o .env e preencha com seus valores reais
# Especialmente: SECRET_KEY, UTILS_SECRET, AZURE_CLIENT_ID, OUTLINE_API_KEY
```

**Importante:** O arquivo `.env` contém secrets e não deve ser commitado no git.

### 3. Inicie os Serviços

```bash
docker-compose up -d
```

**Aguarde 1-2 minutos para inicialização completa.**

### 4. Acompanhe os Logs

```bash
# Ver logs do Outline
docker-compose logs -f outline

# Ver status dos serviços
docker-compose ps
```

Todos os serviços devem estar "Up" e "healthy".

### 5. Acesse o Outline

Abra no navegador: **http://localhost:3000**

Na primeira vez, crie uma conta administradora.

## Portas Utilizadas

| Serviço | Porta | Descrição |
|---------|-------|-----------|
| Outline | 3000 | Interface web |
| PostgreSQL | 5432 | Banco de dados |
| Redis | 6379 | Cache |

## Obter API Key

1. Acesse: http://localhost:3000
2. Faça login
3. Navegue para: **Settings → API & Apps**
4. Clique em **New API Key**
5. Defina um nome (ex: "MCP Server Development")
6. Copie a key gerada (começa com `ol_api_`)

**Importante:** A key é exibida apenas uma vez. Salve em local seguro.

## Configurar Variáveis de Ambiente

### Windows (PowerShell)

```powershell
# Sessão atual
$env:OUTLINE_BASE_URL="http://localhost:3000"
$env:OUTLINE_API_KEY="ol_api_1234567890abcdef"

# Permanente (usuário)
[System.Environment]::SetEnvironmentVariable('OUTLINE_BASE_URL', 'http://localhost:3000', 'User')
[System.Environment]::SetEnvironmentVariable('OUTLINE_API_KEY', 'ol_api_1234567890abcdef', 'User')
```

### Linux/macOS

```bash
# Adicione ao ~/.bashrc ou ~/.zshrc
export OUTLINE_BASE_URL="http://localhost:3000"
export OUTLINE_API_KEY="ol_api_1234567890abcdef"

# Recarregue
source ~/.bashrc  # ou source ~/.zshrc
```

## Verificação

Teste a conexão:

```bash
curl http://localhost:3000/api/auth.config
```

Deve retornar JSON com configuração de autenticação.

## Gerenciar Serviços

```bash
# Parar serviços
docker-compose down

# Parar e remover volumes
docker-compose down -v

# Reiniciar serviço específico
docker-compose restart outline

# Ver logs
docker-compose logs -f
```

## Troubleshooting

### Porta 3000 já em uso

Edite `docker-compose.yml` e altere a porta:

```yaml
services:
  outline:
    ports:
      - "3001:3000"  # Altere 3001 para porta desejada
```

### Serviço não inicia

```bash
# Verifique logs
docker-compose logs outline

# Verifique se portas estão livres
# Windows
netstat -ano | findstr "3000"

# Linux/macOS
lsof -i :3000
```

### Resetar completamente

```bash
docker-compose down -v
docker-compose up -d
```

Isso remove volumes e recria tudo do zero.

## Próximos Passos

Escolha como usar o MCP Server:

- [Executável Self-Contained](./executable.md) - Recomendado
- [.NET SDK Development](./development.md) - Para desenvolvedores
- [SSE Remote Mode](./sse-remote.md) - Para equipes

Ou consulte o [guia completo de uso](../usage/cli.md).
