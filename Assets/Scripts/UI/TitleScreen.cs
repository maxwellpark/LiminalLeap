using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Builds its own UI, so the scene only needs this and some geometry.
public class TitleScreen : MonoBehaviour
{
    [Header("Where to go")]
    [Tooltip("Prefer whatever generated scenes are in Build Settings, resolved at runtime.")]
    [SerializeField] private bool preferGenerated = true;
    [Tooltip("Used when preferGenerated is off, or nothing generated is registered.")]
    [SerializeField] private string[] gameScenes = { "MovementTestScene" };
    [SerializeField] private bool pickRandom;

    [Header("Presentation")]
    [SerializeField] private string title = "LIMINAL LEAP";
    [SerializeField] private string subtitle = "keep running";
    // Controls come from Controls.Summary, not the scene.
    [SerializeField] private float driftSpeed = 1.6f;
    [SerializeField] private float startDelay = 0.6f;

    private TextMeshProUGUI prompt;
    private Transform driftCamera;
    private float elapsed;
    private bool starting;
    private bool stingPlayed;
    [SerializeField] private float audioWait = 0.6f; // the most a start waits for the browser's audio

    private void Start()
    {
        PageReady.Mark();
        PageReady.Title(SaveStore.Data.Daily(DailySeed.TodayKey()).Runs);
        MoodLighting.GetInstance();
        AudioManager.GetInstance();
        ScreenFade.GetInstance();

        BuildUi();

        driftCamera = Camera.main != null ? Camera.main.transform : null;
        ScreenFade.GetInstance().To(0f, 1.2f);
    }

    private void Update()
    {
        elapsed += Time.unscaledDeltaTime;

        // In a browser that's the first click on the title, which no longer starts the story.
        if (!stingPlayed && !starting && PageReady.AudioRunning)
        {
            stingPlayed = true;
            AudioManager.GetInstance().Play(Sound.TitleSting);
            PageReady.StingPlayed();
        }

        if (driftCamera != null)
        {
            driftCamera.position += driftCamera.forward * (driftSpeed * Time.deltaTime);
        }

        if (prompt != null)
        {
            // Breathing alpha rather than a blink, which reads as calmer.
            var pulse = 0.55f + 0.45f * Mathf.Sin(elapsed * 2.2f);
            var c = prompt.color;
            prompt.color = new Color(c.r, c.g, c.b, pulse);
        }

        if (starting || elapsed <= startDelay)
        {
            return;
        }

        // Before anyKeyDown. Not D, which is also steer right.
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            RunMode.ChooseDaily();
            StartGame();
        }
        // anyKeyDown counts mouse buttons, and a click is how a player focuses the game in a page.
        else if (Input.anyKeyDown && !Clicked())
        {
            RunMode.ChooseStory();
            StartGame();
        }
    }

    private static bool Clicked()
    {
        return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);
    }

    private void StartGame()
    {
        if (Resolve() == null)
        {
            Debug.LogWarning("TitleScreen: none of " + string.Join(", ", gameScenes) + " are in Build Settings");
            return;
        }

        starting = true;
        StartCoroutine(ConfirmThenLoad());
    }

    // The key that starts a run is often the browser's first gesture, so its sound waits for
    // the audio that gesture wakes, a fraction of a second at most.
    private System.Collections.IEnumerator ConfirmThenLoad()
    {
        var waited = 0f;
        while (!PageReady.AudioRunning && waited < audioWait)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        AudioManager.GetInstance().Play(Sound.Confirm);
        ScreenFade.GetInstance().To(1f, 0.45f);
        Invoke(nameof(LoadGame), 0.5f);
    }

    private void LoadGame()
    {
        var scene = Resolve();
        if (scene != null)
        {
            SceneManager.LoadScene(scene);
        }
    }

    // From Build Settings at runtime, as a baked list goes stale.
    private string Resolve()
    {
        if (preferGenerated)
        {
            var generated = GeneratedInBuild();
            if (generated.Count > 0)
            {
                return Pick(generated);
            }
        }

        var loadable = new System.Collections.Generic.List<string>();
        if (gameScenes != null)
        {
            foreach (var name in gameScenes)
            {
                if (!string.IsNullOrWhiteSpace(name) && Application.CanStreamedLevelBeLoaded(name))
                {
                    loadable.Add(name);
                }
            }
        }

        return loadable.Count > 0 ? Pick(loadable) : null;
    }

    private string Pick(System.Collections.Generic.List<string> names)
    {
        return pickRandom ? names[Random.Range(0, names.Count)] : names[0];
    }

    private static System.Collections.Generic.List<string> GeneratedInBuild()
    {
        var found = new System.Collections.Generic.List<string>();

        for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            if (path.Contains("/Generated/"))
            {
                found.Add(System.IO.Path.GetFileNameWithoutExtension(path));
            }
        }

        return found;
    }

    // Today's run, its place and your best on it.
    private static string DailyLine()
    {
        var day = DailySeed.TodayKey();
        var record = SaveStore.Data.Daily(day);
        var place = Places.ForDaily(DailySeed.Today()).Name;

        return record.Runs > 0
            ? $"{Controls.Daily}   today's run  {place}      your best  {record.BestScore:N0}"
            : $"{Controls.Daily}   today's run  {place}";
    }

    // Where the story picks up.
    private static string StoryLine()
    {
        var place = Places.Story[Mathf.Clamp(SaveStore.Data.StoryPlace, 0, Places.Story.Length - 1)];
        return SaveStore.Data.StoryPlace > 0 ? $"continue from  {place.Name}" : place.Name;
    }

    private void BuildUi()
    {
        var canvas = RuntimeUi.CreateCanvas("TitleCanvas", 120);
        var centre = new Vector2(0.5f, 0.5f);

        var column = new RuntimeUi.Column(canvas.transform, centre, TextAlignmentOptions.Center, 0f, 180f, 1400f, 14f);

        column.Add("Title", RuntimeUi.Display, RuntimeUi.Ink, 0.3f, 22f).text = title;
        column.Add("Subtitle", RuntimeUi.Body, RuntimeUi.Muted, 0.15f, 14f).text = subtitle;
        column.Space(90f);

        column.Add("Controls", RuntimeUi.Caption, RuntimeUi.Muted, 0.15f, 8f).text = Controls.Summary;
        column.Add("Flashing", RuntimeUi.Caption, RuntimeUi.Muted, 0.15f, 8f).text = Controls.Flashing;
        column.Space(40f);

        prompt = column.Add("Prompt", RuntimeUi.Caption, RuntimeUi.Accent, 0.15f, 10f);
        prompt.text = "press any key";
        column.Add("Story", RuntimeUi.Caption, RuntimeUi.Muted, 0.15f, 8f).text = StoryLine();

        column.Space(26f);
        column.Add("Daily", RuntimeUi.Caption, RuntimeUi.Muted, 0.15f, 8f).text = DailyLine();

        ScreenFade.GetInstance().To(1f, 0f);
    }
}
