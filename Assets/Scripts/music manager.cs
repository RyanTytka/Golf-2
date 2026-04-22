using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class musicmanager : MonoBehaviour
{
    public AudioSource musicAudioSource;
    public AudioSource sfxAudioSource;
    public AudioMixer audioMixer;
    public AudioClip[] playlist;   // List of music tracks
    public int musicNumber;
    public AudioClip[] UIPlaylist; //list of audio clips for UI
    public Slider musicSlider, sfxSlider;


    public static musicmanager Instance;

    void Awake()
    {
        // If another instance already exists, destroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Otherwise, set this as the instance
        Instance = this;

        // Make it persist between scenes
        DontDestroyOnLoad(gameObject);
    }

    public enum UISounds
    {
        ButtonClick
    }

    void Start()
    {
        LoadVolumes();
        musicAudioSource.clip = playlist[musicNumber];
        musicAudioSource.Play();
    }

    public void NextSong()
    {
        musicAudioSource.Stop();
        musicNumber = (musicNumber + 1) % playlist.Length;
        musicAudioSource.clip = playlist[musicNumber];
        musicAudioSource.Play();
    }

    public void PlaySounEffect(UISounds sound)
    {
        sfxAudioSource.PlayOneShot(UIPlaylist[(int)sound]);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 50);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 50);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void LoadVolumes()
    {
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        SetMusicVolume(music);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        SetSFXVolume(sfx);
        musicSlider.value = music;
        sfxSlider.value = sfx;

    }
}
