using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Parametres : MonoBehaviour
{
    public GameObject MenuParametre;
    public Slider SliderLuminosite;
    public Slider SliderSon;
    

    //reb lmaout
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void MainMenu(){
        SceneManager.LoadScene("MainMenu");
    }
    public void ChangeBrightness()
    {
        RenderSettings.ambientIntensity = SliderLuminosite.value; 
    }
    public void ChangeVolume()
    {
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in allAudioSources)
        {
            audioSource.volume = SliderSon.value; 
        }
    }
}
