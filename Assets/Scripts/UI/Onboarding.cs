using UnityEngine;

// Teaches the mirror at the moment it has an answer.
public class Onboarding : Singleton<Onboarding>
{
    [SerializeField] private float hintAtProximity = 0.25f;
    [SerializeField] private bool onlyOncePerSession = true;

    private bool mirrorHintShown;
    private bool laneHintShown;
    private bool hintedThisAttack;
    private Pursuer pursuer;

    private void Update()
    {
        if (!PlayerTrackMovement.Running)
        {
            return;
        }

        pursuer = pursuer != null ? pursuer : Pursuer.Instance;
        if (pursuer == null)
        {
            return;
        }

        if (Features.On(Feature.PursuerAttacks))
        {
            HintOnFirstAttack();
            return;
        }

        HintOnProximity();
    }

    // Told when it's worth acting on.
    private void HintOnFirstAttack()
    {
        var warning = pursuer.Attack != null && pursuer.Attack.Phase == AttackPhase.Warning;

        // Latched per attack, or it fires every frame of the warning.
        if (!warning)
        {
            hintedThisAttack = false;
            return;
        }

        if (laneHintShown || hintedThisAttack)
        {
            return;
        }

        hintedThisAttack = true;
        laneHintShown = onlyOncePerSession;

        if (Raised())
        {
            return; // already looking, so there is nothing to explain
        }

        ToastManager.GetInstance().Show("hold SHIFT to see which lane");
    }

    private void HintOnProximity()
    {
        if (mirrorHintShown || pursuer.Proximity < hintAtProximity)
        {
            return;
        }

        // If they already found it, don't explain it.
        if (Raised())
        {
            mirrorHintShown = true;
            return;
        }

        mirrorHintShown = onlyOncePerSession;
        ToastManager.GetInstance().Show("hold SHIFT to look back");
    }

    private static bool Raised()
    {
        return RearView.Instance != null && RearView.Instance.IsRaised;
    }
}
