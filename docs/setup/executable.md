# Self-Contained Executable Setup

Guia para usar o MCP Server como executável independente.  
**Não requer .NET SDK instalado na máquina do usuário.**

## Opções de Executável

### 1. Self-Contained (Recomendado)

Executável com todas as dependências incluídas.

**Tamanho:** ~70-100MB  
**Vantagens:**
- ✅ Funcionamento garantido
- ✅ Não requer .NET SDK
- ✅ Distribuição simples

### 2. Native AOT (Avançado)

Executável compilado para código nativo.

**Tamanho:** ~15-30MB (3x menor)  
**Vantagens adicionais:**
- ⚡ Inicialização instantânea (10-100x mais rápido)
- 💾 Menor uso de memória (~50% menos)
- 🔒 Mais difícil de fazer engenharia reversa

**Requisitos:**
- **Windows**: Visual Studio com "Desktop Development with C++"
- **Linux**: `clang` e `zlib1g-dev`
- **macOS**: Xcode Command Line Tools

## Publicação do Executável

### Windows

```powershell
# Self-Contained
.\scripts\publish-win-x64.ps1

# Native AOT (requer C++ build tools)
.\scripts\publish-win-x64-aot.ps1
```

**Saída:** `publish/win-x64/Outline.Mcp.Server.exe`

### Linux

```bash
# Self-Contained
./scripts/publish-linux-x64.sh

# Native AOT (requer clang e zlib1g-dev)
./scripts/publish-linux-x64-aot.sh
```

**Saída:** `publish/linux-x64/Outline.Mcp.Server`

### macOS (Apple Silicon)

```bash
# Self-Contained
./scripts/publish-osx-arm64.sh

# Native AOT (requer Xcode Command Line Tools)
./scripts/publish-osx-arm64-aot.sh
```

**Saída:** `publish/osx-arm64/Outline.Mcp.Server`

## Configuração no Cursor/Claude Desktop

### 1. Localize a Pasta de Configuração

**Cursor:**
- Windows: `%APPDATA%\Cursor\User\globalStorage\`
- macOS: `~/Library/Application Support/Cursor/User/globalStorage/`
- Linux: `~/.config/Cursor/User/globalStorage/`

**Claude Desktop:**
- Windows: `%APPDATA%\Claude\`
- macOS: `~/Library/Application Support/Claude/`

### 2. Edite mcp.json

**Windows (Cursor):**

Crie ou edite `%APPDATA%\Cursor\User\globalStorage\mcp.json`:

```json
{
  "mcpServers": {
    "outline": {
      "command": "C:\\Users\\seu-usuario\\repos\\Outline\\publish\\win-x64\\Outline.Mcp.Server.exe",
      "args": [],
      "env": {
        "OUTLINE_API_KEY": "ol_api_1234567890abcdef",
        "OUTLINE_BASE_URL": "http://localhost:3000"
      }
    }
  }
}
```

**macOS/Linux (Cursor):**

```json
{
  "mcpServers": {
    "outline": {
      "command": "/Users/seu-usuario/repos/Outline/publish/osx-arm64/Outline.Mcp.Server",
      "args": [],
      "env": {
        "OUTLINE_API_KEY": "ol_api_1234567890abcdef",
        "OUTLINE_BASE_URL": "http://localhost:3000"
      }
    }
  }
}
```

**Claude Desktop:**

Edite `claude_desktop_config.json` (mesmo formato acima).

**Importante:**
- Use caminho **absoluto** para o executável
- Substitua `seu-usuario` pelo seu usuário
- Use barras duplas (`\\`) no Windows

### 3. Copie o mcp.json.example

O script de publicação cria automaticamente um `mcp.json.example` na pasta `publish/`:

```bash
# Windows
notepad .\publish\win-x64\mcp.json.example

# Linux/macOS
cat ./publish/osx-arm64/mcp.json.example
```

Use esse exemplo como referência.

## Verificação

### 1. Teste o Executável

```bash
# Windows
$env:OUTLINE_API_KEY="your-key"
$env:OUTLINE_BASE_URL="http://localhost:3000"
.\publish\win-x64\Outline.Mcp.Server.exe

# Linux/macOS
export OUTLINE_API_KEY="your-key"
export OUTLINE_BASE_URL="http://localhost:3000"
./publish/osx-arm64/Outline.Mcp.Server
```

Se configurado corretamente, o servidor deve iniciar. Pressione `Ctrl+C` para parar.

### 2. Teste no Cursor

1. Reinicie o Cursor completamente
2. Abra um projeto
3. Use o Agent Mode
4. Digite: "Liste as collections disponíveis no Outline"

O Cursor deve solicitar permissão para executar a tool `list_collections`.

## Distribuição

Para distribuir o executável:

1. Copie a pasta `publish/<platform>/`
2. Skills estão **embedded** no executável - sem dependências de arquivos externos
3. Envie o `mcp.json.example` junto para facilitar configuração

**Estrutura completa:**

```
publish/win-x64/
├── Outline.Mcp.Server.exe    # Executável com skills embedded
└── mcp.json.example           # Template de configuração
```

**Nota:** Skills (templates de documentação) estão embedded no executável como recursos .NET. Não há dependências de pastas `docs/` ou `skills/` externas.

## Troubleshooting

### Erro: "OUTLINE_BASE_URL environment variable is required"

**Causa:** Variáveis de ambiente não definidas.

**Solução:** Defina as variáveis antes de executar:

```bash
# Windows
$env:OUTLINE_API_KEY="your-key"
$env:OUTLINE_BASE_URL="http://localhost:3000"

# Linux/macOS
export OUTLINE_API_KEY="your-key"
export OUTLINE_BASE_URL="http://localhost:3000"
```

### Erro ao publicar Native AOT

**Windows:** Instale "Desktop Development with C++" no Visual Studio Installer

**Linux:**
```bash
sudo apt install clang zlib1g-dev
```

**macOS:**
```bash
xcode-select --install
```

Se o Native AOT continuar falhando, use Self-Contained normal.

### Executável não encontrado no Cursor

1. Verifique se o caminho em `mcp.json` está **absoluto** e correto
2. Confirme que o arquivo existe: `Test-Path C:\path\to\exe` (Windows)
3. Use barras duplas (`\\`) no Windows
4. Reinicie o Cursor após alterar `mcp.json`

## Próximos Passos

- [Guia de Uso CLI](../usage/cli.md)
- [Ferramentas MCP](../usage/mcp-tools.md)
- [Prompts MCP](../usage/mcp-prompts.md)
