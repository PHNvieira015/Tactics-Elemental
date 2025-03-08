using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    public GameObject[] clearOptions;
    public GameObject options;
    public GameObject[] windows;

    [Header("Volume Settings")]
    public Slider[] volumeSliders;
    public TextMeshProUGUI[] volumeValues;

    [Header("Graphic Settings")]
    [SerializeField] private TextMeshProUGUI fullScreen;
    public List<string> screenOptions = new List<string>();
    public TextMeshProUGUI resolutionText = null;
    public Resolution[] resolutions;
    private int currentResolutionIndex = 0;
    [SerializeField] private TextMeshProUGUI vSync;
    [SerializeField] private Slider contrast = null;
    [SerializeField] private TextMeshProUGUI contrastValue = null;
    [SerializeField] private Slider brightness = null;
    [SerializeField] private TextMeshProUGUI brightnessValue = null;
    [SerializeField] private Slider gamma = null;
    [SerializeField] private TextMeshProUGUI gammaValue = null;
    public Slider[] graphicsSliders;

    private bool _isFullscreen;
    private bool _vSync = true;
    private float _contrastLevel;
    private float _brightnessLevel;
    private float _gammaLevel;

    private void Start()
    {
        var fullscreen = PlayerPrefs.GetInt("masterFullScreen") > 0;
        resolutions = Screen.resolutions;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            screenOptions.Add(option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        if (fullscreen)
        {
            Screen.fullScreen = true;
            _isFullscreen = true;
            fullScreen.text = "Full Screen";
        }
        contrast.value = PlayerPrefs.GetFloat("masterContrast");
        brightness.value = PlayerPrefs.GetFloat("masterBrightness");
        gamma.value = PlayerPrefs.GetFloat("masterGamma");
        currentResolutionIndex = PlayerPrefs.GetInt("resolutionIndex");

        resolutionText.text = screenOptions[currentResolutionIndex];
    }

    private void Update()
    {
        for(int i = 0; i < volumeSliders.Length - 3; i++)
        {
            volumeValues[i].text = volumeSliders[i].value.ToString();
        }
    }

    public void changeWindows(int index)
    {
        foreach(GameObject window in windows)
        {
            window.SetActive(false);
        }
        windows[index].SetActive(true);
    }

    public void ExitOptions()
    {
        options.SetActive(false);
        foreach (GameObject @object in clearOptions)
        {
            @object.SetActive(true);
        }
    }

    public void changeVolumePositive(int index)
    {
        volumeSliders[index].value++;
        volumeValues[index].text = volumeSliders[index].value.ToString();
    }
    public void changeVolumeNegative(int index)
    {
        volumeSliders[index].value--;
        volumeValues[index].text = volumeSliders[index].value.ToString();
    }

    public void changeSlidersPositive(int index)
    {
        graphicsSliders[index].value++;
    }
    public void changeSlidersNegative(int index)
    {
        graphicsSliders[index].value--;
    }

    public void SetContrast()
    {
        _contrastLevel = contrast.value;
        contrastValue.text = contrast.value.ToString();
    }

    public void SetBrightness()
    {
        _brightnessLevel = brightness.value;
        brightnessValue.text = brightness.value.ToString();
    }

    public void SetGamma()
    {
        _gammaLevel = gamma.value;
        gammaValue.text = gamma.value.ToString();
    }

    public void SetFullscreen()
    {
        if (_isFullscreen)
        {
            _isFullscreen = false;
            fullScreen.text = "Windowed";
            return;
        }
        else
        {
            _isFullscreen = true;
            fullScreen.text = "Full Screen";
        }
    }

    public void lessResolution()
    {
        currentResolutionIndex--;
        if (currentResolutionIndex < 0)
            currentResolutionIndex = screenOptions.Count - 1;
        if (currentResolutionIndex > screenOptions.Count - 1)
            currentResolutionIndex = 0;
        resolutionText.text = screenOptions[currentResolutionIndex];

    }
    public void moreResolution()
    {
        currentResolutionIndex++;
        if (currentResolutionIndex < 0)
            currentResolutionIndex = screenOptions.Count;
        if (currentResolutionIndex > screenOptions.Count)
            currentResolutionIndex = 0;
        resolutionText.text = screenOptions[currentResolutionIndex];
    }

    public void SetResolution()
    {
        Resolution resolution = resolutions[currentResolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetVSync()
    {
        if (_vSync)
        {
            _vSync = false;
            vSync.text = "OFF";
            return;
        }
        else
        {
            _vSync = true;
            vSync.text = "ON";
        }
    }

    public void GrapphicsApply()
    {
        SetResolution();
        PlayerPrefs.SetInt("resolutionIndex", currentResolutionIndex);
        PlayerPrefs.SetFloat("masterContrast", _contrastLevel);
        PlayerPrefs.SetFloat("masterBrightness", _brightnessLevel);
        PlayerPrefs.SetFloat("masterGamma", _gammaLevel);
        Screen.brightness = _brightnessLevel;
        PlayerPrefs.SetInt("masterFullScreen", (_isFullscreen ? 1 : 0));
        Screen.fullScreen = _isFullscreen;
        PlayerPrefs.SetInt("vSync", (_vSync ? 1 : 0));
    }
}
