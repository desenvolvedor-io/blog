Quantas vezes você já se deparou com um projeto onde as dependências entre as camadas estavam completamente fora de controle? Infraestruturas dependendo de Application, Domínio dependendo de Infrastructure, e assim por diante? 

Se você nunca passou por isso, provavelmente está começando sua jornada. Se já passou, sabe o quão caro e doloroso é corrigir depois.

E se eu te dissesse que é possível **automatizar o enforcement** da arquitetura da sua aplicação? Que você pode, assim como escreve testes de unidade ou integração, escrever testes que garantem que as camadas só se relacionem de acordo com o seu design? 

É exatamente isso que o pacote `NetArchTest.Rules` propõe.

---

## O que é NetArchTest.Rules?

O `NetArchTest.Rules` é uma biblioteca leve que permite escrever **testes de arquitetura** em aplicações .NET utilizando uma sintaxe fluente e expressiva. Com ela, você define **regras estruturais** que suas assemblies e namespaces devem seguir.

Por trás dessa simples premissa, esconde-se uma ferramenta poderosa que pode evitar que seu sistema sofra com o **caos arquitetural,** aquele momento em que a manutenção se torna insustentável e os times começam a desacelerar.

---

## Por que realizar testes de arquitetura?

Você já pensou em como confiamos cegamente que os devs vão sempre respeitar a arquitetura desenhada?

Vamos ser realistas: na pressão da sprint, com prazos apertados e demandas inesperadas, quem nunca fez um `using` indevido para resolver "só um problema rápido"? Esse "jeitinho" muitas vezes passa despercebido até se acumular, transformando seu sistema em uma massa amorfa difícil de evoluir.

**Testes de arquitetura** servem exatamente para impedir isso, automatizando a verificação de regras como:

- Infraestrutura deve depender de Domínio.
- Application não pode depender de Infraestrutura.
- Domínio não pode depender de ninguém.
- Adaptadores não devem conversar diretamente com Repositórios.

---

## Setup para Testes de Arquitetura

Se você quer testar arquitetura no .NET, entenda uma coisa: você **não vai rodar teste de arquitetura sem framework de teste**, e o mais comum no .NET é o `xUnit`. O `NetArchTest.Rules` sozinho NÃO roda nada, ele só verifica as regras, quem executa é o runner do `xUnit`.

Então, **essa dupla sempre anda junta**:

- `xUnit`: responsável por executar os testes (o runner)
- `NetArchTest.Rules`: responsável por validar as regras de arquitetura

### Como configurar o ambiente completo?

Basta criar um projeto de testes e instalar os pacotes necessários:

```bash
# 1 - Criar o projeto de testes com xUnit
dotnet new xunit -n MeuProjeto.Tests.Arquitetura

# 2 - Adicionar o pacote de validação de arquitetura
dotnet add MeuProjeto.Tests.Arquitetura package NetArchTest.Rules

# (opcional) Melhorar asserts com FluentAssertions (se quiser)
dotnet add MeuProjeto.Tests.Arquitetura package FluentAssertions
```

Pronto! Você agora tem o combo `xUnit` + `NetArchTest.Rules` configurado e pronto para rodar testes de arquitetura.

---

### Por que eles sempre vêm juntos?

Porque o `NetArchTest.Rules` **não possui runner próprio**. Ele só diz se a regra foi quebrada ou não. Quem executa o método `[Fact]`, avalia o resultado e marca o teste como `✔ Passed` ou `❌ Failed` é o `xUnit` ou qualquer outro framework de teste que você estiver usando (mas aqui estamos focando no `xUnit` porque é o mais popular no ecossistema .NET).

Sem o `xUnit` você não testa nada. Sem o `NetArchTest.Rules` você não tem as regras de arquitetura.

---

### Estrutura do projeto

Organize a solution de forma clara:

```
/src
   /MeuProjeto.Dominio
   /MeuProjeto.Aplicacao
   /MeuProjeto.Infraestrutura

/tests
   /MeuProjeto.Tests.Arquitetura   <-- Aqui moram os testes de arquitetura
```

E pra rodar? Simples como deve ser:

```bash
dotnet test
```

---

## Exemplo Prático: Testando Dependências de Camadas

### 1ª Regra: Infraestrutura deve depender de Domínio

```csharp
[Fact]
public void Infraestrutura_Deve_Depender_De_Dominio()
{
    var resultado = Types.InAssembly(typeof(PedidoRepository).Assembly)
                         .That()
                         .ResideInNamespace("MeuProjeto.Infraestrutura")
                         .Should()
                         .HaveDependencyOn("MeuProjeto.Dominio")
                         .GetResult();

    Assert.True(resultado.IsSuccessful, "A camada de infraestrutura deve depender da camada de domínio.");
}
```

Esse teste garante que a Infraestrutura tem ao menos alguma dependência de `MeuProjeto.Dominio`. Isso é útil quando queremos ter certeza de que repositórios, providers e contextos dependem dos modelos e interfaces de domínio.

---

### 2ª Regra: Domínio deve ser puro (não depende de ninguém)

```csharp
[Fact]
public void Dominio_Nao_Deve_Depender_De_Outras_Camadas()
{
    var resultado = Types.InAssembly(typeof(Pedido).Assembly)
                         .That()
                         .ResideInNamespace("MeuProjeto.Dominio")
                         .ShouldNot()
                         .HaveDependencyOnAny("MeuProjeto.Infraestrutura", "MeuProjeto.Aplicacao")
                         .GetResult();

    Assert.True(resultado.IsSuccessful, "A camada de domínio não deve depender de infraestrutura ou aplicação.");
}
```

Essa regra é fundamental! O domínio é o coração da aplicação e precisa ser isolado de quaisquer frameworks ou detalhes externos.

---

### 3ª Regra: Application só pode depender de Domínio

```csharp
[Fact]
public void Aplicacao_Deve_Depender_Apenas_De_Dominio()
{
    var resultado = Types.InAssembly(typeof(PedidoHandler).Assembly)
                         .That()
                         .ResideInNamespace("MeuProjeto.Aplicacao")
                         .Should()
                         .OnlyHaveDependenciesOn("MeuProjeto.Dominio", "System", "System.*")
                         .GetResult();

    Assert.True(resultado.IsSuccessful, "A camada de aplicação deve depender apenas de domínio e bibliotecas do .NET.");
}
```

---

## Benefício direto: Documentação viva da arquitetura

Sim, seu código vira **documentação executável**! Nada de diagramas no Confluence esquecidos ou PowerPoints desatualizados. Seu pipeline agora verifica, a cada PR, se a arquitetura está sendo seguida.

---

## Desafio real: Manter arquitetura sob pressão

É no caos do dia a dia que as arquiteturas morrem. Equipes crescem, squads se formam e o turnover acontece. Cada dev novo aprende a arquitetura no onboarding, mas e depois? Sem enforcement, a arquitetura vira apenas "intenção".

NetArchTest te ajuda a garantir que:

- Suas decisões arquiteturais se mantenham ao longo do tempo.
- As regras sejam automatizadas e verificadas continuamente.
- O código continue alinhado com os princípios definidos.

---

## Outras possibilidades com NetArchTest.Rules

Você pode ir além e criar regras como:

- Proibir ciclos de dependência.
- Impedir o uso de namespaces proibidos (ex: `System.Data.SqlClient` quando se usa Dapper).
- Verificar que DTOs estão em namespaces específicos.
- Validar que interfaces de domínio só são implementadas no domínio.

---

## Analogias para reflexão

Imagine uma obra civil sem inspeção de engenheiros, onde cada pedreiro decide por conta própria onde colocar uma viga ou parede. Essa obra, mesmo que entregue no prazo, teria sua integridade estrutural seriamente comprometida.

É isso que acontece com seu código sem testes de arquitetura.

---

## Provocação final

Quantas vezes você já reescreveu ou refatorou um sistema simplesmente porque a arquitetura foi degradada ao longo dos anos?

E se você tivesse testes de arquitetura desde o começo?

Não subestime o poder de garantir sua arquitetura de forma automática e objetiva.

---

## Veja funcionando na prática!

Quer ver como tudo isso funciona de verdade?  
Nosso projeto case, o [Equinox Project](https://github.com/EduardoPires/EquinoxProject/), traz uma implementação completa e real de testes de arquitetura utilizando o `NetArchTest.Rules`.

É só clonar o repositório, rodar os testes e conferir na prática como validar sua arquitetura de forma automatizada e eficaz.

---

## Conclusão

O `NetArchTest.Rules` é simples, direto e poderoso. Em poucos minutos você escreve um conjunto de regras que protegerão sua arquitetura por anos. O custo de entrada é baixo, e o ganho a longo prazo é incalculável.
