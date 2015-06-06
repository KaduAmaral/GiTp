#GiTp

O aplicativo envia os arquivos do último Commit de um repositório Git Local para um servidor FTP.


#Usage

Informe uma conexão para enviar os arquivos do _Commit_.

```
// con     host       user pass           port
connection contoso.com user MyPreciousP4SS 21
```

<sub>O comando `connection` pode ser abreviado para `con`.</sub>


A porta também pode ser informada juntamente com o _host_: `contoso.com:21`. Após a conexão for realizada você pode percorrer a estrutura de diretório utilizando o comando `rd`: 

```
rd caminho/para/diretorio
```

Para listar todos os arquivos e subdiretórios do diretório atual utilize o comando `rdir`. 

Você também pode salvar a conexão utilizando o comando `save connection` para uma maior comodidade. O programa pedirá para que você informe um nome único para essa conexão, assim você poderá utiliza-la com o comando `connection` passando como parâmetro o nome da conexão seguindo de um arroba (@), por exemplo: `connection @contoso`.

Utilizando o comando `connections` você obtém a lista de conexões salvas.
<sub>O comando `connections` pode ser abreviado para `cons`.</sub>

Salve um diretório local e remota para uma conexão ativa usando os comandos:

```
// Informando um diretório local
set dir c:/caminho/do/diretório

// Informando um diretório remoto
set rdir caminho/do/diretório

// Salvando os dados
save connection
```

Para informar que o diretório atual é um repositório utilize o comando: 

```
repo -here

// OU

repo c:/caminho/do/repositório
```

Para visualizar os arquivos do _commit_ utilize o comando `repo files`.

Envie os arquivos para o servidor com o comando `send`.


Para outras informações ou detalhes sobre comandos informe `help` ou `help` + nome do comando.

#ToDo

  1. Disponibilizar opção para escolher o _commit_ pelo _SHA_.
  1. Registrar commits enviados
  1. Documentar Comandos

###Version 1.0.0.2

  . Corrigido retorno em HTML do Servidor

#Author

[Carlos Barcelos](https://github.com/KaduAmaral) | [Devcia](//devcia.com)

#License

[Apache 2.0](https://github.com/KaduAmaral/GiTp/blob/master/LICENSE)
