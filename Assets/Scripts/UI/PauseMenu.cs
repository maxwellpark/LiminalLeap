using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// There was no way to stop, and no way back to the title, so switching between a daily and
// a free run meant reloading the page. Builds its own UI like the other managers.
public class PauseMenu : Singleton<PauseMenu>
{
    [SerializeField] private string titleScene = "TitleScreen";

    private CanvasGroup group;
    private TextMeshProUGUI keys;
    private TextMeshProUGUI options;

    // Static so PlayerTrackMovement.Running can read it without holding a reference.
    public static bool Paused { get; private set; }

    public override void Init()
    {
        Build();
        Show(false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Leaving the scene paused would leave the next one frozen.
        if (Paused)
        {
            Resume();
        }
    }

    private void Update()
    {
        // Esc as well as P, but P is the one to teach: a fullscreen browser eats Esc.
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (Paused)
            {
                Resume();
            }
            else if (PlayerTrackMovement.Running)
            {
                Pause();
            }

            return;
        }

        if (!Paused)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            ToTitle();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            PlayerOptions.ReducedFlashing = !PlayerOptions.ReducedFlashing;
            Refresh();
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            PlayerOptions.Muted = !PlayerOptions.Muted;
            Refresh();
        }
    }

#if !UNITY_EDITOR
    // Coming back to a run that kept going while you were in another tab is a death you
    // never saw. Not in the editor, where clicking away mid test would pause the test.
    private void OnApplicationFocus(bool focused)
    {
        if (!focused && PlayerTrackMovement.Running)
        {
            Pause();
        }
    }
#endif

    public void Pause()
    {
        Paused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Refresh();
        Show(true);
    }

    public void Resume()
    {
        Paused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Show(false);
    }

    private void ToTitle()
    {
        Resume();

        if (Application.CanStreamedLevelBeLoaded(titleScene))
        {
            SceneManager.LoadScene(titleScene);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    private void Refresh()
    {
        if (options == null)
        {
            return;
        }

        keys.text = $"{Controls.Pause}  resume     T  title";
        options.text = $"F  reduced flashing  {(PlayerOptions.ReducedFlashing ? "on" : "off")}     "
            + $"M  sound  {(PlayerOptions.Muted ? "off" : "on")}";
    }

    private void Show(bool visible)
    {
        if (group != null)
        {
            group.alpha = visible ? 1f : 0f;
        }
    }

    private void Build()
    {
        // Under the run summary at 220, which can't be open at the same time anyway.
        var canvas = RuntimeUi.CreateCanvas("PauseCanvas", 210);

        var holder = new GameObject("PauseMenu");
        holder.transform.SetParent(canvas.transform, false);
        group = holder.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;

        var rect = holder.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        RuntimeUi.CreateFullScreenImage(holder.transform, "Dim", new Color(0f, 0f, 0f, 0.6f));

        var column = new RuntimeUi.Column(
            holder.transform, new Vector2(0.5f, 0.5f), TextAlignmentOptions.Center, 0f, 90f, 1400f, 14f);

        column.Add("Headline", RuntimeUi.Headline, RuntimeUi.Ink, 0.22f, 12f).text = "PAUSED";
        column.Space(30f);

        keys = column.Add("Keys", RuntimeUi.Caption, RuntimeUi.Accent, 0.15f, 8f);
        options = column.Add("Options", RuntimeUi.Caption, RuntimeUi.Muted, 0.15f, 8f);
    }
}
