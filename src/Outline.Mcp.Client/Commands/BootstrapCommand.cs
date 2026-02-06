using Outline.Mcp.Shared.Api;
using Outline.Mcp.Shared.Models;
using Spectre.Console;

namespace Outline.Mcp.Client.Commands;

public static class BootstrapCommand
{
    public static async Task ExecuteAsync(
        IOutlineApiClient client,
        string baseUrl,
        bool validateOnly)
    {
        AnsiConsole.Write(new Rule("[yellow]Outline Skills Bootstrap[/]").RuleStyle("yellow"));
        AnsiConsole.WriteLine();

        try
        {
            // Step 1: Validate Environment
            AnsiConsole.MarkupLine("[bold](1/7)[/] Validando ambiente...");
            ValidateEnvironment(baseUrl);

            // Step 2: Test Connection
            AnsiConsole.MarkupLine("[bold](2/7)[/] Testando conexão...");
            await TestConnection(client);

            // Step 3: Test Authentication  
            AnsiConsole.MarkupLine("[bold](3/7)[/] Testando autenticação...");
            await TestAuthentication(client);

            // Step 4: Create/Get Getting Started Collection
            AnsiConsole.MarkupLine("[bold](4/7)[/] Criando collection Getting Started...");
            var gettingStartedCollection = await CreateOrGetGettingStartedCollection(client);
            AnsiConsole.MarkupLine($"  [green]✓ Collection Getting Started: {gettingStartedCollection.Id}[/]");

            // Step 5: Create/Get Outline Skills Collection
            AnsiConsole.MarkupLine("[bold](5/7)[/] Criando collection Outline Skills...");
            var skillsCollection = await CreateOrGetSkillsCollection(client);
            AnsiConsole.MarkupLine($"  [green]✓ Collection Skills: {skillsCollection.Id}[/]");

            // Step 6: Test Tools
            AnsiConsole.MarkupLine("[bold](6/7)[/] Testando ferramentas...");
            await TestTools(client, skillsCollection.Id);

            // Step 7: Validation Complete
            AnsiConsole.MarkupLine("\n[bold green](7/7) Validação completa![/]\n");

            if (validateOnly)
            {
                AnsiConsole.MarkupLine("[yellow]Modo validate-only ativado. Documentação não será criada.[/]");
                return;
            }

            // Populate Getting Started
            AnsiConsole.MarkupLine("[bold]Criando documento de boas-vindas...[/]");
            await PopulateGettingStarted(client, gettingStartedCollection.Id, baseUrl);
            AnsiConsole.MarkupLine($"  [green]✓ Documento de boas-vindas criado[/]");

            // Populate Skills Templates
            AnsiConsole.MarkupLine("[bold]Populando templates em Outline Skills...[/]");
            var templatesCount = await PopulateSkillsTemplates(client, skillsCollection.Id);
            AnsiConsole.MarkupLine($"  [green]✓ {templatesCount} templates criados[/]");

            // Bootstrap Complete
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule().RuleStyle("green"));
            AnsiConsole.MarkupLine($"[green]Bootstrap completo! 2 collections e 4 documentos criados.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Collections criadas:[/]");
            AnsiConsole.MarkupLine($"  [cyan]Getting Started:[/] {baseUrl}/collection/{gettingStartedCollection.Id}");
            AnsiConsole.MarkupLine($"  [cyan]Outline Skills:[/] {baseUrl}/collection/{skillsCollection.Id}");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Próximos passos:[/]");
            AnsiConsole.MarkupLine("1. [yellow]Leia o documento de boas-vindas[/] na collection 'Getting Started'");
            AnsiConsole.MarkupLine("2. Customize os templates na collection 'Outline Skills'");
            AnsiConsole.MarkupLine("3. Teste os prompts MCP no seu workspace!");
            AnsiConsole.Write(new Rule().RuleStyle("green"));
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[red]Erro[/]").RuleStyle("red"));
            AnsiConsole.MarkupLine($"[red]✗ Erro: {ex.Message}[/]");
            throw;
        }
    }

    private static void ValidateEnvironment(string baseUrl)
    {
        var apiKey = Environment.GetEnvironmentVariable("OUTLINE_API_KEY");
        
        AnsiConsole.MarkupLine($"  [green][[OK]][/] OUTLINE_BASE_URL: {baseUrl}");
        AnsiConsole.MarkupLine($"  [green][[OK]][/] OUTLINE_API_KEY: {MaskApiKey(apiKey)}");
    }

    private static async Task TestConnection(IOutlineApiClient client)
    {
        await client.ListCollectionsAsync();
        AnsiConsole.MarkupLine("  [green][[OK]][/] Conexão estabelecida");
    }

    private static async Task TestAuthentication(IOutlineApiClient client)
    {
        var collections = await client.ListCollectionsAsync();
        AnsiConsole.MarkupLine($"  [green][[OK]][/] Autenticação válida");
    }

    private static async Task<OutlineCollection> CreateOrGetGettingStartedCollection(IOutlineApiClient client)
    {
        var collections = await client.ListCollectionsAsync();
        var existing = collections.FirstOrDefault(c => c.Name == "Getting Started");
        
        if (existing != null)
            return existing;
        
        return await client.CreateCollectionAsync(new CreateCollectionRequest
        {
            Name = "Getting Started",
            Description = "Welcome guide and usage examples for Outline MCP Integration",
            Permission = "read_write",
            Icon = "STAR"
        });
    }

    private static async Task<OutlineCollection> CreateOrGetSkillsCollection(IOutlineApiClient client)
    {
        var collections = await client.ListCollectionsAsync();
        var existing = collections.FirstOrDefault(c => c.Name == "Outline Skills");
        
        if (existing != null)
            return existing;
        
        return await client.CreateCollectionAsync(new CreateCollectionRequest
        {
            Name = "Outline Skills",
            Description = "Customizable templates for MCP documentation prompts",
            Permission = "read_write",
            Icon = "DOCUMENT"
        });
    }

    private static async Task PopulateGettingStarted(
        IOutlineApiClient client,
        string collectionId,
        string baseUrl)
    {
        var welcomeContent = @"# Bem-vindo ao Outline MCP Integration

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

        await client.CreateDocumentAsync(new CreateDocumentRequest
        {
            Title = "Welcome to Outline MCP",
            Text = welcomeContent,
            CollectionId = collectionId,
            Publish = true
        });
    }

    private static async Task<int> PopulateSkillsTemplates(
        IOutlineApiClient client, 
        string collectionId)
    {
        var createdCount = 0;
        
        // Find repository root by searching for .git directory
        var repoRoot = FindRepositoryRoot() ?? Directory.GetCurrentDirectory();
        var skillsPath = Path.Combine(repoRoot, "skills");
        
        AnsiConsole.MarkupLine($"    [dim]Skills path: {skillsPath}[/]");

        // Template 1: ProjectDocumentation
        var projectDocPath = Path.Combine(skillsPath, "project-documentation.md");
        if (File.Exists(projectDocPath))
        {
            await client.CreateDocumentAsync(new CreateDocumentRequest
            {
                Title = "ProjectDocumentation",
                Text = File.ReadAllText(projectDocPath),
                CollectionId = collectionId,
                Publish = true
            });
            createdCount++;
            AnsiConsole.MarkupLine("    [green][[OK]][/] project-documentation.md");
        }
        
        // Template 2: FeatureDocumentation
        var featureDocPath = Path.Combine(skillsPath, "feature-documentation.md");
        if (File.Exists(featureDocPath))
        {
            await client.CreateDocumentAsync(new CreateDocumentRequest
            {
                Title = "FeatureDocumentation",
                Text = File.ReadAllText(featureDocPath),
                CollectionId = collectionId,
                Publish = true
            });
            createdCount++;
            AnsiConsole.MarkupLine("    [green][[OK]][/] feature-documentation.md");
        }
        
        // Template 3: BudgetProposal
        var budgetPath = Path.Combine(skillsPath, "budget-proposal.md");
        if (File.Exists(budgetPath))
        {
            await client.CreateDocumentAsync(new CreateDocumentRequest
            {
                Title = "BudgetProposal",
                Text = File.ReadAllText(budgetPath),
                CollectionId = collectionId,
                Publish = true
            });
            createdCount++;
            AnsiConsole.MarkupLine("    [green][[OK]][/] budget-proposal.md");
        }

        return createdCount;
    }

    private static async Task TestTools(IOutlineApiClient client, string collectionId)
    {
        // Test list_collections
        var collections = await client.ListCollectionsAsync();
        AnsiConsole.MarkupLine($"  [green][[OK]][/] list_collections: {collections.Count} encontradas");

        // Test search_documents
        var searchResults = await client.SearchDocumentsAsync("test");
        AnsiConsole.MarkupLine($"  [green][[OK]][/] search_documents: {searchResults.Count} resultados");

        // Test create_document (temporary)
        var testDoc = await client.CreateDocumentAsync(new CreateDocumentRequest
        {
            Title = $"Test {DateTime.UtcNow:yyyyMMddHHmmss}",
            Text = "# Test\n\nTeste automático.",
            CollectionId = collectionId,
            Publish = false
        });
        AnsiConsole.MarkupLine($"  [green][[OK]][/] create_document: ID {testDoc.Id}");

        // Test update_document
        await client.UpdateDocumentAsync(testDoc.Id, new UpdateDocumentRequest
        {
            Text = "# Test\n\nAtualizado."
        });
        AnsiConsole.MarkupLine("  [green][[OK]][/] update_document: atualizado");

        // Test get_document
        await client.GetDocumentAsync(testDoc.Id);
        AnsiConsole.MarkupLine("  [green][[OK]][/] get_document: recuperado");

        // Cleanup
        await client.DeleteDocumentAsync(testDoc.Id, permanent: true);
        AnsiConsole.MarkupLine("  [green][[OK]][/] Limpeza concluída");
    }

    private static string MaskApiKey(string? apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            return "***";
        
        if (apiKey.Length <= 10)
            return new string('*', apiKey.Length);
        
        return apiKey.Substring(0, 7) + new string('*', apiKey.Length - 7);
    }

    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        while (!string.IsNullOrEmpty(currentDir))
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")))
                return currentDir;
            
            var parent = Directory.GetParent(currentDir);
            if (parent == null)
                break;
                
            currentDir = parent.FullName;
        }
        
        return null;
    }
}
