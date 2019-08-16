using UnityEngine;

public class NetworkManager : MonoBehaviour {

    public static NetworkManager instance;

    [SerializeField] private string host; //Server IpAdress
    [SerializeField] private int port;  //Server Port
    public bool isConnected;
    [HideInInspector] public static int connectionID;

    

    private void Awake()
    {
        instance = this;
        host = "localhost";
        port = 7171;
        isConnected = false;
        DontDestroyOnLoad(this);
        UnityThread.initUnityThread();

        ClientHandleData.InitializePacketListener();
        ClientTCP.InitializeClientSocket(host, port);
    }

    public void ReconnectToServer()
    {
        ClientTCP.InitializeClientSocket(host, port);
    }
}
