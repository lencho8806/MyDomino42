﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Domino42
{
    public class TrumpMenu : MonoBehaviour
    {
        private Game domino42;
        public static bool GameIsTrump = false;
        private int Trump = 0;

        public Text textAmount;
        public GameObject trumpMenuUI;

        void Start()
        {
            //probably need to reset values...

            domino42 = FindObjectOfType<Game>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (GameIsTrump)
                {
                    //Resume();
                }
                else
                {
                    //Pause();
                }
            }
        }

        public void DecreaseTrumpAmount()
        {
            Debug.Log("Decrease trump...");

            switch (Trump)
            {
                case -2:
                    //do nothing... min option...
                    break;
                case -1:
                    if (domino42.IsNelO && domino42.players[domino42.CurrentPlayerTurn].BidAmount >= 42)
                    {
                        Trump--;
                        textAmount.text = "Nel-O";
                        textAmount.fontSize = 80;
                    }

                    break;
                case 0:
                    Trump--;
                    textAmount.text = "Follow Me";
                    textAmount.fontSize = 40;
                    break;
                default:
                    Trump--;
                    textAmount.text = Trump.ToString();
                    textAmount.fontSize = 80;
                    break;
            }
        }

        public void IncreaseTrumpAmount()
        {
            Debug.Log("Increase trump...");

            switch (Trump)
            {
                case -2:
                    Trump++;
                    textAmount.text = "Follow Me";
                    textAmount.fontSize = 40;
                    break;
                case 6:
                    Trump++;
                    textAmount.text = "Doubles";
                    textAmount.fontSize = 40;
                    break;
                case 7:
                    // do nothing... max...
                    break;
                default:
                    Trump++;
                    textAmount.text = Trump.ToString();
                    textAmount.fontSize = 80;
                    break;
            }
        }

        public void SetTrump()
        {
            Debug.Log("Set Trump...");

            TrumpEnd();
        }

        public void TrumpEnd()
        {
            domino42.TrumpEnd(Trump);

            trumpMenuUI.SetActive(false);
            Time.timeScale = 1f;
            GameIsTrump = false;
        }

        public void TrumpStart()
        {
            trumpMenuUI.SetActive(true);
            Time.timeScale = 0f;
            GameIsTrump = true;
        }

        private int DefaultTrump()
        {
            //var domino42 = FindObjectOfType<Game>();

            // get list of nums to determine trump
            List<string> dominoNums = new List<string>();
            domino42.players[domino42.CurrentPlayerTurn].Hand.ForEach(domino =>
            {
                var dominoSplit = domino42.dominoes[domino].Split('_');
                dominoNums.Add(dominoSplit[0]);
                dominoNums.Add(dominoSplit[1]);
            });

            var dominoGroups = dominoNums.GroupBy(num => num)
                .Select(x => new
                {
                    Value = x.Key,
                    Count = x.Count()
                })
                .OrderByDescending(x => x.Count).ToList();

            int defaultTrump = -1;
            int.TryParse(dominoGroups[0].Value, out defaultTrump);

            return defaultTrump;
        }
    }
}