# SSE Remote Setup - MCP Server via HTTP

Guia para executar o MCP Server em modo SSE (Server-Sent Events) para acesso remoto.

## O que é SSE Mode?

O MCP Server pode ser executado em dois modos:

**Modo Local (stdio):**
- Comunicação via stdin/stdout
- Um servidor por usuário
- Ideal para desenvolvimento pessoal

**Modo Remoto (SSE):**
- Comunicação via HTTP/SSE
- Múltiplos clientes conectados
- Ideal para equipes e produção

## Quando Usar SSE

Use SSE quando você precisa:
- ✅ Servidor centralizado para equipe
- ✅ Múltiplos clientes conectando ao mesmo servidor
- ✅ Deployment em container Docker
- ✅ Autenticação e controle de acesso
- ✅ Logs e métricas centralizados

**Não use SSE se:**
- Você é o único usuário
- Prefere execução local simples
- Não quer gerenciar infraestrutura

Nestes casos, use [executável local](./executable.md) ou [.NET SDK](./development.md).

## Instalação via Docker

### 1. Configure API Key

```bash
# Windows (PowerShell)
$env:OUTLINE_API_KEY="ol_api_1234567890abcdef"

# Linux/macOS
export OUTLINE_API_KEY="ol_api_1234567890abcdef"
```

### 2. Execute o Stack Completo

O `docker-compose.yml` provisiona:
- MCP Server SSE (porta 8080)
- Outline (porta 3000)
- PostgreSQL
- Redis

```bash
docker-compose up -d
```

### 3. Verifique os Logs

```bash
# Logs do MCP Server
docker-compose logs -f outline-mcp-server

# Status dos serviços
docker-compose ps
```

Todos devem estar "Up".

### 4. Teste o Servidor SSE

```bash
# Deve retornar {"status":"ok"}
curl http://localhost:8080/health
```

## Configuração do Cliente MCP

### Cursor

Edite `%APPDATA%\Cursor\User\globalStorage\mcp.json` (Windows) ou equivalente:

```json
{
  "mcpServers": {
    "outline-remote": {
      "description": "Outline MCP Server (Remote/SSE)",
      "url": "http://localhost:8080/sse"
    }
  }
}
```

### Claude Desktop

Edite `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "outline-remote": {
      "description": "Outline MCP Server (Remote/SSE)",
      "url": "http://localhost:8080/sse",
      "headers": {
        "X-API-Key": "optional-auth-token"
      }
    }
  }
}
```

**Nota:** Headers são opcionais, mas recomendados para autenticação customizada.

### Reinicie o Editor

Feche e reabra o Cursor/Claude Desktop completamente.

## Verificação

1. Abra o Cursor
2. Use Agent Mode
3. Digite: "Liste as collections do Outline"

Deve funcionar normalmente via SSE.

## Swagger UI

Acesse a documentação interativa da API:

```
http://localhost:8080/swagger
```

Use para testar endpoints manualmente.

## Gerenciar Serviços

```bash
# Parar todos os serviços
docker-compose down

# Parar e remover volumes
docker-compose down -v

# Reiniciar apenas MCP Server
docker-compose restart outline-mcp-server

# Ver logs em tempo real
docker-compose logs -f

# Ver uso de recursos
docker stats
```

## Configurações Avançadas

### Alterar Porta do MCP Server

Edite `docker-compose.yml`:

```yaml
services:
  outline-mcp-server:
    ports:
      - "8081:8080"  # Altere 8081 para porta desejada
```

### Adicionar Autenticação Customizada

Edite `src/Outline.Mcp.Server/appsettings.json`:

```json
{
  "Authentication": {
    "ApiKey": "your-secret-key"
  }
}
```

Implemente middleware de autenticação em `Program.cs`.

### Variáveis de Ambiente

Disponíveis via Docker Compose:

```bash
# docker-compose.yml
environment:
  - Outline__BaseUrl=http://outline:3000
  - Outline__ApiKey=${OUTLINE_API_KEY}
  - ASPNETCORE_ENVIRONMENT=Production
  - ASPNETCORE_HTTP_PORTS=8080
```

### Múltiplas Instâncias

Para conectar a diferentes instâncias Outline, crie perfis separados:

```json
{
  "mcpServers": {
    "outline-local": {
      "url": "http://localhost:8080/sse"
    },
    "outline-production": {
      "url": "https://mcp.sua-empresa.com/sse",
      "headers": {
        "X-API-Key": "prod-token"
      }
    }
  }
}
```

## Deployment em Produção

### 1. Use HTTPS

Configure um reverse proxy (nginx, Caddy, Traefik):

```nginx
server {
    listen 443 ssl;
    server_name mcp.sua-empresa.com;

    ssl_certificate /path/to/cert.pem;
    ssl_certificate_key /path/to/key.pem;

    location / {
        proxy_pass http://localhost:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        
        # SSE specific
        proxy_set_header Connection '';
        proxy_http_version 1.1;
        chunked_transfer_encoding off;
        proxy_buffering off;
        proxy_cache off;
    }
}
```

### 2. Implemente Autenticação

Adicione tokens de API e valide no servidor.

### 3. Configure Rate Limiting

Use middleware ASP.NET Core:

```csharp
builder.Services.AddRateLimiter(options => {
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

### 4. Monitore Logs

```bash
# Logs estruturados com Serilog
docker-compose logs -f outline-mcp-server | grep "ERROR"

# Métricas (se implementado)
curl http://localhost:8080/metrics
```

## Vantagens vs Modo Local

| Aspecto | Local (stdio) | Remoto (SSE) |
|---------|---------------|--------------|
| Setup | Simples | Requer Docker |
| Usuários | 1 | Múltiplos |
| Autenticação | Não | Sim (customizável) |
| Logs | Local | Centralizados |
| Escalabilidade | Não | Sim |
| Manutenção | Individual | Centralizada |

## Troubleshooting

### Erro: "Connection refused" ao conectar

**Causa:** Servidor não está rodando ou porta incorreta.

**Solução:**

```bash
# Verifique se está rodando
docker-compose ps

# Teste endpoint health
curl http://localhost:8080/health
```

### SSE connection drops

**Causa:** Proxy ou firewall interrompendo conexão.

**Solução:** Configure keep-alive no reverse proxy:

```nginx
proxy_read_timeout 3600s;
proxy_send_timeout 3600s;
```

### Alto uso de memória

**Causa:** Muitas conexões simultâneas ou vazamento de memória.

**Solução:**

```bash
# Reinicie o servidor
docker-compose restart outline-mcp-server

# Limite memória no docker-compose.yml
services:
  outline-mcp-server:
    mem_limit: 512m
```

### Tools não funcionam via SSE

**Causa:** Configuração incorreta do endpoint.

**Verificação:**

1. URL deve terminar com `/sse`
2. Servidor deve estar acessível
3. Reinicie editor após alterar config

## Próximos Passos

- [Guia de Uso CLI](../usage/cli.md)
- [Ferramentas MCP](../usage/mcp-tools.md)
- [Troubleshooting Geral](../troubleshooting.md)
