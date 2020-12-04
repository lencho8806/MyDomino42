using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinLoseMenu : MonoBehaviour
{
    public GameObject winLoseMenuUI;
    public Text Message;
    public string Next;
    
    public void Restart()
    {
        Next = "RESTART";
        winLoseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void WinLoseStart(string message)
    {
        Next = null;
        winLoseMenuUI.SetActive(true);
        Message.text = message;
        Time.timeScale = 0f;
    }
}
