using System;
using System.Collections.Generic;
using System.Linq;

namespace VibeBingo.Core;

public enum BingoCallerMode
{
    Normal,
    Traditional,
    KidFriendly
}

public class BingoGame
{
    public IReadOnlyList<string> AllBalls { get; }
    public HashSet<string> CalledBalls { get; } = new();
    public string? LastBall { get; private set; }
    private List<string> remainingBalls;
    private readonly Random rng = new();

    public BingoCallerMode CallerMode { get; set; } = BingoCallerMode.Normal;

    public BingoGame()
    {
        AllBalls = GenerateBalls();
        remainingBalls = new List<string>(AllBalls);
    }

    public void StartNewRound()
    {
        CalledBalls.Clear();
        remainingBalls = new List<string>(AllBalls);
        LastBall = null;
    }

    public string? CallNextBall()
    {
        if (remainingBalls.Count == 0)
            return null;
        int idx = rng.Next(remainingBalls.Count);
        var ball = remainingBalls[idx];
        remainingBalls.RemoveAt(idx);
        CalledBalls.Add(ball);
        LastBall = ball;
        return ball;
    }


    public string GetCallerPrompt(string ball)
    {
        string spokenBall = GetBallForSpeech(ball);
        switch (CallerMode)
        {
            case BingoCallerMode.Traditional:
                return GetTraditionalPrompt(spokenBall);
            case BingoCallerMode.KidFriendly:
                return GetKidsPrompt(spokenBall);
            default:
                return spokenBall;
        }
    }

    private string GetBallForSpeech(string ball)
    {
        // Expects input like "B10" and returns "B-ten"
        if (string.IsNullOrWhiteSpace(ball) || ball.Length < 2)
            return ball;
        string letter = ball.Substring(0, 1);
        if (!int.TryParse(ball.Substring(1), out int number))
            return ball;
        return $"{letter}-{NumberToWords(number)}";
    }

    private static string NumberToWords(int number)
    {
        // Handles 1-75 for BINGO
        if (number < 1 || number > 75) return number.ToString();
        string[] ones = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
        string[] tens = { "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy" };
        if (number < 20) return ones[number];
        if (number % 10 == 0) return tens[number / 10];
        if (number < 100)
            return $"{tens[number / 10]}-{ones[number % 10]}";
        return number.ToString();
    }

private string GetTraditionalPrompt(string spokenBall)
{
    // Extract the number from spokenBall (e.g., "B-ten" -> 10)
    int number = 0;
    var dashIdx = spokenBall.IndexOf('-');
    if (dashIdx > 0)
    {
        var numPart = spokenBall[(dashIdx+1)..].Trim();
        // Try parse as int first, then as word (with hyphens)
        if (!int.TryParse(numPart.Replace("-", ""), out number))
        {
            number = NumberFromWords(numPart.ToLowerInvariant());
        }
    }

    var calls = GetTraditionalBingoCalls();
    if (number >= 1 && number <= 75 && calls.TryGetValue(number, out var call))
    {
        return $"{call}! That's {spokenBall}";
    }
    return spokenBall;
}

    private static int NumberFromWords(string words)
    {
        // Only supports 1-75 for BINGO
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            {"one",1},{"two",2},{"three",3},{"four",4},{"five",5},{"six",6},{"seven",7},{"eight",8},{"nine",9},{"ten",10},
            {"eleven",11},{"twelve",12},{"thirteen",13},{"fourteen",14},{"fifteen",15},{"sixteen",16},{"seventeen",17},{"eighteen",18},{"nineteen",19},
            {"twenty",20},{"twenty-one",21},{"twenty-two",22},{"twenty-three",23},{"twenty-four",24},{"twenty-five",25},{"twenty-six",26},{"twenty-seven",27},{"twenty-eight",28},{"twenty-nine",29},
            {"thirty",30},{"thirty-one",31},{"thirty-two",32},{"thirty-three",33},{"thirty-four",34},{"thirty-five",35},{"thirty-six",36},{"thirty-seven",37},{"thirty-eight",38},{"thirty-nine",39},
            {"forty",40},{"forty-one",41},{"forty-two",42},{"forty-three",43},{"forty-four",44},{"forty-five",45},{"forty-six",46},{"forty-seven",47},{"forty-eight",48},{"forty-nine",49},
            {"fifty",50},{"fifty-one",51},{"fifty-two",52},{"fifty-three",53},{"fifty-four",54},{"fifty-five",55},{"fifty-six",56},{"fifty-seven",57},{"fifty-eight",58},{"fifty-nine",59},
            {"sixty",60},{"sixty-one",61},{"sixty-two",62},{"sixty-three",63},{"sixty-four",64},{"sixty-five",65},{"sixty-six",66},{"sixty-seven",67},{"sixty-eight",68},{"sixty-nine",69},
            {"seventy",70},{"seventy-one",71},{"seventy-two",72},{"seventy-three",73},{"seventy-four",74},{"seventy-five",75}
        };
        words = words.Trim().ToLowerInvariant();
        if (map.TryGetValue(words, out int n)) return n;
        return 0;
    }

    private static Dictionary<int, string> GetTraditionalBingoCalls()
    {
        // 1-75 traditional/funny calls (UK/US mix, can be customized)
        return new Dictionary<int, string>
        {
            {1, "Kelly's Eye"},
            {2, "One Little Duck"},
            {3, "Cup of Tea"},
            {4, "Knock at the Door"},
            {5, "Man Alive"},
            {6, "Half a Dozen"},
            {7, "Lucky Seven"},
            {8, "Garden Gate"},
            {9, "Doctor's Orders"},
            {10, "Boris's Den"},
            {11, "Legs Eleven"},
            {12, "One Dozen"},
            {13, "Unlucky for Some"},
            {14, "Valentine's Day"},
            {15, "Young and Keen"},
            {16, "Sweet Sixteen"},
            {17, "Dancing Queen"},
            {18, "Coming of Age"},
            {19, "Goodbye Teens"},
            {20, "One Score"},
            {21, "Royal Salute"},
            {22, "Two Little Ducks"},
            {23, "Thee and Me"},
            {24, "Two Dozen"},
            {25, "Duck and Dive"},
            {26, "Pick and Mix"},
            {27, "Gateway to Heaven"},
            {28, "In a State"},
            {29, "Rise and Shine"},
            {30, "Dirty Gertie"},
            {31, "Get Up and Run"},
            {32, "Buckle My Shoe"},
            {33, "All the Threes"},
            {34, "Ask for More"},
            {35, "Jump and Jive"},
            {36, "Three Dozen"},
            {37, "More than Eleven"},
            {38, "Christmas Cake"},
            {39, "39 Steps"},
            {40, "Life Begins"},
            {41, "Time for Fun"},
            {42, "Winnie the Pooh"},
            {43, "Down on Your Knees"},
            {44, "Droopy Drawers"},
            {45, "Halfway There"},
            {46, "Up to Tricks"},
            {47, "Four and Seven"},
            {48, "Four Dozen"},
            {49, "PC"},
            {50, "Half a Century"},
            {51, "Tweak of the Thumb"},
            {52, "Danny La Rue"},
            {53, "Here Comes Herbie"},
            {54, "Clean the Floor"},
            {55, "Snakes Alive"},
            {56, "Was She Worth It?"},
            {57, "Heinz Varieties"},
            {58, "Make Them Wait"},
            {59, "Brighton Line"},
            {60, "Five Dozen"},
            {61, "Baker's Bun"},
            {62, "Turn the Screw"},
            {63, "Tickle Me 63"},
            {64, "Red Raw"},
            {65, "Old Age Pension"},
            {66, "Clickety Click"},
            {67, "Stairway to Heaven"},
            {68, "Saving Grace"},
            {69, "Either Way Up"},
            {70, "Three Score and Ten"},
            {71, "Bang on the Drum"},
            {72, "Six Dozen"},
            {73, "Queen Bee"},
            {74, "Hit the Floor"},
            {75, "Strive and Strive"}
        };
    }

    private string GetKidsPrompt(string spokenBall)
    {
        var kidsLines = new[]
        {
            $"Give a cheer! It's {spokenBall}!",
            $"Jump up and down for {spokenBall}!",
            $"Clap your hands, it's {spokenBall}!",
            $"Can you shout BINGO? It's {spokenBall}!",
            $"Wiggle your fingers for {spokenBall}!",
            $"Do a silly dance for {spokenBall}!",
            $"Smile big, it's {spokenBall}!",
            $"Touch your toes for {spokenBall}!",
            $"Spin around for {spokenBall}!",
            $"High five someone for {spokenBall}!"
        };
        return kidsLines[rng.Next(kidsLines.Length)];
    }

    public int BallsRemaining => remainingBalls.Count;

    private static List<string> GenerateBalls()
    {
        var balls = new List<string>();
        balls.AddRange(Enumerable.Range(1, 15).Select(n => $"B{n}"));
        balls.AddRange(Enumerable.Range(16, 15).Select(n => $"I{n}"));
        balls.AddRange(Enumerable.Range(31, 15).Select(n => $"N{n}"));
        balls.AddRange(Enumerable.Range(46, 15).Select(n => $"G{n}"));
        balls.AddRange(Enumerable.Range(61, 15).Select(n => $"O{n}"));
        return balls;
    }
}
