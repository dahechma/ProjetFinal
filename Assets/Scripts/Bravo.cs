using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bravo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Quitter(){
         SceneManager.LoadScene("MainMenu");

    }
    public void ChoisirAutreSituation(){
        SceneManager.LoadScene("Situations");
    }
}
