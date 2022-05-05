## O Visual Studio Code é uma das melhores ferramentas do mercado, porém usuários de Visual Studio tradicional sempre passam por algumas dificuldades ao tentar reproduzir a mesma experiência no VS Code. Uma delas é como rodar diversos projetos simultaneamente como no caso de uma solution com diversas APIs.

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

```json

{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Projeto Web",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/WebMvcSample/bin/Debug/net6.0/WebMvcSample.dll",
            "args": [],
            "cwd": "${workspaceFolder}/WebMvcSample",
            "console": "internalConsole",
            "stopAtEntry": false,            
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            }
        },
        {
            "name": "Projeto WebAPI 1",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/WebApiSample_1/bin/Debug/net6.0/WebApiSample_1.dll",
            "args": [],
            "cwd": "${workspaceFolder}/WebApiSample_1",
            "stopAtEntry": false,
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            }
        },
        {
            "name": "Projeto WebAPI 2",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/WebApiSample_2/bin/Debug/net6.0/WebApiSample_2.dll",
            "args": [],
            "cwd": "${workspaceFolder}/WebApiSample_2",
            "stopAtEntry": false,
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            }
        }
    ]
}

```

Agora você pode iniciar seus projetos um a um e rodar quantos deles quiser simultaneamente:

![image](https://user-images.githubusercontent.com/5068797/166866326-0cb705c5-c3ef-444a-9b84-cff117ef465d.png)

Escolhendo qual projeto será executado os comandos de debug:

![image](https://user-images.githubusercontent.com/5068797/166866419-5a86fc61-41c9-465c-9118-070affa800d7.png)

## Iniciando todos projetos de uma vez

Pois é Eduardo, eu achei legal, mas tenho vários projetos e queria subir todos eles com um clique, é possível?

-Claro que é, afinal estou aqui para resolver todos os seus problemas 🦸‍♀️

### Criando uma composição de inicialização

Para selecionar todos os projetos que você quer inicializar juntos basta listar os nomes conforme configurados no launch.json:

```json

 "compounds": [
        {
            "name": "Iniciar todos",
            "configurations": [
                "Projeto Web",
                "Projeto WebAPI 1",
                "Projeto WebAPI 2"
            ],
            "stopAll": true
        }
    ]

```

O arquivo final ficará assim:

```json

{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Projeto Web",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/WebMvcSample/bin/Debug/net6.0/WebMvcSample.dll",
            "args": [],
            "cwd": "${workspaceFolder}/WebMvcSample",
            "console": "internalConsole",
            "stopAtEntry": false,            
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            }
        },
        {
            "name": "Projeto WebAPI 1",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/WebApiSample_1/bin/Debug/net6.0/WebApiSample_1.dll",
            "args": [],
            "cwd": "${workspaceFolder}/WebApiSample_1",
            "stopAtEntry": false,
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            }
        },
        {
            "name": "Projeto WebAPI 2",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/WebApiSample_2/bin/Debug/net6.0/WebApiSample_2.dll",
            "args": [],
            "cwd": "${workspaceFolder}/WebApiSample_2",
            "stopAtEntry": false,
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            }
        }
    ],
    "compounds": [
        {
            "name": "Iniciar todos",
            "configurations": [
                "Projeto Web",
                "Projeto WebAPI 1",
                "Projeto WebAPI 2"
            ],
            "stopAll": true
        }
    ]
}

```

E basta escolher a opção criada:

![image](https://user-images.githubusercontent.com/5068797/166867004-9760de7b-0d9c-48af-87fc-f462412ead11.png)

### Caso você tenha problemas para inicializar algum dos projetos (eu tive), force a criação da configuração deles via Omnisharp:

> Ctrl + Shift + P
> .NET Generate Assets for Build and Debug

![image](https://user-images.githubusercontent.com/5068797/166867218-7b7deb4f-3aa5-4b69-bcc1-af857c09b752.png)

## Sinceramente?

Depois que eu dominei algumas técnicas no VS Code eu me sinto muito mais produtivo do que no Visual Studio tradicional.

Recomendo que execute esse processo em nosso projeto DevStore, pois são 7 APIs e uma aplicação Web, é um desafio bem interessante.

Não conhece? Confira abaixo e aproveite e nos dê uma estrela.

[![ReadMe Card](https://github-readme-stats.vercel.app/api/pin/?username=desenvolvedor-io&repo=dev-store)](https://github.com/desenvolvedor-io/dev-store)
