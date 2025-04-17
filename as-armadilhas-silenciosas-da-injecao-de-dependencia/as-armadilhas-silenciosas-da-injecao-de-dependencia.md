Dependências mal configuradas não gritam. Elas falham em silêncio. E quando você percebe, já está lidando com bugs intermitentes, memória consumida desnecessariamente, dados corrompidos ou concorrência quebrando seu sistema. Neste artigo, vamos explorar os ciclos de vida no ASP.NET Core e mostrar por que entender isso é uma habilidade essencial para qualquer desenvolvedor .NET.

## Introdução: Por que entender o ciclo de vida das dependências é essencial?

Imagine você criando um serviço que compartilha dados de uma requisição com outra sem querer. Ou então, duas threads concorrendo pelo mesmo recurso mutável. Ou uma instância de `DbContext` sendo reutilizada de forma inesperada em requests diferentes. 

Todos esses problemas têm algo em comum: o desconhecimento (ou negligência) sobre os ciclos de vida das dependências no ASP.NET Core.

E o pior? Não aparece erro. Aparece **comportamento estranho**. E nada é mais frustrante do que um bug que você não consegue reproduzir facilmente.

## Tipos de ciclo de vida no ASP.NET Core

O `IServiceCollection` permite registrar dependências com três tipos principais de ciclo de vida:

<table style="width:100%; border-collapse: collapse; color: #eee; background-color: #1a1a1a;">
  <thead>
    <tr style="background-color: #222; color: #fff;">
      <th style="border: 1px solid #444; padding: 8px; text-align: left;">Lifetime</th>
      <th style="border: 1px solid #444; padding: 8px; text-align: left;">Instância criada...</th>
      <th style="border: 1px solid #444; padding: 8px; text-align: left;">Exemplo comum</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td style="border: 1px solid #444; padding: 8px;">Transient</td>
      <td style="border: 1px solid #444; padding: 8px;">Sempre que solicitado</td>
      <td style="border: 1px solid #444; padding: 8px;">Serviços leves, stateless</td>
    </tr>
    <tr>
      <td style="border: 1px solid #444; padding: 8px;">Scoped</td>
      <td style="border: 1px solid #444; padding: 8px;">Uma vez por requisição HTTP</td>
      <td style="border: 1px solid #444; padding: 8px;">DbContext, serviço de usuário</td>
    </tr>
    <tr>
      <td style="border: 1px solid #444; padding: 8px;">Singleton</td>
      <td style="border: 1px solid #444; padding: 8px;">Uma única vez para toda a app</td>
      <td style="border: 1px solid #444; padding: 8px;">Configuração, cache, logging</td>
    </tr>
  </tbody>
</table>


### Demonstração prática com Guid:

Suponha que iremos injetar a interface `IExemploGuid` utilizando os 3 tipos de ciclo de vida e observar os resultados para cada um deles:

```csharp
public interface IExemploGuid
{
    Guid Id { get; }
}

public class ExemploGuid : IExemploGuid
{
    public Guid Id { get; } = Guid.NewGuid();
}

public class HomeController : Controller
{
    public HomeController(IExemploGuid a, IExemploGuid b)
    {
        Console.WriteLine($"A: {a.Id}");
        Console.WriteLine($"B: {b.Id}");
    }
}
```

- Se `ExemploGuid` for `AddTransient`, A e B terão IDs diferentes
- Se for `AddScoped`, A e B terão o mesmo ID
- Se for `AddSingleton`, mesmo ID para todas as requisições

## Quando usar cada um?

### Transient

- Ideal para serviços **stateless**
- Leves, descartáveis, rápidos de instanciar
- Ex: `EmailSender`, `NotificationService`

### Scoped

- Devem ser usados quando o serviço **depende do contexto da requisição**
- Ex: `DbContext`, serviços que manipulam usuário autenticado

### Singleton

- Serviços **pesados de criar** e **thread-safe**
- Ex: `ILogger<>`, `IMemoryCache`, `AppSettings`

## Erros comuns e armadilhas

### Injetar `Scoped` em `Singleton`

> Isso quebra o ciclo de vida. O singleton sobrevive à requisição, mas a dependência scoped não.
> 

Resultado: comportamento indefinido, exceções de tempo de execução ou memory leaks.

### Serviços com estado mutável

> Um singleton que guarda estado mutável é um convite ao caos concorrente.
> 

Use `ConcurrentDictionary`, `lock`, `Immutable*` ou evite serviços de estado compartilhado.

### DbContext como Singleton

> Isso vai funcionar... até que comece a dar problema em 100% dos cenários.
> 

EF Core DbContext é scoped por design. Usar como singleton pode causar corrupção de dados, pois todas requisições irão ler e modificar do mesmo contexto. Já pensou que bagunça?

## Abrindo escopos manualmente com `IServiceScopeFactory`

Serviços executados fora do contexto HTTP (ex: background services) precisam **abrir escopos manualmente**:

```csharp
public class FilaBackground : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public FilaBackground(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var meuServico = scope.ServiceProvider.GetRequiredService<IMeuServico>();
            await meuServico.ProcessarAsync();
            await Task.Delay(1000);
        }
    }
}
```

**Os Background Services no ASP.NET Core são registrados como Singleton por padrão.** 
Isso significa que eles vivem durante toda a vida útil da aplicação. Por esse motivo, **não é seguro injetar diretamente serviços com ciclo de vida Scoped** (como um `DbContext`) dentro deles, o que pode causar exceções ou comportamentos inesperados.

Para resolver isso corretamente, é necessário **abrir manualmente um escopo de serviço** usando `IServiceScopeFactory`, garantindo que as dependências Scoped sejam criadas e descartadas de forma apropriada dentro do contexto do background service.

### Cuidados:

- Evite manter referências fora do escopo criado
- Não compartilhe serviços scoped manualmente entre threads
- Monitore o consumo de memória

## Testando e validando os ciclos de vida

### Testes de unidade:

- Prefira `Transient` ao testar serviços isolados
- Use mocks configurados com `Moq`/`NSubstitute` para simular dependências com escopo

### Testes de integração:

- Use `WebApplicationFactory` para validar ciclos reais entre camadas
- Valide GUIDs, contadores ou objetos mutáveis para detectar comportamento estranho

## Conclusão

A DI no ASP.NET Core é poderosa, mas com grandes poderes... bom, você sabe…

### Checklist final:

Antes de finalizar seu sistema, revise esses pontos para garantir que a injeção de dependência está sob controle:

- [ ]  Eu entendi os três ciclos de vida e consigo explicar suas diferenças com exemplos reais?
- [ ]  Estou usando Transient apenas para serviços simples e descartáveis?
- [ ]  Scoped está sendo aplicado corretamente para serviços que dependem da requisição?
- [ ]  Singleton está reservado apenas para serviços stateless, pesados e thread-safe?
- [ ]  Estou evitando injetar Scoped em Singleton (e vice-versa)?
- [ ]  Nenhum serviço singleton está mantendo estado mutável entre requisições?
- [ ]  Background services usam `IServiceScopeFactory` corretamente, sem vazamento de escopo?
- [ ]  Meus testes (de unidade e de integração) validam comportamentos esperados entre ciclos de vida?
- [ ]  Tenho logs, métricas ou observabilidade que ajudam a identificar problemas de DI em produção?
- [ ]  Os padrões de uso de DI estão documentados e compartilhados com o time?

Entender o ciclo de vida é um **marco na maturidade de qualquer dev .NET.** 

Modularizar, testar e observar o comportamento das dependências é o que separa o código funcional do código confiável. E não tem atalhos pra isso.
