using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using System;

public class Player : MonoBehaviour
{
    public int items = 0;
    public float speed  = 5.0f;
    
    public Text itemAmount;
    public GameObject door;

    private TcpListener tcpListener;

    private Thread tcpListenerThread;
    private TcpClient connectedTcpClient;

    public bool listening = true;
    bool[] fingersUp = new bool[5];

    [SerializeField] private AudioSource moveSoundEffect, itemSoundEffect, doorSoundEffect;
    // Start is called before the first frame update
    void Start()
    {
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    public void stopServer()
    {
        listening = false;
        tcpListenerThread.Interrupt();
        tcpListenerThread.Abort();
    }

    private void OnApplicationQuit()
    {
           stopServer();
    }
    private void ListenForIncommingRequests()
    {
        try
        {
            // Create listener on localhost port 8052. 			
            tcpListener = new TcpListener(IPAddress.Parse("0.0.0.0"), 8055);
            tcpListener.Start();
            Debug.Log("Server is listening");
            Byte[] bytes = new Byte[1024];
            while (listening)
            {
                using (connectedTcpClient = tcpListener.AcceptTcpClient())
                {
                    // Get a stream object for reading 					
                    using (NetworkStream stream = connectedTcpClient.GetStream())
                    {
                        int length;
                        // Read incomming stream into byte arrary. 						
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            // Convert byte array to string message. 							
                            string clientMessage = Encoding.ASCII.GetString(incommingData);


                            try
                            {
                                bool[] fingersUp1 = getJsonArray<bool>(clientMessage);
                                Debug.Log("Fingers: " + fingersUp1[0] + fingersUp1[1] + fingersUp1[2] + fingersUp1[3]);
                                this.fingersUp = fingersUp1;
                            }
                            catch (ArgumentException e)
                            {
                                // Debug.Log(e);
                                Debug.Log("Message: " + clientMessage);
                            }


                        }
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }
    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKey(KeyCode.LeftArrow))
        //{
        //    transform.Translate(-speed * Time.deltaTime, 0, 0);
        //}
        //if(Input.GetKey(KeyCode.RightArrow))
        //{
        //    transform.Translate(speed * Time.deltaTime, 0, 0);
        //}
        //if(Input.GetKey(KeyCode.UpArrow))
        //{
        //    transform.Translate(0, speed * Time.deltaTime, 0);
        //}
        //if(Input.GetKey(KeyCode.DownArrow))
        //{
        //    transform.Translate(0, -speed * Time.deltaTime, 0);
        //}

        if (fingersUp[3]) //KIRI
        {
            transform.Translate(-speed * Time.deltaTime, 0, 0);
        }
        if (fingersUp[0]) // KANAN
        {
            transform.Translate(speed * Time.deltaTime, 0, 0);
        }
        if (fingersUp[1]) //ATAS
        {
            transform.Translate(0, speed * Time.deltaTime, 0);
        }
        if (fingersUp[2]) // BAWAH
        {
            transform.Translate(0, -speed * Time.deltaTime, 0);
        }

        if (items >= 3)
        {
            Destroy(door);
        }
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] fingers_up;
    }
    public static T[] getJsonArray<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.fingers_up;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "items")
        {
            items++;
            itemAmount.text = "Items : " + items;
            Destroy(collision.gameObject);
            if(items >= 3)
            {
                Destroy(door);
                doorSoundEffect.Play();
            }
            else
            {
                itemSoundEffect.Play();
            }
            
            
        }
        if(collision.gameObject.tag == "Finish")
        {
            winScript.isWin = true;
            SceneManager.LoadScene("YouWin");
            stopServer();
        }

        if(collision.gameObject.tag == "Ghost")
        {
            loseScript.isLose = true;
            SceneManager.LoadScene("Game Over");
            stopServer();
        }

        if(collision.gameObject.tag == "Walls")
        {
            if(Input.GetKey(KeyCode.LeftArrow))
            {
                transform.Translate(speed * Time.deltaTime, 0, 0);
            }
            if(Input.GetKey(KeyCode.RightArrow))
            {
                transform.Translate(-speed * Time.deltaTime, 0, 0);
            }
            if(Input.GetKey(KeyCode.UpArrow))
            {
                transform.Translate(0, -speed * Time.deltaTime, 0);
            }
            if(Input.GetKey(KeyCode.DownArrow))
            {
                transform.Translate(0, speed * Time.deltaTime, 0);
            }
        }
    }
}
