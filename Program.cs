using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using dotenv.net;
using System.IO;

internal class Program
{
    private static string? connectionString;
    private static string? queueName;
    private static string playerId;
    private static bool minhaVez;
    private static string[,] board = new string[5, 5];
    private static bool gameOver = false;

    private static string? player1Id = null;
    private static string? player2Id = null;
    private static string filePath = "gameState.txt"; // Caminho para o arquivo que armazena o estado do jogo

    private static async Task Main()
    {
        DotEnv.Load();
        connectionString = Environment.GetEnvironmentVariable("SERVICE_BUS_CONNECTION_STRING");
        queueName = Environment.GetEnvironmentVariable("QUEUE_NAME");

        // Verificar se o arquivo de estado existe para determinar quem é Player 1 e Player 2
        if (!File.Exists(filePath))
        {
            // Se o arquivo não existir, Player 1 será atribuído
            player1Id = "Player1";
            playerId = "Player1"; // O primeiro jogador a se conectar será Player1
            File.WriteAllText(filePath, playerId); // Salvar estado no arquivo
            Console.WriteLine("Você é o Player 1");
        }
        else
        {
            // Se o arquivo existir, atribui Player 2
            string existingPlayer = File.ReadAllText(filePath);
            if (existingPlayer == "Player1")
            {
                player2Id = "Player2";
                playerId = "Player2"; // O segundo jogador a se conectar será Player2
                File.WriteAllText(filePath, playerId); // Atualiza o arquivo com Player2
                Console.WriteLine("Você é o Player 2");
            }
            else
            {
                Console.WriteLine("O jogo já está em andamento.");
                return;
            }
        }

        // Envia o ID do jogador para o Service Bus
        await EnviarMensagem(playerId);

        // Espera pela resposta do outro jogador
        string opponentId = await ReceberMensagem();

        // Se o outro jogador já escolheu o ID, podemos continuar
        if (opponentId != null && opponentId != playerId)
        {
            Console.WriteLine($"Jogador {opponentId} pronto. Iniciando jogo...");
        }
        else
        {
            Console.WriteLine("Esperando o adversário escolher o ID.");
        }

        InicializarTabuleiro(board);

        if (playerId == "Player1")
        {
            minhaVez = true;
            Console.WriteLine("Player 1, posicione seus barcos!");
            PosicionarBarcos();
            MostrarTabuleiro("Seu Tabuleiro");
            await EnviarMensagem("READY");
            Console.WriteLine("Aguardando o Player 2...");
            while (await ReceberMensagem() != "READY") { }
        }
        else if (playerId == "Player2")
        {
            minhaVez = false;
            Console.WriteLine("Aguardando o Player 1 posicionar os barcos...");
            while (await ReceberMensagem() != "READY") { }
            Console.WriteLine("Player 2, posicione seus barcos!");
            PosicionarBarcos();
            MostrarTabuleiro("Seu Tabuleiro");
            await EnviarMensagem("READY");
        }

        while (!gameOver)
        {
            if (minhaVez)
            {
                await JogarTurno();
                minhaVez = false;
                await EnviarMensagem("TURN_END");
            }
            else
            {
                Console.WriteLine("Aguardando o adversário...");
                string mensagem = await ReceberMensagem();
                if (mensagem.StartsWith("ATAQUE"))
                {
                    ProcessarAtaque(mensagem.Substring(7));
                    await EnviarMensagem("RESPONDER " + mensagem.Substring(7));
                }
                minhaVez = true;
            }
        }

        Console.WriteLine("Jogo finalizado!");
    }

    private static void InicializarTabuleiro(string[,] board)
    {
        for (int i = 0; i < 5; i++)
            for (int j = 0; j < 5; j++)
                board[i, j] = "🌊";
    }

    private static void PosicionarBarcos()
    {
        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine($"Posicione o barco {i + 1}: ");
            int x, y;
            while (true)
            {
                Console.Write("Digite a posição (ex: A1): ");
                string input = Console.ReadLine().ToUpper();
                if (input.Length == 2 && input[0] >= 'A' && input[0] <= 'E' && input[1] >= '1' && input[1] <= '5')
                {
                    x = input[1] - '1';
                    y = input[0] - 'A';
                    if (board[x, y] == "🌊")
                    {
                        board[x, y] = "🚢";
                        break;
                    }
                    else
                        Console.WriteLine("Posição já ocupada. Tente outra.");
                }
                else
                    Console.WriteLine("Entrada inválida. Use A1 a E5.");
            }
        }
    }

    private static void MostrarTabuleiro(string titulo)
    {
        Console.WriteLine($"\n{titulo}");
        Console.WriteLine("  A   B   C   D   E");
        for (int i = 0; i < 5; i++)
        {
            Console.Write((i + 1) + " ");
            for (int j = 0; j < 5; j++)
                Console.Write(board[i, j] + " ");
            Console.WriteLine();
        }
    }

    private static async Task JogarTurno()
    {
        Console.Clear();
        MostrarTabuleiro("Seu Tabuleiro");
        Console.WriteLine("Digite a posição do ataque (ex: A5): ");
        string ataque = Console.ReadLine().ToUpper();
        await EnviarMensagem("ATAQUE " + ataque);
        Console.WriteLine("Ataque enviado! Aguardando resposta...");
        string resposta = await ReceberMensagem();
        Console.WriteLine(resposta);  // Exibe a resposta de ACERTOU ou ERROU
    }

    private static void ProcessarAtaque(string ataque)
    {
        int x = ataque[1] - '1';
        int y = ataque[0] - 'A';

        if (board[x, y] == "🚢")
        {
            board[x, y] = "💥";
            Console.WriteLine("Fui atacado em " + ataque + "! Resultado: ACERTOU");
            VerificarFimDeJogo();
        }
        else
        {
            board[x, y] = "❌";
            Console.WriteLine("Fui atacado em " + ataque + "! Resultado: ERROU");
        }
    }

    private static void VerificarFimDeJogo()
    {
        // Verificar se todos os barcos foram afundados
        foreach (var cell in board)
        {
            if (cell == "🚢")
                return;
        }
        gameOver = true;
        Console.WriteLine("Todos os barcos foram afundados! Você venceu!");
    }

    private static async Task EnviarMensagem(string mensagem)
    {
        await using var client = new ServiceBusClient(connectionString);
        ServiceBusSender sender = client.CreateSender(queueName);
        ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(mensagem));
        await sender.SendMessageAsync(message);
    }

    private static async Task<string> ReceberMensagem()
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
