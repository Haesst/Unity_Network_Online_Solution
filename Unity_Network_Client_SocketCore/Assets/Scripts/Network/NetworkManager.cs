using System.Diagnostics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    [SerializeField] private string host; //Server IpAdress
    [SerializeField] private int port;  //Server Port
    public static bool isConnected;
    [HideInInspector] public static int connectionID;
    public static Text pingMs;
    public static Stopwatch elapsedMsTime;

    public GameObject quitButton;
    public GameObject selectName;

    private void Awake()
    {
        instance = this;
        host = "127.0.0.1";
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
        InputField nameField = selectName.transform.GetChild(0).GetComponent<InputField>();
        nameField.Select();
        if (nameField.text == string.Empty)
        {
            nameField.placeholder.color = Color.red;
            return;
        }
        if (nameField.text.Length > 20)
        {
            nameField.placeholder.color = Color.red;
            nameField.placeholder.GetComponent<Text>().text = "Max '20' characters!";
            nameField.text = string.Empty;
            return;
        }

        if (nameField.placeholder.color == Color.red)
        {
            nameField.placeholder.color = new Color(50, 50, 50);
        }

        do
        {
            ClientTCP.Connect(host, port);
        } while (!isConnected);

        quitButton.SetActive(false);
        selectName.SetActive(false);
        Cursor.visible = false;
    }

    private void OnApplicationQuit()
    {
        ClientTCP.PACKAGE_Disconnect();
    }

}
