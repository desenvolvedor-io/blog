**Evite armadilhas clássicas e melhore sua arquitetura com essas verdades técnicas**

O ecossistema do ASP.NET é poderoso, moderno e extremamente produtivo, mas como toda stack rica, ele traz armadilhas escondidas e mitos que acabam se perpetuando entre desenvolvedores. Desde decisões equivocadas sobre controllers até má compreensão do pipeline de middlewares ou injeção de dependência, **existem erros comuns que prejudicam a qualidade, manutenibilidade e segurança das aplicações**.

Separamos aqui 10 mitos e más práticas comuns que você precisa abandonar e o que fazer no lugar.

---

## 1. “O controller é onde tudo acontece”

❌ **Mito** — Muitos projetos começam simples e logo viram um pesadelo de *God Controllers* com 500+ linhas, cheios de lógica, validação, regras de negócio e queries.

✅ **Fato** — O controller é a **porta de entrada** HTTP. Nada mais. Sua responsabilidade deveria ser:

- receber input,
- delegar para um **service** ou **handler**,
- retornar uma resposta.

**Solução:** crie camadas de serviços, comandos ou *handlers* para aplicar a lógica. Use um padrão estilo Mediator se quiser isolar ainda mais o fluxo.

```csharp
[HttpPost]
public IActionResult CriarPedido([FromBody] PedidoDto dto)
{
    var id = _pedidoService.Criar(dto);
    return CreatedAtAction(nameof(Obter), new { id }, null);
}
```

> Você pode concentrar mais responsabilidades na controller, desde que seja um projeto simples, a partir do momento que a complexidade aumentar comece a delegar as responsabilidades para serviços ou etc.

---

## 2. “Precisa usar sempre o ASP.NET Identity”

❌ **Mito** — Em projetos pequenos ou APIs simples, usar Identity pode ser como matar mosquito com canhão.

✅ **Fato** — Em projetos mais simples, **JWT com autenticação via cookies ou tokens** + controle de roles é o suficiente.

Use ASP.NET Identity quando:

- precisa de UI de login pronta (Razor)
- deseja controlar lockout, senha, recovery
- precisa de um sistema pronto de controle de usuários
- há múltiplos tenants ou providers externos

**Alternativa leve:**

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(...);
```

---

## 3. “Preciso usar MVC ou Razor Pages pra fazer frontend”

❌ **Mito** — Razor tem seu lugar, mas não é a única (nem a principal) forma de fazer UI em .NET.

✅ **Fato** — **O padrão atual é separar front e back.** O ASP.NET serve o backend como APIs RESTful (com ou sem Minimal APIs) e o front é feito em **React, Angular ou Blazor**.

Razor Pages ou MVC são úteis para:

- paineis e dashboards adminstrativas
- sistemas internos
- apps sem grandes exigências de UX
- apps onde o uso de JS não é intenso (ninguém merece jQuery)

---

## 4. “Minimal API é só pra protótipo”

❌ **Mito** — Muitos ainda acham que Minimal API é só um jeito rápido de "testar algo", fazer um MVP.

✅ **Fato** — A **Minimal API do ASP.NET Core** evoluiu muito. Você pode:

- versionar
- aplicar autenticação
- usar middlewares, validadores e filtros
- ter OpenAPI/Swagger gerado

Exemplo robusto:

```csharp
app.MapPost("/pedidos", async (CreatePedidoRequest req, IPedidoService svc) =>
{
    var id = await svc.CriarAsync(req);
    return Results.Created($"/pedidos/{id}", null);
})
.RequireAuthorization()
.Produces(StatusCodes.Status201Created);
```

---

## 5. “Configuração via appsettings.json é suficiente e segura”

❌ **Mito** — Muita gente joga tudo no `appsettings.json`, inclusive chaves de API, secrets e configurações mutáveis.

✅ **Fato** — Configuração precisa ser **segura, segregada e mutável**:

- **Secrets** devem vir de Azure Key Vault, AWS Secrets Manager ou User Secrets.
- Use `IOptionsMonitor<T>` para reload dinâmico.
- Integre com Feature Flags para configs em runtime.

---

## 6. “AddScoped resolve quase tudo”

❌ **Mito** — `AddScoped` virou o default mental da maioria dos devs. Mas é a escolha certa?

✅ **Fato** — O ciclo de vida depende do contexto:

- **Transient**: serviços stateless, handlers, lógica de uso curto.
- **Scoped**: uso com `DbContext` ou por request.
- **Singleton**: cache, config, clients reusáveis.

🚨 Misturar `Scoped` com `HttpClient` mal configurado (sem `IHttpClientFactory`) causa bugs difíceis (como `ObjectDisposedException`).

---

## 7. “O pipeline HTTP do ASP.NET é transparente”

❌ **Mito** — Muitos não entendem a **ordem dos middlewares**, nem o impacto de `UseRouting()`, `UseAuthentication()`, `UseEndpoints()` etc.

✅ **Fato** — O pipeline precisa de ordem correta. Um exemplo comum de erro:

```csharp
app.UseRouting();
app.UseAuthorization(); // antes de Authentication = bug de auth silencioso
app.UseAuthentication();
app.UseEndpoints(...);
```

**Dica:** se uma exceção “desaparece”, verifique se ela está antes do `UseExceptionHandler()`.

---

## 8. “Caching é só usar [ResponseCache] ou IMemoryCache”

❌ **Mito** — Isso é apenas o primeiro degrau.

✅ **Fato** — Cache sério precisa de:

- **múltiplos níveis**: in-memory, Redis, CDN
- **estratégias de invalidação**: timeout, tokens, pub/sub
- **versionamento**: cache key por versão de recurso
- **fallback**: dados anteriores, graceful degradation

---

## 9. “UseDeveloperExceptionPage() é só uma conveniência”

❌ **Mito** — Muitos esquecem isso ativado em produção, sem saber o risco.

✅ **Fato** — Expor o stack trace em produção é uma **falta grave de segurança**, pois revela:

- estrutura de pastas
- nomes de classes
- mensagens de exceções
- caminhos internos

Sempre configure handlers globais:

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/erro");
}
```

---

## 10. “ActionResult é só um tipo de retorno bonito”

❌ **Mito** — Ignorar `ActionResult<T>` é perder a chance de fazer uma API REST decente.

✅ **Fato** — Retornar tipos corretos melhora:

- status code
- content-type
- content negotiation

Exemplo bom:

```csharp
public async Task<ActionResult<Pedido>> Get(int id)
{
    var pedido = await _repo.GetByIdAsync(id);
    if (pedido == null) return NotFound();
    return Ok(pedido);
}
```

---

## Conclusão: arquitetura não é só sobre framework, é sobre consciência

Aprender ASP.NET Core é fácil. O desafio é **usar bem**, tomar boas decisões, evitar ciladas e pensar arquitetura de forma clara.

Abandonar mitos como esses é o primeiro passo para criar **aplicações mais simples, robustas e fáceis de manter**. Muita dor de cabeça nasce de conceitos equivocados ou negligenciados.

Quer evoluir como dev ou arquiteto .NET? Aprenda os fundamentos, entenda as entrelinhas e desafie as “verdades absolutas”.
