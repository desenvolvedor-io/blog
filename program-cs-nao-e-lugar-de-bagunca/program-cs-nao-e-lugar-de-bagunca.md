Organizar o `Program.cs` é como manter sua cozinha limpa enquanto cozinha para uma família inteira. Deixar tudo em um lugar só funciona até a hora que não funciona mais. Neste artigo, vamos entender por que modularizar 
esse arquivo é essencial para a manutenção, escalabilidade e legibilidade do seu código .NET Core.

## Introdução

O `Program.cs` passou a assumir o papel central de configuração de uma aplicação ASP.NET Core a partir do .NET 6, com a remoção da classe `Startup`. Essa fusão trouxe benefícios em termos de simplificação e desempenho de inicialização, mas também criou um novo tipo de problema: um arquivo que cresce descontroladamente e se torna um verdadeiro Frankenstein de configurações.

---

## Por que o Program.cs cresce descontroladamente?

Porque simplesmente tudo está sendo colocado ali: configurações de DI, CORS, autenticação, Swagger, health checks, Serilog, configurações específicas de domínio e muito mais. No início, parece prático. Com o tempo, vira um caos.

## Benefícios da modularização via métodos de extensão

- **Clareza**: cada configuração tem seu espaço próprio.
- **Reutilização**: código pode ser replicado entre projetos.
- **Testabilidade**: é mais fácil testar configurações isoladamente.
- **Manutenção**: encontrar bugs ou alterar comportamentos se torna mais simples.
- **Escalabilidade**: times podem trabalhar de forma paralela.

---

## Problema comum: o Program.cs inchado

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddRazorPages();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddAuthentication();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

builder.Services.AddScoped<ICustomerAppService, CustomerAppService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
// ... mais 30 linhas de serviços

var app = builder.Build();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
// ... mais 20 linhas de middlewares

app.MapControllers();
app.Run();
```

---

## A solução: classes especialistas + métodos de extensão bem nomeados

A armadilha comum é simplesmente mover a bagunça para outro lugar, criando métodos de extensão gigantescos com várias responsabilidades. Isso só muda o problema de lugar. A verdadeira solução está em aplicar o princípio da responsabilidade única também na organização das extensões.

Cada contexto da aplicação deve ter sua própria **classe especialista de extensão**. Por exemplo:

- `CorsConfiguration`
- `SwaggerConfiguration`
- `IdentityConfiguration`
- `DependencyInjectionConfiguration`
- `LoggingConfiguration`

---

## Exemplo completo e organizado de extensão

### Serviço: Cors

```csharp
namespace MyProject.API.Configuration
{
    public static class CorsConfiguration
    {
        // Configuração do Builder
        public static WebApplicationBuilder AddCorsConfiguration(this WebApplicationBuilder builder)
        {
            builder.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });
            return builder;
        }
        
        // Configuração da aplicação (Middlewares)
        public static WebApplication UseCorsConfiguration(this WebApplication app)
        {
            return app.UseCors("AllowAll");
        }
    }
}
```

### Serviço: Swagger

```csharp
namespace MyProject.API.Configuration
{
    public static class SwaggerConfiguration
    {
        // Configuração do Builder
        public static WebApplicationBuilder AddSwaggerConfiguration(this WebApplicationBuilder builder)
        {
            builder.AddSwaggerGen();
            return builder;
        }
        
        // Configuração da aplicação (Middlewares)
        public static WebApplication UseSwaggerConfiguration(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
            return app;
        }
    }
}
```

### Aplicação na prática

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddControllers()                          // Configuração geral
       .AddCorsConfiguration()                    // Configuração CORS
       .AddSwaggerConfiguration()                 // Configuração Swagger
       .AddIdentityConfiguration()                // Configuração Identity
       .AddDependencyInjectionConfiguration();    // Configuração Serviços Injetados via DI

var app = builder.Build();

app.UseCorsConfiguration();                       // Configuração Middleware CORS

app.UseAuthentication();
app.UseAuthorization();

app.UseSwaggerConfiguration();                    // Configuração Middleware Swagger

app.MapControllers();
app.Run();
```

---

## Organização de arquivos e boas práticas

- Crie um arquivo de extensão por responsabilidade
- Nome claro e objetivo para os arquivos e métodos
- Use pastas com nomes claros: `Extensions` / `Configurations` / etc.
- Cada método deve ter uma responsabilidade única

---

## Veja na prática

Quer ver como tudo isso fica implementado de verdade?

Nosso projeto case, o Equinox Project, traz uma implementação completa e real de configuração da program.cs com total abstração de cada responsabilidade.

---

## Conclusão

Modularizar não é esconder a bagunça, é tratá-la com a disciplina que ela exige!

Ao criar métodos de extensão especializados e distribuídos por contexto, você alcança o verdadeiro objetivo: manter sua aplicação ASP.NET Core limpa, testável, legível e escalável.

**Desafio para você: Refatore aquele Program.cs que está virando um monstro.**
Mas vá além dos métodos de extensão genéricos: crie classes organizadas por contexto. É aí que mora a verdadeira elegância arquitetural.

