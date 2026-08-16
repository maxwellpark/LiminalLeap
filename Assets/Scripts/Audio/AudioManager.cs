using Events;
using UnityEngine;
using EventType = Events.EventType;

// Spawns itself if the scene has none. Leave authored empty to stay procedural.
public class AudioManager : Singleton<AudioManager>
{
    protected override EventType[] EventTypes => new[] { EventType.Death };

    [SerializeField] private AuthoredAudioLibrary authored;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float windVolume = 0.14f;
    [SerializeField, Range(0f, 1f)] private float windFloor = 0.25f; // audible when stood still
    [SerializeField] private float windPitchAtSpeed = 0.35f;

    [Header("Dread")]
    [SerializeField, Range(0f, 1f)] private float dreadVolume = 0.5f;
    [SerializeField] private float dreadPitchAtContact = 0.45f;

    [Header("Pickup combo")]
    [SerializeField] private float comboWindow = 1.6f;  // gap that resets the run of pickups
    [SerializeField] private int comboSteps = 8;        // pitch stops climbing after this
    [SerializeField] private float semitonesPerStep = 1f;

    private IAudioLibrary library;
    private AudioSource sfx;
    private AudioSource pickup;   // its own source: PlayOneShot uses the source pitch
    private AudioSource wind;
    private AudioSource dread;
    private Pursuer pursuer;
    private int combo;
    private float lastPickupAt = -999f;

    public override void Init()
    {
        library = authored != null ? authored : new ProceduralAudioLibrary();

        EnsureListener();

        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;

        pickup = gameObject.AddComponent<AudioSource>();
        pickup.playOnAwake = false;

        dread = gameObject.AddComponent<AudioSource>();
        dread.clip = library.Get(Sound.Dread);
        dread.loop = true;
        dread.volume = 0f;
        dread.Play();

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

    // Returns how many pickups deep the run is, so the toast can say so too.
    public int PlayPickup()
    {
        combo = Time.time - lastPickupAt > comboWindow ? 1 : Mathf.Min(combo + 1, comboSteps);
        lastPickupAt = Time.time;

        if (pickup != null)
        {
            pickup.pitch = Mathf.Pow(2f, (combo - 1) * semitonesPerStep / 12f);
            pickup.PlayOneShot(library.Get(Sound.Pickup), sfxVolume);
        }

        return combo;
    }

    protected override void OnDeath(OnDeathEvent evt)
    {
        combo = 0;
        lastPickupAt = -999f;
        Play(evt.Completed ? Sound.Success : Sound.Death);
    }

    private void Update()
    {
        if (wind == null)
        {
            return;
        }

        var t = PlayerTrackMovement.SpeedFraction;
        var target = windVolume * Mathf.Lerp(windFloor, 1f, t);
        wind.volume = Mathf.Lerp(wind.volume, target, 2f * Time.deltaTime);
        wind.pitch = 1f + windPitchAtSpeed * t;

        DriveDread();
    }

    // Hearing it approach is what makes raising the mirror a decision rather than a guess.
    private void DriveDread()
    {
        if (dread == null)
        {
            return;
        }

        pursuer = pursuer != null ? pursuer : Pursuer.Instance;
        var near = pursuer != null ? pursuer.Proximity : 0f;

        // Squared, so it stays almost silent at distance and swells late.
        dread.volume = Mathf.Lerp(dread.volume, dreadVolume * near * near, 2.5f * Time.deltaTime);
        dread.pitch = 1f + dreadPitchAtContact * near;
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
