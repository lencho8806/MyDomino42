using Domino42;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOptions : MonoBehaviour
{
    [SerializeField]
    InputField setMarkInputField;
    [SerializeField]
    Toggle neloToggle;
    [SerializeField]
    Toggle forceBidToggle;

    public GameObject Rules;

    public int marks = 3;
    public bool isNelO = false;
    public bool isForceBid = false;

    bool isPractice = false;
    
    private void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void Init()
    {
        setMarkInputField.text = marks.ToString();
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
            marks = 0;
        else
            marks = int.Parse(newValue);

        Debug.Log($"marks to win : {marks}");
    }

    public void OpenRulesWidget()
    {
        Rules.SetActive(true);
    }

    public void CloseRulesWidget()
    {
        if (isPractice)
        {
            if (marks > 0)
            {
                SceneManager.LoadScene("GameScene");
            }
        }
        else
        {
            Rules.SetActive(false);
        }
    }

    public void OnPracticeClicked()
    {
        isPractice = true;

        OpenRulesWidget();

        Init();
    }
}
