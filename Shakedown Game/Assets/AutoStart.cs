using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoStart : MonoBehaviour
{
    private void Start()
    {
        NetworkingManager.Singleton.GetComponent<EnetTransport.EnetTransport>().Address = "127.0.0.1";
        NetworkingManager.Singleton.GetComponent<EnetTransport.EnetTransport>().Port = 15937;

        NetworkingManager.Singleton.StartClient();
    }
}
