using UnityEngine;
public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance { get; private set; }

    [Header("-----------------AUDIO SOURCE-----------------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;
    [Header("-----------------AUDIO CLIP-----------------")]
    public AudioClip background;
    public AudioClip attack;
    public AudioClip attack2;
    public AudioClip attack3;
    public AudioClip hurt;
    public AudioClip hurt2;
    public AudioClip heal;
    public AudioClip dash;
    public AudioClip projectile;
    public AudioClip jump;
    public AudioClip land;

       void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        AudioListener listener = GetComponent<AudioListener>();
        if (listener != null)
        {
            Destroy(listener);
        }
    } 

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }
    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);

    }
}