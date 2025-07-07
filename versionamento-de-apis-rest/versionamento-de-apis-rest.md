Criar uma API é como abrir uma porta para o mundo. Mas uma vez que você permite que clientes comecem a bater nessa porta, você tem um compromisso: **não mudar a fechadura sem avisar**.

Versionamento de APIs é sobre isso. Sobre **garantir estabilidade, previsibilidade e segurança na evolução do seu sistema**.

---

## A dor de quem ignora o versionamento

Talvez você tenha começado simples. Uma API REST exposta para um front-end, alguns consumidores internos. Tudo parece sob controle. Até que:

- Um time cria uma nova versão do front-end e precisa de uma resposta diferente.
- Um sistema externo quebra porque você alterou um campo ou comportamento.
- Alguém pede uma funcionalidade que altera a semântica de uma rota existente.
- Um bug é corrigido, mas isso quebra um módulo legado que dependia daquele comportamento.

Sem versionamento, **qualquer mudança vira uma ameaça**. E o medo de evoluir paralisa seu sistema.

---

## Por que versionar uma API é uma boa prática?

- **Compatibilidade**: garante que clientes antigos continuem funcionando mesmo com evoluções.
- **Evolutividade**: permite melhorar o modelo, corrigir falhas e adaptar regras de negócio.
- **Confiabilidade**: transmite segurança para quem consome a API.
- **Controle**: você sabe quem está usando o quê, e pode definir ciclos de vida para cada versão.

Versionamento não é só uma questão técnica. É uma estratégia de relação com quem depende do seu serviço.

---

## Quando versionar (e quando não)

### Deve versionar quando:

- Vai **alterar a resposta** de um endpoint existente (adicionando, renomeando ou removendo dados).
- Vai **modificar regras de negócio** ou interpretação da requisição.
- Vai **reestruturar recursos** (por exemplo, endpoints CRUD virando um endpoint de operação).
- Precisa **lançar uma nova versão de cliente** que consome dados de forma diferente.

### Não precisa versionar quando:

- Apenas adicionou campos opcionais e **não quebrou compatibilidade**.
- Corrigiu bugs mantendo o contrato atual.
- Mudanças são internas ou não visíveis ao consumidor da API.

> Dica: evite criar versões para cada mudança pequena. O versionamento deve refletir mudanças significativas no contrato da API.
> 

---

## Devo começar versionando?

Depende.

Se sua API é **consumida apenas internamente** e você tem controle sobre todos os pontos de integração, talvez não precise versionar desde o início.

Mas se sua API:

- é **pública ou usada por sistemas externos**,
- é consumida por **apps mobile**, que você não pode atualizar diretamente,
- tem **clientes críticos ou de terceiros** que você não controla,

então sim, já comece com uma estratégia de versionamento.

Mesmo que seja simples (via path ou query string), você está criando um **contrato claro** com seus consumidores.

---

## Estratégias de versionamento

### 1. Via URL

```bash
GET /api/v1/clientes
```

Simples, visível e muito comum. Ótimo ponto de partida.

### 2. Via Query String

```bash
GET /api/clientes?api-version=1.0
```

Prático para testes e debugging. Suporte nativo no ASP.NET Core.

### 3. Via Header ou Media Type

Mais avançado, usado em APIs mais complexas ou para content negotiation.

---

## Como fazer no ASP.NET Core

Com `Microsoft.AspNetCore.Mvc.Versioning`, você pode definir:

```csharp
services.AddApiVersioning(opt =>
{
    opt.ReportApiVersions = true;
    opt.DefaultApiVersion = new ApiVersion(1);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ApiVersionReader = new UrlSegmentApiVersionReader();
});
```

Pode usar também:

```csharp
new QueryStringApiVersionReader("api-version")
new HeaderApiVersionReader("X-Api-Version")
```

E controlar controllers com:

```csharp
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clientes")]
```

---

## Melhores práticas

- Comece com uma **estratégia simples** (URL ou query)
- **Evite quebrar compatibilidade** sempre que possível
- **Documente todas as versões** com Swagger
- **Comunique desativações com antecedência**
- Monitore quem consome qual versão da API

---

## Conclusão

Versionar APIs é uma prática essencial para crescer sem quebrar o que já está funcionando. No .NET você tem ferramentas maduras para aplicar essa prática com segurança, controle e clareza.

Mais do que uma preocupação técnica, versionar é um gesto de respeito com quem confia no seu serviço.
