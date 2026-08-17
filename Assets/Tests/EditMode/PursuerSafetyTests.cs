using System.Collections.Generic;
using NUnit.Framework;

public class PursuerSafetyTests
{
    private const float Spacing = 2f;
    private const float PlayerHalf = 0.6f;

    private static List<HazardLanes.Span> Blocked(params (float centre, float half)[] items)
    {
        var list = new List<HazardLanes.Span>();
        foreach (var (centre, half) in items)
        {
            list.Add(new HazardLanes.Span(centre, half));
        }

        return list;
    }

    private static int Allowed(List<HazardLanes.Span> blocked)
    {
        return PursuerSafety.AllowedLanes(blocked, Spacing, PlayerHalf);
    }

    [Test]
    public void ClearTrackAllowsEveryLane()
    {
        Assert.AreEqual(PursuerAttackModel.AllLanes, Allowed(Blocked()));
    }

    [Test]
    public void ABlockedLaneIsNotATarget()
    {
        var mask = Allowed(Blocked((0f, 1f)));

        Assert.IsFalse(PursuerSafety.LaneAllowed(mask, AttackLane.Centre), "centre is already blocked");
        Assert.IsTrue(PursuerSafety.LaneAllowed(mask, AttackLane.Left));
        Assert.IsTrue(PursuerSafety.LaneAllowed(mask, AttackLane.Right));
    }

    // The rule the whole fairness story rests on.
    [Test]
    public void TheLastOpenLaneIsNeverTargeted()
    {
        var mask = Allowed(Blocked((-2f, 1f), (0f, 1f)));
        Assert.AreEqual(0, mask, "only the right lane was left, so there is no fair attack");
    }

    [Test]
    public void ATotallyBlockedRowAllowsNothing()
    {
        Assert.AreEqual(0, Allowed(Blocked((-2f, 1f), (0f, 1f), (2f, 1f))));
    }

    [Test]
    public void TwoOpenLanesAreEnough()
    {
        var mask = Allowed(Blocked((-2f, 1f)));

        Assert.AreNotEqual(0, mask);
        Assert.IsFalse(PursuerSafety.LaneAllowed(mask, AttackLane.Left));
        Assert.IsTrue(PursuerSafety.LaneAllowed(mask, AttackLane.Centre));
        Assert.IsTrue(PursuerSafety.LaneAllowed(mask, AttackLane.Right));
    }

    [Test]
    public void ANarrowHazardBetweenLanesBlocksNeither()
    {
        // sits at -1, touching neither the left nor the centre lane box
        Assert.AreEqual(PursuerAttackModel.AllLanes, Allowed(Blocked((-1f, 0.3f))));
    }

    [Test]
    public void NullBlockersAreSafe()
    {
        Assert.AreEqual(PursuerAttackModel.AllLanes, PursuerSafety.AllowedLanes(null, Spacing, PlayerHalf));
    }

    // Together these are the guarantee: whatever the mask says, a legal response survives.
    [Test]
    public void AnAllowedTargetAlwaysLeavesSomewhereToGo()
    {
        var rows = new[]
        {
            Blocked(),
            Blocked((0f, 1f)),
            Blocked((-2f, 1f)),
            Blocked((2f, 1f)),
            Blocked((-1f, 0.3f)),
        };

        foreach (var row in rows)
        {
            var mask = Allowed(row);
            if (mask == 0)
            {
                continue;
            }

            foreach (AttackLane lane in new[] { AttackLane.Left, AttackLane.Centre, AttackLane.Right })
            {
                if (!PursuerSafety.LaneAllowed(mask, lane))
                {
                    continue;
                }

                var escape = false;
                foreach (AttackLane other in new[] { AttackLane.Left, AttackLane.Centre, AttackLane.Right })
                {
                    if (other != lane && PursuerSafety.LaneAllowed(mask, other))
                    {
                        escape = true;
                    }
                }

                Assert.IsTrue(escape, "targeting " + lane + " left the player nowhere legal");
            }
        }
    }
}
