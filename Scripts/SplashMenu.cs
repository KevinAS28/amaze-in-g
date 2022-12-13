using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashMenu : MonoBehaviour
{
    void Start()
    {
        Invoke("GoToMenuScene", 4);
    }

    // Update is called once per frame
    void GoToMenuScene()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
