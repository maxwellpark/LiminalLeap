using TMPro;
using UnityEngine;

// Read live, since a lap is laid before the one in front of it has been called.
public class LapSign : MonoBehaviour
{
    public const string WrongLine = "ANYTHING WRONG    KEEP RUNNING";
    public const string RightLine = "NOTHING WRONG    LEAVE";

    [SerializeField] private float numberAt = 2.5f;
    [SerializeField] private float rulesAt = 5.5f;
    [SerializeField] private float numberSize = 9f;
    [SerializeField] private float ruleSize = 2.6f;
    [SerializeField] private Color colour = new(0.62f, 0.66f, 0.72f, 0.85f);

    private TextMeshPro number;
    private TextMeshPro wrongRule;
    private TextMeshPro rightRule;
    private TextMeshPro goalRule;
    private int lap;
    private int shown = int.MinValue;

    public int Shown => shown;

    // The rules only on a clean lap, which is where you land after a wrong call.
    public void Paint(int forLap, bool withRules)
    {
        lap = forLap;

        if (number == null)
        {
            number = WorldSign.Floor(transform, "0", new Vector3(0f, 0.02f, numberAt), numberSize, colour);
            wrongRule = WorldSign.Floor(transform, WrongLine, new Vector3(0f, 0.02f, rulesAt), ruleSize, colour);
            rightRule = WorldSign.Floor(transform, RightLine, new Vector3(0f, 0.02f, rulesAt + 1.4f), ruleSize, colour);
            goalRule = WorldSign.Floor(transform, "", new Vector3(0f, 0.02f, rulesAt + 2.8f), ruleSize, colour);
        }

        var rule = StoryLaps.Rule;
        if (rule != null)
        {
            goalRule.SetText(LapRule.GoalLine(rule.Required));
        }

        number.gameObject.SetActive(true);
        wrongRule.gameObject.SetActive(withRules);
        rightRule.gameObject.SetActive(withRules);
        goalRule.gameObject.SetActive(withRules && rule != null);

        shown = int.MinValue;
        Refresh();
    }

    public void Hide()
    {
        if (number == null)
        {
            return;
        }

        number.gameObject.SetActive(false);
        wrongRule.gameObject.SetActive(false);
        rightRule.gameObject.SetActive(false);
        goalRule.gameObject.SetActive(false);
    }

    private void Update()
    {
        Refresh();
    }

    // Only on change, as SetText still rebuilds the mesh.
    private void Refresh()
    {
        var rule = StoryLaps.Rule;
        if (number == null || rule == null || !number.gameObject.activeSelf)
        {
            return;
        }

        var count = rule.CountShown(lap);
        if (count == shown)
        {
            return;
        }

        shown = count;
        number.SetText("{0}", count);
    }
}
