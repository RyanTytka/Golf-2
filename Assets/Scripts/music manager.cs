using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class musicmanager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] playlist;   // List of music tracks
    public int musicNumber;
    public AudioClip[] UIPlaylist; //list of audio clips for UI


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
        audioSource.clip = playlist[musicNumber];
        audioSource.Play();
    }

    public void NextSong()
    {
        audioSource.Stop();
        musicNumber = (musicNumber + 1) % playlist.Length;
        audioSource.clip = playlist[musicNumber];
        audioSource.Play();
    }

    public void PlaySounEffect(UISounds sound)
    {
        audioSource.PlayOneShot(UIPlaylist[(int)sound]);
    }
}
