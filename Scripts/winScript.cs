using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class winScript : MonoBehaviour
{
    public static bool isWin;

    [SerializeField] private AudioSource winSoundEffect;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isWin)
        {
            winSoundEffect.Play();
            isWin = false;
        }   
    }
}
