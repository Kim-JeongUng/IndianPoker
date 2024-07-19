using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        GameServer server = new GameServer();
        server.Start();
    }
}

public class GameServer
{
    private TcpListener listener;
    public List<Player> players; // players 리스트를 public으로 변경
    private bool isRunning;
    private bool isGameStart;
    private static Random random = new Random();
    private int currentBet = 0;
    private int totalPot = 0;
    private int currentPlayerIndex = 0;
    private bool waitingForBets = false;

    public GameServer()
    {
        listener = new TcpListener(IPAddress.Any, 5000);
        players = new List<Player>();
    }

    public void Start()
    {
        listener.Start();
        isRunning = true;
        Console.WriteLine("Server started on port 5000.");

        while (isRunning)
        {
            try
            {
                TcpClient client = listener.AcceptTcpClient();
                if (players.Count < 4)
                {
                    Player player = new Player(client, this);
                    lock (players)
                    {
                        players.Add(player);
                    }
                    Thread playerThread = new Thread(player.Run);
                    playerThread.Start();
                    Console.WriteLine("Player connected!");

                    if (players.Count == 4)
                    {
                        Broadcast("Four players connected. Ready to start the game. Please press the start button.");
                    }
                }
                else
                {
                    Console.WriteLine("Maximum players reached. Connection refused.");
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    public void Broadcast(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        lock (players)
        {
            foreach (Player player in players)
            {
                player.Stream.Write(data, 0, data.Length);
            }
        }
    }

    public void StartGame()
    {
        if (isGameStart)
            return;

        isGameStart = true;

        List<int> deck = new List<int>();
        for (int i = 1; i <= 13; i++) deck.Add(i);

        lock (players)
        {
            foreach (Player player in players)
            {
                int cardIndex = random.Next(deck.Count);
                int card = deck[cardIndex];
                deck.RemoveAt(cardIndex);

                player.Card = card;
                player.Coins = 30;  
                player.HasPlacedBet = false;
                player.Send($"Your card: {card}");
                Console.WriteLine($"player:{player.Name} :  {card}");
            }
        }

        Broadcast("All players have received their cards.");
        waitingForBets = true;
        NextTurn();
    }

    public void PlaceBet(Player player, int bet)
    {
        lock (players)
        {
            if (bet > player.Coins)
            {
                player.Send("Not enough coins.");
                return;
            }

            if (!waitingForBets)
            {
                player.Send("Not the right time to place a bet.");
                return;
            }

            if (player != players[currentPlayerIndex])
            {
                player.Send("It's not your turn to bet.");
                return;
            }

            currentBet = bet;
            totalPot += bet;
            player.Coins -= bet;
            player.HasPlacedBet = true;
            Broadcast($"{player.Name} bets {bet} coins. Current pot: {totalPot} coins.");

            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

            if (AllPlayersPlacedBets())
            {
                waitingForBets = false;
                EndGame();
            }
            else
            {
                NextTurn();
            }
        }
    }

    private void NextTurn()
    {
        Player currentPlayer = players[currentPlayerIndex];
        currentPlayer.Send("Your turn to bet.");
        Broadcast($"{currentPlayer.Name}'s turn to bet.");
    }

    private bool AllPlayersPlacedBets()
    {
        lock (players)
        {
            foreach (Player player in players)
            {
                if (!player.HasPlacedBet)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public void EndGame()
    {
        Player winner = null;
        int highestCard = 0;

        lock (players)
        {
            foreach (Player player in players)
            {
                if (player.Card > highestCard)
                {
                    highestCard = player.Card;
                    winner = player;
                }
            }

            if (winner != null)
            {
                winner.Coins += totalPot;
                Broadcast($"Player with card {winner.Card} wins and takes the pot of {totalPot} coins! They now have {winner.Coins} coins.");
                currentPlayerIndex = players.IndexOf(winner);
            }
            else
            {
                Broadcast("No winner this round.");
            }

            // Reset for next game
            totalPot = 0;
            currentBet = 0;
            foreach (Player player in players)
            {
                player.HasPlacedBet = false;
            }

            StartGame();
        }
    }

    public void RemovePlayer(Player player)
    {
        lock (players)
        {
            players.Remove(player);
        }
        Broadcast($"{player.Name} has left the game.");
        if (players.Count < 2)
        {
            Broadcast("Not enough players to continue. Waiting for more players to join.");
            waitingForBets = false;
        }
    }
}

public class Player
{
    public TcpClient Client { get; }
    public NetworkStream Stream { get; }
    public GameServer Server { get; }
    public int Card { get; set; }
    public int Coins { get; set; }
    public string Name { get; set; }
    public bool HasPlacedBet { get; set; }

    public Player(TcpClient client, GameServer server)
    {
        Client = client;
        Stream = client.GetStream();
        Server = server;
        Name = "Player " + (server.players.Count + 1);
        HasPlacedBet = false;
    }

    public void Run()
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = Stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received from {Name}: {message}");

                if (message.ToLower() == "exit")
                {
                    break;
                }
                else if (message.ToLower().StartsWith("bet "))
                {
                    if (int.TryParse(message.Split(' ')[1], out int bet))
                    {
                        Server.PlaceBet(this, bet);
                    }
                    else
                    {
                        Send("Invalid bet amount.");
                    }
                }
                else if (message.ToLower() == "start game")
                {
                    Server.StartGame();
                }
                else
                {
                    Server.Broadcast($"{Name}: {message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Player error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"{Name} disconnected.");
            Stream.Close();
            Client.Close();
            Server.RemovePlayer(this);
        }
    }

    public void Send(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        Stream.Write(data, 0, data.Length);
    }
}
