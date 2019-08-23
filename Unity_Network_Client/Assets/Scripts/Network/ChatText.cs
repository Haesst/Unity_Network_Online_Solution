using UnityEngine;
using UnityEngine.UI;

public class ChatText : MonoBehaviour
{
    public static ChatText instance;
    private Text textField;

    private void Awake()
    {
        instance = this;
        textField = GetComponent<Text>();
    }

    public void RecieveChatMessage(string message, int connectionID = 0)
    {
        // Todo get player name before sending
        string chatText = textField.text;
        if (connectionID <= 0) { chatText += "System : " + message + "\n"; } else { chatText += $"{connectionID} : " + message + "\n"; }
        textField.text = chatText;
    }
}
