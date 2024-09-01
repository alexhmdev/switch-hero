using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource musicAltSource;
    [SerializeField] AudioSource SFXSource;

    [Header("Audio Clips")]
    [SerializeField] public AudioClip YellowTheme;
    [SerializeField] public AudioClip RedTheme;
    [SerializeField] public AudioClip jump;
    [SerializeField] public AudioClip footsteps;
    [SerializeField] public AudioClip changeHood;
    [SerializeField] public AudioClip enemyAttack;
    [SerializeField] public AudioClip enemyDeath;
    [SerializeField] public AudioClip enemyHit;
    [SerializeField] public AudioClip playerHit;
    [SerializeField] public AudioClip playerDeath;
    [SerializeField] public AudioClip playerAttack;


    private void Start()
    {
       // Start playing the background default theme Yellow
       // and Red at the same time with volume 0
        musicSource.clip = YellowTheme;
        musicSource.Play();
        musicAltSource.volume = 0;
        musicAltSource.clip = RedTheme;
        musicAltSource.Play();


    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    public void PlaySFXRandomPitch(AudioClip clip)
    {
        SFXSource.pitch = Random.Range(0.6f, 1.3f);
        SFXSource.PlayOneShot(clip);
        SFXSource.pitch = 1;
    }

    public void StopBackground()
    {
        musicSource.Stop();
    }

    public void ToggleMainTheme()
    {

       if (musicSource.volume == 0)
        {
            musicSource.volume = 1;
            musicAltSource.volume = 0;
        }
        else
        {
            musicSource.volume = 0;
            musicAltSource.volume = 1;
        }
    }
}
