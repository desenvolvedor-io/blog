# Componentes do JWT (JWS, JWE, JWA, JWK, JWKS)

Você sabia que o formato JWT pode ser tanto um <span class="txt-color-green">JWS</span> quanto um <span class="txt-color-red">JWE</span>? E que o algoritmo mais recomendado para assinar um JWT é o ECDsa P-256 SHA-256 (ES256)? Veja neste artigo detalhes do JWT e como proteger melhor suas API's.

<img src="https://images.unsplash.com/photo-1453733190371-0a9bedd82893?ixlib=rb-1.2.1&q=80&fm=jpg&crop=entropy&cs=tinysrgb&w=2000&fit=max&ixid=eyJhcHBfaWQiOjExNzczfQ"/>

# Json Web Token (JWT)

O JWT é o acronimo de JSON Web Token. É uma estrutura de dados no formato JSON. Ele é compacto e seguro. URL safe, ou esja, pode trafegar na URL sem prejudicar seu conteúdo. 
O JWT pode ser comparado a uma classe abstrata. <span class="txt-color-green">JWS</span> e <span class="txt-color-red">JWE</span> são as implementações do JWT.

Seu conteúdo é composto por claims. As Claims são um conjunto de chave/valor. Fornecem ao client ou API informações sobre o usuário que está consumindo seus serviços.

Ele é amplamente utilizado para transferir dados através do protocolo HTTP. Entre clients frontends (SPA) e API's. Pode ser também utilizado para transferência de dados entre API's. 

O JWT é uma estrutura JSON. As especificações sobre como criptografar ou assinar digitalmente um JWT é conhecido como **J**avascript **O**bject **S**igning & **E**ncription (JOSE). Ele é composto pelos seguintes componentes. E suas respectivas RFC's.


| Acronimo 	| Nomenclatura                  	| Detalhes                                                                      	|
|----------	|-------------------------------	|-------------------------------------------------------------------------------	|
| JWS      	| Json Web Signature (RFC7515)  	| Processo sobre como assinar digitalmente um JWT                               	|
| JWE      	| Json Web Encryption (RFC7516) 	| Processo sobre como criptografar um JWT                                       	|
| JWK      	| Json Web Key (RFC7517)        	| Define uma estrutura JSON para representar uma chave de criptografia.         	|
| JWA      	| Json Web Algorithm (RFC7518)  	| Define os algoritmos válidos para assinar digitalmente ou criptografar um JWT 	|


JOSE é um framework. Seu objetivo é fornecer um método seguro para transferir as claims com segurança. Garantindo que seu conteúdo não seja adulterado durante o transporte das informações.

# Json Web Signature (JWS)

O <span class="txt-color-green">JWS</span> é o mais popular. Facilmente confundido como o própio **JWT**. Após assinar digitalmente o JWT, ele torna-se um <span class="txt-color-green">JWS</span>. O <span class="txt-color-green">JWS</span> é o conteúdo JWT com uma assinatura digital. 

O que garante segurança ao <span class="txt-color-green">JWS</span> é sua assinatura digital. A assinatura é o resultado da criptografia da composição do **Header** & **Paylooad**.

```
Encrypt(header + '.' + payload, secret);
```
<small>Meramente ilustrativo. O formato como deve ser feito está detalhado abaixo.</small>

Dessa forma o conteúdo do **JWT** (Payload) é legivel, porém, qualquer alteração no conteúdo resultaria numa assinatura diferente. 

A **chave pública** só permite acesso de leitura, por isso sua API ou Client frontend (SPA) consegue validar se o payload do JWT foi alterado. Garantindo confiabilidade e segurança ao consumidores.

Somente quem possui a chave privada consegue gerar uma assinatura válida para o JWT.

O <span class="txt-color-green">JWS</span> é composto por três seções.

* Header
<script src="https://gist.github.com/brunohbrito/33e5bc26511480ff92830f66e6a3f435.js"></script>
        
* Payload
<script src="https://gist.github.com/brunohbrito/461398a598c6c57d839031aaa0ba5f34.js"></script>

* Digital Signature
<script src="https://gist.github.com/brunohbrito/e07f77e4a5bee4d755c1d06f32d2c7df.js"></script>
Nesse caso, ECDSA-P256-S256 é a representação extensa do algoritimo utilizado no header (ES256).

O formato final do <span class="txt-color-green">JWS</span> é muito simples:
<script src="https://gist.github.com/brunohbrito/1ead3f2c6d7b66f56e008c86451a0efa.js"></script>

É a transformação para base64url dos componentes, separados por **.** (ponto).

![jws](https://www.brunobrito.net.br/content/images/2020/02/jws.png)


# Json Web Encryption (JWE)

O <span class="txt-color-red">JWE</span> representa o conteúdo criptografado de um JWT. Assim como o <span class="txt-color-green">JWS</span>, possui uma estrutura de dados JSON própria:

* Header
* Chave Criptografada
* Initialization Vector
* Additional Authenticated Data (AAD)
* Ciphertext
* Authentication Tag

O Header é composto por:

* Per-Recipient Unprotected Header
* Protected Header
* Shared Unprotected Header

Alguns dos valores podem não ser utilizados, isso depende do algoritmo de assinatura utilizado.

Também a exemplo do <span class="txt-color-green">JWS</span> o resultado final do <span class="txt-color-red">JWE</span> é a concatenação de seus componentes.

<script src="https://gist.github.com/brunohbrito/2d0ca562f17ae303c1f723cac5f88fad.js"></script>

![jwe](https://www.brunobrito.net.br/content/images/2020/02/jwe.png)


# Json Web Algorithms (JWA)

Não é qualquer algoritmo que pode ser utilizado para Assinar Digitalmente ou Criptografar um JWT. Existe uma RFC que espeficifica quais algoritmos devem ser utilizados. Apesar de todos algoritmos estar em conformidade com a RFC. Ela recomenda a utilização do **ECDSA using P-256 and SHA-256** (ES256) ou **RSASSA-PSS** (PS256) para assinar digitalmente o JWT.

## JWS

Abaixo uma tabela de quais algoritmos utilizar para assinar um <span class="txt-color-green">JWS</span>.

| algoritmo | Descrição                                     	|
|-----------|---------------------------------------------------|
| HS256 	| HMAC using SHA-256                             	|
| HS384 	| HMAC using SHA-384                             	|
| HS512 	| HMAC using SHA-512                             	|
| RS256 	| RSASSA-PKCS1-v1_5 using SHA-256                	|
| RS384 	| RSASSA-PKCS1-v1_5 using SHA-384                	|
| RS512 	| RSASSA-PKCS1-v1_5 using SHA-512                	|
| ES256 	| ECDSA using P-256 and SHA-256                  	|
| ES384 	| ECDSA using P-384 and SHA-384                  	|
| ES512 	| ECDSA using P-521 and SHA-512                  	|
| PS256 	| RSASSA-PSS using SHA-256 and MGF1 with SHA-256 	|
| PS384 	| RSASSA-PSS using SHA-384 and MGF1 with SHA-384 	|
| PS512 	| RSASSA-PSS using SHA-512 and MGF1 with SHA-512 	|
| none  	| No digital signature or MAC performed          	|

Repare que a RFC preve a opção "Nenhum". Isso pressupõe que uma outra camada de proteção foi utilizada para garantir que o JWT não foi adulterado no seu transporte.

# Json Web Key (JWK)

JWK é uma estrutura JSON que representa uma chave de criptografia. Na sua estrutura contém as informações para criptografar um JWT (<span class="txt-color-red">JWE</span>) ou Assinar Digitalmente (<span class="txt-color-green">JWS</span>). 

Dentro de um ambiente OAuth 2.0, o responsável por criar e gerenciar o JWK é o Authorization Server. Como no OAuth 2.0 todos os JWT's são emitidos por um unico servidor ele é quem gerencia o JWK. Assim somente o Authorization Server pode emitir um JWT.

Certos parâmetros do JWK podem ser públicos, dessa forma os consumidores do JWT podem validar se o JWT não foi adulterado.

Exemplo de um JWK público (Google):
<script src="https://gist.github.com/brunohbrito/87a1fc3c2a29d3b367189b2eb23c7b4c.js"></script>

Exemplo de um JWK privado:
<script src="https://gist.github.com/brunohbrito/2a97b62b956a873121b02fb3a3131a9a.js"></script>


# Json Web Key Set (JWKS)

JWKS é um array de JWK. 

Os algoritmos de criptografia se tornam mais fracos com o tempo. Seja por que as técnicas de análise de criptografia melhoram ou por causa do desempenho dos computadores que melhoram dia após dias. Ambos contribuem para diminuir o tempo necessário para quebrar um algoritmo específico. Portanto, o objetivo do JWK e JWKS é permitir que um servidor OAuth 2.0 seja modular em relação a criptografia. Permitindo que novos algoritmos sejam facilmente inseridos.

Por esse motivo as boas práticas dizem que o JWK deve mudar a cada 90 dias. E com o auxilio do JWKS, ao efetuar a troca do algoritmo, não irá inválidar os tokens que foram gerados com o algoritmo anterior. Permitindo uma troca gradual, sem afetar os sistemas.

Atualmente o OAuth 2.0 utiliza o mesmo JWK para assinar o **access_token** e o **identity_token**. Há uma discussão no OAuth Working Group sobre separar a responsabilidade. Em breve teremos novidades.

Todo servidor OAuth 2.0 mantém um link público de seus JWKS.

Exemplo de um JWKS público [Google](https://www.googleapis.com/oauth2/v3/certs):
<script src="https://gist.github.com/brunohbrito/030a3991c56195bfa41e2bebd61b49f1.js"></script>

Repare que apesar de público, não possui os dados privados. Utilizado para assinar o JWT.

# Referências

* [JSON Web Token (JWT) - RFC 7519](https://tools.ietf.org/html/rfc7519)
* [JSON Web Signature (JWS) - RFC 7515](https://tools.ietf.org/html/rfc7515)
* [JSON Web Encryption (JWE) -RFC 7516](https://tools.ietf.org/html/rfc7516)
* [JSON Web Algorithms (JWA) - RFC 7518](https://tools.ietf.org/html/rfc7518)
* [JSON Web Key (JWK) - RFC 7517](https://tools.ietf.org/html/rfc7517)
* [JOSE](https://jose.readthedocs.io/)
* [JWT, JWS and JWE for Not So Dummies!](https://medium.facilelogin.com/jwt-jws-and-jwe-for-not-so-dummies-b63310d201a3)
* [Recommendation for Key Management, Part 1: General](https://csrc.nist.gov/publications/detail/sp/800-57-part-1/rev-4/final)
