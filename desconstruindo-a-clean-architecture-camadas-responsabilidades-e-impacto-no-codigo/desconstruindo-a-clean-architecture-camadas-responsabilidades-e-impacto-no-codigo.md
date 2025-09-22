## Introdução

Ao longo dos anos, arquiteturas de software foram criadas, adaptadas e abandonadas conforme as necessidades das empresas e a evolução da tecnologia. No entanto, um conceito que se mantém relevante e cada vez mais valorizado é o da **Clean Architecture**, proposta por Robert C. Martin (o “Uncle Bob”).

A Clean Architecture não é apenas uma moda passageira ou um rótulo bonito para impressionar colegas de equipe. Ela é uma maneira prática e eficiente de estruturar sistemas, permitindo que sejam **fáceis de manter, testar, evoluir e escalar**.

Neste artigo, vamos **destrinchar esse conceito**, entender seus blocos principais, suas responsabilidades e por que ele é uma das abordagens mais sólidas para a construção de software moderno.

---

## O Conceito de Clean Architecture

A Clean Architecture organiza o software em **camadas independentes**, cada uma com responsabilidades bem definidas. O núcleo do sistema (regras de negócio) deve permanecer isolado de detalhes externos, como bancos de dados, frameworks ou interfaces gráficas.

A ideia central pode ser resumida assim:

- O **negócio** vem em primeiro lugar.
- A **tecnologia** se adapta ao negócio, e não o contrário.

Com isso, ganhamos:

- **Flexibilidade**: trocar tecnologias sem reescrever a regra de negócio.
- **Testabilidade**: isolar componentes facilita a criação de testes.
- **Baixo acoplamento**: cada camada só conhece o que é essencial.

---

## As Camadas da Clean Architecture

### **🔵 Domain Layer – O Coração do Sistema**

O **Domínio** é a camada mais importante da Clean Architecture, porque é aqui que moram as **regras de negócio puras**, aquelas que dão identidade ao sistema e que não podem depender de nada externo (frameworks, banco de dados, APIs, etc).

Pense nele como o **contrato social** do software: define as leis, as invariantes e o que é permitido ou proibido dentro do negócio.

**Principais elementos:**

- **Entidades** → objetos com identidade própria e ciclo de vida (ex.: `Pedido`, `Cliente`).
- **Value Objects** → conceitos imutáveis, como CPF, Email, etc. Garantem consistência e validação automática.
- **Eventos de Domínio** → descrevem fatos importantes que aconteceram (ex.: `PedidoConfirmado`).
- **Serviços de Domínio** → encapsulam regras que não pertencem a uma entidade específica (ex.: cálculo de frete, política de desconto).
- **Regras obrigatórias (invariantes)**  → São as condições que o domínio deve proteger sempre. As mais simples vivem dentro das entidades e VOs; já regras mais complexas ou variáveis podem ser expressas com **Specification Pattern**, permitindo compor, reutilizar e evoluir a lógica de negócio sem espalhar ifs pelo código.

**Exemplo prático**:

Um `Pedido` pode ser criado em rascunho, ter itens adicionados e só ser confirmado quando atingir o valor mínimo exigido. Ao ser confirmado, ele dispara um evento de domínio (`PedidoConfirmado`) que pode ser tratado por outras partes do sistema.

**Resumo da ideia:**

O domínio não sabe *onde* os dados são salvos nem *como* são exibidos. Ele só sabe **o que pode ou não acontecer** no negócio. Essa separação é o que garante a vida longa do sistema.

---

### **🔴 Application Layer – A Orquestração**

Se o **Domínio** dita as regras, a **Aplicação** decide *quando* e *como* essas regras são acionadas.

Aqui não se inventa regra nova: o papel da camada é **orquestrar o fluxo do negócio**, coordenando entidades, serviços e repositórios para executar os casos de uso.

**Responsabilidades principais:**

- **Casos de uso (Use Cases)** → descrevem operações de negócio como “Confirmar Pedido”, “Cadastrar Cliente” ou “Processar Pagamento”.
- **Serviços de aplicação** → coordenam chamadas ao domínio, aplicam regras de orquestração e garantem transações.
- **Commands e Queries** → separam intenções de escrita e leitura, trazendo clareza e escalabilidade.
- **Interfaces (Ports)** → definem contratos que a infraestrutura implementa (ex.: envio de e-mail, persistência, integração externa).

**Exemplo prático**:

O caso de uso `ConfirmarPedidoHandler` recebe um comando, busca o `Pedido` via repositório, chama o método `Confirmar()` do domínio (que valida invariantes), salva o estado e dispara um evento de integração.

---

### 🟡 **Infrastructure Layer – Os Detalhes da Vida Real**

A **Infraestrutura** é onde o software deixa de ser só modelo mental e passa a encarar o mundo real.

Enquanto o domínio guarda as regras e a aplicação orquestra os fluxos, a infraestrutura se preocupa com **como essas decisões viram código executável**, usando ferramentas, bancos e serviços externos.

Aqui entram as implementações concretas de contratos definidos nas camadas internas. É a camada que pode mudar sem dó trocar SQL por NoSQL, RabbitMQ por Kafka ou e-mail por SMS sem quebrar a lógica de negócio.

**Responsabilidades principais:**

- **Persistência** → implementação de repositórios, ORMs, queries otimizadas, migrações.
- **Integrações externas** → serviços de pagamento, APIs REST/GraphQL, autenticação OAuth.
- **Mensageria** → filas, tópicos e publicação/consumo de eventos (RabbitMQ, Kafka, SQS).
- **Cross-cutting concerns** → logging, cache distribuído, envio de notificações, armazenamento em nuvem.

**Exemplo prático**:

Um `PedidoRepository` implementando `IPedidoRepository` com **EF Core**. Para o domínio, só existe o contrato `Salvar()` ou `ObterPorId()`. Para a infraestrutura, isso significa conexão com SQL Server, mapping, transações e performance tuning.

**Armadilha comum:**

Deixar o domínio depender da infraestrutura. Se a entidade `Pedido` sabe que existe EF Core, o isolamento morreu. O caminho correto é o contrário: a infraestrutura **implementa o contrato** definido dentro do domínio ou da aplicação.

---

### 🟢 **Presentation Layer – O Ponto de Entrada**

A **camada de apresentação** é a fachada do sistema. É o que usuários e outros serviços realmente enxergam, seja via API, interface web, gRPC ou qualquer outro protocolo.

Aqui não se escreve regra de negócio: a função é **traduzir intenções externas** (requisições) em comandos claros para a aplicação, e depois **traduzir respostas** do sistema em algo que faça sentido para quem consome.

**Responsabilidades principais:**

- **Expor endpoints** → REST, GraphQL, gRPC ou interfaces gráficas.
- **Tratar requests/responses** → validação de entrada, serialização, status codes, erros amigáveis.
- **Middleware** → autenticação, autorização, logging, rate limiting.
- **Composição e DI** → configuração inicial, wiring de serviços e casos de uso.
- **Resiliência e experiência** → lidar com exceções sem vazar detalhes internos e devolver respostas consistentes.

**Exemplo prático**:

Um endpoint `POST /pedidos/confirmar` recebe a requisição JSON, valida dados, cria um comando `ConfirmarPedidoCommand` e o envia para o caso de uso correspondente na Application. A resposta sucesso ou erro é convertida em HTTP 200, 400 ou 500, sem expor o domínio diretamente.

**Erro comum:**

Colocar regra de negócio em controllers. Isso gera APIs gordas e difíceis de manter. O controller deve ser **um tradutor**, não o dono da lógica.

---

## A Força da Clean Architecture

A beleza da Clean Architecture está no fato de que **separa regras de negócio dos detalhes de implementação**. Isso torna seu sistema resiliente a mudanças:

- Quer trocar de banco de dados? Troque na infraestrutura.
- Vai migrar de REST para gRPC? Ajuste na apresentação.
- O negócio mudou? Atualize o domínio, sem mexer em tecnologia.

Em um mundo onde frameworks, bancos e linguagens mudam constantemente, a Clean Architecture garante que o que realmente importa, as **regras do seu negócio,** continuem firme e intocadas.

---

## Conclusão

A Clean Architecture não é só uma teoria bonita, mas uma **estratégia prática para sobreviver ao caos inevitável da evolução tecnológica**.

Ela permite que seu sistema cresça sem se tornar um Frankenstein, garante clareza de responsabilidades e mantém sua base sólida mesmo quando tudo ao redor muda.

Em resumo: **tecnologias vão e vêm, mas um domínio limpo e bem estruturado é o que mantém seu software vivo.**
