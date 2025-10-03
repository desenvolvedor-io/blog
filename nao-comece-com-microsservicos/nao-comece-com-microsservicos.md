## Introdução

Microserviços viraram moda. Livros, artigos, palestras e até memes circulam exaltando a “arquitetura definitiva” que separa gigantes como Netflix, Amazon e Uber dos meros mortais. Mas aqui vai a primeira provocação: **será que o seu sistema precisa disso agora?**

Essa é uma armadilha comum no nosso mundo. Desenvolvedores e arquitetos, sedentos por boas práticas e influenciados por cases de empresas bilionárias, caem na tentação de começar seus projetos já “na nuvem”, com orquestração Kubernetes, dez microserviços, filas distribuídas, observabilidade completa… mas sem ao menos ter **clientes, usuários ou problemas reais de escala**.

Resultado? Meses de dor de cabeça, mais tempo configurando pipeline do que entregando valor, e uma dívida técnica que ironicamente nasceu em nome da “boa arquitetura”.

Se você já ficou preso horas tentando debugar por que dois serviços não conversam, ou perdeu um sprint inteiro ajustando configurações do Docker Compose, sabe bem do que estou falando.

Hoje vamos desconstruir o hype e responder a pergunta que muitos não têm coragem de fazer: **“Por que eu não deveria começar com microserviços?”**

---

## O mito dos microserviços como ponto de partida

A indústria vende microserviços como a bala de prata da arquitetura de software. Afinal, quem não gostaria de dizer que sua aplicação está dividida em pequenos serviços independentes, cada um escalando de forma autônoma, com deploy contínuo e pipelines automatizados?

O problema é que esse sonho tem um **custo de entrada** altíssimo.

Sam Newman, autor do clássico *Building Microservices*, alerta desde a primeira edição: microserviços **não são um ponto de partida**, mas sim uma evolução natural de sistemas que cresceram além da capacidade de um monólito.

Martin Fowler, referência em arquitetura, reforça: antes de falar em microserviços, precisamos falar em **modularidade e acoplamento**. Se você não sabe desenhar bem um monólito modular, não vai magicamente desenhar bons microserviços.

A ideia de começar já fragmentando tudo é como **morar sozinho pela primeira vez e já querer administrar dez casas ao mesmo tempo**. Parece poderoso, mas é insustentável.

---

## Quando o monólito ainda é a melhor escolha

Apesar do estigma, o **monólito bem feito** continua sendo a melhor escolha inicial para a maioria dos projetos. E aqui estão algumas razões:

- **Simplicidade arquitetural**
    
    Um único código-fonte, um único processo de deploy. Você não precisa configurar filas, discovery service ou mensageria só para validar se sua ideia faz sentido.
    
- **Velocidade de entrega**
    
    Com monólito, o time consegue focar em entregar funcionalidades de negócio rapidamente, sem se perder em infraestrutura.
    
- **Facilidade de depuração**
    
    Um bug em ambiente local é fácil de reproduzir. Você roda sua aplicação, coloca um breakpoint e resolve. Com microserviços, o bug pode estar na interação entre serviços e reproduzir isso localmente pode ser impossível.
    
- **Menor sobrecarga de infraestrutura**
    
    Você não precisa de um cluster Kubernetes, de um stack completo de observabilidade ou de uma equipe de DevOps só para rodar seu MVP.
    

Em resumo: **monólito não é sinônimo de gambiarra.** Ele pode (e deve) ser organizado, modular e preparado para evoluir.

---

## As dores de adotar microserviços cedo demais

Ok, vamos imaginar que você ignorou os avisos e decidiu começar com microserviços desde o dia zero. O que acontece?

### 1. Orquestração e Deploy

Você precisará de Kubernetes, Docker ou alguma solução de orquestração. Isso significa aprender a lidar com manifestos YAML, balanceadores de carga, secrets distribuídos e políticas de rede **antes mesmo de entregar valor ao usuário**.

### 2. CI/CD Inflado

Cada microserviço precisa de pipeline de build, testes, deploy e rollback. Se você tem 10 serviços, são 10 pipelines. Em pouco tempo, sua equipe vai passar mais tempo mantendo pipelines do que construindo funcionalidades.

### 3. Monitoramento e Observabilidade

Logs centralizados, métricas, tracing distribuído… tudo isso é essencial em microserviços, porque os problemas não aparecem em um só lugar. Mas implementar observabilidade robusta é caro, demorado e exige maturidade que poucas startups têm no início.

### 4. Comunicação entre Serviços

Decidir entre REST, gRPC, mensageria ou eventos. Gerenciar contratos. Garantir resiliência contra falhas. Cuidar de retries, timeouts e circuit breakers. Parece emocionante, mas a verdade é: **boa parte disso não agrega nada ao seu MVP**.

### 5. Custos de Equipe

Microserviços funcionam bem quando há times especializados em cada domínio. Mas se sua equipe tem 5 ou 10 devs, fragmentar responsabilidades entre serviços só cria **ilhas de conhecimento**.

Em resumo: adotar microserviços cedo demais é trocar **complexidade de negócio real** por **complexidade técnica artificial**.

---

## Sinais de que você realmente precisa de microserviços

Nem tudo é contra microserviços. Há casos legítimos em que eles fazem sentido:

- **Escalabilidade real**: você precisa escalar partes diferentes do sistema de forma independente (ex.: checkout de e-commerce é muito mais demandado do que cadastro de usuários).
- **Equipes independentes**: você já tem múltiplos times trabalhando em diferentes módulos do sistema, e o monólito virou gargalo de colaboração.
- **Domínios bem definidos**: você possui clareza sobre os *bounded contexts* (Conceito de Domain-Driven Design), e a divisão natural entre serviços já existe.
- **Alta complexidade de negócio**: quando o software evoluiu a ponto de um único deploy ser arriscado demais, ou quando funcionalidades precisam de cadências de release diferentes.

Aqui, sim, migrar para microserviços pode ser libertador. Mas note: **todos esses sinais vêm depois de o sistema já existir e crescer.**

---

## Estratégias de evolução: do monólito ao microserviço

Se microserviços são evolução, qual é o caminho natural?

1. **Comece com um monólito modular**
    - Estruture seu código por domínios de negócio.
    - Use camadas claras (ex.: Controllers, Services, Repositories).
    - Evite dependências cruzadas entre módulos.
2. **Evolua para um monólito distribuível**
    - Mantenha os domínios isolados.
    - Use interfaces e contratos bem definidos.
    - Facilite o desacoplamento futuro.
3. **Extraia microserviços quando houver motivo real**
    - Identifique gargalos de escala ou de autonomia de equipes.
    - Migre módulos isolados para serviços independentes.
    - Automatize observabilidade e deploy aos poucos, conforme a necessidade.

### Exemplo de diagrama conceitual:

```
Monólito simples  ->  Monólito modular  ->  Microserviços
```

No início, tudo junto. Depois, módulos bem separados. Só então, cada módulo pode se tornar um serviço independente.

---

## Erros comuns e armadilhas do hype

- **Usar microserviços só porque “é moderno”**
    
    Se a sua justificativa é essa, já está errado.
    
- **Copiar grandes empresas sem contexto**
    
    A Netflix precisa de microserviços porque atende milhões de usuários simultâneos. Você não é a Netflix.
    
- **Ignorar maturidade da equipe**
    
    Se sua equipe ainda luta para aplicar testes unitários, não espere que ela dê um salto saudável para microserviços.
    
- **Confundir modularidade com distribuição**
    
    Um código modular não precisa estar em repositórios separados. Separar repositórios não significa ganhar arquitetura.
    

---

## Casos práticos e analogias

Imagine que você vai **transportar uma mochila** para o outro quarteirão. Você pode usar um caminhão de mudanças, contratar guincho, pedir escolta policial… ou simplesmente **colocar a mochila nas costas e andar**.

Começar com microserviços é como usar o caminhão para levar a mochila. É caro, complexo e desproporcional.

---

## Exemplos técnicos e arquiteturais

### Exemplo de monólito simples em C#

```csharp
public class OrderService
{
    private readonly PaymentService _payment;
    private readonly StockService _stock;

    public OrderService(PaymentService payment, StockService stock)
    {
        _payment = payment;
        _stock = stock;
    }

    public void PlaceOrder(Order order)
    {
        _payment.Process(order);
        _stock.Update(order);
        Console.WriteLine("Order placed successfully!");
    }
}
```

### Exemplo de microserviços (pseudo-código simplificado)

- Serviço de pedidos chama API do serviço de pagamento.
- Serviço de pagamento dispara evento de confirmação.
- Serviço de estoque consome evento para atualizar inventário.

```csharp
// OrderService chama Payment API
http.Post("http://payment-service/api/pay", order);

// PaymentService publica evento
eventBus.Publish(new PaymentConfirmed(orderId));

// StockService consome evento
eventBus.Subscribe<PaymentConfirmed>(evt => UpdateStock(evt.OrderId));
```

Perceba a diferença: no monólito, tudo é direto. Nos microserviços, você precisa de APIs, eventos, mensageria, retries… **mais moving parts para dar problema**.

---

## Conclusão

Microserviços são poderosos, mas não são ponto de partida. Eles devem ser consequência de um sistema que **cresceu e exige** mais independência, escalabilidade e resiliência.

Começar com microserviços é um erro tão comum quanto custoso. É abraçar complexidade desnecessária, matar a produtividade do time e gastar energia onde não há retorno imediato.

Então eu te deixo a pergunta final:

**Seu sistema realmente precisa de microserviços agora, ou você está apenas seguindo o hype?**
