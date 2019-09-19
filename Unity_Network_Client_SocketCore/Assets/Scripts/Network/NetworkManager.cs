using System.Diagnostics;
using UnityEngine;
using UnityEngine.Assertions;
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

    public GameObject quitButton;
    public GameObject selectName;

    private void Awake()
    {
        instance = this;
        host = "10.20.2.104";
        port = 7171;
        isConnected = false;
        pingMs = GameObject.Find("Ping").GetComponentInChildren<Text>();
        elapsedMsTime = new Stopwatch();
        quitButton = GameObject.Find("Button_Quit");
        selectName = GameObject.Find("SelectName");
        DontDestroyOnLoad(this);
        UnityThread.initUnityThread();
        ClientHandleData.InitializePacketList();

        Button joinGameButton = selectName.transform.GetChild(1).GetComponent<Button>();
        if (joinGameButton != null)
        {
            joinGameButton.onClick.AddListener(() => JoinGame());
        }
    }

    public void JoinGame()
    {
        Text playerName = selectName.transform.GetChild(0).GetChild(2).GetComponent<Text>();
        if (playerName.text == string.Empty)
        {
            selectName.transform.GetChild(0).GetChild(1).GetComponent<Text>().color = Color.red;
            return;
        }

        if (selectName.transform.GetChild(0).GetChild(1).GetComponent<Text>().color == Color.red)
        {
            selectName.transform.GetChild(0).GetChild(1).GetComponent<Text>().color = new Color(50, 50, 50);
        }

        ClientTCP.Connect(host, port);
        quitButton.SetActive(false);
        selectName.SetActive(false);
        Cursor.visible = false;
    }

}
