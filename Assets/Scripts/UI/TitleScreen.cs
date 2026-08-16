using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Builds its own UI so the scene only needs this component and some geometry to look at.
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
    [SerializeField] private string controls = "A D  steer      SPACE  jump      SHIFT  look back";
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

    // Read from Build Settings at runtime. A baked list of names goes stale the moment
    // scenes are regenerated, which is exactly what happened.
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

    private void BuildUi()
    {
        var canvas = RuntimeUi.CreateCanvas("TitleCanvas", 120);
        var centre = new Vector2(0.5f, 0.5f);

        var column = new RuntimeUi.Column(canvas.transform, centre, TextAlignmentOptions.Center, 0f, 180f, 1400f, 14f);

        column.Add("Title", RuntimeUi.Display, RuntimeUi.Ink, 0.3f, 22f).text = title;
        column.Add("Subtitle", RuntimeUi.Body, RuntimeUi.Muted, 0.15f, 14f).text = subtitle;
        column.Space(90f);

        column.Add("Controls", RuntimeUi.Caption, RuntimeUi.Muted, 0.15f, 8f).text = controls;
        column.Space(40f);

        prompt = column.Add("Prompt", RuntimeUi.Caption, RuntimeUi.Accent, 0.15f, 10f);
        prompt.text = "press any key";

        ScreenFade.GetInstance().To(1f, 0f);
    }
}
