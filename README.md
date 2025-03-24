# Batalha Naval com Azure Service Bus

Este projeto é uma implementação do jogo Batalha Naval utilizando C# e Azure Service Bus para comunicação entre os jogadores. O jogo permite que dois jogadores joguem remotamente, enviando ataques e recebendo respostas através de filas de mensagens no Azure.

## 🎯 Funcionalidades
- Jogo para dois jogadores.
- Posicionamento manual dos barcos.
- Envio e recebimento de ataques utilizando Azure Service Bus.
- Indicação de acerto ou erro nos ataques.
- Tabuleiro atualizado a cada rodada.

## 🛠 Tecnologias Utilizadas
- **C#** (.NET)
- **Azure Service Bus** (para comunicação entre os jogadores)

## 📌 Pré-requisitos
Antes de rodar o projeto, certifique-se de ter:
- .NET 6 ou superior instalado.
- Conta no Azure e uma namespace do **Azure Service Bus** criada.
- Uma **fila** chamada `fila-ataques` no Azure Service Bus.
- A **connection string** do Service Bus para configurar no código.

## 🚀 Como Executar
1. Clone este repositório:
   ```sh
   git clone https://github.com/seu-usuario/BatalhaNaval.git
   cd BatalhaNaval
   ```
2. Abra o projeto em um editor como **Visual Studio Code** ou **Visual Studio**.
3. No arquivo `Program.cs`, configure a `connectionString` com a sua conexão do Azure Service Bus.
4. Execute o comando para rodar o jogo:
   ```sh
   dotnet run
   ```
5. Digite o ID do jogador (`Player1` ou `Player2`).
6. Siga as instruções para posicionar os barcos e jogar.

## 🎮 Como Jogar
1. O **Player 1** inicia e posiciona os barcos.
2. O **Player 2** então posiciona seus barcos.
3. Os jogadores se alternam para atacar.
4. O ataque é enviado via Azure Service Bus.
5. O jogo informa se o ataque foi um acerto (`🔥`) ou um erro (`❌`).
6. O jogo continua até que todos os barcos de um jogador sejam destruídos.

## 📄 Estrutura do Código
```
BatalhaNaval/
├── Program.cs       # Código principal do jogo
├── README.md        # Documentação do projeto
├── .gitignore       # Arquivos a serem ignorados no Git
└── BatalhaNaval.csproj # Configuração do projeto .NET
```

## 📢 Contribuição
Contribuições são bem-vindas! Para contribuir:
1. Faça um **fork** do repositório.
2. Crie uma **branch** para sua feature/bugfix:
   ```sh
   git checkout -b minha-feature
   ```
3. Faça as alterações e **commite**:
   ```sh
   git commit -m "Minha nova feature"
   ```
4. Envie para o repositório:
   ```sh
   git push origin minha-feature
   ```
5. Abra um **Pull Request**.


