using System;

// The lights advance at their own pace. Outrun them and you're in the dark until you
// slow down.
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

    // Advances regardless, so slowing down lets it catch up.
    public static float Advance(float front, float dt, float travelled, Settings s)
    {
        if (s == null || dt <= 0f)
        {
            return front;
        }

        var speed = Math.Max(0f, s.Speed);

        // Catches up faster once you're back in the light.
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
