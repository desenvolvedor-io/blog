# Compreendendo a classe Program

## A partir do .NET 6 a classe Program passou a ser a única classe de configuração de aplicações .NET, absorvendo as responsabilidades da Startup no ASPNET.

A classe Program sempre esteve presente no ASP.NET, as vezes passava despercebida, pois na maioria das vezes não era necessário alterar a configuração padrão de hosting. Inclusive a classe Startup sempre foi uma forma de especializar a Program, afinal quem chamava a Startup era a Program.

### Não lembra? Veja um exemplo:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
```

Note que a classe Startup era chamada na declaração da configuração do host (entenda como pipeline e request/response).

## Por que a Startup deixou de ser utilizada?

Os desenvolvedores não ficaram muito felizes com a mudança, mas foi necessário.  A Microsoft focada em deixar o ASPNET cada vez mais a frente do Nodejs precisou começar a melhorar onde o ASPNET perdia para o Nodejs.

Um dos principais pontos positivos para o Node é o warm-up da aplicação, sim uma aplicação Node ainda acorda e fica disponível mais rápido que o ASP.NET (depois ele perde rs). E nessa briga a Microsoft introduziu o conceito de Minimal APIs, que implementou o conceito de escrever uma API completa em uma única classe. Além disso no .NET 6 (e no 7) a Microsoft tem trabalhado muito nessa questão de melhorar o warm-up.

Entenda mais sobre Minimal APIs [aqui](https://www.youtube.com/watch?v=FWZhGFhpSLk) e aprenda a escrever uma Minimal API completa do zero [aqui](https://www.youtube.com/watch?v=aXayqUfSNvw)

Podemos resumir então que a classe Startup se foi para dar espaço a uma prática mais direto ao ponto de configurar uma aplicação ASP.NET.
Mas o que é essa lágrima rolando aí? Se for saudades da classe Startup eu vou te ensinar a trazer ela de volta mesmo nas novas versões e de maneira super elegante. [Assista aqui](https://www.youtube.com/watch?v=VgjHQvprRy0)

Muitos alunos me perguntam sobre essa mudança, para mim é algo super simples de entender, mas eu também entendo que para uma pessoa que está estudando ASP.NET agora essa mudança pode ser mais chata de absorver. Para ajudar nessa dúvida comum recomendo muito que assista os vídeos recomendados acima e segue abaixo um guia de referência para entender a classe Program no novo formato.

### A classe Program possui 4 pontos importantes de atenção

- Top-level statements (Instruções de nível superior)
- Definição do WebApplicationBuilder
- Configuração dos serviços
- Configuração do request/response

###  Top-level statements (Instruções de nível superior)

Você irá reparar que a classe Program não possui mais a cerimônia de declaração de namespace, classe e métodos. É possível começar a escrever código na primeira linha sem se preocupar com esses 3 elementos que estão presentes em qualquer classe .NET.

Mas calma ai, não é a festa da uva não. Você só pode ter uma classe do tipo Top-level statements em seu projeto, o resto permanece como sempre foi.
Então a Program passou a ser essa classe direto ao ponto, os exemplos de código vão deixar isso claro.

### Definição do WebApplicationBuilder

O primeiro passo na escrita da Program é a criação do WebApplicationBuilder, que é a classe responsável por oferecer acesso a toda configuração da aplicação. É interessante que entenda a estrutura dessa classe, caso tenha essa curiosidade [veja aqui](https://github.com/dotnet/aspnetcore/blob/313ee06a672385ede5d2c9a01d31a7d9d35a6340/src/DefaultBuilder/src/WebApplicationBuilder.cs).

Na prática é tudo resolvido na primeira linha de código:

```csharp
var builder = WebApplication.CreateBuilder(args);
```
O método [CreateBuilder] retorna uma instância de WebApplicationBuilder.

### Configuração dos serviços

Na classe Startup o primeiro método era o [ConfigureServices] que fazia a configuração dos serviços dentro do pipeline do ASP.NET, e isso permanece da mesma maneira na classe Program. Após definir o builder o próximo passo é configurar os serviços:

```csharp
// Declaração do builder
var builder = WebApplication.CreateBuilder(args);

// Configurando o AppSettings.json conforme ambiente de execução
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

// *** Configurando serviços no container ***

// Adicionando suporte ao contexto do Identity via EF
builder.Services.AddDbContext<MeuContextoDoEF>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Resolvendo classes para injeção de dependência
builder.Services.AddScoped<IMinhaDependencia, MinhaDependencia>();

// Adicionando Suporte a controllers e Views (MVC) no pipeline
builder.Services.AddControllersWithViews();

// Adicionando suporte a componentes Razor (ex. Telas do Identity)
builder.Services.AddRazorPages();

// Realizando o build da aplicação (sempre na última linha).
var app = builder.Build();
```
Podemos observar algumas coisas importantes:

- O builder.Services é o acesso ao IServiceCollection (da mesma forma que era na Startup)

- A configuração dos serviços significa a adição de middlewares no pipeline, qualquer configuração do middleware feita nesse processo não poderá ser modificada conforme o comportamentos das chamadas da aplicação)

- A última linha de código precisa ser sempre a última, pois é ai que é feito o build de tudo para retornar a instância da aplicação que será configurada na sequência. Já observei muitos erros de pessoas que tentaram adicionar serviços depois do build da App, respeite sempre essa divisão de responsabilidades.

### Configuração do request/response

Na classe Startup o segundo método era o [Configure] que fazia a configuração do comportamento do request/response dos serviços na aplicação ASP.NET, e isso permanece da mesma maneira na classe Program. 

A única diferença é que a App é representada agora pela classe WebApplication e antigamente era pela interface IApplicationBuilder, mas não tem problema, pois a classe WebApplication implementa essa interface, mantendo muito código de versões anteriores compatíveis. Conheça um pouco mais da classe WebApplication [aqui](https://github.com/dotnet/aspnetcore/blob/f08285d0b6918fbb2b485d97f4e411dc9ea9a94f/src/DefaultBuilder/src/WebApplication.cs).

Seguindo na Program a partir do build da App temos o seguinte:

```csharp
// Aqui retorna uma instância de WebApplication (IApplicationBuilder)
var app = builder.Build();

// *** Configurando o resquest dos serviços no pipeline *** 

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler();
    app.UseHsts();
}

// Redirecionamento para HTTPs
app.UseHttpsRedirection();

// Uso de arquivos estáticos (ex. CSS, JS)
app.UseStaticFiles();

// Adicionando suporte a rota
app.UseRouting();

// Autenticacao e autorização (Identity)
app.UseAuthentication();
app.UseAuthorization();

// Rota padrão (no caso MVC)
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

// Mapeando componentes Razor Pages (ex: Identity)
app.MapRazorPages();

// Coloca a aplicação para rodar.
app.Run();
```

Podemos observar novamente algumas coisas importantes:

- A configuração da App via WebApplication (IApplicationBuilder) permanece igual era na classe Startup

- É possível configurar comportamentos do request/response conforme o ambiente (Dev,Prod, etc)

- A ordem das declarações AFETA TOTALMENTE o comportamento da aplicação, pois a execução do pipeline segue a ordem da declaração da Program. O erro mais comum de todos é chamar o UseAuthorization antes do UseAuthentication, a aplicação nunca vai autorizar uma Claim por ex. Muita atenção para a ordem das configurações ok?

- Como visto na configuração a última linha [app.Run] precisa ser a última por motivos óbvios :)

### Viu só como não é um bicho de 7 cabeças?

Espero ter ajudado e se estiver procurando se tornar Expert em ASP.NET adquira a nossa formação:

- [ASP.NET Core Expert](https://desenvolvedor.io/formacao/asp-net-core-expert)

Caso queira começar por algum curso individual deixo a minha recomendação abaixo.
