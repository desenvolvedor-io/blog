## Abordaremos neste artigo um dos assuntos que é extremamente importante para uma aplicação muito mais performática e muita das vezes somos omissos seja por falta de conhecimento ou por existir uma demanda de entregas rápidas em nosso dia-a-dia e sempre deixamos melhorias de performance como dívida técnica, pois bem aqui é onde mora o perigo, na maioria das vezes não costumamos pagar esse tipo de dívida seja por esquecimento ou por existir a necessidade de entregar novas features, mas de alguma forma o universo costuma cobrar da gente e geralmente é da pior forma possível, um exemplo simples e que acontece frequentemente é o conhecido crash de container por falta de recurso seja memória ou disco.

## Introdução
Estamos vivendo a era da computação em nuvem, onde frequentemente ouvimos falar de sistemas distribuídos, resiliência, escalabilidade horizontal e outras coisas legais, 
pois bem uma dessas coisas legais é o Kubernetes, geralmente utilizamos ele para fornecer a capacidade de escalar o processamento de dados e fornecer várias instâncias 
de nossas aplicações, com isso limitamos os recursos de cada pod/container para usar a menor unidade de recurso possível, sendo assim customizamos o limite de memória 
que será utilizado, aqui é onde começamos a pensar fora da caixa, ou seja, será que estamos nos preocupando com essa limitação de recurso?! 
Memory leak é um dos problemas mais comuns que ocorrem em uma aplicação dentro de um container por falta do bom gerenciamento de memória, sendo assim vamos ver como 
podemos escrever aplicações mais performáticas fazendo um bom gerenciamento de memória. Faremos um compilado de dicas e boas práticas para obter o melhor desempenho 
com .NET em nossas aplicações diminuindo alocações na memória e coletas do GC (Garbage Collector).

Vamos colocar a mão na massa!

## Destrutores são um pesadelo para sua aplicação
Em .NET todo objeto que herda o tipo class pode ter um construtor e um destrutor. Geralmente usamos no destrutor instruções para limpar objetos na memória não 
gerenciada ou seja, que não estão na Heap, com isso evitamos vazamento de memória, mas existe outra forma de fazer isso, um exemplo é utilizar um Pattern Dispose, 
a coleta feita pelo GC na geração 0 é a mais rápida, mas quando usamos finalizadores e o GC inicializa o ciclo de coleta e encontra um objeto com um destrutor, esse 
objeto sobrevive à primeira coleta e é promovido para próxima geração sendo colocado em uma de fila de finalização, portanto quando é chamado o Finalize internamente 
pela thread dedicada e responsável por fazer essas execuções o objeto se torna legível para ser recuperado e liberado da memória, para provar isso faremos um benchmark 
para ver o quão custoso é um destrutor em sua classe mesmo que esteja vazio, que para muitos pode ser inofensivo.

### As seguintes classes serão utilizadas como exemplos:

```csharp
public class ClasseSemFinalizador
{
    public int Id { get; set; }
    public string Nome { get; set; }
}

public class ClasseComFinalizador
{
    public int Id { get; set; }
    public string Nome { get; set; }

    ~ClasseComFinalizador()
    {
       // Fazer algo
    }
}
```

Iremos utilizar a biblioteca BenchmarkDotNet para rastrear e analisar o desempenho, temos dois métodos responsáveis por criar em algumas fases 
(1.000, 10.000 e 100.000) instâncias das classes acima apresentadas.

```csharp
[MemoryDiagnoser]
public class PerformanceDestrutor
{
    [Params(1_000, 10_000, 100_000)]
    public int Size { get; set; }

    [Benchmark]
    public void ComFinalizador()
    {
        for (int i = 0; i < Size; i++)
        {
            var classe = new ClasseComFinalizador
            {
                Id = i,
                Nome = "Teste"
            };
        } 
    }

    [Benchmark]
    public void SemFinalizador()
    {
        for (int i = 0; i < Size; i++)
        {
            var classe = new ClasseSemFinalizador
            {
                Id = i,
                Nome = "Teste"
            }; 
        } 
    }
}
```

Para saber como utilizar a biblioteca BenchmarkDotNet basta acessar BenchmarkDotNet apos executar o teste de performance vamos analisar o resultado produzido 
na seguinte imagem:

```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
Intel Core i7-7500U CPU 2.70GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
.NET SDK=6.0.100-preview.6.21355.2
  [Host]     : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT
  DefaultJob : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT


|         Method |   Size |          Mean |       Error |    Gen 0 |    Gen 1 | Allocated |
|--------------- |------- |--------------:|------------:|---------:|---------:|----------:|
| ComFinalizador |   1000 |    133.693 us |   2.6418 us |   3.4180 |   1.7090 |     31 KB |
| SemFinalizador |   1000 |      8.018 us |   0.1612 us |   3.4637 |        - |     31 KB |
| ComFinalizador |  10000 |  1,333.029 us |  26.5482 us |  33.2031 |  15.6250 |    312 KB |
| SemFinalizador |  10000 |     76.591 us |   1.5304 us |  34.6680 |        - |    312 KB |
| ComFinalizador | 100000 | 13,295.902 us | 262.2954 us | 343.7500 | 171.8750 |  3,125 KB |
| SemFinalizador | 100000 |    779.552 us |  15.4507 us | 347.6563 |        - |  3,125 KB |
```

Fica óbvio que podemos degradar consideravelmente a performance de nossa aplicação, mesmo usando um destrutor vazio temos um custo alto de aproximadamente 
1700% ao utilizar classes com destrutor comparado a uma classe que não possui destrutor, observando melhor temos vários objetos que foram promovidos para geração 1, 
apenas só por existir um destrutor vazio na classe, sendo assim se existir a necessidade de liberar recursos na memória não gerenciada utilize o Pattern Dispose 
você vai ter um melhor ganho de performance além de diminuir significativamente a quantidade de coletas feitas pelo GC.

## Concatenar string ou utilizar StringBuilder ?

É muito comum existir a necessidade de concatenar strings durante o ciclo de desenvolvimento de um software, muitas das vezes é por existir a necessidade de 
construir algum tipo de informação com objetivo de passar para um algoritmo que possa processar esse dado, uma string é um dado imutável, significa que quando 
queremos concatenar um caractere ou uma nova cadeia de caracteres a uma string o que está acontece na verdade é uma nova cópia na memória com os dados novos 
concatenados.

![image](https://user-images.githubusercontent.com/5068797/169920546-0dece42e-3731-4640-8813-b209a4a295dd.png)

Quando usamos StringBuilder o que acontece é um comportamento um pouco diferente, basicamente ele reserva um espaço na memória e os novos caracteres são inseridos nesse buffer sem existir a necessidade de fazer uma nova cópia na memória dos 
dados que estão sendo inseridos.

![image](https://user-images.githubusercontent.com/5068797/169920571-dfac00b6-c867-4f89-a2e8-a7179b160478.png)

Vamos pegar um exemplo hipotético aqui onde precisamos montar uma string no formato JSON, é apenas para nossa didática, dado que temos classes robustas dedicadas 
para serializar e desserializar objetos, para isso temos dois métodos, um que concatena caracteres fazendo a junção de duas strings e outro que utiliza StringBuilder, 
veja a imagem seguinte:

```csharp
[MemoryDiagnoser]
public class ManipularString
{
    [Params(1, 100, 1000)]
    public int Size { get; set; }

    [Benchmark]
    public string ConcatenacaoString()
    {
        var json = "[";
        for (int i = 0; i <= Size; i++)
        {
            json += "{";
            json += "\"id\"";
            json += ":";
            json += i;
            json += "}";
        }
        json += "]";
        return json;
    }

    [Benchmark]
    public string StringBuilderString()
    {
        var json = new StringBuilder("[", 100);
        for (int i = 0; i <= Size; i++)
        {
            json.Append("{").Append("\"id\"") 
                .Append(":").Append(i) 
                .Append("}");
        }
        json.Append("]");

        return json.ToString();
    }
}
```

Depois de executar nosso teste de performance podemos analisar o benchmark e confirmar que o primeiro método que faz junção de string é muito mais lento e aloca mais espaço.

```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
Intel Core i7-7500U CPU 2.70GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
.NET SDK=6.0.100-preview.6.21355.2
  [Host]     : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT
  DefaultJob : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT


|              Method | Size |           Mean |     Gen 0 |    Gen 1 |    Allocated |
|-------------------- |----- |---------------:|----------:|---------:|-------------:|
|  ConcatenacaoString |    1 |       268.8 ns |    0.0572 |        - |        528 B |
| StringBuilderString |    1 |       150.5 ns |    0.0367 |        - |        336 B |
|  ConcatenacaoString |  100 |    78,626.9 ns |   51.0254 |   0.1221 |    469,184 B |
| StringBuilderString |  100 |     4,270.6 ns |    0.5875 |        - |      5,392 B |
|  ConcatenacaoString | 1000 | 8,968,043.9 ns | 5562.5000 | 109.3750 | 49,249,192 B |
| StringBuilderString | 1000 |    64,200.5 ns |    5.1880 |   0.2441 |     46,008 B |
```

Conforme a quantidade de caracteres vão crescendo temos um custo maior para copiar esses dados na memória para um novo endereço além de alocar muito mais 
espaço na memória, e se multiplicar isso em um aplicação que trabalha com muita threads podemos chegar a uma conclusão que iremos degradar a performance de nossa 
aplicação, sendo assim utilize sempre que possível StringBuilder para concatenar strings, o GC e sua memória agradece.

## Regex e suas armadilhas

Regex sem sombra de dúvidas é um dos recursos mais fantásticos que podemos ter em uma linguagem de programação, ele nos proporciona uma excelente produtividade.

O .NET nos oferece dois sabores de Regex, o interpretado e o compilado, vamos testar a performance de ambos, para isso iremos usar o seguinte cenário no qual 
precisamos saber se uma string contém números e para isso iremos usar o Regex, na imagem a seguir temos dois métodos um que utiliza uma instância do objeto Regex 
interpretado e outro que utiliza a instância do Regex Compilado os dois utilizam o mesmo pattern que é validar se existe números em uma string.

```csharp
[MemoryDiagnoser]
public class PerformanceRegex
{
    private const string _dados = "lUk*avdr!ZhbbNF^J7yxsGueVAufYC3ixB8vqt";
    private const string _pattern = @"[0-9]";
    private readonly Regex _regexNaoCompilado = new(_pattern);
    private readonly Regex _regexCompilado = new(_pattern, RegexOptions.Compiled);

    [Benchmark]
    public void RegexNormal()
    {
        for (int i = 0; i < Size; i++)
        {
            _ = _regexNaoCompilado.IsMatch(_dados);
        } 
    }

    [Benchmark]
    public void RegexCompilado()
    {
        for (int i = 0; i < Size; i++)
        {
            _ = _regexCompilado.IsMatch(_dados);
        }
    }
    
    [Params(100, 1_000, 10_000)]
    public int Size { get; set; }
}
```

Depois de executar os testes de performance obtemos o seguinte resultado:

```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
Intel Core i7-7500U CPU 2.70GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
.NET SDK=6.0.100-preview.6.21355.2
  [Host]     : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT
  DefaultJob : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT


|           Method |  Size |             Mean |          Error |         StdDev |
|----------------- |------ |-----------------:|---------------:|---------------:|
|      RegexNormal |   100 |        20.858 us |      0.4311 us |      1.2643 us |
|   RegexCompilado |   100 |         7.929 us |      0.1889 us |      0.5570 us |
|      RegexNormal |  1000 |       206.609 us |      4.1243 us |      8.1409 us |
|   RegexCompilado |  1000 |        80.109 us |      1.5968 us |      4.3171 us |
|      RegexNormal | 10000 |     2,125.230 us |     68.2312 us |    196.8627 us |
|   RegexCompilado | 10000 |       799.817 us |     15.9848 us |     36.7277 us |
```

 Fica explicitamente claro que temos um ganho de aproximadamente 260% ao utilizar o Regex compilado, quando estamos processando um alto volume de dados isso 
 faz toda diferença, mas certamente podemos melhorar isso e pensar um pouco fora da caixa, o uso do Regex gera um pequeno custo adicional no quesito performance 
 em nossa aplicação, existem cenários que podemos escrever nosso próprio algoritmo para fazer pequenas otimizações e esse é um deles, não necessariamente 
 precisamos de Regex para saber se existe ou não número em uma string, vamos então vamos utilizar seguinte método para comparar a performance.

```csharp
[Benchmark]
public void MetodoCustomizado()
{
    for (int i = 0; i < Size; i++)
    {
        _ = ContemNumero(_dados.AsSpan());
    }
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool ContemNumero(ReadOnlySpan<char> span)
{
    for (var i = 0; i < span.Length; i++)
    {
        if (span[i] >= '0' && span[i] <= '9')
        {
            return true;
        }
    }

    return false;
}

```

Executando os testes de performance novamente obtivemos o seguinte resultado:

```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
Intel Core i7-7500U CPU 2.70GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
.NET SDK=6.0.100-preview.6.21355.2
  [Host]     : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT
  DefaultJob : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT


|            Method |  Size |             Mean |          Error |         StdDev |
|------------------ |------ |-----------------:|---------------:|---------------:|
|       RegexNormal |   100 |        20.858 us |      0.4311 us |      1.2643 us |
|    RegexCompilado |   100 |         7.929 us |      0.1889 us |      0.5570 us |
| MetodoCustomizado |   100 |         1.335 us |      0.0265 us |      0.0428 us |
|       RegexNormal |  1000 |       206.609 us |      4.1243 us |      8.1409 us |
|    RegexCompilado |  1000 |        80.109 us |      1.5968 us |      4.3171 us |
| MetodoCustomizado |  1000 |        16.470 us |      0.3225 us |      0.6137 us |
|       RegexNormal | 10000 |     2,125.230 us |     68.2312 us |    196.8627 us |
|    RegexCompilado | 10000 |       799.817 us |     15.9848 us |     36.7277 us |
| MetodoCustomizado | 10000 |       162.689 us |      3.1943 us |      4.5811 us |
```

Fica claro que tivemos um absurdamente de performance comparado com o Regex, se analisar corretamente temos um ganho de aproximadamente 590% sobre o Regex 
compilado e 1.560% sobre o Regex interpretado isso só prova que sempre que possível devemos escrever nossos próprios algoritmos, vamos ver uma das grandes 
desvantagens de utilizar o Regex de forma errônea, o cenário é o seguinte, você não quer escrever algoritmos e quer se beneficiar da performance do Regex 
compilado dado que ele é mais performático que o interpretado certo? Errado, se não souber usar ele de forma correta pode ser seu maior problema de performance, 
em vez de utilizar as instâncias do Regex estaticamente como apresentado anteriormente vamos instanciar a cada execução e comparar sua performance, vamos utilizar 
os seguintes métodos:

```csharp
[Benchmark]
public void RegexNormalInstanciado()
{
    for (int i = 0; i < Size; i++)
    {
        var regex = new Regex(_pattern);
        _ = regex.IsMatch(_dados);
    }
}

[Benchmark]
public void RegexCompiladoInstanciado()
{
    for (int i = 0; i < Size; i++)
    {
        var regex = new Regex(_pattern, RegexOptions.Compiled);
        _ = regex.IsMatch(_dados);
    }
}
```
Novamente depois de executar todos os testes obtivemos o seguinte resultado:

```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
Intel Core i7-7500U CPU 2.70GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
.NET SDK=6.0.100-preview.6.21355.2
  [Host]     : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT
  DefaultJob : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT


|                    Method |  Size |             Mean |     Gen 0 |     Gen 1 |     Gen 2 |    Allocated |
|-------------------------- |------ |-----------------:|----------:|----------:|----------:|-------------:|
|               RegexNormal |   100 |        20.858 us |         - |         - |         - |            - |
|            RegexCompilado |   100 |         7.929 us |         - |         - |         - |            - |
|         MetodoCustomizado |   100 |         1.335 us |         - |         - |         - |            - |
|    RegexNormalInstanciado |   100 |       253.201 us |   29.2969 |         - |         - |    268,000 B |
| RegexCompiladoInstanciado |   100 |    87,663.636 us |         - |         - |         - |    776,800 B |
|               RegexNormal |  1000 |       206.609 us |         - |         - |         - |            - |
|            RegexCompilado |  1000 |        80.109 us |         - |         - |         - |            - |
|         MetodoCustomizado |  1000 |        16.470 us |         - |         - |         - |            - |
|    RegexNormalInstanciado |  1000 |     2,536.451 us |         - |         - |2,680,000 B|            - |
| RegexCompiladoInstanciado |  1000 |   861,472.006 us |         - |         - |         - |  7,768,000 B |
|               RegexNormal | 10000 |     2,125.230 us |         - |         - |         - |            - |
|            RegexCompilado | 10000 |       799.817 us |         - |         - |         - |            - |
|         MetodoCustomizado | 10000 |       162.689 us |         - |         - |         - |            - |
|    RegexNormalInstanciado | 10000 |    25,384.456 us | 2937.5000 |         - |         - | 26,800,000 B |
| RegexCompiladoInstanciado | 10000 | 8,872,594.393 us | 9000.0000 | 5000.0000 | 1000.0000 | 78,220,016 B |
```

Não é porque o Regex é compilado que será sempre mais rápido, como podemos observar ele ficou drasticamente muito mais lento e fez com que objetos fossem 
promovidos praticamente em todas as gerações pelo GC além de alocar muitos objetos na memória, podemos resolver isso? Sim, Essa lentidão apresentada é porque 
existe um custo no momento de criar uma instância do objeto Regex, isso porque o código do Regex é compilado em tempo de execução para ser otimizado, uma boa 
prática para melhorar a performance é reutilizar a instância do objeto, se sua aplicação não tem a necessidade constante de alterar a expressão que o regex irá 
utilizar então instanciar os objetos irá fazer com o tempo utilizado na compilação seja evitado.

Uma outra dica importante ao utilizar o Regex é aplicar Timeout dado que nossas expressões se beneficiam de retrocesso com objetivo de fazer otimização, 
para mais informações sobre retrocesso basta acessar: Microsoft retrocesso, o timeout garante que a expressão seja validada dentro de uma janela de tempo 
específica, se não for processada no intervalo especificado será lançada uma exception: RegexMatchTimeoutException.

## Considerações

### Lições aprendidas com Regex:
- Podemos escrever sempre um algoritmo melhor
- Regex Compilado não é bala de prata
- Não crie instância do Regex para cada validação se a expressão não muda
- Se vai utilizar Regex escolha sempre que possível Regex compilado

