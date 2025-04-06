using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    [Header("Title")]
    public TextMeshProUGUI titleText;

    [Header("Options")]
    public Slider volumeMaster;
    public Toggle mute;
    public AudioMixer mixer;
    public AudioSource fxSource;
    public AudioClip clickSound;
    private float lastVolume;
    [Header("Panels")]

    public GameObject startPanel;
    public GameObject mainPanel;
    public GameObject optionsPanel;
   


    private void Awake()
    {
        volumeMaster.onValueChanged.AddListener(ChangeVolumeMaster);
    }

    public void Open(string game)
    {
        SceneManager.LoadScene(game);
    }
    public void StartPanel(GameObject panel)
    {
        startPanel.SetActive(false);
        mainPanel.SetActive(false);
        optionsPanel.SetActive(false);
    

        panel.SetActive(true);
        PlaySoundButton();
    }

    public void ExitGame()
    {
        PlaySoundButton();
        Application.Quit();
    }
    public void SetMute()
    {
        if (mute.isOn)
        {
            mixer.GetFloat("VolMaster", out lastVolume);
            mixer.SetFloat("VolMaster", -80);
        }
        else
            mixer.SetFloat("VolMaster", lastVolume);
    }

    public void OpenPanel(GameObject panel)
    {
        startPanel.SetActive(false);
        mainPanel.SetActive(false);
        optionsPanel.SetActive(false);
      

        panel.SetActive(true);

        if (panel == optionsPanel )
        {
            titleText.gameObject.SetActive(false);
        }
        else
        {
            titleText.gameObject.SetActive(true);
        }

        PlaySoundButton();
    }

    public void ChangeVolumeMaster(float v)
    {
        mixer.SetFloat("VolMaster", v);
    }
    public void PlaySoundButton()
    {
        fxSource.PlayOneShot(clickSound);
    }
}
