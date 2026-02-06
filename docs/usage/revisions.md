# Document Revisions - Version Control

Guia completo do sistema de revisões do Outline.

## O que são Revisões?

O Outline mantém **histórico completo** de todas as alterações em documentos. Cada edição cria uma nova revisão, permitindo:

- ✅ Rastrear mudanças ao longo do tempo
- ✅ Ver quem fez cada alteração
- ✅ Restaurar versões anteriores
- ✅ Comparar diferentes versões
- ✅ Auditoria completa de modificações

## Ferramentas Disponíveis

### 1. list_revisions

Lista todo o histórico de revisões de um documento.

**CLI:**

```bash
dotnet run --project src/Outline.Mcp.Client -- list-revisions \
  --document-id "doc-001"
```

**Cursor/Claude Desktop:**

```
"Liste todas as revisões do documento 'API Reference' no Outline"
```

**Saída:**

```
╭─────────────── Revisions ───────────────╮
│ Document ID: doc-001                     │
│ Total Revisions: 5                       │
│                                          │
│ Revision 1                               │
│   ID: rev-001                            │
│   Created: 2026-02-01 10:00              │
│   Author: João Silva                     │
│   Preview: # API Reference...            │
│                                          │
│ Revision 2                               │
│   ID: rev-002                            │
│   Created: 2026-02-02 14:30              │
│   Author: Maria Santos                   │
│   Preview: # API Reference               │
│            Updated endpoints...          │
└──────────────────────────────────────────┘
```

### 2. get_revision

Obtém conteúdo completo de uma revisão específica.

**CLI:**

```bash
dotnet run --project src/Outline.Mcp.Client -- get-revision \
  --revision-id "rev-002"
```

**Cursor/Claude Desktop:**

```
"Mostre o conteúdo completo da revisão rev-002"
```

```
"Mostre a versão anterior do documento 'API Reference'"
```

**Saída:**

```
╭─────────────── Revision ───────────────╮
│ ID: rev-002                            │
│ Document: doc-001                      │
│ Title: API Reference                   │
│ Created: 2026-02-02 14:30              │
│ Author: Maria Santos                   │
│                                        │
│ Content:                               │
│ # API Reference                        │
│                                        │
│ ## Updated Endpoints                   │
│ ...                                    │
└────────────────────────────────────────┘
```

### 3. restore_revision

Restaura documento para uma revisão anterior.

**⚠️ Importante:** Restauração **não é destrutiva**. Cria uma **nova revisão** com o conteúdo antigo, mantendo todo o histórico.

**CLI:**

```bash
dotnet run --project src/Outline.Mcp.Client -- restore-revision \
  --document-id "doc-001" \
  --revision-id "rev-002"
```

**Cursor/Claude Desktop:**

```
"Restaure o documento 'API Reference' para a revisão de ontem"
```

```
"Desfaça as últimas mudanças no documento 'Setup Guide'"
```

**Saída:**

```
╭─────────── Document Restored ───────────╮
│ Document ID: doc-001                      │
│ Restored to Revision: rev-002             │
│                                           │
│ New Document:                             │
│   ID: doc-001                             │
│   Title: API Reference                    │
│   Updated: 2026-02-03 16:00               │
│   Revision: 6                             │
│   URL: http://localhost:3000/doc/api-ref  │
│                                           │
│ Message: Document successfully restored   │
│          to revision rev-002              │
└───────────────────────────────────────────┘
```

## Casos de Uso Práticos

### 1. Desfazer Mudanças Acidentais

**Cenário:** Você atualizou um documento e percebeu que perdeu conteúdo importante.

```
"Liste as revisões do documento 'User Guide' e restaure para a versão anterior"
```

O agente irá:
1. Executar `list_revisions` para encontrar a última revisão
2. Executar `restore_revision` para restaurar

### 2. Comparar Versões

**Cenário:** Quer ver o que mudou entre duas versões.

```
"Mostre o conteúdo da revisão rev-003 e da revisão rev-005 do documento 'API Reference' 
e liste as principais diferenças"
```

O agente irá:
1. Executar `get_revision` para rev-003
2. Executar `get_revision` para rev-005
3. Comparar e listar diferenças

### 3. Auditoria de Mudanças

**Cenário:** Precisa saber quem alterou um documento e quando.

```
"Mostre todo o histórico de revisões do documento 'Security Policy' 
com autores e datas"
```

**Saída esperada:**

```
Histórico de Revisões - Security Policy

1. rev-001 - 2026-01-15 10:00 - João Silva
   Versão inicial da política de segurança
   
2. rev-002 - 2026-01-20 14:30 - Maria Santos
   Adicionada seção de autenticação multi-fator
   
3. rev-003 - 2026-02-01 09:15 - João Silva
   Atualizada política de senhas (mínimo 12 caracteres)
   
4. rev-004 - 2026-02-03 16:45 - Pedro Costa
   Adicionados requisitos de criptografia para dados sensíveis
```

### 4. Recuperar Conteúdo Deletado

**Cenário:** Seção foi removida do documento e precisa recuperar.

```
"Busque no histórico de revisões do 'API Reference' 
a última vez que a seção 'Rate Limiting' existia 
e restaure essa versão"
```

### 5. Rastrear Evolução de Documentação

**Cenário:** Quer ver como o documento evoluiu ao longo do tempo.

```
"Para o documento 'Architecture Overview', 
mostre as 5 primeiras revisões e resuma as principais mudanças 
em cada uma"
```

## Workflow Recomendado

### Antes de Grandes Alterações

1. **Verifique a versão atual:**

```bash
dotnet run --project src/Outline.Mcp.Client -- get --id "doc-id"
```

2. **Liste revisões existentes:**

```bash
dotnet run --project src/Outline.Mcp.Client -- list-revisions \
  --document-id "doc-id"
```

3. **Faça as alterações**

4. **Se necessário, restaure:**

```bash
dotnet run --project src/Outline.Mcp.Client -- restore-revision \
  --document-id "doc-id" \
  --revision-id "rev-xxx"
```

### Integração com CI/CD

```yaml
# .github/workflows/docs-audit.yml
name: Documentation Audit

on:
  schedule:
    - cron: '0 0 * * 1'  # Toda segunda-feira

jobs:
  audit:
    runs-on: ubuntu-latest
    steps:
      - name: Check critical docs changes
        env:
          OUTLINE_API_KEY: ${{ secrets.OUTLINE_API_KEY }}
          OUTLINE_BASE_URL: ${{ secrets.OUTLINE_BASE_URL }}
        run: |
          # Lista revisões da última semana
          dotnet run --project src/Outline.Mcp.Client -- list-revisions \
            --document-id "${{ vars.SECURITY_DOC_ID }}"
          
          # Envia relatório
          # ... lógica de notificação
```

## Metadados de Revisão

Cada revisão contém:

| Campo | Descrição |
|-------|-----------|
| `id` | ID único da revisão |
| `documentId` | ID do documento |
| `title` | Título do documento na revisão |
| `text` | Conteúdo completo (markdown) |
| `createdAt` | Data/hora de criação |
| `createdBy` | Autor da revisão (usuário) |

## Como Funciona Internamente

### Criação de Revisões

O Outline cria revisões automaticamente:

1. **A cada edição** (mínimo de 5 minutos entre revisões)
2. **Ao publicar** documento draft
3. **Ao restaurar** revisão anterior (cria nova revisão)

**Não é possível:**
- Criar revisões manualmente via API
- Forçar criação de revisão
- Apagar revisões via API (apenas em edições licensed)

### Armazenamento

Revisões são armazenadas no PostgreSQL:

- Cada revisão é um registro completo (não delta)
- Conteúdo markdown é armazenado na íntegra
- Metadados incluem autor, data, preview

### Restauração

Quando você restaura uma revisão:

1. Outline copia conteúdo da revisão antiga
2. Cria **nova revisão** com esse conteúdo
3. Atualiza documento para apontar à nova revisão
4. **Histórico permanece intacto**

**Exemplo de timeline após restauração:**

```
rev-001 (original)
rev-002 (edição 1)
rev-003 (edição 2) <-- problema aqui
rev-004 (edição 3)
rev-005 (restaurado para rev-002) <-- cria nova revisão com conteúdo de rev-002
```

## Limitações

### 1. Intervalo Mínimo

O Outline não cria revisões para edições muito próximas:

- **Intervalo:** ~5 minutos entre revisões
- **Comportamento:** Edições dentro de 5 min atualizam a revisão atual

**Implicação:** Múltiplas edições rápidas podem ser "agrupadas" em uma revisão.

### 2. Armazenamento

Revisões ocupam espaço no banco de dados:

- Cada revisão = conteúdo completo do documento
- Documentos grandes com muitas revisões = alto uso de storage
- **Recomendação:** Documentos < 500KB

### 3. Não há "Diff" Nativo

A API não fornece diff entre revisões:

- Você recebe conteúdo completo de cada revisão
- Para comparar, precisa fazer diff localmente
- **Solução:** Use ferramentas como `diff` (Unix) ou libs de diff

**Exemplo:**

```bash
# Obter rev-002
dotnet run --project src/Outline.Mcp.Client -- get-revision \
  --revision-id "rev-002" > rev-002.md

# Obter rev-003
dotnet run --project src/Outline.Mcp.Client -- get-revision \
  --revision-id "rev-003" > rev-003.md

# Comparar
diff rev-002.md rev-003.md
```

### 4. Retenção

- **Self-hosted:** Revisões mantidas indefinidamente
- **Cloud/Licensed:** Pode ter política de retenção customizada
- **Free tier:** Retenção ilimitada (mas pode mudar no futuro)

### 5. Sem "Branches"

O Outline não suporta branches de documentação:

- Apenas uma "linha" de revisões
- Não há merge de branches
- **Workaround:** Use documentos separados como "drafts"

## Best Practices

### 1. Inclua Metadados de Versão

Adicione informações de versão nos documentos:

```markdown
# API Documentation

**Versão:** 2.1.0  
**Data de Atualização:** 2026-02-03  
**Revisão:** 15  
**Status:** Atual

---

## Histórico de Mudanças

- **v2.1.0** (2026-02-03): Adicionados endpoints de webhooks
- **v2.0.0** (2026-01-15): Breaking changes na autenticação
- **v1.0.0** (2025-12-01): Versão inicial
```

### 2. Documente Mudanças Importantes

Para mudanças significativas, adicione nota no documento:

```markdown
## Changelog Interno

### 2026-02-03 - v2.1
- Adicionada seção de webhooks
- Atualizado exemplo de autenticação
- Corrigidos typos na seção de rate limiting
```

### 3. Use Revisões para Rollback

Antes de mudanças grandes, verifique que pode fazer rollback:

```bash
# Liste revisões
dotnet run --project src/Outline.Mcp.Client -- list-revisions \
  --document-id "doc-critical"

# Faça mudanças...

# Se necessário, restaure
dotnet run --project src/Outline.Mcp.Client -- restore-revision \
  --document-id "doc-critical" \
  --revision-id "rev-last-good"
```

### 4. Auditoria Regular

Monitore mudanças em documentos críticos:

```bash
#!/bin/bash
# audit-critical-docs.sh

CRITICAL_DOCS=("security-policy" "api-reference" "architecture")

for DOC in "${CRITICAL_DOCS[@]}"; do
  echo "=== $DOC ==="
  dotnet run --project src/Outline.Mcp.Client -- list-revisions \
    --document-id "$DOC" | \
    head -20
  echo ""
done
```

### 5. Não Confie em Edição Simultânea

Evite múltiplos usuários editando simultaneamente:

- Outline usa revisões para conflict resolution
- Última edição "vence"
- Mudanças anteriores podem ser perdidas

**Solução:** Use comunicação de equipe ou locks externos.

## Troubleshooting

### Revisão não aparece

**Causa:** Edição muito recente (< 5 min) ou não foi salva.

**Solução:**
- Aguarde alguns minutos
- Verifique se documento foi salvo no Outline
- Force refresh: `list-revisions` novamente

### Erro ao restaurar

**Causa:** Revisão não existe ou foi deletada.

**Solução:**
```bash
# Liste revisões disponíveis
dotnet run --project src/Outline.Mcp.Client -- list-revisions \
  --document-id "doc-id"

# Use ID válido da lista
```

### Conteúdo restaurado diferente do esperado

**Causa:** Restauração criou nova revisão com conteúdo correto, mas edição posterior sobrescreveu.

**Solução:** Verifique timestamp da última edição e restaure novamente se necessário.

## Próximos Passos

- [CLI](./cli.md) - Comandos básicos
- [MCP Tools](./mcp-tools.md) - Uso com AI agents
- [MCP Prompts](./mcp-prompts.md) - Workflows guiados
