# Inversion of Control

IoC é a melhor estratégia para desacoplar um sistema. Facilita a criação de testes, separa as dependências e isola o domínio.

### O que é

De acordo com Wikipédia “Inversão de controle, IoC,  é um princípio abstrato que descreve o aspecto do design e arquitetura de software no qual o fluxo de controle do sistema é invertido em comparação com a programação procedural”.

Com o IoC é possível manter o domínio limpo. Sem necessidade de dependências diretas. Isola o domínio separando “o que de deve ser feito” do “como deve ser feito”. Melhora a legibilidade do código. Deixa o código flexível para as futuras modificações e facilita na criacao dos testes unitários.

### Por que?

As melhores práticas de engenharia e design de software defendem a utilização do princípio SOLI**D**, sendo inversão de dependência um destes princípios. 

Seguir melhores práticas é como cortar um atalho. Existem outros caminhos, mas seguir por este aumenta as chances de sucesso. Os benefícios a longo prazo traz ganhos na qualidade do software. Há uma relação direta entre a criação de testes e a diminuição na incidência de bugs. Consequentemente há uma queda na quantidade de esforço gasto na manutenção, um benefício econômico para empresa, além de melhorar a percepção do cliente com um sistema estável.


## Cenário

A seguir o consumidor, domínio, necessita da classe repositório.

![dependencia](https://www.brunobrito.net.br/content/images/2018/05/dependencia1-2.PNG)

Aparentemente não há problemas neste desenho, mas há uma reflexão, o domínio precisa da classe repositório?

O domínio precisa salvar no banco (O que deve ser feito). É desnecessário o conhecimento dos métodos, implementação e as demais propriedades da classe repositório (como deve ser feito).

Para resolver é necessário abstrair a definição do método “Salvar” do repositório em uma interface, ilustrado abaixo. 

![dependencia-invertendo](https://www.brunobrito.net.br/content/images/2018/05/dependencia-invertendo-2.PNG)

Na ilustração acima o serviço de repositório implementa a interface e o domínio usa uma instância da interface. O domínio continua utilizando o serviço de repositório. No entanto o domínio não sabe quem tem esse conhecimento. No exemplo foi utilizado uma classe chamada PedidoRepositorySql, mas que pode ser facilmente substituído por uma biblioteca Oracle ou MySql. Desde que mantenha a assinatura da interface.

### Beneficios

O domínio tornou-se independente do PedidoRepositorySql. Portanto é menos provável que haja a necessidade de fazer alterações no domínio quando houver alterações no PedidoRepositorySql. No formato que foi concebido é improvável que uma modificação no domínio implique em implementações no Repository.

Outro benefício é que o código do domínio está isolado do servico de banco de dado, facilitando a criação de testes unitários. Na primeira abordagem para criar um teste seria necessário também configurar o banco de dados.

### Da teoria à prática

Seguindo o cenário anterior, será criado uma classe de domínio que precisa enviar e-mail e salvar no banco.

O código será refatorado passo a passo. Até chegar na inversão de dependência.

#### 1. Código espaguete
<pre><code data-language="java">
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
</code></pre>

Pelo princípio do SOLID, o envio de e-mail deveria ser responsabilidade de outra classe.  Respeitando  também os padrões do DDD, o envio de e-mail e o Repositório deveriam estar em outra camada (projeto).

#### 2. Aplicando a IoC.

Repare que no código abaixo o PedidoService desconhece como criar o repositório, desconhece como enviar e-mail, apenas sabe que deve fazê-lo, mas não sabe como e tampouco detalhes da implementação.

<pre><code data-language="java">
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
</code></pre>

Para que funcione será necessário criar classes que implemente as interfaces.

<pre><code data-language="java">
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
    
</code></pre>

As implementações dos códigos acima poderia estar em outra classe, em outro projeto ou até mesmo numa referência do nuget

#### Flexibilidade

Com este padrão, o PedidoService é independente, não conhece os detalhes dos serviços que utiliza. Torna-se flexível, abrindo o cenário para novas possibilidades, abaixo algumas delas.

<pre><code data-language="java">
    // repositorio Sql, email do Google
    var pedidoService = new PedidosService(new PedidoRepositorySql(), new EmailGoogleService());
</code></pre>

<pre><code data-language="java">
    // repositorio oracle, email do Google 
    var pedidoService = new PedidosService(new PedidoRepositoryOracle(), new EmailGoogleService());
</code></pre>

<pre><code data-language="java">
    // repositorio MySql, email do Google
    var pedidoService = new PedidosService(new PedidoRepositoryMySql(), new EmailGoogleService());
</code></pre>

<pre><code data-language="java">
    // repositorio Mongo, e-mail chimp
    var pedidoService = new PedidosService(new PedidoRepositoryMongoDb(), new MaailChimpService());
</code></pre>

Basta as classes implementarem a interface. E como um passe de mágica, um comportamento totalmente novo. Sem a necessidade de nenhuma alteração no serviço.
