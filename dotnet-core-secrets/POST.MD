## Sabia que é possível sobrescrever as configurações do appsettings.json da sua aplicação? Existem pegadinhas que podem te deixar de cabelo em pé! Veja nesse post algumas dicas!

![options](https://user-images.githubusercontent.com/7241156/182509843-b2c7d720-e200-4993-8174-3bfe41158bae.png)

## appsettings.json

O `appsettings.json` é o arquivo de configuração do .NET Core. Se você trabalhou com as versões anteriores do .NET provavelmente está habituado ao **web.config** ou **app.config**. 

Era um XML com diversas configurações da tua aplicação. O .NET Core foi reescrito e com ele introduzido o arquivo de configuração no formato **JSON**.

Ao criar um site ASP.NET Core é criado um arquivo chamado appsettings.json.

O appsettings.json substitui as configurações localizadas anteriormente no web.config. O .NET Core suporta arquivos JSON, XML e ini. 

Nas primeiras versões do ASP.NET Core a configuração era feita no arquivo **Startup.cs**. A partir da versão 6 a configuração está imbutida no método `WebApplication.CreateBuilder(args);` no **Progam.cs**

```csharp
var builder = WebApplication.CreateBuilder(args);
```

## Os detalhes importam

Veja como o WebHost cria a configuração:

```csharp
public static IWebHostBuilder CreateDefaultBuilder(string[] args)
{
    // ...
    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

    if (env.IsDevelopment())
    {
        var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
        if (appAssembly != null)
        {
            config.AddUserSecrets(appAssembly, optional: true);
        }
    }

    config.AddEnvironmentVariables();

    if (args != null)
    {
        config.AddCommandLine(args);
    }
    // ...
}
```

Antes de explicar como o AppSettings é montado no código acima. Veja alguns detalhes importantes. 

A variável `env.EnvironmentName` vai conter o valor que foi setado na EnvironmentVariable **ASPNETCORE_ENVIRONMENT**. Essa pode ser alterado no arquivo **launchSettings.json** na pasta <kbd>Properties</kbd>. Ou no próprio ambiente:

![Windows](https://user-images.githubusercontent.com/7241156/182509893-d7aa687a-3021-4cd1-b123-b4d877a0f8bc.png)

Se essa variável tiver o valor `Development`. Então vai carregar o <span class="txt-color-yellow">User Secrets</span>. Por fim carrega as configurações que podem ser passados na inicialização do projeto.

![camadas](https://user-images.githubusercontent.com/7241156/182509914-3a1857de-a9ef-4146-ade4-b90d4eab345f.png)

## As camadas

O AppSettings é construido em Layers. Geralmente segue esta sequencia, pois é auto-configurado no WebHost.

1. A arquivo `appsettings.json` que está na pasta raiz do projeto.
2. O arquivo na raiz concatenado com `.{VARIAVEL_DE_AMBIENTE}.json`, por exemplo `appsettings.Development.json`
3. User Secrets (Um arquivo de configuração que é criado pelo Visual Studio e que fica apenas na maquina de desenvolvedor)
4. Váriaveis de ambiente
5. Váriaveis dos argumentos passados na inicialização via linha de comando.

A ordem pode ser alterada, de acordo com a necessidade do desenvolvedor e projeto. Porém alterar essa ordem não é procedimento comum.

## Niveis 

O AppSettings é criado de forma hierárquica. Conforme as camadas são adicionadas, eventualmente, pode sobrescrever as chaves da camada atual.

![Appsettings](https://user-images.githubusercontent.com/7241156/182510219-546a7ef9-37a7-47f5-bcdf-b5f141b7b398.gif)

Dessa forma seu aplicativo pode ter uma configuração diferente para cada ambiente. É possível explorar as diversas configurações para cada um dos ambientes em que a app é executada.

## User Secrets

O User Secrets permite guardar informações sensíveis sem a necessidade de colocar no projeto. Dessa forma o arquivo fica fora do controle de versões. Apenas na máquina do desenvolvedor.

### Configurando o User Secrets

Se você estiver usando o Visual Studio para adicionar o User Secrets basta clicar com o botão direito do mouse no projeto e selecionar a opção Manage User Secrets.

![image](https://user-images.githubusercontent.com/7241156/182510243-38bc4832-4532-4c3c-889a-e183b1153ccd.png)


O arquivo será salvo na máquina local. O caminho será  `%USERPROFILE%\AppData\Roaming\Microsoft\UserSecrets\`. Além disso, será adicionado uma chave no arquivo csproj.

![image](https://user-images.githubusercontent.com/7241156/182510265-c64ea434-035e-4d17-87bc-26590b2ce565.png)


## Pode ser utilizado por vários projetos

Como é um arquivo local, você pode aproveitar o User Secrets em outros projetos. Para isso basta adicionar a mesma chave `<UserSecretsId>id</UserSecretsId>` no **csproj**.


# Variáveis de ambiente

O ASP.NET Core usa uma variável de ambiente chamada `ASPNETCORE_ENVIRONMENT` para indicar o ambiente. O valor pode ser qualquer coisa. Por default seu valor é Development, Staging ou Production. 

**O valor não diferencia maiúsculas de minúsculas no Windows e Mac OS, mas diferencia maiúsculas de minúsculas no Linux.** E isso pode gerar um baita mal estar. Então tome cuidado.

Dentro do Visual Studio, é possível alterar o valor das variáveis de ambiente. Em propriedades do projeto > Debug.

![DebugSection](https://user-images.githubusercontent.com/7241156/182510376-d674a368-a933-4aa4-ad3e-73434665228e.png)

Além disso a variável de Ambiente **substitui** as configurações do `appSettings.json`. ISSO mesmo!

Suponha que você tem o seguinte `appSettings.config`:

```json
{
  "EmailParaEnviarErro": "meuemail@gmail.com"
}
```

Para sobrescrever essa configuração é necessário adicionar uma variável de ambiente:

![image](https://user-images.githubusercontent.com/7241156/182510651-f81891da-5b96-438e-9033-fa0556da0c37.png)

Após adicionar ela vai aparecer na lista

![image](https://user-images.githubusercontent.com/7241156/182510664-0c2a522b-c279-41ef-86e7-30b7a6842966.png)

Assim, quando sua aplicação carregar, a variável de ambiente vai substituir a variável que está no `appSettings.json`.

Se você vem de uma stack Microsoft, inicialmente essa configuração pode parecer estranho. Mas essa possibilidade é extremamente útil em ambientes de Cloud, em containers, kubernetes.

## Docker

É possível setar as variáveis de ambiente através do DOCKERFILE ou no Compose.

Dockerfile:

```dockerfile
// ...
ENV ASPNETCORE_ENVIRONMENT=Production
```

Compose:

```yml
environment: 
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:5000
      CUSTOMCONNSTR_SSOConnection: "server=jpdatabase,port=3306;database=jpproject;user=jp;password=10203040"
      ApplicationSettings:EnableExternalProviders: "false" # Because Auth url is http://jpproject (modified by host to point to 127.0.0.1), then Google and Facebook reject it.
      ApplicationSettings:DatabaseType: MySql
      CertificateOptions:Type: Temporary
```

## Azure AppService

Para modificar as variáveis vá em App Services > Seu App Service > Configuration.

![image](https://user-images.githubusercontent.com/7241156/182510968-c4ad5931-46a3-4018-84e9-cf55682f8b16.png)

## Alterando os subníveis

É normal que o `appSettings.json` seja mais complexo. Contendo mais níveis JSON do que apenas o primeiro nível. Como foi demonstrado nesses exemplos.

Considere a seguinte configuração.

```json
{
  "ApplicationSettings": {
    "EmailError": "random@gmail.com",
    "KeyToConnect": "randomData",
    "OneMoreLevel": {
      "3rdLevel": "data"
    }
  }
}
```

Como fazer para alterar a chave **3rdLevel**? A resposta depende de qual ambiente a aplicação está rodando.

## Docker

Para navegar e especificar o nível que quer chegar utilize o **:** (Dois pontos) tanto no compose quanto no dockerfile.

* Compose

```yml
api:
    image: bhdebrito/jpproject-api
    ports:
        - "5003:80"
    environment: 
        ApplicationSettings:OneMoreLevel:3rdLevel: "Changed!" # <- 
```

* Dockerfile

```dockerfile
ENV ApplicationSettings:OneMoreLevel:3rdLevel: "Changed!"
```

## Azure AppService

Aqui é o maior ponto de atenção. No Docker essa configuração depende do sistema operacional do AppService.

![webapp](https://user-images.githubusercontent.com/7241156/182511665-437d7a7c-3168-41e4-b811-953b56e4bcd7.png)


## Windows

Na máquina Windows é igual ao docker. Utilizando o **:** (dois pontos).

![Azureadding](https://user-images.githubusercontent.com/7241156/182511651-ebb4636a-912e-4484-b44a-88182f379fc3.png)

![configurationChanged](https://user-images.githubusercontent.com/7241156/182511634-58876c0d-19cd-4ec2-acc0-28ba00230b41.png)


## Linux 

Já para máquinas linux, a configuração não aceita **:** (dois pontos) Por isso é necessário alterar para **__** (Dois underlines).

![configurationChanged_linux](https://user-images.githubusercontent.com/7241156/182511621-e2a0d2f5-37c1-4e0c-ad4c-55ce5e1645b3.png)


## Bonus

Sabia que é possível consultar todas as variáveis de ambiente do **Azure AppService** ?. Algumas vezes as configurações não funcionam. Parecem estar com bug. Essa dica pode ser útil para aqueles que estão tentando debugar.

É possível visualizar todas as variáveis de ambiente de um AppService. Para isso abra o Kudu:

AppService > Seu App Service > Advanced Tools > Go

![advanced tools](https://user-images.githubusercontent.com/7241156/182511550-61a9341f-c796-403c-a824-0330100e5cf7.png)



Assim que abrir, vá na opção **Environment**:

![kudu](https://user-images.githubusercontent.com/7241156/182511529-c0ea0a51-c8fc-487f-bf31-a9d697cfd549.png)
