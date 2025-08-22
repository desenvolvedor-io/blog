Se você é desenvolvedor, certamente já teve que debugar um comportamento “estranho” no sistema e pensou: *"mas esse código funcionava ontem..."*.

Esse tipo de instabilidade geralmente está associado a um vilão silencioso: **efeitos colaterais**.

E uma das armas mais poderosas contra ele é um conceito antigo, porém subestimado: **funções puras**.

Neste artigo, vamos mergulhar fundo no que são funções puras, por que você deveria usá-las com mais frequência e, principalmente, como escrever suas próprias funções puras no mundo real, inclusive dentro de arquiteturas mais complexas como ASP.NET, APIs e microsserviços.

---

## 📚 O que é uma função pura?

Uma **função pura** é uma função que obedece a duas regras:

1. **Mesmas entradas geram mesmas saídas**
2. **Não possui efeitos colaterais observáveis**

Parece simples? É. Mas isso carrega implicações profundas.

### Vamos ver um exemplo de função pura em C#:

```csharp
public int Soma(int a, int b)
{
    return a + b;
}
```

- Sempre que você passar `2` e `3`, ela devolverá `5`.
- Ela não lê banco de dados, não acessa variáveis globais, não escreve em disco, nem altera nada fora dela mesma.

Agora compare com esta função **impura**:

```csharp
private int _contador = 0;

public int SomaComContador(int a, int b)
{
    _contador++;
    return a + b;
}
```

Essa função tem **efeito colateral**: ela altera o estado de uma variável global. Logo, **ela é imprevisível e difícil de testar isoladamente**.

---

## 🔍 Por que funções puras importam tanto?

A resposta está em três palavras que deveriam guiar toda decisão técnica:

> Previsibilidade, testabilidade e manutenibilidade.
> 

### ✅ Previsibilidade

Funções puras sempre retornam o mesmo resultado para os mesmos inputs. Isso as torna **determinísticas**.

E determinismo é sinônimo de confiança no comportamento do sistema.

### ✅ Testabilidade

Não é à toa que bibliotecas puramente funcionais têm altíssimos índices de cobertura de testes com poucos mocks.

Você testa a função passando entradas e verifica saídas. Simples assim.

```csharp
[Fact]
public void Soma_DeveRetornarResultadoCorreto()
{
    var resultado = Soma(2, 3);
    Assert.Equal(5, resultado);
}
```

### ✅ Manutenibilidade

Se a função não interage com nada externo, você pode refatorar com tranquilidade, sabendo que **não vai quebrar outra parte do sistema sem querer**.

---

## 🧪 Como escrever funções puras na prática

Nem toda função precisa ser pura, mas sempre que possível, **você deveria extrair a parte pura de uma função impura**.

### Exemplo prático: Validação de CPF

Vamos supor que você esteja implementando a regra de validação de CPF. Você poderia cair na tentação de usar `Console.WriteLine`, `DateTime.Now` ou acessar o banco para verificar duplicidade. Evite!

Mantenha a lógica principal separada:

```csharp
public bool ValidarCpf(string cpf)
{
    var numeros = cpf.Where(char.IsDigit).ToArray();

    if (numeros.Length != 11)
        return false;

    // Lógica de verificação de dígitos
    // ...

    return true;
}
```

Essa função é 100% testável, previsível e não depende de contexto externo.

Se quiser usar logs, persistência ou métricas, faça isso **fora da função**.

---

### 🧱 Funções puras em classes estáticas: devo usar?

Em C#, é comum organizar funções puras dentro de **classes estáticas**. Isso levanta uma dúvida recorrente:

> "Funções puras precisam estar em classes estáticas?"
> 

A resposta curta é: **não precisam, mas pode ser vantajoso.**

---

### ✔️ Quando usar classes estáticas?

Use classes estáticas quando:

- As funções não precisam de estado interno (e não precisarão no futuro)
- A função representa uma operação utilitária genérica
- Você quer evitar instanciamento e simplificar o uso (ex: `ValidadorCpf.Validar(...)`)

```csharp
public static class ValidadorCpf
{
    public static bool Validar(string cpf)
    {
        // lógica de validação
    }
}
```

Esse padrão é **simples, direto, e elimina ambiguidade**: o leitor sabe que não existe estado interno.

---

### ⚠️ Quando evitar classes estáticas?

Evite classes estáticas se:

- A lógica pode evoluir para precisar de dependências (como serviços externos)
- Você quer aplicar injeção de dependência (IoC)
- Vai precisar fazer mocking para testes
- Quer manter coesão com outras funcionalidades de um módulo

Nesse caso, prefira usar **classes com instância**, e mantenha a pureza apenas nos métodos:

```csharp
public class CalculadoraFinanceira
{
    public decimal CalcularTotal(decimal valor, decimal desconto)
    {
        return valor - desconto;
    }
}
```

Você ainda pode manter a função pura, mas agora ela está embutida num objeto, o que **facilita extensões futuras**, como logs, telemetria, cache, etc.

---

### 🎯 Dica de ouro

> Comece com uma classe estática se a lógica for simples, autocontida e improvável de crescer.
> 
> 
> Prefira instâncias **quando a complexidade ou acoplamento com infraestrutura for inevitável**.
> 

---

## 🎯 Quando funções puras brilham

- **Serviços de domínio (Domain Services)** em DDD
- **Regras de negócio puras**, como cálculos financeiros
- **Validações**
- **Conversões e transformações de dados**
- **Funções auxiliares em pipelines de dados**
- **MapReduce, LINQ e operações sobre coleções**

---

## 🧩 Efeitos colaterais: o vilão silencioso

Efeito colateral é tudo aquilo que **altera o estado do mundo externo**, como:

- Escrever no console, arquivo ou banco de dados
- Modificar variáveis globais ou campos de instância
- Fazer chamadas HTTP
- Depender de `DateTime.Now` ou `Guid.NewGuid()`

É claro que aplicações reais precisam disso, mas você deve **isolar** esses efeitos em partes pequenas, enquanto mantém o core da lógica **pura**.

---

## 🧠 Analogias do cotidiano

### 1. Receita de bolo (pura) x fazer o bolo (impura)

Uma **receita de bolo** sempre vai listar os mesmos ingredientes e passos. Isso é como uma função pura: entrada → processamento → saída.

Mas fazer o bolo na cozinha envolve:

- Abrir o forno (efeito colateral)
- Ligar o timer (efeito colateral)
- Mexer a massa (estado compartilhado)

Separar a lógica pura da execução prática é como separar a receita da cozinha real.

### 2. Calculadora x impressora fiscal

Uma **calculadora** é uma função pura: sempre responde a mesma coisa.

Uma **impressora fiscal**, além de calcular, gera logs, códigos, serializações, arquivos. Isso envolve diversos efeitos colaterais.

Quando você mistura os dois, o código vira um caos.

---

## 🤯 Provocações

- Quantas funções no seu projeto **dependem implicitamente do estado global**?
- Você já teve que mockar o `DateTime.Now` ou o `HttpContext` para testar alguma coisa?
- Alguma vez um bug foi causado por uma função que funcionava “às vezes”?

Talvez esteja na hora de começar a extrair funções puras do seu código.

---

## 🛠️ Transformando funções impuras em puras

Imagine esse método típico de serviço:

```csharp
public decimal CalcularTotalPedido(Guid pedidoId)
{
    var pedido = _repository.ObterPedidoPorId(pedidoId);
    var desconto = _descontoService.Calcular(pedido);
    return pedido.Valor - desconto;
}
```

Aqui temos várias dependências externas: repositório e outro serviço.

Agora veja como poderíamos extrair a lógica pura:

```csharp
public decimal CalcularTotal(Pedido pedido, decimal desconto)
{
    return pedido.Valor - desconto;
}
```

Agora a função `CalcularTotal` pode ser testada **sem banco, sem dependências, sem mock**.

---

## 🚀 Conclusão: seja estratégico

Nem toda função precisa ser pura. Mas...

> Toda lógica de negócio que puder ser escrita como função pura, deveria ser.
> 

Isso não é apenas um capricho funcional. É uma estratégia de arquitetura.

Se você quer um código mais fácil de testar, refatorar, evoluir, e que cause menos bugs imprevisíveis, **funções puras são um superpoder que você precisa usar mais**.

---

## 🔚 TL;DR

- Funções puras são previsíveis, testáveis e fáceis de manter
- Devem evitar efeitos colaterais e retornar sempre o mesmo resultado para os mesmos inputs
- Isolar efeitos colaterais é fundamental para manter a lógica limpa
- Adoção estratégica de funções puras melhora significativamente a qualidade do software
