## O Visual Studio Code é uma das melhores ferramentas do mercado, porém usuários de Visual Studio tradicional sempre passam por algumas dificuldades ao tentar reproduzir a mesma experiência no VS Code, uma delas é como rodar diversos projetos simultaneamente como no caso de uma solution com diversas APIs.

Vamos supor que você esteja com uma máquina nova em mãos e sem Visual Studio instalado. Você conseguiria trabalhar apenas no VS Code? 

Possivelmente precisaria aprender diversos truques como este que irei lhe ensinar, é tudo uma questão de entender como o VS Code funciona. Inclusive esta técnica pode ser usada com N tipos de projeto (.NET, Node, SPAs, etc...)

## Vamos começar do começo!

Para atender ao cenário proposto você só precisará do .NET SDK e VS Code instalados.

### Vamos criar um projeto via linha de comando e abrir no VS Code

Abra o console e rode o comando abaixo:

```
dotnet new mvc -n WebAppTeste
```

O resultado será algo do tipo:

![image](https://user-images.githubusercontent.com/5068797/166855879-42c01078-6eda-45e4-8a7f-2ca7c2f8aea5.png)

Nesse momento você poderia executar o comando `[dotnet watch run]` ou apenas `[dotnet run]` que sua aplicação já estará rodando. Mas e se você precisar debugar esse projeto?

Nesse caso será necessário configurar via VS Code, então ainda no console execute os dois comandos abaixo:

```
cd WebAppTeste

code .
```

Seu projeto estará aberto no VS Code:

![image](https://user-images.githubusercontent.com/5068797/166856398-40c5cb0e-7a12-4171-93bd-1fa542bde8d3.png)


### Vamos agora criar um arquivo de execução e debug do projeto (launch.json)

Para isso antes será necessário instalar a extensão de suporte a C# caso ainda não possua:

![image](https://user-images.githubusercontent.com/5068797/166856834-5deabb32-eea5-486a-9aaa-d1a9a795ab78.png)

E na sequência selecione a aba de Debug e crie um arquivo para .NET:

![image](https://user-images.githubusercontent.com/5068797/166857158-b892cb4d-8c00-42fd-969a-09ebb4340a20.png)

O arquivo launch.json será originalmente criado assim:

```json

{
    // Use IntelliSense to learn about possible attributes.
    // Hover to view descriptions of existing attributes.
    // For more information, visit: https://go.microsoft.com/fwlink/?linkid=830387
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch (web)",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/bin/Debug/net6.0/WebAppTeste.dll",
            "args": [],
            "cwd": "${workspaceFolder}",
            "stopAtEntry": false,
            "serverReadyAction": {
                "action": "openExternally",
                "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
            },
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            },
            "sourceFileMap": {
                "/Views": "${workspaceFolder}/Views"
            }
        },
        {
            "name": ".NET Core Attach",
            "type": "coreclr",
            "request": "attach"
        }
    ]
}

```

Vamos ajustar e deixar essa configuração mais enxuta:

```json

{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch (web)",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/bin/Debug/net6.0/WebAppTeste.dll",
            "args": [],
            "cwd": "${workspaceFolder}",
            "stopAtEntry": false,
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            }
        }
    ]
}

```

Note que agora o VS Code lhe dará uma opção para iniciar seu projeto utilizando o nome configurado na chave "name" do arquivo JSON:

![image](https://user-images.githubusercontent.com/5068797/166857686-6302b6ca-491a-4be2-a1fe-5d76cef1823a.png)

Basta agora colocar um breakpoint na controller e verificar como a experiência de debug é bem agradável e completa no VS Code:

![image](https://user-images.githubusercontent.com/5068797/166857854-bdb728e5-4b40-4c18-9bd7-d000163fcb39.png)

## Executando diversos projetos simultaneamente

Vamos supor que estamos trabalhando em uma solution com um projeto MVC e duas APIs e você precisa executar os 3 projetos simultaneamente para debug e etc.

Primeiro vamos criar a solution e os projetos para que no Visual Studio tenha esse efeito:

![image](https://user-images.githubusercontent.com/5068797/166858537-c21cec6f-fcdb-4cf6-8d50-17df7174ba37.png)

Execute no console os comandos a seguir:

```console

-- Nova Solution
dotnet new sln -n BlogSample

-- Novo projeto Web MVC
dotnet new mvc -n WebMvcSample

-- Novo projeto WebAPI 1 e 2
dotnet new api -n WebApiSample_1

dotnet new api -n WebApiSample_2

-- Adicionando Projeto MVC na Solution
dotnet sln add WebMvcSample

-- Adicionando Projeto API 1 e 2 na Solution / Pasta APIs
dotnet sln add WebApiSample_1 --solution-folder APIs

dotnet sln add WebApiSample_2 --solution-folder APIs

-- Verificando se os 3 projetos foram adicionados na Solution
dotnet sln list

-- Output:

Project(s)
----------
WebMvcSample\WebMvcSample.csproj
WebApiSample_1\WebApiSample_1.csproj
WebApiSample_2\WebApiSample_2.csproj

-- Abrindo VS Code na pasta raiz

code .

```

Vamos começar criando um arquivo launch.json como anteriormente, porém um para cada projeto, ao selecionar que deseja criar um arquivo para .NET (.NET Core) você poderá escolher o primeiro projeto:

![image](https://user-images.githubusercontent.com/5068797/166859584-e65c746d-4864-4399-b2a3-bbf36b5fc89d.png)

Eu escolhi primeiro o projeto MVC e para seguir adicionando os demais cliquei em Add Configuration e depois escolhi a opção mais adequada *.NET Launch a local .NET Core Web App*. Fiz isto duas vezes e o resultado final ficou assim:



