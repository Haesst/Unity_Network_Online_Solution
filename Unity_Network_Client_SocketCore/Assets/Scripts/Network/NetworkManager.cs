using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    [SerializeField] private string host; //Server IpAdress
    [SerializeField] private int port;  //Server Port
    public bool isConnected;
    [HideInInspector] public static int connectionID;
    public Text pingMs;
    public Stopwatch elapsedMsTime;

    private void Awake()
    {
        instance = this;
        host = "127.0.0.1";
        port = 7171;
        isConnected = false;
        pingMs = GameObject.Find("Ping").GetComponentInChildren<Text>();
        elapsedMsTime = new Stopwatch();
        DontDestroyOnLoad(this);
        UnityThread.initUnityThread();

        ClientHandleData.InitializePacketList();
        ClientTCP.Connect(host, port);
    }

}
