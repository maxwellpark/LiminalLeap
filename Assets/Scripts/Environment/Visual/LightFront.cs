using System;

// The lights come on at their own pace, ahead of you, and they do not hurry.
//
// Every other system here rewards speed without qualification. This is the one that does
// not: push past the front and you are running into a corridor you cannot see, and the
// only way back into the light is to slow down and let it catch up.
public static class LightFront
{
    [Serializable]
    public class Settings
    {
        public float Speed = 14f;       // units/sec the lighting advances, mid range on purpose
        public float HeadStart = 60f;   // lit corridor you begin with
        public float FadeRange = 45f;   // how far past the front before it is fully dark
        public float Recovery = 1.6f;   // extra catch up once you are behind it again
    }

    public static float Start(Settings s)
    {
        return Math.Max(0f, s?.HeadStart ?? 0f);
    }

    // Advances regardless of what the player does. Slowing lets it gain on you, which is
    // the whole recovery mechanic.
    public static float Advance(float front, float dt, float travelled, Settings s)
    {
        if (s == null || dt <= 0f)
        {
            return front;
        }

        var speed = Math.Max(0f, s.Speed);

        // Once you are back inside the lit stretch it closes up a little faster, so a
        // recovery does not take the whole rest of the run.
        if (front > travelled)
        {
            speed *= Math.Max(1f, s.Recovery);
        }

        return front + speed * dt;
    }

    // 0 while the front is still ahead of you, 1 once you are a full fade past it.
    public static float Darkness(float travelled, float front, Settings s)
    {
        var range = Math.Max(0.0001f, s?.FadeRange ?? 1f);
        var over = travelled - front;

        return over <= 0f ? 0f : Math.Min(1f, over / range);
    }
}
