using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Builds its own UI so the scene only needs this component and some geometry to look at.
public class TitleScreen : MonoBehaviour
{
    [SerializeField] private string gameScene = "MovementTestScene";
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
        starting = true;
        AudioManager.GetInstance().Play(Sound.Confirm);
        ScreenFade.GetInstance().To(1f, 0.45f);
        Invoke(nameof(LoadGame), 0.5f);
    }

    private void LoadGame()
    {
        if (Application.CanStreamedLevelBeLoaded(gameScene))
        {
            SceneManager.LoadScene(gameScene);
            return;
        }

        Debug.LogWarning($"TitleScreen: '{gameScene}' is not in Build Settings, staying put");
        starting = false;
        ScreenFade.GetInstance().To(0f, 0.4f);
    }

    private void BuildUi()
    {
        var canvas = RuntimeUi.CreateCanvas("TitleCanvas", 120);
        var centre = new Vector2(0.5f, 0.5f);

        var heading = RuntimeUi.Style(
            RuntimeUi.CreateText(canvas.transform, "Title", centre, new Vector2(0f, 120f), new Vector2(1400f, 180f), RuntimeUi.Display, TextAlignmentOptions.Center),
            RuntimeUi.Ink, 0.3f, 22f);
        heading.text = title;

        var sub = RuntimeUi.Style(
            RuntimeUi.CreateText(canvas.transform, "Subtitle", centre, new Vector2(0f, 20f), new Vector2(1200f, 80f), RuntimeUi.Body, TextAlignmentOptions.Center),
            RuntimeUi.Muted, 0.15f, 14f);
        sub.text = subtitle;

        prompt = RuntimeUi.Style(
            RuntimeUi.CreateText(canvas.transform, "Prompt", centre, new Vector2(0f, -180f), new Vector2(1200f, 80f), RuntimeUi.Caption, TextAlignmentOptions.Center),
            RuntimeUi.Accent, 0.15f, 10f);
        prompt.text = "press any key";

        ScreenFade.GetInstance().To(1f, 0f);
    }
}
