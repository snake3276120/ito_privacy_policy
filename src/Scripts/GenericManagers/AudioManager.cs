using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles all game audios that will be played.
/// It governs the frequency of how many time a same type of audio can be played per second.
/// It not only improves game performance but also makes audios distinguishable for human ears.
///
/// !!! TODO: move all audio component to this class!!!
/// </summary>
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip GunFireAudioClip = null;
    [SerializeField] private AudioClip MortarFireAudioClip = null;
    [SerializeField] private AudioClip QTETileEffectStart = null;
    [SerializeField] private AudioClip QTETileEffectExpire = null;
    [SerializeField] private AudioClip QTESequenceRewarded = null;
    [SerializeField] private AudioClip[] SoldierDieAudioClips = null;
    [SerializeField] private AudioClip[] SingleKeyStrokeClips = null;
    [SerializeField] private AudioSource MasterAudioSource = null;
    [SerializeField] private AudioSource ContdKeyStrokeAudioSource = null;
    [SerializeField] private Toggle SFXToggle = null;

    public static AudioManager Instance;

    private float m_TowerFireCountdown, m_TowerFireThreshold;
    private float m_SoldierDieCountdown, m_SoldierDieThreshold;
    private float m_MotarFireCountdown;

    private bool m_PlaySfx = true;

    void Awake()
    {
        if (null != Instance)
        {
            Debug.LogError("More than one AudioManager instances!");
            return;
        }
        Instance = this;
    }

    void Start()
    {
        m_TowerFireThreshold = 1f / Constants.AUDIO_FREQ_TOWER_FIRE;
        m_SoldierDieThreshold = 1f / Constants.AUDIO_FREQ_SOLDIER_DIE;
        SFXToggle.onValueChanged.AddListener(delegate
        {
            m_PlaySfx = SFXToggle.isOn;
        });
    }

    private void Update()
    {
        if (m_TowerFireCountdown > 0f)
            m_TowerFireCountdown -= Time.deltaTime;

        if (m_SoldierDieCountdown > 0f)
            m_SoldierDieCountdown -= Time.deltaTime;

        if (m_MotarFireCountdown > 0f)
            m_MotarFireCountdown -= Time.deltaTime;

    }

    public void PlayProjectileTurretFireSound()
    {
        if (m_PlaySfx && m_TowerFireCountdown <= 0f)
        {
            MasterAudioSource.PlayOneShot(GunFireAudioClip, 0.2f);
            m_TowerFireCountdown += m_TowerFireThreshold;
        }
    }

    public void PlayMortarTurretFireSound()
    {
        if (m_PlaySfx && m_MotarFireCountdown <= 0f)
        {
            MasterAudioSource.PlayOneShot(MortarFireAudioClip, .75f);
            m_MotarFireCountdown += m_TowerFireThreshold;
        }
    }

    public void PlayTileQTEActivateSound()
    {
        if (m_PlaySfx)
            MasterAudioSource.PlayOneShot(QTETileEffectStart, 1f);
    }

    public void PlayTileQTEExpireSound()
    {
        if (m_PlaySfx)
            MasterAudioSource.PlayOneShot(QTETileEffectExpire, 1f);
    }

    public void PlaySoldierDieSound()
    {
        if (m_PlaySfx && m_SoldierDieCountdown <= 0f)
        {
            MasterAudioSource.PlayOneShot(SoldierDieAudioClips[Random.Range(0, SoldierDieAudioClips.Length)], 1f);
            m_SoldierDieCountdown += m_SoldierDieThreshold;
        }
    }

    public void PlayQTESequenceRewardSound()
    {
        if (m_PlaySfx)
            MasterAudioSource.PlayOneShot(QTESequenceRewarded);
    }

    public void PlaySingleKeyStroke()
    {
        if (m_PlaySfx)
            MasterAudioSource.PlayOneShot(SingleKeyStrokeClips[Random.Range(0, SingleKeyStrokeClips.Length)], 1f);
    }

    public void PlayContdKeyStroke()
    {
        if (m_PlaySfx)
            ContdKeyStrokeAudioSource.Play();
    }

    public void StopContdKeyStroke()
    {
        ContdKeyStrokeAudioSource.Stop();
    }
}
