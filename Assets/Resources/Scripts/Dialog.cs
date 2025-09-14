using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Dialog : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI txtMessage;
    public static Dialog Instance;

    private void Awake()
    {
        Instance = this;
    }
    public void ShowMessage(string message)
    {
        
        txtMessage.text = message;
        panel.SetActive(true);
    }
}
