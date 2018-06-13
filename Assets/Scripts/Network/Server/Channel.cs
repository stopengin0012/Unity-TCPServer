using System;
using System.Collections.Generic;
#if UNITY_UWP
using System.IO;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#elif UNITY_EDITOR || UNITY_STANDALONE
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Text;

#endif

using HoloLensModule.Network;
using UnityEngine;

public class Channel
{
    //Field : Network関係
    private TcpNetworkServerManager serverManager;
#if UNITY_EDITOR || UNITY_STANDALONE
    private TcpClient tcpClient;
    private NetworkStream stream;  //開始元のstream
#endif

    //
    private bool isShakeHands = false;  //接続後のシェイクハンドを行ったかどうか
    public string handle = "";//接続先の名前

    public Channel(TcpNetworkServerManager sManager, TcpClient client)
    {
        Debug.Log("Channel Start");
        this.serverManager = sManager;
        this.tcpClient = client;
        this.stream = this.tcpClient.GetStream();
    }

    //ListerLoop
    public void Listen()
    {
        while (serverManager.getListenFlag())
        {
            try
            {
                byte[] bytes = new byte[tcpClient.ReceiveBufferSize];
                stream.Read(bytes, 0, bytes.Length);

                string Message = System.Text.Encoding.UTF8.GetString(bytes);
                ProcessControlCommand(Message);
            }
            catch (Exception) { }
            if (tcpClient.Client.Poll(1000, SelectMode.SelectRead) && tcpClient.Client.Available == 0) break;
        }
        stream.Close();
        tcpClient.Close();

    }

#if UNITY_EDITOR || UNITY_STANDALONE
    public NetworkStream GetStream()
    {
        return this.stream;
    }
#endif

    /// <summary>
    /// コマンド内容を処理する関数
    /// </summary>
    /// <param name="Mes"></param>
    private void ProcessControlCommand(string Mes)
    {
        Debug.Log("Mes:" + Mes);
        byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(Mes);
        serverManager.broadcast(sendBytes);
    }
}

