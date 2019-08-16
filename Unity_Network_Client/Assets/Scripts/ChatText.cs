using UnityEngine;
using UnityEngine.UI;

public class ChatText : MonoBehaviour
{
    private Text textField;

    private void Awake()
    {
        textField = GetComponent<Text>();
    }

    public void RecieveChatMessage(string message)
    {
        // Todo get player name before sending
        string chatText = textField.text;
        chatText += "David : " + message + "\n";
        textField.text = chatText;
    }
}
