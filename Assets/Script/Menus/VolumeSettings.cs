using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider MasterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SoundSlider;
    [SerializeField] private Slider SFXSlider;
    private void Awake()
    {

        MasterSlider.value = 0.5f;
        musicSlider.value = 0.5f;
        SoundSlider.value = 0.5f;
        SFXSlider.value = 0.5f;
    }
    private void Start()
    {

        if (PlayerPrefs.HasKey("musicVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMasterVolume();
            SetmusicVolume();
            SetSoundVolume();
            SetSFXVolume();
        }
    }


    public void SetMasterVolume()
    {
        float volume = MasterSlider.value;
        myMixer.SetFloat("Master", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }
    public void SetmusicVolume()
    {
        float volume = musicSlider.value;
        myMixer.SetFloat("music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }
    public void SetSoundVolume()
    {
        float volume = SoundSlider.value;
        myMixer.SetFloat("Sound", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SoundVolume", volume);
    }
    public void SetSFXVolume()
    {
        float volume = SFXSlider.value;
        myMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
    private void LoadVolume()
    {
        MasterSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        SoundSlider.value = PlayerPrefs.GetFloat("SoundVolume");
        SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume");

        SetMasterVolume();
        SetmusicVolume();
        SetSoundVolume();
        SetSFXVolume();
    }
}
