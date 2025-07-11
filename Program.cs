
using System;
// using VibeBingo.Core; (removed to avoid type conflict)

namespace vibe_bingo;

class Program
{
    static void Main(string[] args)
    {
        var game = new VibeBingo.Core.BingoGame();
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Vibe BINGO Caller ===\n");
            Console.WriteLine("1. Start new round");
            Console.WriteLine("2. Exit");
            Console.Write("Select an option: ");
            var input = Console.ReadLine();
            if (input == "1")
            {
                StartNewRound(game);
            }
            else if (input == "2")
            {
                break;
            }
        }
    }

    static void StartNewRound(VibeBingo.Core.BingoGame game)
    {
        game.StartNewRound();
        int[] delays = { 0, 5000, 10000, 15000, 20000 };
        string[] delayLabels = { "Disabled", "5s", "10s", "15s", "20s" };
        int delayIdx = 0;
        bool autoPaused = false;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Vibe BINGO Caller ===\n");
            Console.WriteLine("Select auto-call delay:");
            for (int i = 0; i < delays.Length; i++)
                Console.WriteLine($"{i}. {delayLabels[i]}");
            Console.Write("Enter choice (0-{0}): ", delays.Length - 1);
            if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 0 && idx < delays.Length)
                delayIdx = idx;
            else
                delayIdx = 0;
            break;
        }

        int autoDelay = delays[delayIdx];
        autoPaused = false;
        var autoCallCts = new System.Threading.CancellationTokenSource();
        var autoCallToken = autoCallCts.Token;

        var autoCallTask = autoDelay > 0 ? Task.Run(async () => {
            while (game.BallsRemaining > 0 && !autoCallToken.IsCancellationRequested)
            {
                if (!autoPaused)
                {
                    var ball = game.CallNextBall();
                    if (ball != null) SpeakBall(ball);
                    DisplayGrid(game);
                }
                for (int i = 0; i < autoDelay / 500 && !autoCallToken.IsCancellationRequested; i++)
                {
                    await Task.Delay(500, autoCallToken);
                    if (autoPaused) break;
                }
                while (autoPaused && !autoCallToken.IsCancellationRequested)
                {
                    await Task.Delay(500, autoCallToken);
                }
            }
        }, autoCallToken) : Task.CompletedTask;

        while (game.BallsRemaining > 0)
        {
            DisplayGrid(game);
            Console.WriteLine($"Balls remaining: {game.BallsRemaining}");
            Console.WriteLine("Press Enter to call next ball, 'p' to pause/resume auto-call, 'q' to quit this round.");
            var input = Console.ReadLine();
            if (input?.Trim().ToLower() == "q")
            {
                autoCallCts.Cancel();
                break;
            }
            if (input?.Trim().ToLower() == "p" && autoDelay > 0)
            {
                autoPaused = !autoPaused;
                Console.WriteLine(autoPaused ? "Auto-call paused." : "Auto-call resumed.");
                continue;
            }
            if (autoDelay == 0 && game.BallsRemaining > 0)
            {
                var ball = game.CallNextBall();
                if (ball != null) SpeakBall(ball);
            }
        }
        autoCallCts.Cancel();
        autoCallTask.Wait();
        DisplayGrid(game);
        if (game.BallsRemaining == 0)
        {
            Console.WriteLine("All balls have been called this round!");
        }
        Console.WriteLine("\nRound ended. Press Enter to return to main menu.");
        Console.ReadLine();
    }

    static void DisplayGrid(VibeBingo.Core.BingoGame game)
    {
        Console.Clear();
        string[] labels = {"B", "I", "N", "G", "O"};
        for (int col = 0; col < 5; col++)
        {
            Console.Write($"  {labels[col],-4}");
        }
        Console.WriteLine();
        for (int row = 0; row < 15; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                int number = 1 + row + col * 15;
                string letter = "BINGO"[col].ToString();
                string ball = $"{letter}{number}";
                if (game.CalledBalls.Contains(ball))
                {
                    if (game.LastBall == ball)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"[{ball,3}]");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($" {ball,3} ");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.Write($"  .   ");
                }
                Console.Write(" ");
            }
            Console.WriteLine();
        }
        if (!string.IsNullOrEmpty(game.LastBall))
        {
            Console.WriteLine($"\n>>> {game.LastBall} <<<\n");
        }
    }

    static void SpeakBall(string ball)
    {
        try
        {
            using var synth = new System.Speech.Synthesis.SpeechSynthesizer();
            synth.Speak(ball.Replace("B", "B ").Replace("I", "I ").Replace("N", "N ").Replace("G", "G ").Replace("O", "O "));
        }
        catch
        {
            // If TTS fails (e.g., not Windows), ignore
        }
    }
}

