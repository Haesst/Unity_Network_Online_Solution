using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{
    public Button quit;
    private void Awake()
    {
        Button btn = quit.GetComponent<Button>();
        btn.onClick.AddListener(Application.Quit);
    }

}
