using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public GameObject Menu;

    // Start is called before the first frame update
    void Start()
    {
        // Menu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // public void MainMenu(){
    //     Menu.SetActive(true);
    // }

    public void Quitter(){
        Application.Quit();
    }
}
