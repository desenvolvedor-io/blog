# ASP.NET Core - Adicionando Cache em qualquer componente com Injeção de Dependência

IoC é a melhor estratégia para desacoplar um sistema. Facilita a criação de testes, separa as dependências e isola o domínio. No entanto existe o pattern DECORATOR, bem pouco usado, que permite interceptar um componente e adicionar novos comportamentos, e ainda assim executar o componente original.

## Performance 

Se houvesse uma forma de definir performance em uma única palavra, Cache seria a resposta.

Há muitas formas de obter performance. Otimizando o código. Melhorando a consulta no banco. Criando um índice. Melhorando o algoritmo. Usando programação paralela. Adicionando mais hardware. Ainda assim, quando o seu sistema se tornar grande o suficiente, atendendo milhares de milhões de requisições por minuto, você será obrigado a rever a estratégia de cache da sua aplicação.


# O problema

Considere o seguinte código:

```csharp
[ApiController, Route("cars")]
public class CarController : ControllerBase
{
    private readonly ICarStore _store;
    public CarController(ICarStore store)
    {
        _store = store;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_store.List());
    }
}
```

Veja que a `Controller` espera um `ICarStore`. Que por sua vez acessa o banco de dados. Efetua uma pesquisa e devolve a resposta da requisição.

Uma pergunta:

## Como adicionar Cache sem alterar a Controller e nem o CarStore?

Imagine que a implementação do `ICarStore` esteja num componente fora do seu alcance. Uma biblioteca do Nuget, por exemplo. E esse componente está demorando muito para responder.

## A técnica

A técnica consiste em utilizar o IoC. A estratégia é composta por duas etapas. 

A primeira é criar uma classe de cache que receba o `ICarStore` e também faça a implementação da interface.

A segunda é registrar a Classe concreta que implementa o `ICarStore` e substituir o registro dele no DI.

## Primeira etapa

Veja a implementação abaixo

```csharp
public class CarCachingStore<T> : ICarStore
    where T : ICarStore
{
    private readonly IMemoryCache _memoryCache;
    private readonly T _inner;
    private readonly ILogger<CarCachingStore<T>> _logger;

    public CarCachingStore(IMemoryCache memoryCache, T inner, ILogger<CarCachingStore<T>> logger)
    {
        _memoryCache = memoryCache;
        _inner = inner;
        _logger = logger;
    }

    public IEnumerable<Car> List();
    public Car Get(int id);
}
```

Esse componente vai receber uma classe Concreta que implementa `ICarStore` e ao mesmo tempo, ela implementa essa interface.

## Segunda etapa

Em algum lugar do código há um registro da Interface com a implementação dela.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    services.AddScoped<ICarStore, CarStore>();
}

public interface ICarStore
{
    IEnumerable<Car> List();
    Car Get(int id);
}
```

É necessário interferir no DI do ASP.NET Core.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    RegisterServices(services);
    EnableCache(services);

}

private static void RegisterServices(IServiceCollection services)
{
    services.AddScoped<ICarStore, CarStore>();
}

private void EnableCache(IServiceCollection services)
{
    services.AddScoped<CarStore>();
    services.AddScoped<ICarStore, CarCachingStore<CarStore>>();
}
```

Primeiro o ASP.NET Core associa o `ICarStore` à sua implementação original, `CarStore`.

Em seguida o código registra a classe Concreta `CarStore` e refaz o registro de `ICarStore`, só que dessa vez, associando ao componente criado anteriormente.

Lembre-se que esse é um demo. Geralmente existe uma lógica para habilitar ou não essa substituição.

Abaixo o código do `CarCachingStore` completo

```csharp
public class CarCachingStore<T> : ICarStore
    where T : ICarStore
{
    private readonly IMemoryCache _memoryCache;
    private readonly T _inner;
    private readonly ILogger<CarCachingStore<T>> _logger;

    public CarCachingStore(IMemoryCache memoryCache, T inner, ILogger<CarCachingStore<T>> logger)
    {
        _memoryCache = memoryCache;
        _inner = inner;
        _logger = logger;
    }

    public IEnumerable<Car> List()
    {
        var key = "Cars";
        var item = _memoryCache.Get<IEnumerable<Car>>(key);

        if (item == null)
        {
            item = _inner.List();
            if (item != null)
            {
                _memoryCache.Set(key, item, TimeSpan.FromMinutes(1));
            }
        }

        return item;
    }

    public Car Get(int id)
    {
        var key = GetKey(id.ToString());
        var item = _memoryCache.Get<Car>(key);

        if (item == null)
        {
            _logger.LogTrace("Cache miss for {cacheKey}", key);
            item = _inner.Get(id);
            if (item != null)
            {
                _logger.LogTrace("Setting item in cache for {cacheKey}", key);
                _memoryCache.Set(key, item, TimeSpan.FromMinutes(1));
            }
        }
        else
        {
            _logger.LogTrace("Cache hit for {cacheKey}", key);
        }

        return item;
    }


    private static string GetKey(string key)
    {
        return $"{typeof(T).FullName}:{key}";
    }
}
```

## Casos de Uso

Um bom uso dessa técnica é com Cache. A aplicação só conhece uma interface. Ela não precisa entender quando o dado virá do Cache. Assim evita implementações como:

* ICarCacheStore
* ICarStore

E é possivel controlar quando ativar o Cache de acordo com o ambiente. Utilizar Cache somente no ambiente de produção e em desenvolvimento manter os dados sempre frescos.

Você pode adicionar Log em componentes.

## Quem utiliza?

Além de usarmos ele aqui na desenvolvedor.io, outro lugar que ele é utilizado é no IdentityServer4. Eles utilizam através do Builder do IdentityServer4, onde há um método para habilitar cache.

## Download

<img src="https://github.com/ashleymcnamara/artwork/blob/master/clippy_octocat.png?raw=true" width="250" class="center" />

O código do projeto está disponível no [GitHub](https://github.com/brunohbrito/CacheStrategy)
