using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<SoundManager>();
            }
            return instance;
        }
    }

    private static readonly string SOUND_MUTED_PREF = "sound_muted_pref";

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioButton;
    [SerializeField] private AudioSource audioPing;
    [SerializeField] private AudioSource audioNotification;
    [SerializeField] private AudioSource audioSuccess;
    [SerializeField] private AudioSource audioError;
    [SerializeField] private AudioSource audioBgm;

    private void Start()
    {
        RefreshSoundSettings();
    }

    public void PlayButtonSound()
    {
        if (!Instance) return;
        Instance.audioButton.Play();
    }

    public void PlayPingSound()
    {
        if (!Instance) return;
        Instance.audioPing.Play();
    }

    public void PlayNotificationSound()
    {
        if (!Instance) return;
        Instance.audioNotification.Play();
    }

    public void PlaySuccessSound()
    {
        if (!Instance) return;
        Instance.audioSuccess.Play();
    }

    public void PlayErrorSound()
    {
        if (!Instance) return;
        Instance.audioError.Play();
    }

    public void RefreshSoundSettings()
    {
        audioButton.mute = IsMuted();
        audioPing.mute = IsMuted();
        audioNotification.mute = IsMuted();
        audioSuccess.mute = IsMuted();
        audioError.mute = IsMuted();
        audioBgm.mute = IsMuted();
    }

    public void ToggleMute()
    {
        PlayerPrefs.SetInt(SOUND_MUTED_PREF, IsMuted() ? 0 : 1);
        RefreshSoundSettings();
    }

    public bool IsMuted()
    {
        return PlayerPrefs.GetInt(SOUND_MUTED_PREF, 0) == 1;
    }
}
