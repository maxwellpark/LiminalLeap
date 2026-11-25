using System;
using System.Collections.Generic;

// Prints what the real rules decide, for the acceptance pack. Seeded, so it reads the same each run.
public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("== The office: laps of 30, 2 right calls to get out, seed 7");
        Console.WriteLine("   The player calls right except on lap 2, where they take the exit past an anomaly.");
        var rule = new LapRule(7, 2, 0.5f);
        for (var guard = 0; guard < 12; guard++)
        {
            var lap = rule.Lap;
            var kind = rule.KindFor(lap);
            var shown = rule.CountShown(lap);
            var rules = lap == rule.CleanLap ? "rules on floor" : "no rules";
            var takes = kind == AnomalyKind.None;
            if (lap == 2 && kind != AnomalyKind.None)
            {
                takes = true;
            }
            var where = kind is AnomalyKind.None or AnomalyKind.WrongCount ? "" : $" (pieces {rule.StretchStart(lap, 30)}-{rule.StretchStart(lap, 30) + LapRule.StretchPieces - 1})";
            var call = rule.Call(takes);
            Console.WriteLine($"   lap {lap,2}: floor shows {shown}, {rules}, anomaly {kind}{where}; {(takes ? "takes the exit" : "runs past")} -> {call}, count {rule.Streak}");
            if (call == LapCall.Leave)
            {
                break;
            }
        }

        var calls = new List<string>();
        foreach (var right in rule.History)
        {
            calls.Add(right ? "right" : "wrong");
        }
        Console.WriteLine($"   calls made: {string.Join(", ", calls)}");
        Console.WriteLine();
        Console.WriteLine("   Share line on getting out:");
        foreach (var line in ShareLine.ForPlace("the office", rule.History).Split('\n'))
        {
            Console.WriteLine("     " + line);
        }

        Console.WriteLine();
        Console.WriteLine("== Forks in a free run: every 24 pieces after the first 20, seed 1234");
        var loops = new LoopRule(1234, 24, 20);
        var lastFork = int.MinValue / 2;
        var fork = 0;
        for (var piece = 0; piece < 120; piece++)
        {
            if (!loops.Due(piece, lastFork))
            {
                continue;
            }
            Console.WriteLine($"   piece {piece,3}: fork {fork}, the {(loops.LoopDoor(fork) == LoopRule.LeftDoor ? "left" : "right")} door lays the last 10 pieces again");
            lastFork = piece;
            fork++;
        }

        Console.WriteLine();
        Console.WriteLine("== The HUD distance through a looping door: 10 m pieces, an exit at piece 15, the fork is piece 20");
        Console.WriteLine("   Exits aren't laid again, so the replay is 10 pieces over 110 m of the first run.");
        var memory = new TrackMemory(32);
        var laid = 0f;
        for (var piece = 0; piece < 20; piece++)
        {
            if (piece != 15)
            {
                memory.Add(new TrackMemory.Piece(0, piece, laid));
            }
            laid += 10f;
        }
        laid += 10f; // the fork's own piece, which isn't remembered either
        var replay = new TrackMemory.Piece[10];
        var replayed = memory.CopyLast(10, replay);
        var readout = new DistanceReadout();
        var behind = 0f;
        for (var i = 0; i < replayed; i++, laid += 10f)
        {
            // As the generator does for each piece it lays again.
            if (laid - replay[i].Start != behind)
            {
                behind = laid - replay[i].Start;
                readout.From(laid, behind);
            }
        }
        foreach (var at in new[] { 150f, 159f, 160f, 205f, 209f, 210f, 260f, 269f, 270f, 310f, 400f })
        {
            readout.Advance(at);
            Console.WriteLine($"   true {at,4:0} m   HUD {readout.Shown(at),4:0} m");
        }

        Console.WriteLine();
        Console.WriteLine("== Daily share lines from the real ShareLine");
        var marks = new[] { StretchMark.Quiet, StretchMark.NearMiss, StretchMark.LookedBack, StretchMark.Dodged, StretchMark.Quiet, StretchMark.Dodged };
        Console.WriteLine("   Got out at 287 m with 3,120, through an exit:");
        foreach (var line in ShareLine.For("2026-11-24", "the house", RunOutcome.Banked, 287f, 3120f, true, marks).Split('\n'))
        {
            Console.WriteLine("     " + line);
        }
        Console.WriteLine("   Caught at 287 m after passing an exit, so the score is lost and not claimed:");
        foreach (var line in ShareLine.For("2026-11-24", "the house", RunOutcome.Died, 287f, 3120f, false, marks).Split('\n'))
        {
            Console.WriteLine("     " + line);
        }
        Console.WriteLine("   Summary preview of that strip (rich text in the game font):");
        Console.WriteLine("     " + ShareLine.Preview(287f, marks));
    }
}
