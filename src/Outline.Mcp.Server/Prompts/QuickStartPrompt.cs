using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Outline.Mcp.Server.Helpers;

namespace Outline.Mcp.Server.Prompts;

public static partial class OutlinePrompts
{
    [McpServerPrompt, Description("Execute bootstrap to create Outline Skills collection with customizable templates")]
    public static GetPromptResult QuickStart()
    {
        // Read embedded skills templates
        var projectTemplate = EmbeddedResourceHelper.ReadEmbeddedResource("project-documentation.md");
        var featureTemplate = EmbeddedResourceHelper.ReadEmbeddedResource("feature-documentation.md");
        var budgetTemplate = EmbeddedResourceHelper.ReadEmbeddedResource("budget-proposal.md");

        var gettingStartedContent = @"# Bem-vindo ao Outline MCP Integration

Parabéns! Você configurou com sucesso a integração entre o **Model Context Protocol (MCP)** e o **Outline**.

Esta integração permite que agentes de IA interajam diretamente com seu Outline Wiki, criando e gerenciando documentação profissional de forma automatizada.

---

## O que foi criado?

### 1. Collection **""Outline Skills""**
Contém 3 templates customizáveis que guiam a IA na criação de documentação:
- **ProjectDocumentation**: Template para documentar arquitetura e estrutura de projetos
- **FeatureDocumentation**: Template para documentar funcionalidades específicas
- **BudgetProposal**: Template para criar propostas técnico-comerciais

**Nota:** Você pode editar estes templates para adequá-los ao padrão da sua organização.

### 2. Collection **""Getting Started""** (esta collection)
Documentação de boas-vindas e guia de uso rápido.

---

## Ferramentas (Tools) Disponíveis

### Gestão de Collections
- `list_collections` - Listar todas as collections
- `create_collection` - Criar nova collection

### Gestão de Documentos
- `create_document` - Criar novo documento
- `get_document` - Obter documento por ID
- `update_document` - Atualizar documento existente
- `delete_document` - Deletar documento
- `search_documents` - Buscar documentos por query

### Revisões (Histórico)
- `list_revisions` - Listar revisões de um documento
- `get_revision` - Obter revisão específica
- `restore_revision` - Restaurar documento para uma revisão anterior

---

## Prompts (Comandos) Disponíveis

### `/doc-project`
Documenta a estrutura e arquitetura completa de um projeto.

**Exemplo de uso:**
```
/doc-project projectName:""MeuApp""
```

**O que faz:**
1. Analisa arquivos do workspace (package.json, .csproj, etc.)
2. Pergunta sobre ícones e estrutura de páginas
3. Cria documentação completa com diagramas Mermaid
4. Usa template de ProjectDocumentation (customizável)

---

### `/doc-feature`
Documenta uma funcionalidade específica com detalhes técnicos.

**Exemplo de uso:**
```
/doc-feature featureName:""Sistema de Login"" collectionId:""abc123""
```

**O que faz:**
1. Busca código relacionado no workspace
2. Analisa implementação real
3. Cria documentação com casos de uso, APIs e testes
4. Usa template de FeatureDocumentation (customizável)

---

### `/get-budget`
Cria proposta técnico-comercial com roadmap e estimativas.

**Exemplo de uso:**
```
/get-budget clientName:""Acme Corp"" projectName:""Portal E-commerce""
```

**O que faz:**
1. Coleta requisitos através de perguntas específicas
2. Analisa complexidade baseada em projetos similares no workspace
3. Gera roadmap com Gantt, estimativas em horas e análise de riscos
4. Usa template de BudgetProposal (customizável)

---

### `/quick-start`
Executa este bootstrap novamente (útil para reinstalar).

---

## Exemplos Práticos

### Exemplo 1: Documentar um Projeto React

1. Execute o prompt:
   ```
   /doc-project projectName:""MyReactApp""
   ```

2. A IA irá:
   - Detectar React no package.json
   - Perguntar sobre ícones e estrutura de páginas
   - Analisar components/, pages/, hooks/
   - Criar doc com arquitetura, componentes principais e fluxos

### Exemplo 2: Documentar Feature de Autenticação

1. Execute:
   ```
   /doc-feature featureName:""JWT Authentication""
   ```

2. A IA irá:
   - Buscar arquivos relacionados (auth.ts, login.tsx, etc.)
   - Ler implementação real
   - Criar doc com diagrama de sequência, APIs e testes

### Exemplo 3: Criar Proposta para Cliente

1. Execute:
   ```
   /get-budget clientName:""Startup XYZ"" projectName:""MVP Mobile App""
   ```

2. A IA irá:
   - Fazer perguntas sobre requisitos
   - Estimar complexidade por feature
   - Gerar roadmap com fases e estimativas em horas

---

## Configuração

### Variáveis de Ambiente
```bash
OUTLINE_BASE_URL=http://localhost:3000
OUTLINE_API_KEY=ol_api_your_key_here
```

### Cenários de Uso

#### Opção 1: Local (via .NET SDK)
```powershell
dotnet run --project src/Outline.Mcp.Server
```

#### Opção 2: Executável Self-Contained
```powershell
.\publish\win-x64\Outline.Mcp.Server.exe
```

#### Opção 3: Remoto (Docker/SSE)
```powershell
docker-compose up -d
```

---

## Customização

### Editando Templates

1. Acesse a collection **""Outline Skills""**
2. Edite os documentos:
   - ProjectDocumentation
   - FeatureDocumentation
   - BudgetProposal
3. Os prompts usarão automaticamente suas versões customizadas!

### Exemplo de Customização
Quer adicionar uma seção ""Segurança"" em todos os projetos?

1. Edite **ProjectDocumentation**
2. Adicione:
   ```markdown
   ### 10. Segurança
   - Autenticação e autorização
   - Proteção de dados sensíveis
   - Conformidade (LGPD, GDPR)
   ```
3. Pronto! Novos projetos incluirão esta seção.

---

## Documentação Adicional

- **Repositório**: [Outline MCP on GitHub](https://github.com/your-repo)
- **Outline API**: [getoutline.com/developers](https://www.getoutline.com/developers)
- **MCP Protocol**: [modelcontextprotocol.io](https://modelcontextprotocol.io)

---

## Próximos Passos

1. Collections criadas
2. Templates instalados
3. **Teste os prompts**: Comece com `/doc-project` no seu workspace
4. Customize os templates conforme necessário
5. Documente seus projetos

---

**Dica**: Use os guardrails dos prompts a seu favor! Eles impedem que a IA crie documentação genérica, forçando-a a analisar o código real do seu projeto.

Boa documentação!";

        var promptContent = $@"Bootstrap specialist initializing Outline MCP integration.

## Instructions
You have the embedded template content below. Use it EXACTLY as provided to create documents in Outline.

## Steps
1. list_collections → check existing collections

2. Create 'Getting Started' collection if missing:
   - name: ""Getting Started""
   - description: ""Welcome guide and usage examples for Outline MCP Integration""
   - permission: ""read_write""
   - icon: ""STAR""

3. Create welcome document in 'Getting Started' collection:
   **Document: Welcome to Outline MCP**
   ```markdown
{gettingStartedContent}
   ```

4. Create 'Outline Skills' collection if missing:
   - name: ""Outline Skills""
   - description: ""Customizable templates for MCP documentation prompts""
   - permission: ""read_write""
   - icon: ""DOCUMENT""

5. Populate 'Outline Skills' collection with these 3 template documents:

   **Document 1: ProjectDocumentation**
   ```markdown
{projectTemplate}
   ```

   **Document 2: FeatureDocumentation**
   ```markdown
{featureTemplate}
   ```

   **Document 3: BudgetProposal**
   ```markdown
{budgetTemplate}
   ```

6. Report success:
   - List both collection IDs created (Getting Started + Outline Skills)
   - Document URLs with direct links
   - Confirm welcome doc + 3 templates created (total: 4 documents)
   - Highlight: Read the welcome document for complete usage guide
   - Next steps: Test prompts starting with /doc-project

Use the EXACT content provided above. Do NOT modify or summarize it.";

        return new GetPromptResult
        {
            Description = "Execute bootstrap to create Outline Skills collection with customizable templates",
            Messages = new[]
            {
                OutlinePrompts.CreatePromptMessage(Role.User, promptContent)
            }
        };
    }
}
