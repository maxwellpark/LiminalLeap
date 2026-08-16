using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Builds its own UI so the scene only needs this component and some geometry to look at.
public class TitleScreen : MonoBehaviour
{
    [Header("Where to go")]
    [Tooltip("Tried in order. First one that is in Build Settings wins.")]
    [SerializeField] private string[] gameScenes = { "MovementTestScene" };
    [SerializeField] private bool pickRandom;

    [Header("Presentation")]
    [SerializeField] private string title = "LIMINAL LEAP";
    [SerializeField] private string subtitle = "keep running";
    [SerializeField] private float driftSpeed = 1.6f;
    [SerializeField] private float startDelay = 0.6f;

    private TextMeshProUGUI prompt;
    private Transform driftCamera;
    private float elapsed;
    private bool starting;

    private void Start()
    {
        MoodLighting.GetInstance();
        AudioManager.GetInstance();
        ScreenFade.GetInstance();

        BuildUi();

        driftCamera = Camera.main != null ? Camera.main.transform : null;
        AudioManager.GetInstance().Play(Sound.TitleSting);
        ScreenFade.GetInstance().To(0f, 1.2f);
    }

    private void Update()
    {
        elapsed += Time.unscaledDeltaTime;

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

        if (!starting && elapsed > startDelay && Input.anyKeyDown)
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        if (Resolve() == null)
        {
            Debug.LogWarning("TitleScreen: none of " + string.Join(", ", gameScenes) + " are in Build Settings");
            return;
        }

        starting = true;
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

    // Generated scenes are gitignored, so the list has to tolerate missing entries.
    private string Resolve()
    {
        if (gameScenes == null || gameScenes.Length == 0)
        {
            return null;
        }

        var loadable = new System.Collections.Generic.List<string>();
        foreach (var name in gameScenes)
        {
            if (!string.IsNullOrWhiteSpace(name) && Application.CanStreamedLevelBeLoaded(name))
            {
                loadable.Add(name);
            }
        }

        if (loadable.Count == 0)
        {
            return null;
        }

        return pickRandom ? loadable[Random.Range(0, loadable.Count)] : loadable[0];
    }

    private void BuildUi()
    {
        var canvas = RuntimeUi.CreateCanvas("TitleCanvas", 120);
        var centre = new Vector2(0.5f, 0.5f);

        var column = new RuntimeUi.Column(canvas.transform, centre, TextAlignmentOptions.Center, 0f, 180f, 1400f, 14f);

        column.Add("Title", RuntimeUi.Display, RuntimeUi.Ink, 0.3f, 22f).text = title;
        column.Add("Subtitle", RuntimeUi.Body, RuntimeUi.Muted, 0.15f, 14f).text = subtitle;
        column.Space(120f);

        prompt = column.Add("Prompt", RuntimeUi.Caption, RuntimeUi.Accent, 0.15f, 10f);
        prompt.text = "press any key";

        ScreenFade.GetInstance().To(1f, 0f);
    }
}
