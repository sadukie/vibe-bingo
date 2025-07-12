
using System;
using VibeBingo.Core;
using System.Threading.Tasks;
using Spectre.Console;

namespace VibeBingo.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new BingoGame();
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

        static void StartNewRound(BingoGame game)
        {
            game.StartNewRound();
            // TTS voice selection
            string? selectedVoice = null;
            using (var synth = new System.Speech.Synthesis.SpeechSynthesizer())
            {
                var voices = synth.GetInstalledVoices().Select(v => v.VoiceInfo).ToList();
                Console.WriteLine("Select TTS voice:");
                for (int i = 0; i < voices.Count; i++)
                {
                    var v = voices[i];
                    Console.WriteLine($"{i}. {v.Name} ({v.Culture}){(v.Name == synth.Voice.Name ? " [default]" : "")}");
                }
                Console.Write($"Enter choice (0-{voices.Count - 1}, Enter for default): ");
                var vinput = Console.ReadLine();
                int vidx = 0;
                if (!string.IsNullOrWhiteSpace(vinput) && int.TryParse(vinput, out int tmp) && tmp >= 0 && tmp < voices.Count)
                    vidx = tmp;
                selectedVoice = voices[vidx].Name;
            }

            // Caller mode selection
            Console.WriteLine("Select Caller Mode:");
            var modeNames = Enum.GetNames(typeof(BingoCallerMode));
            for (int i = 0; i < modeNames.Length; i++)
            {
                // Show friendly names
                string display = modeNames[i] switch
                {
                    "Normal" => "Normal",
                    "Traditional" => "Traditional",
                    "KidFriendly" => "Kid-Friendly",
                    "Pirate" => "Pirate",
                    _ => modeNames[i]
                };
                Console.WriteLine($"{i}. {display}");
            }
            Console.Write($"Enter choice (0-{modeNames.Length - 1}, Enter for Normal): ");
            var minput = Console.ReadLine();
            int midx = 0;
            if (!string.IsNullOrWhiteSpace(minput) && int.TryParse(minput, out int tmpm) && tmpm >= 0 && tmpm < modeNames.Length)
                midx = tmpm;
            game.CallerMode = (BingoCallerMode)midx;

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
                Console.Write($"Enter choice (0-{delays.Length - 1}): ");
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
                        if (ball != null)
                        {
                            RedrawGridWithStatus(game);
                            SpeakBallWithPrompt(game, ball, selectedVoice);
                        }
                    }
                    int interval = 100;
                    int waited = 0;
                    while (waited < autoDelay && !autoCallToken.IsCancellationRequested)
                    {
                        await Task.Delay(interval, autoCallToken);
                        waited += interval;
                        // Check for key press
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.P)
                            {
                                autoPaused = !autoPaused;
                                RedrawGridWithStatus(game);
                            }
                            else if (key.Key == ConsoleKey.Q)
                            {
                                autoCallCts.Cancel();
                                return;
                            }
                        }
                        if (autoPaused) break;
                    }
                    while (autoPaused && !autoCallToken.IsCancellationRequested)
                    {
                        await Task.Delay(200, autoCallToken);
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.P)
                            {
                                autoPaused = !autoPaused;
                                RedrawGridWithStatus(game);
                                break;
                            }
                            else if (key.Key == ConsoleKey.Q)
                            {
                                autoCallCts.Cancel();
                                return;
                            }
                        }
                    }
                }
            }, autoCallToken) : Task.CompletedTask;

            if (autoDelay == 0)
            {
                while (game.BallsRemaining > 0)
                {
                    RedrawGridWithStatus(game);
                    var input = Console.ReadLine();
                    if (input?.Trim().ToLower() == "q")
                    {
                        autoCallCts.Cancel();
                        break;
                    }
                    if (input?.Trim().ToLower() == "p")
                    {
                        // Pause/resume is not relevant when auto-caller is off
                        continue;
                    }
                    var ball = game.CallNextBall();
                    if (ball != null)
                    {
                        RedrawGridWithStatus(game);
                        SpeakBallWithPrompt(game, ball, selectedVoice);
                    }
                }
            }
            else
            {
                // Only the auto-caller task will update the grid and status
                autoCallTask.Wait();
            }
            autoCallCts.Cancel();
            try
            {
                autoCallTask.Wait();
            }
            catch (AggregateException ex)
            {
                // Ignore TaskCanceledException, rethrow others
                ex.Handle(e => e is TaskCanceledException);
            }
            RedrawGridWithStatus(game);
            if (game.BallsRemaining == 0)
            {
                Console.WriteLine("All balls have been called this round!");
            }
            Console.WriteLine("\nRound ended. Press Enter to return to main menu.");
            Console.ReadLine();
        }

        static void DisplayGrid(BingoGame game)
        {
            var table = new Table();
            string[] labels = {"B", "I", "N", "G", "O"};
            foreach (var label in labels)
                table.AddColumn(new TableColumn($"[bold blue]{label}[/]").Centered());

            for (int row = 0; row < 15; row++)
            {
                var cells = new List<Markup>();
                for (int col = 0; col < 5; col++)
                {
                    int number = 1 + row + col * 15;
                    string letter = labels[col];
                    string ball = $"{letter}{number}";
                    if (game.CalledBalls.Contains(ball))
                    {
                        if (game.LastBall == ball)
                        {
                            cells.Add(new Markup($"[black on yellow][[{ball,3}]][/]").Centered());
                        }
                        else
                        {
                            cells.Add(new Markup($"[white on green]{ball,3}[/]").Centered());
                        }
                    }
                    else
                    {
                        cells.Add(new Markup("[grey] .  [/]").Centered());
                    }
                }
                table.AddRow(cells);
            }
            AnsiConsole.Write(table);
            if (!string.IsNullOrEmpty(game.LastBall))
            {
                AnsiConsole.MarkupLine($"\n[bold yellow]>>> {game.LastBall} <<<[/]\n");
            }
        }

        static void RedrawGridWithStatus(BingoGame game)
        {
            AnsiConsole.Clear();
            DisplayGrid(game);
            AnsiConsole.MarkupLine($"[bold cyan]Balls remaining:[/] [bold]{game.BallsRemaining}[/]");
            AnsiConsole.MarkupLine("[grey]Press Enter to call next ball, 'p' to pause/resume auto-call, 'q' to quit this round.[/]");
        }

        static void SpeakBallWithPrompt(BingoGame game, string ball, string? voiceName)
        {
            try
            {
                using var synth = new System.Speech.Synthesis.SpeechSynthesizer();
                if (!string.IsNullOrEmpty(voiceName))
                {
                    try { synth.SelectVoice(voiceName); } catch { /* ignore if not found */ }
                }
                var prompt = game.GetCallerPrompt(ball);
                synth.Speak(prompt);
            }
            catch
            {
                // If TTS fails (e.g., not Windows), ignore
            }
        }
    }
}
