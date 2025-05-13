## A velha armadilha: fazer tudo dentro da requisição HTTP

Imagine a cena: o cliente clicou em "Finalizar Pedido". O controller é acionado, os dados do carrinho são processados, a transação é registrada no banco. Tudo certo. Mas agora você precisa:

- Enviar um e-mail de confirmação
- Atualizar o estoque em um sistema externo
- Notificar um ERP

E você faz tudo isso **dentro do controller**, antes de dar o `return Ok()`.

No ambiente de desenvolvimento, com uma base pequena e nenhum concorrente acessando, tudo parece bem. A resposta demora 3, talvez 5 segundos. "Tranquilo".

Agora coloca isso em produção. Com 100, 500, 1000 usuários simultâneos. Cada requisição ocupando uma thread, esperando um e-mail sair, ou uma API lenta responder. Em pouco tempo:

- Seu pool de conexões é saturado
- O Kestrel começa a engasgar
- O Server começa a matar requisições por timeout

E a aplicação que "rodava suave" agora trava em hora de pico.

## Por que isso acontece? A arquitetura não separa responsabilidades

No modelo tradicional, a aplicação tenta resolver tudo no mesmo lugar. Uma requisição HTTP é tratada como um "workflow completo", e não como um disparador de eventos.

Resultado?

- Toda lógica, mesmo a que **não precisa ser imediata**, é executada dentro do request.
- Lógicas que **podem falhar sem impactar o usuário** causam `500 Internal Server Error`
- **Escalabilidade vai por água abaixo**: você precisa escalar a API para aguentar carga que deveria ser absorvida por outro componente

E o pior: tudo isso parece normal até dar problema.

## A solução: tratar essas tarefas como serviços de segundo plano

Tarefas como envio de e-mails, chamadas para sistemas externos, sincronizações, geração de relatórios, são **naturalmente assíncronas**. Elas:

- Não precisam acontecer dentro do request
- Não precisam travar a experiência do usuário
- Não devem impactar a disponibilidade da API

A boa notícia? O .NET tem uma solução oficial, estável, elegante e **extensível** para isso: 
**Hosted Services**, em especial os **Background Services**.

---

## Hosted Services vs Background Services: Qual a Diferença?

Esses dois conceitos são frequentemente confundidos, mas possuem distinções claras:

| Aspecto | `IHostedService` | `BackgroundService` |
| --- | --- | --- |
| Interface base | Sim | Sim, por herança |
| Classe abstrata | Não | Sim (`BackgroundService`) |
| Abstração do loop | Manual (precisa iniciar a `Task`) | Pronta (`ExecuteAsync`) |
| Complexidade | Maior controle, mais verboso | Abstração simplificada |
| Tratamento do ciclo de vida | Você precisa lidar com start, stop e dispose | Encapsulado |
| Cancelamento | Você cria e gerencia o CancellationTokenSource | Fornecido automaticamente |
| Recomendação de uso | Ideal para orquestrações, múltiplas tarefas, timers customizados, controle fino | Ideal para loops contínuos com lógica única, filas |

**O `BackgroundService` é, na prática, uma implementação de `IHostedService` com estrutura pré-definida.**

> Se você quer looping com controle total, vá de IHostedService.
Se quer algo mais direto ao ponto com menos código, o BackgroundService vai te servir bem.
> 

---

## Como Funciona um Hosted Service?

Quando sua aplicação .NET inicia (`Host.StartAsync()`), todos os serviços que implementam `IHostedService` são iniciados. A interface define dois métodos principais:

```csharp
public interface IHostedService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
```

Esses métodos são invocados pelo runtime nos momentos certos do ciclo de vida da aplicação.

Já a classe `BackgroundService` implementa esses métodos e adiciona o método abstrato `ExecuteAsync`, onde o loop de execução contínuo deve ser implementado:

```csharp
protected abstract Task ExecuteAsync(CancellationToken stoppingToken);
```

---

## O Que Você Precisa Saber Antes de Usar

1. **Thread-safe:** Como são tarefas assíncronas e concorrentes, seus serviços devem ser *thread-safe*.
2. **Controlados por injeção de dependência (DI):** Eles são registrados via `services.AddHostedService<T>()`.
3. **Escopo de serviços:** Você não pode injetar `scoped` services diretamente — é necessário criar escopos via `IServiceScopeFactory`.
4. **Observabilidade:** Eles não são visíveis como APIs ou endpoints. Monitore logs, exceptions e status via ferramentas como Application Insights, Serilog, etc.
5. **Uso de `CancellationToken`:** Nunca ignore. É ele que permitirá uma parada limpa no shutdown da aplicação.
6. **Reinício automático:** Serviços que lançam exceções são finalizados. Use `try-catch` robusto e, se necessário, implemente política de *retry* ou backoff.

---

## #1 - Exemplo do Mundo Real: Processamento de Pedidos Pendentes

Imagine que você precise processar um pedido pendente, por exemplo realizar alguma integração ou simplesmente enviar um e-mail. Um `BackgroundService` irá consumir esses pedidos e processá-los assincronamente.

### Criando o BackgroundService

```csharp
public class NotificacaoPedidosPendentesService : BackgroundService
{
    private readonly ILogger<NotificacaoPedidosPendentesService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public NotificacaoPedidosPendentesService(
        ILogger<NotificacaoPedidosPendentesService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Serviço de notificação de pedidos pendentes iniciado.");

        // Roda até que o serviço seja cancelado
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Cria um escopo para resolver os serviços necessários em Singleton (Necessidade do BackgroundService)
                using var scope = _scopeFactory.CreateScope();
                var pedidoService = scope.ServiceProvider.GetRequiredService<IPedidoService>();

                // Obtém os pedidos pendentes
                var pendentes = await pedidoService.ObterPedidosPendentesAsync();

                // Envia notificações para os pedidos pendentes
                foreach (var pedido in pendentes)
                {
                    await pedidoService.EnviarNotificacaoAsync(pedido);
                    _logger.LogInformation("Notificação enviada para pedido #{PedidoId}", pedido.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar notificações");
            }

            // Aguarda 30 segundos antes de verificar novamente
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        _logger.LogInformation("Serviço de notificação encerrado.");
    }
}
```

### Registrando o serviço

No `Program.cs`:

```csharp
builder.Services.AddHostedService<NotificacaoPedidosPendentesService>();
```

---

## #2 - Exemplo do Mundo Real: Processamento de Multiplas Tarefas

Imagine que você precise processar mais de uma tarefa, porém em intervalos diferentes. Utilizar `IHostedService` irá lhe oferecer muito mais controle.

### Criando o IHostedService

```csharp
public class MultiTarefaHostedService : IHostedService, IDisposable
{
    private readonly ILogger<MultiTarefaHostedService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private CancellationTokenSource _cts;
    private Task _verificacaoPedidosTask;
    private Task _sincronizacaoEstoqueTask;

    public MultiTarefaHostedService(
        ILogger<MultiTarefaHostedService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MultiTarefaHostedService iniciado.");
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _verificacaoPedidosTask = Task.Run(() => VerificarPedidosAsync(_cts.Token));
        _sincronizacaoEstoqueTask = Task.Run(() => SincronizarEstoqueAsync(_cts.Token));

        return Task.CompletedTask;
    }

    private async Task VerificarPedidosAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var pedidoService = scope.ServiceProvider.GetRequiredService<IPedidoService>();

                var pendentes = await pedidoService.ObterPedidosPendentesAsync();

                foreach (var pedido in pendentes)
                {
                    await pedidoService.EnviarNotificacaoAsync(pedido);
                    _logger.LogInformation("Notificado pedido #{PedidoId}", pedido.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na verificação de pedidos");
            }

            // Aguarda 30 segundos antes de verificar novamente
            await Task.Delay(TimeSpan.FromSeconds(30), token);
        }
    }

    private async Task SincronizarEstoqueAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var estoqueService = scope.ServiceProvider.GetRequiredService<IEstoqueService>();

                await estoqueService.SincronizarComERPAsync();
                _logger.LogInformation("Estoque sincronizado com ERP.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na sincronização de estoque");
            }

            // Aguarda 5 minutos antes de sincronizar novamente
            await Task.Delay(TimeSpan.FromMinutes(5), token);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parando MultiTarefaHostedService...");
        _cts.Cancel();

        // Aguarda a conclusão de todas as tarefas
        await Task.WhenAll(_verificacaoPedidosTask, _sincronizacaoEstoqueTask);
    }

    public void Dispose()
    {
        // Libera os recursos
        _cts?.Dispose();
    }
}
```

### Registrando o serviço

No `Program.cs`:

```csharp
builder.Services.AddHostedService<MultiTarefaHostedService>();
```

---

## Dependências Scoped vs Singleton

Os `BackgroundServices` são sempre executados como `Singleton`, portanto evite injetar diretamente repositórios ou serviços `scoped` em `BackgroundService`. Sempre use `IServiceScopeFactory` para obter a instância dentro do escopo correto e evitar erros como:

> "Cannot consume scoped service from singleton"
> 

---

## Vantagens reais de usar Background Services

### 1. Libera o pipeline HTTP

O controller responde rápido. O cliente segue a experiência. A tarefa pesada roda depois.

### 2. Escalabilidade sob controle

Sua aplicação não precisa escalar para tarefas que **nem fazem parte do request**.

### 3. Maior resiliência

Um erro na tarefa em background não afeta o request do usuário. Pode ser logado, tratado, reexecutado com retry.

### 4. Observabilidade

Hosted Services podem ser monitorados com health checks, logs, métricas. Tudo do jeito certo.

---

## Quando NÃO Usar

- Quando a tarefa depende diretamente da resposta ao usuário. Use `Task` ou `Controller` diretamente.
- Para orquestrações complexas e com alta disponibilidade — prefira Kafka, RabbitMQ ou implemente um bom controle de Sagas.
- Quando múltiplas instâncias paralelas da aplicação estiverem rodando (e o serviço não for *idempotente*).

---

## Conclusão

Tarefas de “background” não pertencem a controller.

Toda vez que você coloca um `await` dentro de uma controller que **não afeta a resposta direta ao usuário**, você está pagando um custo de performance, escalabilidade e manutenção.

`BackgroundService` e `IHostedService` existem para te dar um caminho limpo, confiável e estruturado para resolver esse problema.

Quer construir sistemas modernos, escaláveis e resilientes? Aprenda a tirar lógicas assíncronas do caminho do usuário. E Hosted Services são um dos melhores começos para isso.

A pergunta que você deveria se fazer não é mais *"será que eu preciso de um BackgroundService?"*, mas sim:

> "Quantas partes da minha lógica assíncrona ainda estão presas no pipeline HTTP, esticando requisições e comprometendo a escalabilidade da aplicação?"
>
