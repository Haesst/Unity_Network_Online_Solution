using System;
using System.Net.Sockets;
using UnityEngine;

public class NetworkManager : MonoBehaviour {

    public static NetworkManager instance;

    [SerializeField] private string host; //Server IpAdress
    [SerializeField] private int port;  //Server Port

    private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private static byte[] buffer = new byte[1024 * 4];

    public static Socket GetSocket { get => _clientSocket; }
    public static byte[] GetBuffer { get => buffer; }
    public static bool IsConnected { get => _clientSocket.Connected; }



    private void Awake()
    {
        instance = this;
        host = "127.0.0.1";
        port = 7171;
        DontDestroyOnLoad(this);
        UnityThread.initUnityThread();

        //ClientHandleData.InitializePacketListener();
        LoopConnect(host, port);
    }

    private static void LoopConnect(string host, int port)
    {
        while (!_clientSocket.Connected)
        {
            int attempts = 0;
            try
            {
                _clientSocket.Connect(host, port);
                if (_clientSocket.Connected)
                {
                    Debug.Log($"Connected to: {_clientSocket.RemoteEndPoint}");
                    _clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(HandleClientData.RecieveCallback), _clientSocket);
                    // Request ID
                    HandleClientData.RequestPlayerID();
                }
            }
            catch (SocketException)
            {
                attempts++;
                Debug.Log($"Connection Attempt: {attempts}");
            }
        }
    }

    public static void Send(byte[] buffer)
    {
        _clientSocket.Send(buffer);
    }
}
