using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }

    private AudioSource sfxSource;
    private AudioSource musicSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            sfxSource = GetComponent<AudioSource>();
            musicSource = transform.GetChild(0).GetComponent<AudioSource>();

            ChangeMusicVolume(0);
            ChangeSoundVolume(0);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void ChangeSoundVolume(float step)
    {
        AdjustVolume(1f, "soundVolume", step, sfxSource);
    }

    public void ChangeMusicVolume(float step)
    {
        AdjustVolume(0.3f, "musicVolume", step, musicSource);
    }

     private void AdjustVolume(float baseVolume, string prefKey, float step, AudioSource source)
    {
        float current = PlayerPrefs.GetFloat(prefKey, 1f);
        current += step;

        if (current > 1f)
            current = 0f;
        else if (current < 0f)
            current = 1f;

        source.volume = current * baseVolume;
        PlayerPrefs.SetFloat(prefKey, current);
    }
    
}
