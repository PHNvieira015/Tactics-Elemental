using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource soundSource;
    public AudioSource sfxSource;

    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioClip soundClip;
    [SerializeField] private AudioClip sfxClip;

    void Start()
    {
        // Ensure all sources start with no clip playing
        musicSource.clip = null;
        soundSource.clip = null;
        sfxSource.clip = null;
    }

    public void PlayMusic()
    {
        PlayClip(musicSource, musicClip);
    }

    public void PlaySound()
    {
        PlayClip(soundSource, soundClip);
    }

    public void PlaySFX()
    {
        PlayClip(sfxSource, sfxClip);
    }

    private void PlayClip(AudioSource source, AudioClip clip)
    {
        if (clip != null)
        {
            // Stop any currently playing clip on the specified source
            if (source.isPlaying)
            {
                source.Stop();
            }

            // Play the new clip on the specified source
            source.clip = clip;
            source.Play();
        }
        else
        {
            Debug.LogWarning("Attempting to play a null AudioClip.");
        }
    }
}