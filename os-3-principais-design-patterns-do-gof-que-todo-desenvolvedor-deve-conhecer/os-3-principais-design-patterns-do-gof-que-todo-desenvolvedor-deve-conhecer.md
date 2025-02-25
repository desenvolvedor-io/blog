A arquitetura de software pode ser um campo traiçoeiro. Quantas vezes você já se pegou refatorando código que ficou impossível de manter? Ou implementando soluções que pareciam boas no início, mas logo se tornaram um pesadelo técnico?

Se você quer evitar esse tipo de armadilha e escrever código mais flexível, reutilizável e sustentável, precisa conhecer os padrões de projeto (Design Patterns). E hoje vamos falar sobre três dos mais importantes do catálogo do **GoF (Gang of Four)**.

## O que é o GoF?

O **Gang of Four (GoF)** refere-se a quatro autores – Erich Gamma, Richard Helm, Ralph Johnson e John Vlissides – que publicaram o famoso livro *Design Patterns: Elements of Reusable Object-Oriented Software*. Nesse livro, eles documentaram 23 padrões de projeto que se tornaram fundamentais para o desenvolvimento de software orientado a objetos. Esses padrões ajudam os desenvolvedores a resolver problemas recorrentes, promovendo reuso, flexibilidade e manutenção do código.

Os três padrões abordados neste artigo são:

- **Factory Method (Criacional)**
- **Strategy (Comportamental)**
- **Adapter (Estrutural)**

## 1. Factory Method – Criando Objetos de Forma Flexível

### O problema

Você precisa criar instâncias de classes específicas, mas não quer que seu código fique preso a uma implementação específica. Imagine que você está desenvolvendo um sistema de pedidos e pode precisar de diferentes tipos de pagamento (Cartão, Boleto, Pix). Como evitar múltiplos `if` ou `switch` para criar instâncias dessas classes?

### A solução

O Factory Method encapsula a lógica de criação de objetos em subclasses, promovendo maior flexibilidade e separação de responsabilidades.

### Exemplo de Código (C# com comentários em português)

```csharp
// Interface para o método de pagamento
public interface IPagamento {
    void ProcessarPagamento();
}

// Implementações concretas dos métodos de pagamento
public class PagamentoCartao : IPagamento {
    public void ProcessarPagamento() {
        Console.WriteLine("Pagamento com Cartão de Crédito realizado.");
    }
}

public class PagamentoPix : IPagamento {
    public void ProcessarPagamento() {
        Console.WriteLine("Pagamento via Pix realizado.");
    }
}

// Classe abstrata que define o Factory Method
public abstract class PagamentoFactory {
    public abstract IPagamento CriarPagamento();
}

// Implementações concretas da fábrica para diferentes meios de pagamento
public class PagamentoCartaoFactory : PagamentoFactory {
    public override IPagamento CriarPagamento() => new PagamentoCartao();
}

public class PagamentoPixFactory : PagamentoFactory {
    public override IPagamento CriarPagamento() => new PagamentoPix();
}

// Uso do Factory Method
var factory = new PagamentoPixFactory(); // Poderia ser substituído por outra fábrica
var pagamento = factory.CriarPagamento();
pagamento.ProcessarPagamento();
```

### Quando usar?

- Quando a criação de um objeto envolve lógica complexa.
- Quando você quer garantir que a criação de objetos seja consistente.
- Quando deseja desacoplar a criação da utilização.

## 2. Strategy – Tornando Algoritmos Intercambiáveis

### O problema

Você tem múltiplas estratégias de execução para uma funcionalidade e quer evitar condicionalizações pesadas. Imagine um sistema de cálculo de frete, onde diferentes estratégias podem ser aplicadas (Correios, Transportadora, Retirada na loja).

### A solução

O Strategy permite definir uma família de algoritmos, encapsulando-os e tornando-os intercambiáveis.

### Exemplo de Código (C# com comentários em português)

```csharp
// Interface comum para estratégias de frete
public interface IFreteStrategy {
    decimal CalcularFrete(decimal valorPedido);
}

// Implementações concretas para cada estratégia de frete
public class FreteCorreios : IFreteStrategy {
    public decimal CalcularFrete(decimal valorPedido) => valorPedido * 0.1m;
}

public class FreteTransportadora : IFreteStrategy {
    public decimal CalcularFrete(decimal valorPedido) => valorPedido * 0.2m;
}

// Contexto que define qual estratégia de frete será usada
public class FreteContexto {
    private IFreteStrategy _estrategia;
    public FreteContexto(IFreteStrategy estrategia) {
        _estrategia = estrategia;
    }

    public decimal CalcularCustoFrete(decimal valorPedido) {
        return _estrategia.CalcularFrete(valorPedido);
    }
}

// Uso do padrão Strategy
var frete = new FreteContexto(new FreteCorreios()); // Pode ser substituído por outra estratégia
Console.WriteLine($"Custo do frete: {frete.CalcularCustoFrete(100)}");
```

### Quando usar?

- Quando existem múltiplas formas de realizar uma mesma operação.
- Quando deseja evitar grandes blocos de `if-else` ou `switch`.
- Quando precisa permitir que o comportamento de um objeto mude em tempo de execução.

## 3. Adapter – Integrando Sistemas Incompatíveis

### O problema

Você precisa integrar uma nova API ou componente ao seu sistema, mas sua interface não é compatível com a que já existe.

### A solução

O Adapter converte a interface de uma classe em outra interface esperada pelos clientes.

### Exemplo de Código (C# com comentários em português)

```csharp
// Interface esperada pelo sistema
public interface IRelatorioAntigo {
    void GerarRelatorio();
}

// Classe existente no sistema
public class RelatorioAntigo : IRelatorioAntigo {
    public void GerarRelatorio() {
        Console.WriteLine("Relatório gerado pelo sistema antigo.");
    }
}

// Nova classe com interface incompatível
public class RelatorioModerno {
    public void GerarNovoRelatorio() {
        Console.WriteLine("Relatório gerado pelo sistema moderno.");
    }
}

// Adapter que adapta a interface antiga para a nova
public class RelatorioAdapter : IRelatorioAntigo {
    private readonly RelatorioModerno _novoRelatorio;
    public RelatorioAdapter(RelatorioModerno novoRelatorio) {
        _novoRelatorio = novoRelatorio;
    }

    public void GerarRelatorio() {
        _novoRelatorio.GerarNovoRelatorio();
    }
}

// Uso do Adapter
IRelatorioAntigo relatorio = new RelatorioAdapter(new RelatorioModerno());
relatorio.GerarRelatorio();
```

### Quando usar?

- Quando precisa compatibilizar interfaces diferentes.
- Quando deseja reutilizar código legado sem grandes mudanças.
- Quando precisa integrar APIs externas sem refatorar o código existente.

## Conclusão

Agora que você conhece três dos principais padrões de projeto do GoF, que tal começar a aplicá-los no seu código? Escolha um deles e refatore um trecho do seu sistema para testar os benefícios. Você verá como seu código ficará mais modular, flexível e sustentável. Não espere para melhorar sua arquitetura, a hora é agora! 🚀
