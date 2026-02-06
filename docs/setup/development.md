# Development Setup - .NET SDK

Guia para desenvolvimento do MCP Server usando .NET SDK.

## Pré-requisitos

- **.NET SDK 8.0** ou superior
- **Git**
- **Outline rodando** (local ou cloud)

### Verificar Instalação

```bash
dotnet --version  # Deve retornar >= 8.0
git --version
```

Se o .NET SDK não estiver instalado: https://dotnet.microsoft.com/download

## Instalação

### 1. Clone o Repositório

```bash
git clone <repository-url>
cd Outline
```

### 2. Restaure Dependências

```bash
dotnet restore
```

### 3. Compile o Projeto

```bash
dotnet build
```

**Saída esperada:**

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 4. Configure Variáveis de Ambiente

#### Windows (PowerShell)

```powershell
# Sessão atual
$env:OUTLINE_BASE_URL="http://localhost:3000"
$env:OUTLINE_API_KEY="ol_api_1234567890abcdef"

# Permanente (usuário)
[System.Environment]::SetEnvironmentVariable('OUTLINE_BASE_URL', 'http://localhost:3000', 'User')
[System.Environment]::SetEnvironmentVariable('OUTLINE_API_KEY', 'ol_api_1234567890abcdef', 'User')
```

#### Linux/macOS

```bash
# Adicione ao ~/.bashrc ou ~/.zshrc
export OUTLINE_BASE_URL="http://localhost:3000"
export OUTLINE_API_KEY="ol_api_1234567890abcdef"

# Recarregue
source ~/.bashrc
```

### 5. Execute o MCP Server

```bash
dotnet run --project src/Outline.Mcp.Server
```

Se configurado corretamente, o servidor inicia. Pressione `Ctrl+C` para parar.

## Configuração no Cursor/Claude Desktop

### 1. Localize o Arquivo mcp.json

**Cursor:**
- Windows: `%APPDATA%\Cursor\User\globalStorage\mcp.json`
- macOS: `~/Library/Application Support/Cursor/User/globalStorage/mcp.json`
- Linux: `~/.config/Cursor/User/globalStorage/mcp.json`

**Claude Desktop:**
- Windows: `%APPDATA%\Claude\claude_desktop_config.json`
- macOS: `~/Library/Application Support/Claude/claude_desktop_config.json`

### 2. Configure o MCP Server

Edite o arquivo e adicione:

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
        "OUTLINE_API_KEY": "ol_api_1234567890abcdef",
        "OUTLINE_BASE_URL": "http://localhost:3000"
      }
    }
  }
}
```

**Importante:**
- Use caminho **absoluto** para o `.csproj`
- Substitua `seu-usuario` pelo seu usuário
- Flag `--no-build` evita recompilação a cada execução (mais rápido)

**macOS/Linux:** Use barras normais `/` no caminho.

### 3. Compile Antes de Usar

```bash
dotnet build src/Outline.Mcp.Server
```

### 4. Reinicie o Cursor/Claude Desktop

Feche e reabra completamente o editor.

## Verificação

### No Cursor

1. Abra um projeto
2. Use o Agent Mode
3. Digite: "Liste as collections disponíveis no Outline"

O Cursor deve solicitar permissão para executar `list_collections`.

## Estrutura do Projeto

```
Outline.Mcp.sln
├── src/
│   ├── Outline.Mcp.Server/          # MCP Server (stdio/SSE)
│   │   ├── Program.cs               # Entry point
│   │   ├── Tools/                   # MCP tools
│   │   └── Prompts/                 # MCP prompts
│   ├── Outline.Mcp.Client/          # CLI de teste
│   └── Outline.Mcp.Shared/          # Código compartilhado
│       ├── Models/                  # DTOs do Outline
│       ├── Api/                     # OutlineApiClient
│       └── Mermaid/                 # Builders de diagramas
├── tests/
│   └── Outline.Mcp.Tests/           # Testes unitários e integração
├── docs/                            # Documentação
└── skills/                          # Templates de prompts
```

## Desenvolvimento

### Executar Testes

```bash
# Todos os testes
dotnet test

# Apenas unitários
dotnet test --filter "FullyQualifiedName~Unit"

# Apenas integração (requer Outline rodando)
dotnet test --filter "FullyQualifiedName~Integration"
```

### Hot Reload (Watch Mode)

```bash
# Recompila automaticamente ao detectar mudanças
dotnet watch --project src/Outline.Mcp.Server
```

### Debug no VS Code

Crie `.vscode/launch.json`:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Debug MCP Server",
      "type": "coreclr",
      "request": "launch",
      "program": "${workspaceFolder}/src/Outline.Mcp.Server/bin/Debug/net8.0/Outline.Mcp.Server.dll",
      "args": [],
      "cwd": "${workspaceFolder}",
      "env": {
        "OUTLINE_API_KEY": "ol_api_1234567890abcdef",
        "OUTLINE_BASE_URL": "http://localhost:3000"
      },
      "preLaunchTask": "build"
    }
  ]
}
```

### Adicionar Nova Tool MCP

1. Edite `src/Outline.Mcp.Server/Tools/OutlineTools.cs`
2. Adicione método com atributo `[Tool]`
3. Implemente a lógica usando `OutlineApiClient`
4. Recompile e teste

**Exemplo:**

```csharp
[Tool(Name = "my_new_tool", Description = "Descrição da ferramenta")]
public async Task<string> MyNewTool(
    [Parameter(Description = "Parâmetro")] string param)
{
    var result = await _client.MinhaOperacaoAsync(param);
    return JsonSerializer.Serialize(result);
}
```

## Troubleshooting

### Erro: "Project file does not exist"

**Causa:** Caminho incorreto no `mcp.json`.

**Solução:** Use caminho absoluto e válido:

```bash
# Windows (PowerShell)
Get-Item "C:\Users\seu-usuario\repos\Outline\src\Outline.Mcp.Server\Outline.Mcp.Server.csproj"

# Linux/macOS
ls "/Users/seu-usuario/repos/Outline/src/Outline.Mcp.Server/Outline.Mcp.Server.csproj"
```

### Erro: "Failed to parse message"

**Causa:** Logs sendo escritos para stdout.

**Solução:** O projeto já está configurado para logs em stderr. Se persistir:

1. Verifique se não há `Console.WriteLine()` no código
2. Confirme logging em `Program.cs`
3. Recompile: `dotnet build src/Outline.Mcp.Server`

### Tools não aparecem no Cursor

1. Verifique sintaxe do `mcp.json` (JSON válido)
2. Use caminho absoluto para `.csproj`
3. Compile antes: `dotnet build src/Outline.Mcp.Server`
4. Reinicie o Cursor completamente
5. Verifique logs do Cursor para erros MCP

### Build lento no Cursor

Use flag `--no-build` no `mcp.json`:

```json
"args": [
  "run",
  "--project",
  "path/to/project.csproj",
  "--no-build"
]
```

E compile manualmente quando fizer mudanças:

```bash
dotnet build src/Outline.Mcp.Server
```

## Logs Detalhados

Para debug avançado:

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

## Próximos Passos

- [Guia de Uso CLI](../usage/cli.md)
- [Criar Novas Tools MCP](../usage/mcp-tools.md)
- [Entender Arquitetura](../architecture.md)
