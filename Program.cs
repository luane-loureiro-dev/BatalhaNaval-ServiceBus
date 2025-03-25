using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

class Program
{
    static string connectionString = "Endpoint";//colocar o endereço do endpoint da fila do servicebus
    static string queueName = "casa14.queue";
    static string playerId;

    static string[,] boardPlayer1 = new string[5, 5];
    static string[,] boardPlayer2 = new string[5, 5];

    static async Task Main()
    {
        Console.WriteLine("Digite seu ID de jogador (ex: Player1 ou Player2): ");
        playerId = Console.ReadLine();

        // Inicializa os tabuleiros com água
        InicializarTabuleiro(boardPlayer1);
        InicializarTabuleiro(boardPlayer2);

        // Posicionamento dos barcos
        if (playerId == "Player1")
        {
            Console.WriteLine("Player 1, posicione seus barcos!");
            PosicionarBarcos(boardPlayer1);
            MostrarTabuleiro(boardPlayer1, "Tabuleiro do Player 1");
            Console.WriteLine("Agora é a vez do Player 2!");
            Console.ReadLine(); // Aguardar Player 2 posicionar seus barcos
        }
        else if (playerId == "Player2")
        {
            Console.WriteLine("Aguardando o Player 1 posicionar seus barcos...");
            Console.ReadLine(); // Aguardar Player 1 posicionar seus barcos
            Console.WriteLine("Player 2, posicione seus barcos!");
            PosicionarBarcos(boardPlayer2);
            MostrarTabuleiro(boardPlayer2, "Tabuleiro do Player 2");
            Console.WriteLine("Agora é a vez do Player 1!");
            Console.ReadLine(); // Aguardar Player 1 posicionar seus barcos
        }

        // Jogo alternado
        while (true)
        {
            if (playerId == "Player1")
            {
                await JogarTurno(boardPlayer2, "Player 1", "Player 2");
            }
            else
            {
                await JogarTurno(boardPlayer1, "Player 2", "Player 1");
            }
        }
    }

    // Inicializa os tabuleiros com água
    static void InicializarTabuleiro(string[,] board)
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                board[i, j] = "🌊"; // Água
            }
        }
    }

    // Permite que o jogador posicione seus barcos
    static void PosicionarBarcos(string[,] board)
    {
        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine($"Posicionando o barco {i + 1}...");
            int x, y;
            while (true)
            {
                Console.WriteLine("Digite a posição do barco (ex: A1): ");
                string input = Console.ReadLine().ToUpper(); // Tornar a entrada em maiúscula
                if (input.Length == 2 && input[0] >= 'A' && input[0] <= 'E' && input[1] >= '1' && input[1] <= '5')
                {
                    x = input[1] - '1'; // Converte o número da linha (1 a 5) para o índice
                    y = input[0] - 'A'; // Converte a letra da coluna (A a E) para o índice

                    if (board[x, y] == "🌊")
                    {
                        board[x, y] = "🚢"; // Coloca o barco
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Posição inválida ou já ocupada. Tente novamente.");
                    }
                }
                else
                {
                    Console.WriteLine("Formato inválido. Digite a posição no formato A1 a E5.");
                }
            }
        }
    }

    // Mostra o tabuleiro do jogador
    static void MostrarTabuleiro(string[,] board, string titulo)
    {
        Console.WriteLine(titulo);
        Console.WriteLine("  A   B   C   D   E");
        for (int i = 0; i < 5; i++)
        {
            Console.Write((i + 1) + " ");
            for (int j = 0; j < 5; j++)
            {
                Console.Write(board[i, j] + " ");
            }
            Console.WriteLine();
        }
    }

    // Realiza o turno de um jogador
    static async Task JogarTurno(string[,] boardOponente, string jogadorAtacante, string jogadorDefensor)
    {
        Console.Clear();
        Console.WriteLine($"{jogadorAtacante} está atacando!");
        MostrarTabuleiro(boardOponente, $"{jogadorDefensor} - Tabuleiro");

        Console.WriteLine("\nDigite a posição do ataque (ex: A5): ");
        string ataque = Console.ReadLine().ToUpper();

        await EnviarMensagem(ataque);
        Console.WriteLine("Ataque enviado! 🎯 Aguardando resposta...");

        // Aguarda o outro jogador para processar o ataque
        await ReceberAtaque(boardOponente);

        Console.WriteLine("\nPressione qualquer tecla para o próximo turno...");
        Console.ReadLine();
    }

    // Recebe o ataque e processa a resposta
    static async Task ReceberAtaque(string[,] board)
    {
        Console.WriteLine("Aguardando ataque...");

        string ataque = await ReceberMensagem();
        if (!string.IsNullOrEmpty(ataque))
        {
            // Aqui você já tem o código para processar as posições
            if (ataque.Length == 2 && ataque[0] >= 'A' && ataque[0] <= 'E' && ataque[1] >= '1' && ataque[1] <= '5')
            {
                string pos = ataque.ToUpper(); // Converte para maiúsculas para garantir a consistência
                int x = pos[1] - '1'; // Convertendo o número da linha
                int y = pos[0] - 'A'; // Convertendo a letra da coluna

                if (x >= 0 && x < 5 && y >= 0 && y < 5)
                {
                    if (board[x, y] == "🚢")
                    {
                        board[x, y] = "🔥"; // Acertou
                        Console.WriteLine($"💥 Seu navio foi atingido em {pos}!");
                    }
                    else
                    {
                        board[x, y] = "❌"; // Errou
                        Console.WriteLine($"💨 O ataque em {pos} errou!");
                    }
                }
                else
                {
                    Console.WriteLine("Posição fora dos limites do tabuleiro!");
                }
            }
            else
            {
                Console.WriteLine("Formato de ataque inválido! Tente novamente.");
            }
        }
    }

    static async Task EnviarMensagem(string mensagem)
    {
        await using var client = new ServiceBusClient(connectionString);
        ServiceBusSender sender = client.CreateSender(queueName);

        ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(mensagem));
        await sender.SendMessageAsync(message);
    }

    static async Task<string> ReceberMensagem()
    {
        await using var client = new ServiceBusClient(connectionString);
        ServiceBusReceiver receiver = client.CreateReceiver(queueName);

        ServiceBusReceivedMessage message = await receiver.ReceiveMessageAsync();
        if (message != null)
        {
            string body = Encoding.UTF8.GetString(message.Body);
            await receiver.CompleteMessageAsync(message);
            return body;
        }

        return null;
    }
}
