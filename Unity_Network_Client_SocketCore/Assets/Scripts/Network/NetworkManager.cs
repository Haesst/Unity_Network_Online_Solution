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
    public static Text pingMs;
    public static Stopwatch elapsedMsTime;

    private void Awake()
    {
        instance = this;
        host = "81.230.233.47";
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
