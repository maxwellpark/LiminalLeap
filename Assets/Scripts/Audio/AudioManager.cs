using Events;
using UnityEngine;
using EventType = Events.EventType;

// Self-provisioning: GetInstance spawns one if the scene has none, so greybox audio
// works without touching the scene. Leave authored empty to stay fully procedural.
public class AudioManager : Singleton<AudioManager>
{
    protected override EventType[] EventTypes => new[] { EventType.Death };

    [SerializeField] private AuthoredAudioLibrary authored;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float windVolume = 0.35f;
    [SerializeField] private float windPitchAtSpeed = 0.6f; // extra pitch at full speed

    private IAudioLibrary library;
    private AudioSource sfx;
    private AudioSource wind;

    public override void Init()
    {
        library = authored != null ? authored : new ProceduralAudioLibrary();

        EnsureListener();

        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;

        wind = gameObject.AddComponent<AudioSource>();
        wind.clip = library.Get(Sound.Wind);
        wind.loop = true;
        wind.volume = 0f;
        wind.Play();
    }

    public void Play(Sound sound)
    {
        if (sfx != null)
        {
            sfx.PlayOneShot(library.Get(sound), sfxVolume);
        }
    }

    private void Update()
    {
        if (wind == null)
        {
            return;
        }

        var t = Mathf.Clamp01(PlayerTrackMovement.CurrentSpeed / 22f);
        wind.volume = Mathf.Lerp(wind.volume, windVolume * t, 3f * Time.deltaTime);
        wind.pitch = 1f + windPitchAtSpeed * t;
    }

    protected override void OnDeath(OnDeathEvent evt)
    {
        Play(Sound.Death);
    }

    // Nothing is audible without one, and the scene currently has none.
    private static void EnsureListener()
    {
        if (FindFirstObjectByType<AudioListener>() != null)
        {
            return;
        }

        var cam = Camera.main;
        if (cam != null)
        {
            cam.gameObject.AddComponent<AudioListener>();
        }
    }
}
