using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class volumeScript : MonoBehaviour
{
    private bool isMute = false;
    // Start is called before the first frame update
    public void setAudio()
    {
        if (isMute)
        {
            AudioListener.volume = 1;
            isMute = false;
        }
        else
        {
            AudioListener.volume = 0;
            isMute = true;
        }
    }
}
