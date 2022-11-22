using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame() 
    {
        SceneManager.LoadScene(1); //Loads scene 1 in unity's build setting
    }

    public void ExitGame() 
    {
        Debug.Log("Quit Selected");
        Application.Quit(); //quits app, doesn't work in editor
    }
}
