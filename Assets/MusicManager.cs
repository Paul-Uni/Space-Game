using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    private bool MamaL;
    public AudioSource audioSource;
    public AudioClip musicClip;

    void Awake()
    {
        // Here we use a Singleton pattern to make sure we only ever have one MusicManager.
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // Make this object persist across scenes.
        }
        else
        {
            Destroy(gameObject); // Destroy this, there is already a MusicManager.
            return;
        }
    }

    void Start()
    {
        audioSource.clip = musicClip;
        audioSource.loop = true; // If you want the music to loop
        audioSource.Play();
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu" && audioSource.volume!=0.2f && !MamaL)
        {
            StartCoroutine(MamaLauda());
        }
    }

    public void ChangeVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public IEnumerator FadeOut(float fadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }
    }

    IEnumerator MamaLauda()
    {
        MamaL = true;
        yield return new WaitForSeconds(0.5f);
        audioSource.Stop();
        audioSource.Play();
        audioSource.volume = 0.2f;
        MamaL = false;
    }
}
