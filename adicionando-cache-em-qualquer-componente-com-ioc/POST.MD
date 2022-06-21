## Implemente cache através do mecanismo de DI sem precisar modificar o componente.

Iremos demonstrar como aplicar o design pattern Decorator utilizando uma estratégia para interceptar um componente, adicionar novos comportamentos e ainda assim executar o componente original.

## Performance

Se houvesse uma forma de definir performance em uma única palavra, `CACHE` seria a melhor resposta.

Há muitas formas de obter performance:

- Otimizando o código.
- Melhorando a consulta no banco.
- Criando um índice.
- Melhorando o algoritmo.
- Usando programação paralela.
- Adicionando mais hardware.

Ainda assim, quando o seu sistema se tornar grande o suficiente, atendendo milhares de milhões de requisições por minuto você será obrigado a rever a estratégia de cache da sua aplicação.

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

Veja que a `[Controller]` recebe a injeção de um `[ICarStore]`. Que por sua vez acessa o banco de dados, efetua uma pesquisa e devolve a resposta da requisição.

Uma pergunta:

## Como adicionar cache sem alterar a Controller e nem o CarStore?

Imagine que a implementação do `[ICarStore]` esteja num componente fora do seu alcance. Uma biblioteca Nuget, por exemplo, e a consulta SQL que esse componente realiza está demorando muito para responder.

## A técnica

A técnica consiste em utilizar o design pattern `[Decorator]`.

A estratégia é composta por duas etapas:

- A criação de uma classe de cache que receba a injeção de `[ICarStore]` e também implemente a mesma interface.

- O registro da classe concreta que implementa o `[ICarStore]` e a substituição do registro no DI.

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

Esse componente irá receber em `<T>` uma classe que implementa a interface `[ICarStore]` ao mesmo tempo que também implementa a mesma interface.

## Segunda etapa

Em algum lugar do código há um registro de DI da interface `[ICarStore]` e sua implementação.

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

É necessário interferir no mecanismo de DI do ASP.NET Core.

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

No primeiro passo associamos o `[ICarStore]` à sua implementação original `[CarStore]`.

Na sequência registramos a classe concreta `[CarStore]` e refazemos o registro de `[ICarStore]`, só que dessa vez, associando ao componente criado anteriormente.

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

A estratégia utilizada é muito indicada para habilitar cache sem mudar o código original, conforme demonstrado.

A aplicação só conhece uma interface e não precisa entender quando o dado virá do cache. Assim evitamos implementações do tipo:

- ICarCacheStore
- ICarStore

Além de tudo é possivel controlar quando ativar o cache de acordo com o ambiente. Utilizar cache somente no ambiente de produção e em desenvolvimento obter sempre diretamente do banco.

Outro caso de uso dessa estratégia é na implementação de Log em algum componente.

## Quem utiliza?

Além de utilizarmos essa estratégia na plataforma desenvolvedor.io, outro lugar em que ela é utilizada é no IdentityServer4. Eles a utilizam através do `[Builder]` do IdentityServer4, onde há um método para habilitar cache.

## Código Fonte

O projeto está disponível no [GitHub](https://github.com/brunohbrito/CacheStrategy)
