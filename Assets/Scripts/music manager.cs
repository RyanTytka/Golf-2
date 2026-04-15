using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class musicmanager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] playlist;   // List of music tracks
    public int musicNumber;
    public AudioClip[] UIPlaylist; //list of audio clips for UI

    public enum UISounds
    {
        ButtonClick
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
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
