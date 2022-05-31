## Coleções são estruturas de dados essenciais em qualquer linguagem. No .NET temos implementações genéricas como `[IList<T>]`, `[ICollection<T>]`, `[IEnumerable<T>]` que são super importantes e de domínio obrigatório para a escrita de código performático.
  
Coleções sempre foi um tema muito interessante de tratar, toda vez que falamos sobre coleções estamos falando também de LINQ, que na minha opinião foi a implementação
mais importante no ecosistema .NET e é algo que todo desenvolvedor(a) deveria dominar.
  
No .NET basicamente podemos converter qualquer coisa pra uma coleção de dados, mas existe algumas coisas que você precisa saber, quando usar por exemplo um: 
`[IList<T>]`, `[ICollection<T>]` ou `[IEnumerable<T>]`, então vamos esclarecer alguns pontos muito importantes sobre esses tipos de coleção.

## Anatomia das coleções
Esse é um exemplo simples que refere-se às interfaces genéricas `[IList<T>]`, `[ICollection<T>]` e `[IEnumerable<T>]` já que temos outras interfaces IList, ICollection e IEnumerable não genéricas para implementar comportamentos diferentes.

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
  
  
Interface IEnumerable<T>
public interface IEnumerable<T> : IEnumerable
{
    IEnumerator<T> GetEnumerator();
}
Quando usar?
Quando você precisar apenas ler objetos de uma coleção.
Exemplo: aquelas consultinhas que você faz no banco e apenas serializa o resultado.
Interface ICollection<T>
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
Quando usar?
Quando você precisar ler objetos de uma coleção, saber o tamanho de sua coleção e até mesmo modificar determinados objetos em sua coleção.
Interface IList<T>
public interface IList<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
    T this[int index] { get; set; }
    int IndexOf(T item);
    void Insert(int index, T item);
    void RemoveAt(int index);
}
Quando usar?
Quando precisar de tudo que existe no IColletion<`T`> e tiver a necessidade de acessar diretamente um objeto de sua coleção por meio de um índice.
var list = new List<string>{"A","B","C"};
var item = list[2];
O classe genérica List<T> sempre terá todos objetos em memória, além de ser muito rica em métodos, dado esse cenário já podemos observar que seu comportamento é diferente do IEnumerable<T> que não tem seus objetos em memória, ixi ficou confuso, calma vamos entender essa confusão.

IEnumerable<T> vs List<T>
Vamos pensar em um cenário onde temos um lista de tags e precisaremos fazer uma consulta.

var tagsList = new List<string>
{
  "CORE",
  "AZURE",
  "EFCORE",
  "SCYLLADB"
  "ASPNETCORE",
};
Agora vamos efetuar uma consulta:

var tags = tagsList.Where(t => t.Length >= 8);
tagsList[0] = "SQLSERVER";

foreach (var tag in tags)
{
    Console.WriteLine(tag);
}
Vamos analisar aqui, fizemos uma consulta que esperariamos o seguinte resultado:

SCYLLADB
ASPNETCORE
que são maior ou igual a 8 caracteres.

OK? …errado!
Observe que logo após fazer meu where(minha consulta) eu modifiquei o item tagsList[0] de minha lista de tags, no qual eu atribui um novo valor para o mesmo SQLSERVER.
Onde quero chegar com isso?
O que quero dizer aqui é, quando você executa uma consulta que seu retorno é um IEnumerable<T>, na verdade ele não está trazendo os objetos para memória como falei um pouco acima, essa consulta é retardada, essa tarefa é adiada para o compilador, e você só vai ter acesso ao objeto no momento de sua iteração. Isso significa que a resposta para nossa pergunta acima seria:

SQLSERVER
SCYLLADB
ASPNETCORE
e não:

SCYLLADB
ASPNETCORE
Como algumas pessoas poderiam pensar, o que aconteceu foi que, o compilador preservou o estado de minha consulta, executando-a de fato quando fiz a iteração com a consulta foreach (var tag in tags).

Agora vamos fazer a mesma consulta com ToList():

var tags = tagsList.Where(t => t.Length >= 8).ToList();
tagsList[0] = "SQLSERVER";

foreach (var tag in tags)
{
    Console.WriteLine(tag);
}
Agora sim o retorno será exatamente:

SCYLLADB
ASPNETCORE
Isso porque quando executo o ToList() ele imediatamente carrega os objetos para memória e deixa disponível para o consumidor, então qualquer alteração em minha lista após executar o método ToList() não terá mais nenhum efeito sobre a mesma.

Considerações
List<`T`> implementa IEnumerable<`T`>, mas toda a coleção está na memória, ou seja o carregamento foi adiantado.
IEnumerable<`T`> contém um método que obtém o próximo item de sua coleção, ele não precisa alocar tudo em memória, ele nem sabe quantos itens existe em sua coleção, então basicamente o que ele faz é chamar o próximo item MoveNext(), até que não existe mais nenhum dado para ser lido.
