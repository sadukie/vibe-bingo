using System;
using System.Collections.Generic;
using System.Linq;

namespace VibeBingo.Core;

public class BingoGame
{
    public IReadOnlyList<string> AllBalls { get; }
    public HashSet<string> CalledBalls { get; } = new();
    public string? LastBall { get; private set; }
    private List<string> remainingBalls;
    private readonly Random rng = new();

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
