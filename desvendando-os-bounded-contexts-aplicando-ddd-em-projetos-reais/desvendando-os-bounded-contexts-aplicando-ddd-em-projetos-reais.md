Domain-Driven Design (DDD) é uma abordagem estratégica para o desenvolvimento de software, mas um de seus conceitos mais mal compreendidos é, sem dúvida, o "Bounded Context". Afinal, o que significa delimitar um contexto? Como isso se reflete na prática do dia a dia de um time de desenvolvimento? E, mais importante: como isso pode salvar seu projeto de uma espiral de complexidade incontrolável?

Neste artigo, vamos explorar a ideia de Bounded Context com exemplos reais, analogias do cotidiano e provocações que todo desenvolvedor, arquiteto ou tech lead precisa encarar.

---

## Entendendo o Conceito

Imagine que você trabalha em uma empresa que desenvolve uma plataforma de e-commerce. Nela, temos um módulo de pagamentos, outro de logística e um terceiro de atendimento ao cliente. Cada um desses domínios tem seu próprio vocabulário, regras de negócio e prioridades.

Por exemplo, "pedido" no contexto do time de logística significa algo que precisa ser embalado e despachado. Já no time financeiro, "pedido" é uma entidade vinculada à uma nota fiscal, status de pagamento e repasses. Embora ambos usem a mesma palavra, claramente estão falando de coisas diferentes.

É aqui que entra o **Bounded Context**: ele define uma fronteira dentro da qual um determinado modelo é válido. Fora dessa fronteira, o modelo perde sentido ou assume significados distintos.

---

## Por que isso importa tanto?

A ausência de Bounded Contexts claros é uma das causas mais comuns de falhas de comunicação, bugs e inconsistências em sistemas distribuídos.

Você já se deparou com aquele sistema onde a entidade "Usuário" tem 60 propriedades, das quais metade é nula dependendo do ponto da aplicação? Isso é um forte indicativo de mistura de contextos.

Separar contextos também permite:

- **Independência de evolução**: um contexto pode evoluir sem quebrar os outros.
- **Time-to-market acelerado**: equipes menores com foco em partes específicas do sistema.
- **Alta coesão e baixo acoplamento**, os dois pilares de uma boa arquitetura.

---

## Aplicando Bounded Contexts em Projetos Reais

### 1. Identifique os Subdomínios

Antes de sair separando projetos em pastas diferentes, **é essencial entender o negócio**. 
DDD não é uma questão técnica, mas estratégica.

- Conduza workshops de **Event Storming**.
- Converse com os especialistas de negócio.
- Identifique linguagens diferentes para o mesmo termo.

> Event Storming **é uma técnica de modelagem de domínio que permite a exploração colaborativa e o entendimento de um domínio de negócios**. É um método baseado em workshops que busca identificar e mapear eventos, atores e comandos em um sistema, utilizando um processo leve e simples.
> 

### 2. Modele os Contextos

Cada contexto terá suas próprias entidades, regras e comportamentos. Evite o compartilhamento de modelos entre contextos.

```csharp
// Contexto: Pagamentos
public class Pedido {
    public decimal ValorTotal { get; set; }
    public bool Pago { get; set; }
}

// Contexto: Logística
public class Pedido {
    public string CodigoRastreio { get; set; }
    public DateTime DataEnvio { get; set; }
}
```

Mesmo nome, significados e responsabilidades diferentes. 

> **Fora do contexto, o significado da entidade muda**. Essa é a essência dos Bounded Contexts: um mesmo termo pode carregar significados radicalmente diferentes dependendo do contexto onde é aplicado.
> 

### 3. Defina as Integrações

A comunicação entre contextos deve ser feita de forma **explícita**. Isso pode ser via:

- APIs
- Mensageria
- Eventos de domínio

Evite dependências diretas de código ou repositórios compartilhados. 

### 4. Mapeie os Context Maps

DDD propõe tipos de relação entre contextos, como:

- **Parceria (Partnership)**
- **Fornecedor-Consumidor (Customer-Supplier)**
- **Conformista**
- **Anticorruption Layer**

Saber como um contexto depende de outro ajuda a planejar integrações futuras e evitar o efeito dominó de falhas.

---

## Uma analogia que ajuda (muito) a entender

Pense em uma empresa com vários departamentos. Todos lidam com a mesma entidade: **funcionário**. Mas o significado dessa entidade muda radicalmente conforme o contexto:

- **Para o RH**, funcionário é um colaborador com salário, benefícios, plano de carreira e férias programadas. Ele é visto como **recurso humano**.
- **Para o time de TI**, esse mesmo funcionário é um conjunto de credenciais: login, senha, permissões de acesso, conta de e-mail, token de VPN. Ele é visto como **identidade digital**.
- **Para o departamento de Vendas**, o funcionário é um agente comercial: possui um código de vendedor, metas, comissões, ranking. Ele é visto como **representante de performance**.

Apesar de estarem falando da mesma pessoa física, cada departamento opera dentro de um **Bounded Context** com um modelo totalmente diferente e coerente com seus próprios objetivos.

> E é exatamente por isso que tentar compartilhar o mesmo “modelo de funcionário” entre todos os departamentos é pedir para entrar num pântano de acoplamentos e bugs.
> 

## Então eu deveria ter uma classe “Funcionário” em cada contexto?

**Sim… e não.**

Você *pode* ter classes chamadas `Funcionario`, `Usuario`, `Vendedor`  e isso não é um problema. Na verdade, isso é desejável **desde que cada uma exista dentro de seu respectivo contexto**, representando um modelo coerente com sua realidade.

É aqui que entra a importância da **Linguagem Ubíqua (Ubiquitous Language)**: em cada Bounded Context, a "mesma pessoa" pode assumir papéis e nomes diferentes, conforme os objetivos daquele domínio. O RH enxerga um funcionário. O time de TI, um usuário. Vendas, um vendedor.

> Não se trata de duplicação. Trata-se de clareza semântica. Um modelo genérico o suficiente para tentar cobrir todos esses papéis será inútil para todos eles.
> 

E o mesmo vale para entidades como **“Produto”**. O nome pode até se repetir entre os contextos, mas o modelo quase nunca será o mesmo.

- No **catálogo**, o Produto tem nome, descrição, categoria, imagem e status de disponibilidade.
- No **estoque**, o Produto tem código de barras, SKU, quantidade disponível, local de armazenagem e regras de reposição.
- No **fiscal**, o Produto está associado a preço, taxas, tipos de tributação etc.

> A grande armadilha é assumir que só porque o nome da entidade é igual, o modelo também deveria ser.
> 

Cada contexto resolve **problemas de negócio diferentes**, e portanto precisa de uma **visão específica da entidade**. Forçar um modelo genérico, que tenta atender a todos ao mesmo tempo, é um convite ao acoplamento, à complexidade desnecessária e à perda de significado.

---

## Como identificar contextos?

Uma das maiores dificuldades ao aplicar DDD na prática é justamente saber por onde começar a definir os Bounded Contexts. A boa notícia é que nem sempre é preciso acertar de primeira. Definir contextos é um processo **evolutivo**, não uma ciência exata.

Uma abordagem prática é iniciar **mapeando os departamentos da empresa**: financeiro, vendas, logística, atendimento, etc. Cada um costuma possuir uma linguagem, processos e metas diferentes, o que normalmente já indica que estamos lidando com domínios distintos.

> Comece por onde há fronteiras naturais de comunicação e ownership. Se dois times têm objetivos diferentes e pouco acoplamento no dia a dia, há um forte indício de que ali há um contexto separado.
> 

No entanto, com o tempo e a evolução do sistema, você vai perceber que alguns desses contextos podem estar **grandes demais ou genéricos demais**. Nesses casos, você pode **refinar os contextos**, quebrando-os em unidades menores, mais coesas e com responsabilidades mais bem definidas.

Por exemplo:

- O contexto de **"Vendas"** pode ser dividido em **"Gestão de Preços"**, **"Controle de Comissão"** e **"Processamento de Pedidos"**.
- O contexto de **"Atendimento"** pode se separar em **"Gestão de Reclamações"**, **"Autoatendimento"** e **"Relacionamento com o Cliente"**.

Esse refino costuma acontecer de forma **natural**, conforme o software cresce, os domínios se tornam mais bem compreendidos e as equipes passam a se especializar.

> A quebra não é feita “porque o modelo ficou grande”. Ela é feita quando diferentes partes do contexto começam a resolver **problemas diferentes demais**, com regras que se chocam, se ignoram ou competem por atenção.
> 

Lembre-se: o objetivo dos Bounded Contexts é **alinhar o modelo de software com o modelo mental do negócio**. Se o modelo está confuso ou excessivamente genérico, ele está te dizendo que chegou a hora de redesenhar as fronteiras.

A boa arquitetura é sensível à evolução. Bounded Contexts não são escritos em pedra, são um instrumento para organizar complexidade de forma progressiva e inteligente.

---

## Perguntas que valem ouro

Nem sempre é fácil perceber que você está atravessando fronteiras de contexto. Mas algumas perguntas certeiras podem revelar que algo não está bem modelado:

- O modelo que estou usando faz sentido neste contexto ou estou apenas reaproveitando estruturas genéricas?
- Outro time que consome esse modelo o interpreta da mesma forma que o meu?
- Estou evitando explicitar integrações por comodidade, acoplando domínios diferentes num mesmo código?

Se as respostas forem "não, não, sim"... você tem um excelente motivo para revisar seus contextos. Esse atrito entre modelos é o primeiro sinal de que algo precisa ser isolado, explicitado ou redesenhado.

---

## Conclusão

Bounded Context é muito mais do que uma organização de código. É uma forma de dar clareza ao modelo de negócio, permitir que times se movimentem com autonomia e garantir que sistemas cresçam de forma ordenada.

DDD não é uma bala de prata, mas ignorar seus princípios em projetos complexos é abrir as portas para um legado disfuncional.

Quer evoluir sua arquitetura? Comece delimitando seus contextos.
