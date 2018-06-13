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

//確認用
//using UnityEngine;

namespace HoloLensModule.Network
{
    /// <summary>
    /// TCPプロトコルによるサーバーマネージャークラス
    /// (y.saitoによって改変)
    /// </summary>
    public class TcpNetworkServerManager
    {

#if UNITY_UWP
        private StreamSocketListener streamsocketlistener;
        private List<StreamWriter> writer = new List<StreamWriter>();
#elif UNITY_EDITOR || UNITY_STANDALONE
        //private List<NetworkStream> streams = new List<NetworkStream>();
        private List<Channel> streams = new List<Channel>();
        private TcpListener tcpserver;  //y.saito

        //TODO : 再利用できるよう書き直す?
        //中央集権的のメリット：選択的に通信する、可視化ツールとつなげる、通信ログを取りデバッグしやすくする、など




#endif
        private bool ListenFlag = false;

        public TcpNetworkServerManager(int port)
        {
            ListenFlag = true;
#if UNITY_UWP
            Task.Run(async()=>{
                streamsocketlistener = new StreamSocketListener();
                streamsocketlistener.ConnectionReceived += ConnectionReceived;
                await streamsocketlistener.BindServiceNameAsync(port.ToString());
            });
#elif UNITY_EDITOR || UNITY_STANDALONE
            tcpserver = new TcpListener(IPAddress.Any, port);   //y.saito
            tcpserver.Start();
            Thread listenerthread = new Thread(ListenThread);
            listenerthread.Start();
#endif
        }

        public void DeleteManager()
        {
            ListenFlag = false;
#if UNITY_UWP
            writer.Clear();
            streamsocketlistener.Dispose();
#elif UNITY_EDITOR || UNITY_STANDALONE
            streams.Clear();
#endif
        }

#if UNITY_UWP
        private async void ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            StreamReader reader = new StreamReader(args.Socket.InputStream.AsStreamForRead());
            reader.BaseStream.ReadTimeout = 100;
            writer.Add(new StreamWriter(args.Socket.OutputStream.AsStreamForWrite()));
            while (ListenFlag)
            {
                try
                {
                    string data = await reader.ReadToEndAsync();
                    for (int i = 0; i < writer.Count; i++)
                    {
                        await writer[i].WriteAsync(data);
                        await writer[i].FlushAsync();
                    }
                }
                catch (Exception) { }
            }
        }
#elif UNITY_EDITOR || UNITY_STANDALONE
#endif

        /// <summary>
        /// broadcast all channels
        /// </summary>
        /// <param name="message"></param>
        public void broadcast(byte[] message)
        {
#if UNITY_UWP
            //TODO : Write UWP version
#elif UNITY_EDITOR || UNITY_STANDALONE

            for (int i = 0; i < streams.Count; i++)
            {
                if (streams[i].GetStream().CanWrite) streams[i].GetStream().Write(message, 0, message.Length);
            }
#endif
        }

        /// <summary>
        /// Lestener thread
        /// </summary>
        private void ListenThread()
        {
#if UNITY_UWP
            //TODO : Write UWP version
#elif UNITY_EDITOR || UNITY_STANDALONE

            while (ListenFlag)
            {
                TcpClient tcpclient = tcpserver.AcceptTcpClient();  //接続待機
                tcpclient.ReceiveTimeout = 100;

                //接続後はThreadを新しくたてる
                Thread thread = new Thread(() =>
                {
                    Channel channel = new Channel(this, tcpclient);
                    //NetworkStream stream = tcpclient.GetStream();
                    //channel.SetStream(tcpclient.GetStream());
                    streams.Add(channel);
                    channel.Listen();

                });
                thread.Start();
            }
            tcpserver.Stop();
#endif
        }


        public bool getListenFlag()
        {
            return this.ListenFlag;
        }

        public TcpListener getTcpListener()
        {
            return this.tcpserver;
        }

        public List<Channel> getChannels()
        {
            return this.streams;
        }

    }

}
