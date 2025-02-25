Erros inesperados devem ser tratados de maneira adequada. Embora seja recomendável minimizar sua ocorrência sempre que possível, eles inevitavelmente surgirão em sua aplicação. Por isso, é fundamental contar com uma estratégia eficiente para lidar com essas situações.

Você pode optar por configurar um tratamento global de exceções ou tratar erros específicos de forma individualizada. O ASP.NET Core disponibiliza diferentes métodos para essa finalidade. Mas qual abordagem seguir?

Neste artigo, vamos analisar tanto a estratégia convencional quanto a nova funcionalidade implementada no ASP.NET Core 8 para gerenciar exceções.

## Abordagem Tradicional: Middleware de Tratamento de Exceções

A forma mais comum de tratar exceções no ASP.NET Core é por meio de middleware. Middleware permite adicionar lógica antes ou depois da execução de uma requisição HTTP. Assim, podemos estender esse conceito para interceptar exceções e retornar respostas de erro apropriadas.

A abordagem baseada em convenção exige a definição de um método `InvokeAsync`. Veja um exemplo de `ExceptionHandlingMiddleware`:

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Ocorreu uma exceção: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro no Servidor"
            };

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
```

Este middleware captura qualquer exceção não tratada e retorna uma resposta no formato `ProblemDetails`. A quantidade de detalhes retornados pode ser ajustada conforme necessário.

Para utilizar esse middleware, basta adicioná-lo ao pipeline de requisição do ASP.NET Core:

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

## Nova Abordagem: `IExceptionHandler`

O ASP.NET Core 8 introduziu uma nova abstração para gerenciamento de exceções: a interface `IExceptionHandler`. O middleware de tratamento de exceções embutido utiliza implementações de `IExceptionHandler` para capturar e processar erros.

Essa interface possui um único método: `TryHandleAsync`. Ele tenta lidar com uma exceção específica dentro do pipeline do ASP.NET Core. Se a exceção puder ser tratada, o método deve retornar `true`; caso contrário, `false`. Isso permite implementar lógica personalizada para diferentes cenários.

Veja um exemplo de `GlobalExceptionHandler`:

```csharp
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ocorreu uma exceção: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Erro no Servidor"
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
```

### Configurando implementações do tipo `IExceptionHandler`

Para adicionar uma implementação de `IExceptionHandler` ao pipeline do ASP.NET Core, siga dois passos:

1. Registre o serviço `IExceptionHandler` na injeção de dependência:

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
```

1. Adicione o middleware `ExceptionHandlerMiddleware` ao pipeline de requisição:

```csharp
app.UseExceptionHandler();
```

## Encadeamento de Handlers de Exceção

O ASP.NET Core 8 permite adicionar múltiplas implementações de `IExceptionHandler`, sendo chamadas na ordem em que são registradas. Isso pode ser útil para cenários de controle de fluxo, como diferenciar exceções específicas.

Por exemplo, considere as classes `BadRequestExceptionHandler` e `NotFoundExceptionHandler`:

### `BadRequestExceptionHandler`

```csharp
internal sealed class BadRequestExceptionHandler : IExceptionHandler
{
    private readonly ILogger<BadRequestExceptionHandler> _logger;

    public BadRequestExceptionHandler(ILogger<BadRequestExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BadRequestException badRequestException)
        {
            return false;
        }

        _logger.LogError(badRequestException, "Ocorreu uma exceção: {Message}", badRequestException.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Requisição Inválida",
            Detail = badRequestException.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
```

### `NotFoundExceptionHandler`

```csharp
internal sealed class NotFoundExceptionHandler : IExceptionHandler
{
    private readonly ILogger<NotFoundExceptionHandler> _logger;

    public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException notFoundException)
        {
            return false;
        }

        _logger.LogError(notFoundException, "Ocorreu uma exceção: {Message}", notFoundException.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Não Encontrado",
            Detail = notFoundException.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
```

Para registrar ambos os handlers:

```csharp
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
```

O `BadRequestExceptionHandler` será executado primeiro e irá tentar manipular a exception, se a exception não for manipulada o `NotFoundExceptionHandler` será executado na sequência e fará a tentativa de manipulação. Esteja sempre atento a ordem de registro dos manipuladores.

## Conclusão

O uso de middleware para tratamento de exceções continua sendo uma ótima solução no ASP.NET Core. No entanto, com o ASP.NET Core 8, a interface `IExceptionHandler` oferece uma abordagem mais modular e flexível. Para novos projetos, essa nova abordagem é altamente recomendada.

Uma observação importante é que exceptions são muito caras, existem outras maneiras de tratar alguns tipos de erros e evitar lançar exceptions por qualquer motivo. **Exceptions devem ser utilizadas como último recurso**.

Espero que tenha gostado e continue nos acompanhando!
