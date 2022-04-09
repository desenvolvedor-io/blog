## Obter dados do usuário logado em determinada operação pode ser uma tarefa complicada dependendo da camada em que está ocorrendo o processamento. Esta dificuldade não existe no ASP.NET.

O ASP.NET possui uma estrutura de interfaces projetadas para facilitar diversas implementações sem corromper a responsabilidade de determinada camada. Essas interfaces foram desenhadas para desacoplar qualquer responsabilidade ou até mesmo dependências diretamente ligadas ao ASPNET.

## Cenário 1
Suponha que determinados tipos de registros de banco de dados devam ser inseridos com o ID do usuário logado. Esta necessidade é muito comum devido auditoria ou outros fatores. Como repassar as informações do usuário até chegar na camada de dados onde está o repositório?

## Cenario 2
Em uma aplicação multi-tenancy é necessário validar se o usuário pertence ao grupo que pode acessar ou modificar determinados registros.

## Cenario 3
É necessário validar as permissões do usuário baseadas em roles ou claims em alguma camada que não seja a camada de apresentação (ASP.NET), muitas vezes na camada de Application esta validação é requerida.

## Resumindo
Solução para resolver estes cenários comuns sempre existiu, o difícil mesmo era escrever uma solução elegante e que preservasse a responsabilidade da camada (não devemos deixar que camadas de domínio e dados por ex dependam do contexto de usuário do ASP.NET Identity).

## Solução
O ASP.NET possui uma biblioteca de abstrações HTTP que possui uma interface chamada *IHttpContextAccessor*. Esta interface possui uma property do nosso conhecido HttpContext e é implementada na classe HttpContextAccessor.

Isso significa que podemos injetar esta interface e obter dados do HttpContext.

Mas calma! Não vá por ai... Vamos melhorar isso!

Primeiramente registre no container de DI as dependencias:

```csharp
// Na program.cs ou Startup.cs, depende da versão do ASP.NET
// ASP.NET HttpContext dependency

services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
```

Agora vamos criar uma interface que vai representar nosso usuário logado, esta interface pode ser criada na camada de domínio. A modelagem desta interface pode ser feita para atender as suas necessidades em relação a manipulação dos dados do usuário:

```csharp
using System.Collections.Generic;
using System.Security.Claims;

public interface IUser
{
    string Name { get; }
    bool IsAuthenticated();
    IEnumerable<Claim> GetClaimsIdentity();
}
```

Na sequência crie a classe que representa o usuário logado, esta classe pode ficar numa camada de Infra (eu costumo isolar o Identity em Infra então esta classe já tem lugar apropriado).

Repare que a interface *IHttpContextAccessor* está sendo injetada no construtor e será através dela que obteremos os dados necessários:

```csharp
using System.Collections.Generic;
using System.Security.Claims;

public class AspNetUser : IUser
{
    private readonly IHttpContextAccessor _accessor;

    public AspNetUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string Name => _accessor.HttpContext.User.Identity.Name;

    public bool IsAuthenticated()
    {
        return _accessor.HttpContext.User.Identity.IsAuthenticated;
    }

    public IEnumerable<Claim> GetClaimsIdentity()
    {
        return  _accessor.HttpContext.User.Claims;
    }
}
```

Esta implementação permite

- Obter o Nome do usuário logado
- Validar se está autenticado
- Obter a lista de Claims (utilizada para armazenar dados extras e permissões)
- Consultar todos os dados disponíveis conforme o ASP.NET disponibiliza

Para utilizarmos esta classe via injeção de dependência devemos registrá-la também:

```csharp
// Na program.cs ou Startup.cs, depende da versão do ASP.NET

services.AddScoped<IUser, AspNetUser>();
```

A implementação está pronta e agora a aplicação pode obter dados do usuário logado em qualquer camada e em qualquer momento da operação. 

No exemplo a seguir o usuário está sendo injetado no repositório para que o registro contenha o dado do usuário que efetuou a ação:

```csharp
public class MeuRepositorio : IMeuRepositorio
{
    private readonly IUser _user;

    public MeuRepositorio(IUser user)
    {
        _user = user;
    }

    public void Adicionar(MinhaEntidade entidade)
    {
        entidade.UsuarioRegistro = _user.Name

        DbContext.MinhaEntidade.Add(entidade);
        DbContext.SaveChanges();
    }
}
```

Além desta implementação ter ficado muito elegante e 100% responsável é importante lembrar que também é totalmente testável. 

Os créditos desta implementação vão para a abstração na implementação das classes do ASP.NET e para a injeção de dependência que como sempre digo não é luxo, é obrigação.

Em meu projeto de referência o Equinox Project esta abordagem está implementada e sendo utilizada no storage de eventos utilizado pelo Event Sourcing.

Além disso deixei uma demo bem direto ao ponto aqui também:

- [Demo](https://github.com/desenvolvedor-io/blog/tree/main/asp-net-acesse-o-usuario-da-aplicaca-de-qualquer-camada/Demo/AspNetUserDemo)
- [Equinox Project](https://github.com/EduardoPires/EquinoxProject)

Espero que tenha gostado, não deixe de compartilhar e veja abaixo um curso que te ensinará tudo que precisa sobre ASP.NET :D
