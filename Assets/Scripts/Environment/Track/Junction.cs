using UnityEngine;

public class Junction : DistanceActivatable
{
    [SerializeField] private Track[] tracks;
    private GUIStyle style;

    private void Update()
    {
        if (!InRange)
        {
            return;
        }

        // Steer into the branch (Left/Right, Up for a middle path) instead of a number-key
        // menu, so the player keeps their eyes on the run.
        int choice = -1;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            choice = 0;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            choice = tracks.Length - 1;
        }
        else if (tracks.Length >= 3 && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)))
        {
            choice = 1;
        }

        if (choice >= 0 && choice < tracks.Length)
        {
            TrackManager.GetInstance().SwitchTrack(tracks[choice]);
            Destroy(gameObject);
        }
    }

    private void OnGUI()
    {
        if (!InRange)
        {
            return;
        }

        style ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 32,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperLeft
        };

        var text = tracks.Length >= 3
            ? "Steer:  A/left    W/straight    D/right"
            : "Steer:  A/left    D/right";
        GUI.Label(new Rect(40, 200, 700, 100), text, style);
    }
}
