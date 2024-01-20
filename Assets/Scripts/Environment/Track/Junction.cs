using UnityEngine;

public class Junction : DistanceActivatable
{
    [SerializeField] private Track[] tracks;
    private readonly KeyCode[] keyCodes = new[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };
    private GUIStyle style;

    private void Update()
    {
        if (!InRange)
        {
            return;
        }

        for (int i = 0; i < tracks.Length; i++)
        {
            if (Input.GetKeyDown(keyCodes[i]))
            {
                var tm = TrackManager.GetInstance();
                tm.SwitchTrack(tracks[i]);
                Destroy(gameObject);
            }
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

        var text = "Junction options: ";

        for (int i = 0; i < tracks.Length; i++)
        {
            text += keyCodes[i].ToString();

            if (i < tracks.Length - 1)
            {
                text += ", ";
            }
        }
        GUI.Label(new Rect(40, 200, 300, 100), text, style);
    }
}
