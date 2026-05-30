using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// W14 (manual §44): Singleton audio system
//
// Setup expected in Unity (manual 44.2 – 44.14):
//   Assets/Audio/BGM  — 6 AudioClips
//   Assets/Audio/SFX  — 11 AudioClips
//   Assets/Audio/AudioMixer  with Groups: Master ➜ BGM, SFX
//     Expose Master/BGM/SFX volume parameters named "MasterVolume", "BGMVolume", "SFXVolume"
//   Scene object Audio
//     ├── BGM   — N child AudioSource objects (one per BGM clip, Output = BGM mixer group)
//     └── SFX   — M child AudioSource objects (one per SFX clip, Output = SFX mixer group)
//   AudioManager component goes on the Audio root object, with the AudioSource arrays + mixer wired up.
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource[] bgmSources;   // index = BGM id
    [SerializeField] private AudioSource[] sfxSources;   // index = SFX id

    [Header("Settings UI (optional — re-wire per scene if needed)")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    // Track volumes separately so we can mute/restore without losing the value
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    private float masterVolume = 1f;

    // PlayerPrefs keys for persistence
    private const string PREF_MASTER = "audio.master";
    private const string PREF_BGM    = "audio.bgm";
    private const string PREF_SFX    = "audio.sfx";

    public static AudioManager instance;

    void Awake()
    {
        // Singleton — persists across scene loads so music keeps playing through warps
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Restore saved volumes
        masterVolume = PlayerPrefs.GetFloat(PREF_MASTER, 1f);
        bgmVolume    = PlayerPrefs.GetFloat(PREF_BGM,    1f);
        sfxVolume    = PlayerPrefs.GetFloat(PREF_SFX,    1f);
    }

    void Start()
    {
        // Apply saved volumes to the mixer (mixer params need a frame after Awake to accept SetFloat)
        ApplyVolume("MasterVolume", masterVolume);
        ApplyVolume("BGMVolume",    bgmVolume);
        ApplyVolume("SFXVolume",    sfxVolume);
    }

    // ── BGM ───────────────────────────────────────────────────────────────────

    // Stop every BGM source and play the one matching id (-1 stops all)
    public void PlayBGM(int id)
    {
        if (bgmSources == null) return;
        for (int i = 0; i < bgmSources.Length; i++)
        {
            if (bgmSources[i] == null) continue;
            if (i == id)
            {
                if (!bgmSources[i].isPlaying) bgmSources[i].Play();
            }
            else
            {
                bgmSources[i].Stop();
            }
        }
    }

    public void StopBGM()
    {
        PlayBGM(-1);
    }

    // ── SFX ───────────────────────────────────────────────────────────────────

    public void PlaySFX(int id)
    {
        if (sfxSources == null || id < 0 || id >= sfxSources.Length) return;
        AudioSource src = sfxSources[id];
        if (src == null) return;
        // PlayOneShot lets multiple SFX overlap on the same source
        if (src.clip != null) src.PlayOneShot(src.clip);
        else src.Play();
    }

    // ── Volume (settings menu) ────────────────────────────────────────────────

    // vol is linear 0–1; the mixer expects logarithmic dB
    public void SetMasterVolume(float vol)
    {
        masterVolume = Mathf.Clamp01(vol);
        PlayerPrefs.SetFloat(PREF_MASTER, masterVolume);
        ApplyVolume("MasterVolume", masterVolume);
    }
    public void SetBGMVolume(float vol)
    {
        bgmVolume = Mathf.Clamp01(vol);
        PlayerPrefs.SetFloat(PREF_BGM, bgmVolume);
        ApplyVolume("BGMVolume", bgmVolume);
    }
    public void SetSFXVolume(float vol)
    {
        sfxVolume = Mathf.Clamp01(vol);
        PlayerPrefs.SetFloat(PREF_SFX, sfxVolume);
        ApplyVolume("SFXVolume", sfxVolume);
    }

    public float MasterVolume { get { return masterVolume; } }
    public float BGMVolume    { get { return bgmVolume; } }
    public float SFXVolume    { get { return sfxVolume; } }

    private void ApplyVolume(string paramName, float vol)
    {
        if (audioMixer == null) return;
        // Linear 0–1 → -80 dB (silent) to 0 dB (full)
        float db = vol <= 0.0001f ? -80f : Mathf.Log10(vol) * 20f;
        audioMixer.SetFloat(paramName, db);
    }

    // ── Settings Panel UI ─────────────────────────────────────────────────────

    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null) return;
        bool show = !settingsPanel.activeInHierarchy;
        settingsPanel.SetActive(show);
        if (show) SyncVolumeSliders();
    }

    public void OpenSettingsPanel()
    {
        if (settingsPanel == null) return;
        settingsPanel.SetActive(true);
        SyncVolumeSliders();
    }

    public void CloseSettingsPanel()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // Set sliders to the current persistent volumes (no notify → no callback loop)
    private void SyncVolumeSliders()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.SetValueWithoutNotify(masterVolume);
        if (bgmVolumeSlider != null)    bgmVolumeSlider.SetValueWithoutNotify(bgmVolume);
        if (sfxVolumeSlider != null)    sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
    }
}
