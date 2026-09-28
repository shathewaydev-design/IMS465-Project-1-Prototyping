using UnityEngine;

public class SoundManager : MonoBehaviour
{

    [SerializeField] private AudioClip gunShot;

    [SerializeField] private AudioClip normalBKG;
    [SerializeField] private AudioClip slowedBKG;

    [SerializeField] private AudioSource selfAudioSource;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //selfAudioSource.clip = normalBKG;
        //selfAudioSource.Play();
        PlayBKGMusic();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        Enemy.OnEnemyShoot += PlayShot;
        Player.OnPlayerShoot += PlayShot;
        Player.OnSlowTrigger += PlayBKGMusic;
        GameManager.OnTimeRestored += PlayBKGMusic;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyShoot -= PlayShot;
        Player.OnPlayerShoot -= PlayShot;
        Player.OnSlowTrigger -= PlayBKGMusic;
        GameManager.OnTimeRestored -= PlayBKGMusic;
    }

    private void PlayShot(AudioSource audioSource)
    {
        audioSource.PlayOneShot(gunShot);
    }

    private void PlayBKGMusic()
    {
        float currentTime = selfAudioSource.time;


        if (GameManager.Instance.GetTimeSlowed())
        {
            //play slowed music
            selfAudioSource.Stop();
            selfAudioSource.pitch = 0.5f;

            selfAudioSource.clip = slowedBKG;
            selfAudioSource.time = currentTime;
            selfAudioSource.Play();

            //selfAudioSource.pitch = 0.5f;
        }
        else
        {
            //play normal bkg music
            selfAudioSource.Stop();
            selfAudioSource.pitch = 1f;

            selfAudioSource.clip = normalBKG;
            selfAudioSource.time = currentTime;
            selfAudioSource.Play();

            //selfAudioSource.pitch = 1f;


        }

    }

}
