using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class musicmanager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] playlist;   // List of music tracks
    public int musicNumber;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        audioSource.clip = playlist[musicNumber];
        audioSource.Play();
    }
}
