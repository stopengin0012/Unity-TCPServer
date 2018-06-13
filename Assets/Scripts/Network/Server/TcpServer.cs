using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloLensModule.Network;

public class TcpServer : MonoBehaviour {

    private TcpNetworkServerManager tcpManager;    

    // Use this for initialization
    void Start () {
        tcpManager = new TcpNetworkServerManager(28000);
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnDestroy()
    {
        tcpManager.DeleteManager();
    }
}
