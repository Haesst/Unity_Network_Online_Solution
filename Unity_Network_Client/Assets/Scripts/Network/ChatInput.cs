using UnityEngine;
using UnityEngine.UI;

public class ChatInput : MonoBehaviour
{
    private InputField inputField;
    private ChatText chatText;

    private void Awake()
    {
        inputField = GetComponent<InputField>();
        chatText = FindObjectOfType<ChatText>();
    }

    public void SendChatMessage()
    {
        string message = inputField.text;
        inputField.text = "";
        chatText.RecieveChatMessage(message);
        ClientTCP.PACKAGE_BroadcastMsg(message);
    }
}
