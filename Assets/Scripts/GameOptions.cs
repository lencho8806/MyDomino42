using Domino42;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOptions : Game
{
    [SerializeField]
    InputField setMarkInputField;
    [SerializeField]
    Toggle neloToggle;
    [SerializeField]
    Toggle forceBidToggle;

    public GameObject Rules;
    
    public void Init()
    {
        setMarkInputField.text = marksToWin.ToString();
        neloToggle.isOn = isNelO;
        forceBidToggle.isOn = isForceBid;
    }
    
    public void UpdateIsNelO(bool newValue)
    {
        isNelO = newValue;
        Debug.Log($"{(isNelO ? "true" : "false")}");
    }

    public void UpdateIsForceBid(bool newValue)
    {
        isForceBid = newValue;
        Debug.Log($"{(isForceBid ? "true" : "false")}");
    }

    public void UpdateSetMark(string newValue)
    {
        if (string.IsNullOrWhiteSpace(newValue))
            marksToWin = 0;
        else
            marksToWin = int.Parse(newValue);

        Debug.Log($"marks to win : {marksToWin}");
    }

    public void CloseRulesWidget()
    {
        Rules.SetActive(false);
    }

    public void OnPracticeClicked()
    {
        if (marksToWin > 0)
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}
