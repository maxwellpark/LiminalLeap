// Kept across deaths, so dying replays a lap rather than rerolling it.
public static class StoryLaps
{
    private static readonly System.Random Seeds = new();
    private static string placeId;

    public static LapRule Rule { get; private set; }

    // Set by the generator while it lays laps. A hand-built track has no laps to call.
    public static bool Laying { get; set; }

    public static LapRule Current => Laying ? Rule : null;

    public static LapRule For(Place place)
    {
        if (place == null || place.Calls <= 0)
        {
            return null;
        }

        if (Rule == null || placeId != place.Id)
        {
            placeId = place.Id;
            Rule = new LapRule(Seeds.Next(), place.Calls, place.AnomalyChance);
        }

        return Rule;
    }

    public static void Clear()
    {
        Rule = null;
        placeId = null;
        Laying = false;
    }
}
