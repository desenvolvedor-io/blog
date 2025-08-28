Gerenciar pacotes NuGet sempre foi um mal necessário em soluções .NET. Em projetos pequenos, tudo parece sob controle: você adiciona um pacote aqui, outro ali, atualiza quando precisa e vida que segue.

Mas basta a solução crescer, dez, vinte ou cinquenta projetos para o castelo desmoronar. De repente, o que era simples se transforma em um jogo de “onde está a versão errada?”, builds quebram, e o time inteiro perde tempo em tarefas que deveriam ser triviais.

Foi exatamente para acabar com essa dor que surgiu o **Central Package Management (CPM)**.

Ele centraliza a definição de versões em um único arquivo, simplifica a manutenção e garante consistência em toda a solution. Mas não para por aí: também dá controle sobre dependências transitivas, permite injeção de pacotes globais e ainda traz governança para equipes grandes.

Neste artigo, você vai entender:

- O **problema real** que o CPM resolve.
- **Como funciona por baixo do capô**.
- **Exemplos práticos** de uso em projetos reais.
- E por que **você não pode ignorar essa feature** no seu dia a dia como dev .NET.

---

## A dor (que você já normalizou sem perceber)

Você tem uma solução com 12 projetos. Em algum momento, cada um recebeu sua própria versão de `Newtonsoft.Json`, `Serilog`, `FluentValidation`.

Hoje:

- API → `Newtonsoft.Json` **12.0.3**
- Domain → `Newtonsoft.Json` **13.0.1**
- Infra → `Newtonsoft.Json` **12.0.1**

Sintomas:

- **Build intermitente** (compila na sua máquina, quebra no CI).
- **Bugs “fantasmas”** vindos de mudanças transitivas.
- **Atualizações lentas**: você abre 12 arquivos pra subir uma versão.
- **Zero governança**: ninguém sabe qual é a “versão oficial” de nada.

Essa é a **taxa oculta** que seu time paga todos os meses.

---

## A luz: Central Package Management (CPM)

O **CPM** cria uma autoridade central de versões: um arquivo **`Directory.Packages.props`** na raiz da solution.

Os `.csproj` continuam dizendo **o que** usar; **a versão** passa a vir do arquivo central.

**Benefício imediato:** consistência, atualização em um lugar só e `.csproj` limpos.

---

## Como funciona “por baixo do capô”

### Descoberta e opt-in

- O restore do NuGet/MSBuild **procura `Directory.Packages.props` subindo diretórios** a partir do projeto e usa **o mais próximo** encontrado.
- Ative o modo central com:

```xml
<!-- Directory.Packages.props (na raiz da solution) -->
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <PackageVersion Include="Newtonsoft.Json" Version="13.0.1" />
    <PackageVersion Include="Serilog"         Version="3.1.1"  />
    <PackageVersion Include="FluentValidation" Version="11.7.0" />
  </ItemGroup>
</Project>
```

### O que muda nos projetos

Nos `.csproj`, **remova `Version`** dos `PackageReference` a versão é resolvida centralmente:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" />
    <PackageReference Include="Serilog" />
    <PackageReference Include="FluentValidation" />
  </ItemGroup>
</Project>
```

### Como um projeto “olha” (ou não) pro central

- **Olha automaticamente** para o `Directory.Packages.props` **mais próximo** na hierarquia.
- **Pode ignorar** com opt-out local:

```xml
<PropertyGroup>
  <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
</PropertyGroup>
```

- **Pode compor políticas** (um props por pasta + um global). Se quiser que um props “filho” herde do “pai”, faça um **`<Import/>`** no filho:

```xml
<Project>
  <!-- importa o props do nível acima, se você quiser compor -->
  <Import Project="..\Directory.Packages.props" />
  <ItemGroup>
    <!-- ajustes locais para essa sub-árvore -->
    <PackageVersion Include="Serilog" Version="3.1.2" />
  </ItemGroup>
</Project>
```

### Exceções por projeto (quando você **precisa** furar a fila)

Se um projeto **específico** precisa de uma versão diferente, use **`VersionOverride`** nele:

```xml
<ItemGroup>
  <PackageReference Include="Serilog" VersionOverride="3.1.0" />
</ItemGroup>
```

Você pode **bloquear** overrides no repo:

```xml
<PropertyGroup>
  <CentralPackageVersionOverrideEnabled>false</CentralPackageVersionOverrideEnabled>
</PropertyGroup>
```

### Pinagem de **dependências transitivas**

Um detalhe que muita gente esquece é que, ao instalar um pacote, você não está trazendo só ele:

ele também carrega uma cadeia de **dependências transitivas**.

Exemplo: você instala `Serilog.AspNetCore`.

Ele traz junto `Serilog`, que por sua vez pode trazer `System.Diagnostics.DiagnosticSource`, e por aí vai.

O problema: se um outro pacote da sua solução também trouxer `Serilog` (ou `System.Text.Json`) mas em versão diferente, o NuGet vai tentar conciliar e nem sempre da forma que você quer. Isso gera **conflitos de versão, builds quebrados e comportamento inesperado em runtime**.

### Como resolver

Com o **Central Package Management**, você pode ligar a propriedade:

```xml
<PropertyGroup>
  <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
</PropertyGroup>

<ItemGroup>
  <!-- Mesmo sem PackageReference direto, essa versão é a oficial -->
  <PackageVersion Include="System.Text.Json" Version="8.0.4" />
</ItemGroup>
```

**Regras importantes:**

- Você **só pode manter ou subir** a versão de uma dependência transitiva.
    
    Se um pacote exige no mínimo a versão 8.0.0, você pode fixar em 8.0.4, mas não em 7.5.0.
    
- O NuGet usa essa versão como **fonte de verdade** durante o restore, garantindo consistência entre todos os projetos.
- Isso é especialmente útil em cenários de **segurança** (quando sai um patch de vulnerabilidade em uma transitiva) ou quando você precisa evitar que projetos diferentes tragam versões divergentes da mesma biblioteca.

**Antes x Depois**

- **Antes:** transitivas “decidiam sozinhas” qual versão usar, podendo variar entre projetos.
- **Depois:** você define no arquivo central a versão oficial, e toda a solution obedece.

### “Pacotes de todos os projetos” com **GlobalPackageReference**

`PackageVersion` só define versão **se** o projeto referenciar.

Se você quer **injetar** um pacote em **todos** os projetos (analisadores, toolings), use **`GlobalPackageReference`**:

```xml
<ItemGroup>
  <GlobalPackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.507" />
</ItemGroup>
```

**Diferença chave**:

- `PackageVersion` = **política de versão** (passiva).
- `GlobalPackageReference` = **injeção global** (ativa).
    
    Use para **analyzers e ferramentas de build**, não para libs de runtime.
    

### Condicionais por **Target Framework**

Precisa de versões diferentes por TFM? Sem drama:

```xml
<ItemGroup>
  <PackageVersion Include="PackageA" Version="1.0.0"
                  Condition="'$(TargetFramework)'=='netstandard2.0'" />
  <PackageVersion Include="PackageA" Version="2.0.0"
                  Condition="'$(TargetFramework)'=='net8.0'" />
</ItemGroup>
```

### Metadados centralizados (PrivateAssets/IncludeAssets)

Você pode **padronizar metadados** para todos os projetos:

```xml
<ItemGroup>
  <PackageVersion Include="Some.Analyzers" Version="1.0.0"
                  PrivateAssets="All" IncludeAssets="runtime; build; native; contentfiles; analyzers; buildtransitive" />
</ItemGroup>
```

---

## Exemplo “mundo real” (cola na sua solution)

```
/src
  /Api
    Api.csproj
  /Domain
    Domain.csproj
  /Infra
    Infra.csproj
/tests
  /Api.Tests
    Api.Tests.csproj
Directory.Packages.props
```

**`Directory.Packages.props` (raiz):**

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
    <CentralPackageVersionOverrideEnabled>true</CentralPackageVersionOverrideEnabled>
  </PropertyGroup>

  <ItemGroup>
    <!-- Pacotes “core” padronizados -->
    <PackageVersion Include="Newtonsoft.Json" Version="13.0.1" />
    <PackageVersion Include="Serilog"         Version="3.1.1" />
    <PackageVersion Include="FluentValidation" Version="11.7.0" />

    <!-- Pino transitivo crítico -->
    <PackageVersion Include="System.Text.Json" Version="8.0.4" />
  </ItemGroup>

  <!-- Analyzer global para tudo -->
  <ItemGroup>
    <GlobalPackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.507"
                            PrivateAssets="All" />
  </ItemGroup>
</Project>
```

**`Api.csproj` (limpo, sem versões):**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" />
    <PackageReference Include="Serilog" />
  </ItemGroup>
</Project>
```

**`Infra.csproj` (exceção controlada):**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <!-- Precisa travar numa versão específica? ok, mas visível e auditável -->
    <PackageReference Include="Serilog" VersionOverride="3.1.0" />
  </ItemGroup>
</Project>
```

---

## Guia “colou-rodou” (5 minutos)

1. **Gerar o arquivo base**
    
    ```
    dotnet new packagesprops
    ```
    
2. **Mover** esse `Directory.Packages.props` para a **raiz da solution**.
3. **Preencher** `PackageVersion` para seus pacotes principais.
4. **Remover** `Version="x.y.z"` dos `PackageReference` nos `.csproj`.
5. (Opcional) **Ligar pinagem** de transitivas e **injetar** analyzers globalmente.
6. **dotnet restore** e **rodar** o build/CI.

---

## Objeções comuns (e respostas sinceras)

- **“Mas eu preciso de versões diferentes em projetos diferentes.”**
    
    Use `VersionOverride` **onde realmente precisar** (e monitore). 99% dos casos não precisam.
    
- **“Isso vai quebrar minha solução enorme.”**
    
    CPM **não muda a lógica de compilação**; só centraliza a fonte de versões. A tendência é **quebrar menos**.
    
- **“Dá trabalho migrar.”**
    
    Um script simples remove `Version` de todos os `.csproj` e você preenche o props. **É uma tarde de trabalho** que paga meses de paz.
    

---

## Quando **NÃO** usar (sim, existe)

- POCs ou projetos **de 1 arquivo**.
- Repositórios intencionalmente **heterogêneos**, onde cada projeto é um produto independente (e documentado como tal).
    
    Se não é seu caso, CPM é **a escolha certa**.
    

---

## Conclusão

O CPM é a diferença entre uma **plataforma previsível** e uma **coleção de projetos imprevisíveis**.

Ele corta custo operacional, acelera updates, melhora segurança (pinando transitivas) e dá governança real (override controlado + analyzers globais).

**Próximo passo:** crie o `Directory.Packages.props`, pinte seus pacotes core, limpe os `.csproj` e habilite pinagem.

**Se doer**, é porque você precisava disso ontem.
