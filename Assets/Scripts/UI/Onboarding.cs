using UnityEngine;

// The floor signage covers the controls you need at the start. This is for the one that
// can't be a sign: the mirror only means anything once something is behind you.
public class Onboarding : Singleton<Onboarding>
{
    [SerializeField] private float hintAtProximity = 0.25f;
    [SerializeField] private bool onlyOncePerSession = true;

    private bool mirrorHintShown;
    private Pursuer pursuer;

    private void Update()
    {
        if (mirrorHintShown)
        {
            return;
        }

        pursuer = pursuer != null ? pursuer : Pursuer.Instance;
        if (pursuer == null || pursuer.Proximity < hintAtProximity)
        {
            return;
        }

        // If they already found it, don't explain it.
        if (RearView.Instance != null && RearView.Instance.IsRaised)
        {
            mirrorHintShown = true;
            return;
        }

        mirrorHintShown = onlyOncePerSession;
        ToastManager.GetInstance().Show("hold SHIFT to look back");
    }
}
