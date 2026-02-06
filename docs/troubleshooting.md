# Troubleshooting Guide

Guia completo para resolução de problemas comuns.

## Problemas de Configuração

### Erro: "OUTLINE_API_KEY not configured"

**Causa:** Variável de ambiente não definida ou não visível.

**Verificação:**

```bash
# Windows (PowerShell)
$env:OUTLINE_API_KEY

# Windows (CMD)
echo %OUTLINE_API_KEY%

# Linux/macOS
echo $OUTLINE_API_KEY
```

**Solução:**

```bash
# Windows (PowerShell - sessão atual)
$env:OUTLINE_BASE_URL="http://localhost:3000"
$env:OUTLINE_API_KEY="ol_api_1234567890abcdef"

# Windows (PowerShell - permanente)
[System.Environment]::SetEnvironmentVariable('OUTLINE_BASE_URL', 'http://localhost:3000', 'User')
[System.Environment]::SetEnvironmentVariable('OUTLINE_API_KEY', 'ol_api_1234567890abcdef', 'User')

# Linux/macOS (adicione ao ~/.bashrc ou ~/.zshrc)
export OUTLINE_BASE_URL="http://localhost:3000"
export OUTLINE_API_KEY="ol_api_1234567890abcdef"
source ~/.bashrc
```

### Erro: "OUTLINE_BASE_URL environment variable is required"

**Causa:** Variável não definida ou mal formatada.

**Verificação:**

```bash
echo $OUTLINE_BASE_URL  # Deve retornar URL válida
```

**Solução:** Defina URL correta:

```bash
# Docker local
export OUTLINE_BASE_URL="http://localhost:3000"

# Cloud
export OUTLINE_BASE_URL="https://your-team.getoutline.com"
```

**Importante:** URLs cloud devem usar HTTPS.

## Problemas de Autenticação

### Erro: "Connection failed: 401 Unauthenticated"

**Causa:** API Key inválida, expirada ou incorreta.

**Verificação:**

```bash
# Teste a API key diretamente
curl -H "Authorization: Bearer ol_api_your_key" \
  http://localhost:3000/api/auth.config
```

**Solução:**

1. Gere nova API Key no Outline:
   - Settings → API & Apps → New API Key

2. Atualize a variável de ambiente:

```bash
export OUTLINE_API_KEY="nova-api-key-aqui"
```

3. Reinicie o MCP Server ou terminal

### Erro: "Connection failed: 403 Forbidden"

**Causa:** API Key válida mas sem permissões necessárias.

**Solução:**

1. Verifique permissões da API Key no Outline
2. Regenere a key com permissões adequadas
3. Use conta com privilégios de admin se necessário

## Problemas de Conexão

### Erro: "Connection failed: 404"

**Causa:** URL base incorreta ou Outline não está rodando.

**Verificação:**

```bash
# Teste se Outline está acessível
curl http://localhost:3000

# Para Docker
docker-compose ps  # Todos devem estar "Up"
```

**Solução:**

1. Confirme que Outline está rodando:

```bash
docker-compose up -d
docker-compose logs -f outline
```

2. Verifique a URL:

```bash
# Local deve ser http://
export OUTLINE_BASE_URL="http://localhost:3000"

# Cloud deve ser https://
export OUTLINE_BASE_URL="https://your-team.getoutline.com"
```

### Erro: "Connection timed out"

**Causa:** Firewall, proxy ou rede lenta.

**Diagnóstico:**

```bash
# Teste latência
curl -w "@curl-format.txt" -o /dev/null -s http://localhost:3000/api/collections.list
```

**Solução:**

1. Verifique firewall local
2. Se usando proxy, configure variáveis:

```bash
export HTTP_PROXY="http://proxy:port"
export HTTPS_PROXY="http://proxy:port"
```

3. Aumente timeout no código (se necessário)

## Problemas com Docker

### Porta 3000 já em uso

**Causa:** Outra aplicação usando a porta.

**Verificação:**

```bash
# Windows
netstat -ano | findstr "3000"

# Linux/macOS
lsof -i :3000
```

**Solução:**

Opção 1 - Pare o processo conflitante:

```bash
# Windows
taskkill /PID <pid> /F

# Linux/macOS
kill -9 <pid>
```

Opção 2 - Altere porta no `docker-compose.yml`:

```yaml
services:
  outline:
    ports:
      - "3001:3000"  # Mude 3001 para porta desejada
```

### Serviço Docker não inicia

**Causa:** Erro na configuração ou recurso insuficiente.

**Diagnóstico:**

```bash
# Ver logs
docker-compose logs outline

# Ver eventos
docker-compose events
```

**Solução:**

1. Verifique recursos disponíveis (RAM, disco)
2. Limpe containers antigos:

```bash
docker-compose down -v
docker system prune -a
```

3. Reinicie Docker daemon

### Outline "unhealthy" no Docker

**Causa:** PostgreSQL ou Redis não inicializou completamente.

**Verificação:**

```bash
docker-compose ps  # Veja status de todos serviços
docker-compose logs postgres
docker-compose logs redis
```

**Solução:**

```bash
# Aguarde mais tempo (1-2 minutos)
docker-compose logs -f outline

# Se persistir, reinicie
docker-compose restart outline
```

## Problemas com MCP Integration

### ListCollections retorna lista vazia mesmo com collections criadas

**Sintoma:** A ferramenta `list_collections` retorna uma lista vazia `[]` mesmo havendo collections criadas no Outline.

**Causa Raiz:** A API do Outline retorna collections no campo `data`, não no campo `collections`. O formato da resposta é:

```json
{
  "pagination": {...},
  "data": [...],  // Collections estão aqui
  "policies": [...],
  "status": 200,
  "ok": true
}
```

**Solução:** O modelo `CollectionsData` foi atualizado para suportar ambos os campos `data` e `collections` para compatibilidade:

```csharp
public class CollectionsData
{
    [JsonPropertyName("data")]
    public List<OutlineCollection>? Data { get; set; }
    
    [JsonPropertyName("collections")]
    public List<OutlineCollection>? Collections { get; set; }
    
    public List<OutlineCollection> GetCollections()
    {
        return Data ?? Collections ?? new List<OutlineCollection>();
    }
}
```

**Passos para Aplicar a Correção:**

1. Atualize o código com a correção
2. Reconstrua o container Docker:

```bash
docker-compose up -d --build outline-mcp-server
```

3. Se usando modo local, recompile:

```bash
dotnet build
```

4. Reinicie o Cursor ou aguarde a reconexão do servidor MCP

**Verificação:** Execute `list_collections` e verifique se as collections aparecem.

**Teste Manual da API:**

Para confirmar o formato da resposta da API:

```powershell
# Windows (PowerShell)
$headers = @{
    "Authorization" = "Bearer $env:OUTLINE_API_KEY"
    "Content-Type" = "application/json"
}
Invoke-RestMethod -Uri "$env:OUTLINE_BASE_URL/api/collections.list" `
    -Method Post -Headers $headers -Body "{}" | ConvertTo-Json -Depth 10
```

```bash
# Linux/macOS
curl -X POST "$OUTLINE_BASE_URL/api/collections.list" \
  -H "Authorization: Bearer $OUTLINE_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{}' | jq .
```

### Tools não aparecem no Cursor

**Causa:** MCP Server não foi carregado corretamente.

**Verificação:**

1. Verifique `mcp.json` (sintaxe válida, caminho correto)
2. Use Help → Toggle Developer Tools → Console no Cursor
3. Busque por erros relacionados a MCP

**Solução:**

1. Verifique sintaxe do JSON:

```bash
# Valide JSON
cat mcp.json | jq .
```

2. Use caminho **absoluto**:

```json
{
  "mcpServers": {
    "outline": {
      "command": "C:\\Users\\usuario\\repos\\Outline\\publish\\win-x64\\Outline.Mcp.Server.exe",
      "args": []
    }
  }
}
```

3. Para .csproj, compile antes:

```bash
dotnet build src/Outline.Mcp.Server
```

4. Reinicie o Cursor **completamente**

### Erro: "Failed to parse message"

**Causa:** Logs sendo escritos para stdout.

**Solução:**

O projeto já está configurado para logs em stderr. Se persistir:

1. Confirme que não há `Console.WriteLine()` no código
2. Verifique configuração de logging em `Program.cs`
3. Recompile:

```bash
dotnet build src/Outline.Mcp.Server
```

### Erro: "Permission denied" ao executar tool

**Causa:** Cursor/Claude solicitando aprovação do usuário.

**Solução:** Aprove a execução na interface do Cursor/Claude.

**Para auto-aprovar (não recomendado):** Configure no settings do Cursor.

### Cursor não reconhece executável

**Causa:** Caminho incorreto ou arquivo não existe.

**Verificação:**

```bash
# Windows (PowerShell)
Test-Path "C:\path\to\Outline.Mcp.Server.exe"

# Linux/macOS
ls -la /path/to/Outline.Mcp.Server
```

**Solução:**

1. Use caminho absoluto no `mcp.json`
2. Confirme que executável foi gerado:

```bash
# Windows
.\scripts\publish-win-x64.ps1

# Linux
./scripts/publish-linux-x64.sh
```

3. Use barras duplas (`\\`) no Windows

## Problemas de Compilação

### Erro: "MSB3027" ou "MSB3021" (DLL locked)

**Causa:** Processo ainda rodando ou arquivo bloqueado.

**Solução:**

```bash
# Windows (PowerShell)
Get-Process | Where-Object { $_.ProcessName -like "*Outline*" } | Stop-Process -Force

# Linux/macOS
pkill -9 Outline
```

Aguarde alguns segundos e tente novamente:

```bash
dotnet clean
dotnet build
```

### Erro: "Project file does not exist"

**Causa:** Caminho incorreto.

**Verificação:**

```bash
# Windows
Get-Item "C:\path\to\project.csproj"

# Linux/macOS
ls "/path/to/project.csproj"
```

**Solução:** Corrija o caminho no `mcp.json` ou comando.

### Native AOT build failed - "Platform linker not found"

**Causa:** C++ build tools não instalados.

**Solução:**

**Windows:**
- Instale Visual Studio
- Workload: "Desktop Development with C++"

**Linux:**

```bash
sudo apt install clang zlib1g-dev
```

**macOS:**

```bash
xcode-select --install
```

Se continuar falhando, use self-contained normal:

```bash
# Windows
.\scripts\publish-win-x64.ps1

# Linux
./scripts/publish-linux-x64.sh
```

## Problemas com CLI

### Erro: "Collection not found"

**Causa:** Collection ID inválido ou sem permissão.

**Solução:**

1. Liste collections disponíveis:

```bash
dotnet run --project src/Outline.Mcp.Client -- list-collections
```

2. Use ID válido da lista retornada

### Erro: "Document not found"

**Causa:** Document ID inválido.

**Solução:**

1. Busque o documento:

```bash
dotnet run --project src/Outline.Mcp.Client -- search --query "título"
```

2. Use ID correto da busca

### CLI retorna erro mas documento foi criado

**Causa:** Timeout ou falha na deserialização da resposta.

**Verificação:**

```bash
# Busque o documento criado
dotnet run --project src/Outline.Mcp.Client -- search --query "título-do-documento"
```

**Solução:** Documento foi criado com sucesso. Ignore o erro do CLI.

### Caracteres especiais quebram comando

**Causa:** Shell interpretando caracteres especiais.

**Solução:** Use heredoc:

```bash
dotnet run --project src/Outline.Mcp.Client -- create \
  --title "Document" \
  --text "$(cat <<'EOF'
# Content
Text with "quotes" and 'apostrophes'
EOF
)" \
  --collection-id "abc-123"
```

## Problemas de Performance

### Resposta muito lenta

**Causas possíveis:**
- Documento muito grande
- Rede lenta
- Instância Outline sobrecarregada
- Rate limiting

**Diagnóstico:**

```bash
# Teste latência
curl -w "\nTotal time: %{time_total}s\n" \
  -o /dev/null \
  -s http://localhost:3000/api/collections.list
```

**Solução:**

1. Divida documentos grandes (< 500KB recomendado)
2. Use limit em buscas:

```bash
dotnet run --project src/Outline.Mcp.Client -- search \
  --query "API" \
  --limit 10
```

3. Verifique recursos do servidor Outline

### Rate limit atingido

**Erro:** HTTP 429 Too Many Requests

**Causa:** Excedeu 1000 requests/minuto.

**Solução:**

O cliente já implementa retry automático. Para operações em lote:

```bash
# Adicione delay entre comandos
for doc in docs/*.md; do
  dotnet run --project src/Outline.Mcp.Client -- create \
    --text "$(cat $doc)" \
    --collection-id "abc-123"
  sleep 1  # 1 segundo de delay
done
```

## Problemas com Bootstrap

### Bootstrap falha em "Validando ambiente"

**Causa:** Variáveis de ambiente não definidas.

**Solução:** Configure antes de executar:

```bash
export OUTLINE_BASE_URL="http://localhost:3000"
export OUTLINE_API_KEY="ol_api_your_key"

dotnet run --project src/Outline.Mcp.Client -- bootstrap \
  --collection-id "your-id"
```

### Bootstrap cria documentação genérica

**Causa:** Arquivos `docs/` ou `skills/` não encontrados.

**Solução:**

1. Verifique se arquivos existem:

```bash
ls docs/
ls skills/
```

2. Para Docker SSE, confirme Dockerfile copia as pastas:

```dockerfile
COPY ["docs/", "/app/publish/docs/"]
COPY ["skills/", "/app/publish/skills/"]
```

3. Para executável, republique:

```bash
.\scripts\publish-win-x64.ps1
```

### Bootstrap não encontra templates

**Causa:** Templates não foram copiados para executável.

**Solução:**

Confirme que `.csproj` inclui skills:

```xml
<ItemGroup>
  <Content Include="..\..\skills\**\*.md" LinkBase="skills">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

## Problemas com Prompts MCP

### Prompt cria conteúdo genérico

**Causa:** Guardrails falharam ou arquivos não encontrados.

**Solução:**

1. Confirme que `docs/` e `skills/` existem
2. Para Docker, rebuild sem cache:

```bash
docker build --no-cache -f src/Outline.Mcp.Server/Dockerfile .
```

3. Para executável, republique
4. Reexecute prompt com mais contexto

### Template não encontrado

**Causa:** Arquivo não existe em `skills/` ou collection.

**Solução:**

```bash
# Verifique arquivos locais
ls skills/

# Execute bootstrap para criar collection
dotnet run --project src/Outline.Mcp.Client -- bootstrap \
  --collection-id "your-collection-id"
```

## Problemas com Revisões

### Revisão não aparece

**Causa:** Edição muito recente (< 5 min).

**Solução:** Aguarde alguns minutos e liste novamente:

```bash
dotnet run --project src/Outline.Mcp.Client -- list-revisions \
  --document-id "doc-id"
```

### Erro ao restaurar revisão

**Causa:** Revisão não existe ou foi deletada.

**Solução:**

1. Liste revisões disponíveis:

```bash
dotnet run --project src/Outline.Mcp.Client -- list-revisions \
  --document-id "doc-id"
```

2. Use ID válido da lista

## Logs Detalhados

Para diagnóstico avançado, habilite logs detalhados:

### MCP Server

```bash
# Windows (PowerShell)
$env:DOTNET_LOGGING__LOGLEVEL__DEFAULT="Debug"
$env:DOTNET_LOGGING__LOGLEVEL__MODELCONTEXTPROTOCOL="Trace"
dotnet run --project src/Outline.Mcp.Server

# Linux/macOS
export DOTNET_LOGGING__LOGLEVEL__DEFAULT=Debug
export DOTNET_LOGGING__LOGLEVEL__MODELCONTEXTPROTOCOL=Trace
dotnet run --project src/Outline.Mcp.Server
```

### Cursor

1. Help → Toggle Developer Tools
2. Console tab
3. Busque por "mcp" ou "outline"

### Docker

```bash
# Logs em tempo real
docker-compose logs -f

# Logs de serviço específico
docker-compose logs -f outline
docker-compose logs -f outline-mcp-server

# Últimas 100 linhas
docker-compose logs --tail=100
```

## Obter Ajuda

Se o problema persistir:

1. Verifique [issues no GitHub](https://github.com/seu-repo/issues)
2. Consulte [documentação do MCP](https://modelcontextprotocol.io/)
3. Revise [Outline API docs](https://www.getoutline.com/developers)
4. Abra um novo issue com:
   - Descrição do problema
   - Passos para reproduzir
   - Logs relevantes
   - Versão do .NET, Docker, sistema operacional

## Próximos Passos

- [Architecture](./architecture.md) - Entenda a estrutura técnica
- [Setup Guides](./setup/) - Guias de instalação
- [Usage Guides](./usage/) - Guias de uso
