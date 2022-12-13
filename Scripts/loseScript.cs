using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class loseScript : MonoBehaviour
{
    public static bool isLose;
    [SerializeField] private AudioSource loseSoundEffect;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isLose)
        {
            loseSoundEffect.Play();
            isLose = false;
        }
    }
}
