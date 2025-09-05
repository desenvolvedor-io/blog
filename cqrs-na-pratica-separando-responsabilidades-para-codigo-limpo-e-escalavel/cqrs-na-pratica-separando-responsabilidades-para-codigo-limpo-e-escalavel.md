# Contexto e motivação

Quando a base de código cresce, misturar leitura e escrita nos mesmos serviços cria classes inchadas, endpoints imprevisíveis e dificuldades de evolução. O padrão **CQRS (Command Query Responsibility Segregation)** ataca esse problema separando operações que mudam o estado (**commands**) das operações que apenas leem dados (**queries**).

Essa separação não é cosmética: ela força clareza de propósito, reduz acoplamento e facilita otimizações direcionadas.

---

# O que é CQRS — sem atalhos e sem misticismo

**Command (Comando):** representa uma **intenção única de negócio**. É um pedido explícito para mudar o estado do sistema (ex.: `DesativarFuncionario`, `AtualizarValorProduto`).

- **Escopo:** uma mudança pontual e transacional.
- **Objetivo:** aplicar regras de domínio e invariantes com consistência.
- **Resultado esperado:** lado de escrita consistente; emissão de eventos de domínio, se aplicável.

**Query (Consulta):** leitura sem efeitos colaterais.

- **Escopo:** recuperar dados de forma otimizada e específica para o caso de uso.
- **Objetivo:** entregar projeções (DTOs/view models) com o mínimo de acoplamento ao domínio.
- **Resultado esperado:** latência baixa, ausência de lógica de domínio e previsibilidade.

**Regra prática:** *Commands carregam regras de negócio; Queries carregam projeções.*

---

# Command como “intenção única de negócio”

**Definição operacional:** um Command nomeia uma ação do domínio em linguagem ubíqua, encapsula os dados necessários para uma única mudança, e delimita a transação na qual as invariantes são verificadas.

**Por que isso desacopla regras de negócio?**

Porque a regra deixa de viver em “serviços gordos” multiuso e passa a residir em **handlers** específicos, cada um refletindo uma ação do negócio. Em vez de uma classe com dezenas de métodos genéricos, você tem unidades pequenas, coesas, testáveis e alinhadas com o vocabulário do domínio.

**Propriedades recomendadas de um Command:**

- Nome imperativo alinhado ao domínio (`DesativarFuncionario`, `AtualizarValorProduto`).
- Dados mínimos para executar a intenção (evite “sacos de parâmetros”).
- Idempotência quando exposto via HTTP (chaves idempotentes ou controle de repetição no handler).
- Uma transação por comando; sucesso → efeitos aplicados; falha → nada aplicado.
- Emissão de eventos de domínio após sucesso, quando necessário.

---

# Queries são projeções: simples, rápidas e sem domínio

**Query** não carrega regra de negócio. Ela projeta dados para DTOs, usando leitura **sem tracking** e selecionando apenas colunas necessárias.

Em .NET, o **EF Core** bem ajustado resolve leitura e escrita: `AsNoTracking`, seleções projetadas, paginação, `FromSql` quando necessário, **Compiled Queries** em pontos críticos.

---

# Um ou dois bancos? CQRS permite os dois

- **Modelo 1 — um único banco:** leitura e escrita no mesmo banco. É suficiente para muita gente. Menos complexidade, zero sincronização.
- **Modelo 2 — bancos separados (escrita/leitura):** leitura em banco (ou replicação) dedicado, escrita em outro. Útil quando leitura domina ou quando há SLAs distintos.

**Sincronização:** replicação física/lógica, Eventos de Domínio, CDC (*Change Data Capture*) ou **Outbox Pattern** para propagar eventos e materializar **read models**.

**Impacto:** *eventual consistency* para consultas. É aceitável quando o negócio tolera pequeno atraso.

**Resumo honesto:** geralmente não é necessário separar bancos no início; mas o modelo permite essa evolução sem reescrever a aplicação, se (e quando) for preciso.

---

# Exemplo completo e direto (C# / .NET / EF Core)

## Domínio (entidades com invariantes + controle de concorrência otimista)

```csharp
public class Produto
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public decimal Preco { get; private set; }
    public bool Ativo { get; private set; } = true;
    public byte[] VersaoLinha { get; private set; } = Array.Empty<byte>();

    private Produto() { } // EF Core

    public Produto(string nome, decimal preco)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome inválido");
        if (preco < 0) throw new ArgumentException("Preço negativo não permitido");

        Nome = nome.Trim();
        Preco = preco;
    }

    public void AjustarPreco(decimal novoPreco)
    {
        if (novoPreco < 0) throw new InvalidOperationException("Preço negativo não permitido");
        Preco = novoPreco;
    }

    public void Desativar()
    {
        if (!Ativo) return;
        Ativo = false;
    }
}
```

---

## Command: Ajustar preço (intenção única, uma transação)

PS - Usando de EF Core “Puro” para explicar um comportamento interno, é possível isolar em um repositório.

```csharp
public record AjustarPrecoProdutoCommand(int ProdutoId, decimal NovoPreco, byte[] VersaoLinha)
    : IRequest<ResultadoComando>;

public record ResultadoComando(bool Sucesso, string? Erro);

public class AjustarPrecoProdutoHandler
    : IRequestHandler<AjustarPrecoProdutoCommand, ResultadoComando>
{
    private readonly AppDbContext _db;

    public AjustarPrecoProdutoHandler(AppDbContext db) => _db = db;

    public async Task<ResultadoComando> Handle(
        AjustarPrecoProdutoCommand request,
        CancellationToken ct)
    {
        var produto = await _db.Produtos
            .FirstOrDefaultAsync(p => p.Id == request.ProdutoId, ct);

        if (produto is null)
            return new(false, "Produto não encontrado");

        _db.Entry(produto).Property(nameof(Produto.VersaoLinha)).OriginalValue = request.VersaoLinha;

        try
        {
            produto.AjustarPreco(request.NovoPreco);

            await _db.SaveChangesAsync(ct);

            return new(true, null);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new(false, "Concorrência: versão desatualizada");
        }
        catch (Exception ex)
        {
            return new(false, ex.Message);
        }
    }
}
```

---

## Command: Desativar funcionário (outro exemplo de intenção única)

```csharp
public record DesativarFuncionarioCommand(int FuncionarioId, byte[] VersaoLinha)
    : IRequest<ResultadoComando>;

public class DesativarFuncionarioHandler
    : IRequestHandler<DesativarFuncionarioCommand, ResultadoComando>
{
    private readonly AppDbContext _db;
    public DesativarFuncionarioHandler(AppDbContext db) => _db = db;

    public async Task<ResultadoComando> Handle(
        DesativarFuncionarioCommand request,
        CancellationToken ct)
    {
        var funcionario = await _db.Funcionarios.FirstOrDefaultAsync(e => e.Id == request.FuncionarioId, ct);
        if (funcionario is null) return new(false, "Funcionário não encontrado");

        _db.Entry(funcionario).Property("VersaoLinha").OriginalValue = request.VersaoLinha;

        try
        {
            funcionario.Desativar();
            await _db.SaveChangesAsync(ct);
            return new(true, null);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new(false, "Concorrência: versão desatualizada");
        }
    }
}
```

---

## 6.4 Query: projeção direta e sem tracking

```csharp
public record ProdutoDto(int Id, string Nome, decimal Preco, bool Ativo, byte[] VersaoLinha);

public record BuscarProdutoPorIdQuery(int Id) : IRequest<ProdutoDto?>;

public class BuscarProdutoPorIdHandler
    : IRequestHandler<BuscarProdutoPorIdQuery, ProdutoDto?>
{
    private readonly AppDbContext _db;
    public BuscarProdutoPorIdHandler(AppDbContext db) => _db = db;

    public async Task<ProdutoDto?> Handle(BuscarProdutoPorIdQuery request, CancellationToken ct)
    {
        return await _db.Produtos
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new ProdutoDto(p.Id, p.Nome, p.Preco, p.Ativo, p.VersaoLinha))
            .FirstOrDefaultAsync(ct);
    }
}
```

---

### Pontos de performance com EF Core na leitura:

- `AsNoTracking()`.
- Projeção seletiva (`Select`) direto para DTO.
- Paginação sempre que necessário.
- **Compiled queries** para *hot paths*:

```csharp
public static class ProdutoQueries
{
    public static readonly Func<AppDbContext, int, CancellationToken, Task<ProdutoDto?>>
        BuscarProdutoPorIdCompilado = EF.CompileAsyncQuery(
            (AppDbContext ctx, int id, CancellationToken ct) =>
                ctx.Produtos.AsNoTracking()
                   .Where(p => p.Id == id)
                   .Select(p => new ProdutoDto(p.Id, p.Nome, p.Preco, p.Ativo, p.VersaoLinha))
                   .FirstOrDefault());
}
```

**Uso:**

```csharp
var dto = await ProdutoQueries.BuscarProdutoPorIdCompilado(_db, request.Id, ct);
```

---

# Pipeline de aplicação: validação, transação e observabilidade

Com **MediatR** (ou pipeline próprio), trate *cross-cutting concerns* sem poluir handlers:

- **Validação** (FluentValidation) antes do handler.
- **Transação/Unit of Work** por comando.
- **Logging/Tracing/Metrics** por handler.
- **Autorização** baseada em políticas.

---

# Lado de leitura com banco separado (opcional)

Quando (e somente quando) a demanda exigir, mova o lado de leitura para outro banco.

**Opções:**

- Replicação (física/lógica).
- Eventos de Domínio
- CDC + *view models materializados*.
- **Outbox Pattern**.

Exemplo simplificado de Outbox:

```csharp
public class MensagemOutbox
{
    public long Id { get; set; }
    public DateTime OcorreuEmUtc { get; set; }
    public string Tipo { get; set; } = null!;
    public string ConteudoJson { get; set; } = null!;
    public bool Processada { get; set; }
}
```

---

# Contratos de API e idempotência

- **Commands via HTTP:**
    - `POST /produtos/{id}/preco` com `Idempotency-Key`.
    - `409 Conflict` em falha de concorrência otimista.
    - `ETag`/`If-Match` nas queries para enviar `VersaoLinha`.
- **Queries via HTTP:**
    - Sempre paginadas.
    - Retornar **DTOs**, não entidades.

---

# Anti-padrões comuns

- Command “faz-tudo”.
- Regra de negócio em Query.
- Retornar entidade em Query.
- Misturar transações.
- Ignorar concorrência.
- Separar bancos sem necessidade.
- Services anêmicos inchados.

---

# Migração incremental para CQRS

1. Separe handlers de command/query no código.
2. Traga invariantes para o domínio.
3. Ajuste queries (`AsNoTracking`, projeções).
4. Introduza concorrência otimista (`RowVersion`).
5. Monitore métricas por handler.
6. Só depois considere banco separado / outbox.

---

# Checklist rápido de adoção

- Commands nomeados por intenção única de negócio.
- 1 command = 1 transação.
- Queries sem efeitos colaterais.
- Concorrência otimista (RowVersion/ETag).
- Observabilidade por handler.
- Nada de services gordos.
- Sem banco separado sem demanda.
- Outbox/CDC se precisar escalar leitura.

---

# Representação
<img width="785" height="480" alt="image" src="https://i0.wp.com/www.eduardopires.net.br/wp-content/uploads/2016/07/CQRS_BUS.jpg?resize=785%2C480&ssl=1" />


- Command Stack ⇒ Processamento de regrasde negócios, processos assíncronos
- Query Stack ⇒ Leituras e projeções (não necessariamente assíncrono)
- Fila ⇒ Fila física (RabbitMQ por ex) ou um mecanismo de Mediator
- Evento ⇒ Produto da execução do Command (se necessário para integrações)
- Componente ⇒ Classe responsável por delegar um evento para processamento (ex: atualizar base de leitura)
- Dapper ⇒ Opcional, EF Core faz o mesmo papel com performance similar.

---

# Conclusão

CQRS tem dois pilares:

- **Commands** representam intenções únicas e isolam regras de negócio.
- **Queries** entregam projeções rápidas e desacopladas.

Separar bancos é opcional. O ganho é clareza, testabilidade e evolução previsível.

Se sua aplicação sofre com serviços inchados, é hora de nomear intenções em commands e tratar queries como projeções.
