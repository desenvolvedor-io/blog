## IoC - Inversion of Control é a melhor estratégia para escrever código com baixo acoplamento. Facilita a criação de testes, separa as dependências e isola o domínio.

## O que é

IoC é uma estratégia de inversão de controle. Muitas pessoas entendem IoC e DI (Dependency Injection) como a mesma coisa, porém DI é apenas uma das maneiras de se fazer IoC.

Outra maneira de se fazer IoC é utilizar AOP (Aspect Oriented Programming). Durante o processo de compilação trechos de código são injetados em determinada classe, assim evitando o forte acoplamento durante a escrita do código.

Com o IoC é possível manter o domínio limpo e as responsabilidades segregadas. É basicamente separar “*o que de deve ser feito*” do “*como deve ser feito*”.

Aplicar IoC melhora a legibilidade do código e o mantém flexível para as futuras modificações, além possibilitar a criação de testes de unidade.

## Por que?

As melhores práticas de engenharia e design de código defendem a utilização dos princípios SOLID, sendo inversão de dependência um destes princípios.

A grande maioria dos frameworks modernos oferecem um mecanismo de IoC de forma nativa. O uso de IoC já não é mais uma espécie de "luxo" que você adiciona ao código. É uma obrigação, afinal é a única maneira de garantir baixo acoplamento, testabilidade e manutenibilidade.

## Cenário

Neste case temos uma classe de domínio que necessita da classe repositório.

![dependencia](https://www.brunobrito.net.br/content/images/2018/05/dependencia1-2.PNG)

Aparentemente não há problemas neste desenho, mas há uma reflexão:

O domínio precisa da classe repositório ou apenas da funcionalidade que ela oferece?

O domínio precisa salvar no banco (*o que deve ser feito*), porém é desnecessário saber como instanciar a classe de repositório, o funcionamento dos métodos e as demais propriedades (*como deve ser feito*).

Para resolver é necessário abstrair as funcionalidades (definições dos métodos) do repositório em uma interface:

![dependencia-invertendo](https://www.brunobrito.net.br/content/images/2018/05/dependencia-invertendo-2.PNG)

Na ilustração acima o repositório implementa uma interface e o domínio faz uso desta interface. O domínio continua utilizando as funcionalidades do repositório, entretanto o domínio não possui o conhecimento de como esse repositório funciona.

A interface (também conhecida como contrato) define o que o repositório deve ter de comportamento e isto é tudo que a classe de domínio precisa saber.

No exemplo foi utilizado uma classe chamada PedidoRepositorySql, mas que pode ser facilmente substituída por uma classe Oracle ou MySql desde que mantenha a implementação da mesma interface.

## Benefícios

O domínio tornou-se independente da implementação do repositório.

Assim se a classe PedidoRepositorySql mudar por algum motivo o domínio não precisa ser alterado. O domínio só seria impactado caso a interface (contrato) mudasse.

O termo contrato cabe muito bem aqui. É uma garantia de que tudo que está explicitamente definido na interface está definido na classe.

Falando de SOLID, podemos entender que o DIP (*Dependency Inversion Principle*) é uma maneira de aplicar o SRP (*Single Responsibility Principle*), pois agora a classe de domínio não tem mais a responsabilidade de saber como a classe de repositório funciona.

## Da teoria à prática

Neste cenário criaremos uma classe de domínio que precisa enviar e-mail e salvar no banco.

O código será feito com alto acoplamento e será refatorado até chegar na injeção de dependência (DI).

### 1. Código espaguete

```csharp
public class PedidosService
{
    public void AnalisarPedido(Pedido pedido)
    {
        if (pedido.Total > 10m)
        {
            pedido.Suspeito();
            this.EnviarEmailAlerta(pedido);
        }

        new PedidoRepositorySql().SalvarPedido(pedido);
    }

    private void EnviarEmailAlerta(Pedido pedido)
    {
        // Codigo para enviar e-mail
    }
}
```

Seguindo o SRP do SOLID, o envio de e-mail deveria ser responsabilidade de outra classe. Por se tratar de uma dependência de infraestrutura deveria inclusive ser implementada em uma camada diferente da do domínio.

### 2. Aplicando a IoC

Repare que no código abaixo o PedidoService desconhece implementar as classes de repositório e de serviço de e-mail. 

A única informação que possui são os contratos que expõem métodos para atender estas necessidades.

```csharp
public class PedidosService
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IEmailService _emailService;

    public PedidosService(
        IPedidoRepository pedidoRepository,
        IEmailService emailService)
    {
        _pedidoRepository = pedidoRepository;
        _emailService = emailService;
    }

    public void AnalisarPedido(Pedido pedido)
    {
        if (pedido.Total > 10m)
        {
            pedido.Suspeito();
            _emailService.EnviarEmailAlerta(pedido);
        }

        _pedidoRepository.SalvarPedido(pedido);
    }
}
```

Para que tudo funcione será necessário criar as classes que implementem as interfaces.

```csharp
public interface IEmailService
{
    void EnviarEmailAlerta(Pedido pedido);
}

public interface IPedidoRepository
{
    void SalvarPedido(Pedido pedido);
}

public class PedidoRepositorySql : IPedidoRepository
{
    public void SalvarPedido(Pedido pedido) { }
}

public  class  EmailGoogleService: IEmailService
{
    public void EnviarEmailAlerta(Pedido pedido) { }
}    
```

As implementações dos códigos acima poderiam estar em outra camada, em outro projeto ou até mesmo numa referência de um pacote Nuget.

### Flexibilidade

Com a técnica do IoC o PedidoService torna-se independente, afinal não conhece os detalhes dos serviços que utiliza.

Inclusive abrindo cenário para novas possibilidades, por exemplo:

```csharp
// repositorio Sql, email do Google
var pedidoService = new PedidosService(new PedidoRepositorySql(), new EmailGoogleService());

// repositorio oracle, email do Google 
var pedidoService = new PedidosService(new PedidoRepositoryOracle(), new EmailGoogleService());

// repositorio MySql, email do Google
var pedidoService = new PedidosService(new PedidoRepositoryMySql(), new EmailGoogleService());

// repositorio Mongo, e-mail MailChimp
var pedidoService = new PedidosService(new PedidoRepositoryMongoDb(), new MailChimpService());
```

Basta que estas novas classes implementem as mesmas interfaces.

Como num passe de mágica tudo continua funcionando! Sem a necessidade de alterações no serviço de domínio.

### Injeção de dependência e Resolução de dependência

No código acima foram criadas as instâncias das classes diretamente nos parâmetros, porém o certo seria fazer o uso das interfaces.

O mecanismo que oferece as instâncias de classes que implementam as interfaces é chamado de mecanismo de DI. Ou seja, ao injetar uma interface no construtor de uma classe é como você estivesse dizendo a este mecanismo:

-Hey, me dê uma instância da classe que implementa esse contrato aqui!

Sendo assim conforme o código abaixo, ao obter uma instância da classe PedidosService você também já terá uma instância da classe de repositório e de serviço de e-mail disponibilizadas através das propriedades que implementam os seus contratos.

```csharp
public class PedidosService
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IEmailService _emailService;

    public PedidosService(
        IPedidoRepository pedidoRepository,
        IEmailService emailService)
    {
        _pedidoRepository = pedidoRepository;
        _emailService = emailService;
    }

    public void AnalisarPedido(Pedido pedido)
    {
        if (pedido.Total > 10m)
        {
            pedido.Suspeito();
            _emailService.EnviarEmailAlerta(pedido);
        }

        _pedidoRepository.SalvarPedido(pedido);
    }
}
```

Essa é a mágica da Injeção de Dependência!

Cada framework possui o seu mecanismo de resolução de dependências. Não é tão simples, tem alguns detalhes muito importantes mas isto já é assunto para um outro artigo.
