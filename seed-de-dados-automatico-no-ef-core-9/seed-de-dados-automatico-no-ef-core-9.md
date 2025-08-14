O **seeding de dados** sempre foi um ponto sensível no Entity Framework Core.

Embora a API tradicional (`HasData`) seja suficiente para cenários simples, ela engessa lógicas mais complexas, não suporta código assíncrono e não se integra bem com injeção de dependências.

Com o EF Core 9, a Microsoft introduziu uma melhoria significativa: **`UseAsyncSeeding`**.

Essa funcionalidade moderniza o *pipeline* de inicialização de dados, permitindo escrever código de seeding assíncrono, expressivo e desacoplado do modelo.

Neste artigo, vamos além do “como usar”, vamos entender o **porquê**, analisar os trade-offs, comparar com abordagens anteriores e fechar com um guia de boas práticas.

---

## **Contexto: o que era o seeding antes do EF Core 9**

Historicamente, o EF Core oferecia duas formas principais de seeding:

1. **`HasData` no `OnModelCreating`**
    - Declarativo, mas limitado a dados estáticos.
    - Sem suporte a `await` ou lógica condicional avançada.
    - Difícil integrar com serviços externos.
2. **Execução manual após migração**
    - Com flexibilidade total, mas espalhando código de seeding fora do ciclo de vida do `DbContext`.
    - Maior risco de esquecimento em ambientes automatizados.

📌 *Provocação:* quantas vezes você já teve que escrever scripts SQL à parte porque o `HasData` não dava conta? E quantos bugs surgiram por esquecimento de rodar esses scripts após `dotnet ef database update`?

---

## **A proposta do `UseAsyncSeeding`**

Segundo a documentação oficial da Microsoft, o `UseAsyncSeeding`:

- Registra um **delegate assíncrono** que será executado após `EnsureCreated` ou `Migrate`.
- Funciona integrado ao ciclo de vida do EF Core, sem hacks ou scripts externos.
- Recebe três parâmetros:
    1. `DbContext` configurado e pronto para uso.
    2. `bool` indicando se houve alguma operação de criação/atualização de schema.
    3. `CancellationToken` para controle de cancelamento.
- Pode **usar injeção de dependência** e chamadas assíncronas (`await`).
- Pode ser combinado com `UseSeeding` (versão síncrona) para suportar *design-time* tools.

---

## **Exemplo prático: PostgreSQL + `UseAsyncSeeding`**

Suponha que precisamos popular a tabela `Products` apenas se ela estiver vazia:

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options
        .UseNpgsql(configuration.GetConnectionString("Postgres"))
        .UseAsyncSeeding(async (dbContext, _, cancellationToken) =>
        {
            if (!await dbContext.Set<Product>().AnyAsync(cancellationToken))
            {
                var products = GenerateProducts();

                dbContext.Set<Product>().AddRange(products);

                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }));
```

### **O que acontece aqui**

1. **Configuração do provider**
    - `UseNpgsql` aponta para nosso PostgreSQL.
2. **Registro do seeding assíncrono**
    - `UseAsyncSeeding` recebe um lambda com `dbContext` já instanciado.
    - O `_` ignora o parâmetro de alteração de schema.
    - `cancellationToken` é repassado para consultas e `SaveChangesAsync`.
3. **Execução condicional**
    - Verifica com `AnyAsync` se já existem produtos.
    - Gera a lista com `GenerateProducts()`.
    - Persiste com `AddRange` + `SaveChangesAsync`.

---

## **Benefícios reais para o desenvolvedor**

- **Código mais limpo e centralizado**
    
    O seeding fica na configuração do contexto, não espalhado pelo projeto.
    
- **Integração com async/await**
    
    Possibilidade de consultar APIs, carregar arquivos externos ou buscar dados iniciais via HTTP.
    
- **Flexibilidade**
    
    Podemos usar lógica condicional avançada, inclusive multi-tabelas e multi-contextos.
    
- **Melhor DX (Developer Experience)**
    
    Não precisamos mais "lembrar" de rodar seeding manualmente em cada ambiente.
    

---

## **Trade-offs e cuidados**

- **Performance na inicialização**
    
    Se o seeding fizer operações pesadas, a aplicação pode demorar mais para subir.
    
- **Concorrência**
    
    Em cenários com múltiplas instâncias iniciando ao mesmo tempo (Kubernetes, por exemplo), é preciso garantir que não haja duplicação de dados, use verificações sólidas e locks no banco se necessário.
    
- **Compatibilidade com ferramentas**
    
    Algumas ferramentas de *design-time* ainda executam apenas a versão síncrona (`UseSeeding`). Se você depende dessas ferramentas, implemente as duas versões.
    

---

## **Boas práticas recomendadas**

1. **Sempre verificar antes de inserir**
    
    Use `AnyAsync` para evitar duplicações.
    
2. **Mantenha rápido e idempotente**
    
    O seeding deve poder rodar várias vezes sem causar inconsistências.
    
3. **Separar dados de teste e dados reais**
    
    Use condicionais de ambiente (`IHostEnvironment`) para evitar inserir dados de teste em produção.
    
4. **Logar a execução**
    
    Logue quando e o que foi semeado, útil para auditoria e depuração.
    

---

## **Conclusão**

O `UseAsyncSeeding` é mais que uma melhoria incremental: ele redefine a forma como tratamos dados iniciais no EF Core, unindo a praticidade do seeding automático com o poder do código assíncrono moderno.

Para equipes que já lidaram com a dor de manter scripts de seeding externos, duplicação de lógica e inconsistências entre ambientes, essa é uma oportunidade de simplificar e ganhar robustez.

💡 **Desafio para você:**

Migre hoje um seeding estático (`HasData`) para `UseAsyncSeeding` e veja como o código fica mais expressivo e sustentável.
