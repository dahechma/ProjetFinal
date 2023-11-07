using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public GameObject Menu;
    public Boolean isPaused;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
     public void Quitter(){
        Application.Quit();
    }
    public void Resume(){
        if(isPaused){
            Menu.SetActive(false);
            isPaused = false;
        }
        else{
            Menu.SetActive(true);
            isPaused = true;

        }
    }
    public void Options(){

        SceneManager.LoadScene("Parametres");

    }
     public void Menuprincipal(){

        SceneManager.LoadScene("MainMenu");

    }
}
