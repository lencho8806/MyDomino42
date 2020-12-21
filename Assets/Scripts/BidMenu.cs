using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Domino42
{
    public class BidMenu : MonoBehaviour
    {
        public bool GameIsBid = false;
        private int Amount = 30;
        private Text textAmount;
        private Game domino42;

        public GameObject bidMenuUI;

        void Start()
        {
            // probably need to reset values...

            domino42 = FindObjectOfType<Game>();

            var textChildren = new List<Text>(bidMenuUI.GetComponentsInChildren<Text>());
            textAmount = textChildren.Find(text => text.name == "BidAmountText");
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (GameIsBid)
                {
                    //Resume();
                }
                else
                {
                    //Pause();
                }
            }
        }

        public void DecreaseBidAmount()
        {
            Debug.Log("Decrease bid...");

            switch (Amount)
            {
                case -1:
                    // do nothing... min...
                    break;
                case 30:
                    // set to pass
                    Amount = -1;
                    textAmount.text = "Pass";

                    break;
                default:
                    Amount--;
                    textAmount.text = Amount.ToString();
                    break;
            }
        }

        public void IncreaseBidAmount()
        {
            Debug.Log("Increase bid...");

            switch (Amount)
            {
                case -1:
                    // do nothing... min...
                    Amount = 30;
                    textAmount.text = Amount.ToString();
                    break;
                case 42:
                    // do nothing... max...
                    break;
                default:
                    Amount++;
                    textAmount.text = Amount.ToString();
                    break;
            }
        }

        public void Pass()
        {
            Debug.Log("Pass...");

            Amount = -1;

            textAmount.text = "Pass";

            BidEnd();
        }

        public void Bid()
        {
            Debug.Log("Bid...");

            //var domino42 = FindObjectOfType<Game>();
            int? maxBid = domino42.players.Max(player => player.BidAmount);

            if (maxBid.HasValue)
            {
                if (maxBid != -1)
                {
                    if (Amount <= maxBid && Amount != -1)
                        return;
                }
            }

            BidEnd();
        }

        public void BidEnd()
        {
            //var domino42 = FindObjectOfType<Game>();
            domino42.players[domino42.CurrentPlayerTurn].BidAmount = Amount;

            domino42.playerBidTexts[domino42.CurrentPlayerTurn].text = Amount.ToString();

            bidMenuUI.SetActive(false);
            Time.timeScale = 1f;
            GameIsBid = false;
        }

        public void BidStart()
        {
            bidMenuUI.SetActive(true);
            Time.timeScale = 0f;
            GameIsBid = true;
        }
    }

}