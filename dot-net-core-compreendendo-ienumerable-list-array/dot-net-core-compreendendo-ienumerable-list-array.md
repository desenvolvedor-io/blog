## Coleções são estruturas de dados essenciais em qualquer linguagem. No .NET temos implementações genéricas como `[IList<T>]`, `[ICollection<T>]` e `[IEnumerable<T>]` que são super importantes e de domínio obrigatório para a escrita de código performático.
  
O uso de coleções sempre foi um tema muito interessante de tratar, toda vez que falamos sobre coleções estamos falando também de LINQ, que na minha opinião foi a implementação
mais importante no ecosistema .NET e é algo que todo desenvolvedor(a) deveria dominar.
  
No .NET basicamente podemos converter qualquer coisa pra uma coleção de dados, mas existe algumas coisas que você precisa saber, quando usar por exemplo um: 
`[IList<T>]`, `[ICollection<T>]` ou `[IEnumerable<T>]`, então vamos esclarecer alguns pontos muito importantes sobre esses tipos de coleção.

## Anatomia das coleções

Os exemplos a seguir referem-se às interfaces genéricas `[IList<T>]`, `[ICollection<T>]` e `[IEnumerable<T>]`, pois temos outras interfaces ex: `[IList]`, `[ICollection]` e `[IEnumerable]` não genéricas para implementar comportamentos diferentes.

### `[IEnumerable<T>]`

Implementa a interface não genérica `[IEnumerable]` e o método `[GetEnumerator()]` para acessar a instância de um objeto.
  
### `[ICollection<T>]`
  
Implementa as interfaces `[IEnumerable]` e `[IEnumerable<T>]` e os seguintes métodos

- `[Clear()]`
- `[Add(T item)]`
- `[Remove(T item)]`

Possui a propriedade `[Count]` onde expõe a quantidade de objetos da coleção.
  
### `[IList<T>]`
  
Implementa as interfaces `[ICollection<T>]`, `[IEnumerable<T>]` e `[IEnumerable]` e os seguintes métodos

- `[IndexOf(T item)]`
- `[Insert(int index, T item)]`
- `[RemoveAt(int index)]`

Fornece acesso a um determinado objeto da coleção através do indice, ex `[this[int index]]`
  
  
## Interface `[IEnumerable<T>]`

```csharp
public interface IEnumerable<T> : IEnumerable
{
    IEnumerator<T> GetEnumerator();
}
```

> **Quando usar?**
> 
> Quando você precisar apenas **LER** objetos de uma coleção.
> 
> **Exemplo:**  Retornos de uma consulta ao banco de dados.

## Interface `[ICollection<T>]`

```csharp
public interface ICollection<T> : IEnumerable<T>, IEnumerable
{
    int Count { get; }
    bool IsReadOnly { get; }
    void Add(T item);
    void Clear();
    bool Contains(T item);
    void CopyTo(T[] array, int arrayIndex);
    bool Remove(T item);
}
```

> **Quando usar?**
> 
> Quando você precisar ler objetos de uma coleção, saber o tamanho de sua coleção e até mesmo modificar determinados objetos em sua coleção.

## Interface `[IList<T>]`

```csharp
public interface IList<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
    T this[int index] { get; set; }
    int IndexOf(T item);
    void Insert(int index, T item);
    void RemoveAt(int index);
}
```

> **Quando usar?**
> 
> Quando precisar de tudo que existe no `[IColletion<T>]` e tiver a necessidade de acessar diretamente um objeto de sua coleção por meio de um índice.

```csharp
var list = new List<string>{"A","B","C"};
var item = list[2];
```

O classe genérica `[List<T>]` sempre terá todos objetos em memória e é muito rica em métodos. Com isso podemos observar que seu comportamento é diferente do IEnumerable<T> que não tem seus objetos em memória. Ficou confuso? Vamos entender o por que!

## `[IEnumerable<T>]` vs `[List<T>]`

Imagine um cenário onde temos um lista de tags e precisamos fazer uma consulta:

```csharp  
var tagsList = new List<string>
{
  "CORE",
  "AZURE",
  "EFCORE",
  "SCYLLADB"
  "ASPNETCORE",
};  
```
Efetuando uma consulta:

```csharp
var tags = tagsList.Where(t => t.Length >= 8);
tagsList[0] = "SQLSERVER";

foreach (var tag in tags)
{
    Console.WriteLine(tag);
}
```
  
O esperado era o seguinte resultado:

- SCYLLADB
- ASPNETCORE
  
Pois são maior ou igual a 8 caracteres.
Certo?
  
Não... Está errado!  

Observe que logo após fazer o `[Where]` (na consulta) foi modificado o item `[tagsList[0]]` da lista de tags, no qual foi atriuido um novo valor: `[SQLSERVER]`.

O que isto significa?
  
A ideia é que quando uma consulta da qual o retorno é um `[IEnumerable<T>]` é executada, na realidade a consulta não está trazendo os objetos para a memória, esse tipo de consulta é tardia "(lazy)", ou seja, essa tarefa é adiada para o compilador e teremos acesso ao objeto no momento de sua iteração. 
  
Isso significa que a resposta para a questão acima seria:

- SQLSERVER
- SCYLLADB
- ASPNETCORE

E não:

- SCYLLADB
- ASPNETCORE

O que ocorreu foi que o compilador preservou o estado da consulta, executando-a de fato quando foi feita a iteração com o laço `[foreach (var tag in tags)]`.

Agora se fizermos a mesma consulta com o `[ToList()]`:

```csharp  
var tags = tagsList.Where(t => t.Length >= 8).ToList();
tagsList[0] = "SQLSERVER";

foreach (var tag in tags)
{
    Console.WriteLine(tag);
}
```  
  
Agora sim o retorno será exatamente:

- SCYLLADB
- ASPNETCORE
  
Isso porque quando o `[ToList()]` é executado ele imediatamente carrega os objetos para memória e deixa disponível para o consumidor. 
Qualquer alteração na lista após executar o método `[ToList()]` não terá mais nenhum efeito.

## Considerações

`[List<T>]` implementa `[IEnumerable<T>]`, mas toda a coleção está na memória, ou seja, o carregamento é adiantado (eager).
  
  
`[IEnumerable<T>]` contém um método que obtém o próximo item da coleção, não é necessário alocar tudo em memória, nem se sabe quantos itens existem na coleção.
Logo o que ele faz é chamar o próximo item `[MoveNext()]`, até que não haja mais nenhuma posição para ser lida.
