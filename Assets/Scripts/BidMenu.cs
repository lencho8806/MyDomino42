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
        [SerializeField]
        Slider bidSlider;
        float previousSliderValue;
        private int Amount = 30;
        private Text textAmount;
        private Game domino42;
        private int minBid = 30;

        public GameObject bidMenuUI;

        void Start()
        {
            domino42 = FindObjectOfType<Game>();

            previousSliderValue = bidSlider.value;

            minBid = domino42.players.Max(p => 
                p.BidAmount ?? -1
            );
            minBid = minBid == -1 ? 30 : (minBid + 1);
            Amount = minBid;

            previousSliderValue = -1;
            bidSlider.value = -1;
            if (minBid >= 30)
            {
                previousSliderValue = minBid - 30;
                bidSlider.value = (float)minBid - 30;
            }

            var textChildren = new List<Text>(bidMenuUI.GetComponentsInChildren<Text>());
            textAmount = textChildren.Find(text => text.name == "BidAmountText");
            
            if (Amount == 42)
            {
                textAmount.text = "1M";
            }
            else if (Amount == 43)
            {
                textAmount.text = "2M";
            }
            else
            {
                textAmount.text = Amount.ToString();
            }
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

            Decrease();

            if (Amount == -1)
            {
                previousSliderValue = bidSlider.value;
                bidSlider.SetValueWithoutNotify(Amount);
            }
            else if (Amount >= 30)
            {
                previousSliderValue = bidSlider.value;
                bidSlider.SetValueWithoutNotify(Amount - 30);
            }
        }

        public void Decrease()
        {
            if (Amount == -1)
            {
                // do nothing... min bid...
            }
            else if (Amount == minBid)
            {
                Amount = -1;
                textAmount.text = "Pass";
            }
            else if (Amount == 43)
            {
                Amount--;
                textAmount.text = "1M";
            }
            else
            {
                Amount--;
                textAmount.text = Amount.ToString();
            }
        }

        public void IncreaseBidAmount()
        {
            Debug.Log("Increase bid...");

            Increase();

            if (Amount == -1)
            {
                previousSliderValue = bidSlider.value;
                bidSlider.SetValueWithoutNotify(Amount);
            }
            else if (Amount >= 30)
            {
                previousSliderValue = bidSlider.value;
                bidSlider.SetValueWithoutNotify(Amount - 30);
            }
        }

        public void Increase()
        {
            switch (Amount)
            {
                case -1:
                    // increase to min bid...
                    Amount = minBid;
                    textAmount.text = Amount.ToString();
                    break;
                case 41:
                    Amount++;
                    textAmount.text = "1M";
                    break;
                case 42:
                    Amount++;
                    textAmount.text = "2M";
                    break;
                case 43:
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

            if (domino42.players[domino42.CurrentPlayerTurn].IsDealer && domino42.IsForceBid)
            {
                if (domino42.players.Any(p => p.BidAmount != -1 && p.Id != domino42.players[domino42.CurrentPlayerTurn].Id))
                {
                    // continue...
                }
                else
                {
                    domino42.MessageText.text = "(Dealer has to bid)";
                    return;
                }
            }

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

            if (domino42.players[domino42.CurrentPlayerTurn].IsDealer && domino42.IsForceBid)
            {
                if (domino42.players.Any(p => p.BidAmount != -1 && p.Id != domino42.players[domino42.CurrentPlayerTurn].Id))
                {
                    // continue...
                }
                else
                {
                    if (Amount == -1)
                    {
                        domino42.MessageText.text = "(Dealer has to bid)";
                        return;
                    }
                }
            }

            BidEnd();
        }

        public void BidEnd()
        {
            domino42.BidEnd(Amount);

            bidMenuUI.SetActive(false);
            //Time.timeScale = 1f;
            GameIsBid = false;
        }

        public void BidStart()
        {
            bidMenuUI.SetActive(true);
            //Time.timeScale = 0f;
            GameIsBid = true;

            if (domino42 != null)
                Init();
        }

        public void Init()
        {
            minBid = domino42.players.Max(p =>
                 p.BidAmount ?? -1
             );
            minBid = minBid == -1 ? 30 : (minBid + 1);
            Amount = minBid;

            textAmount.text = BidText(Amount);

            previousSliderValue = -1;
            bidSlider.value = -1;
            if (minBid >= 30)
            {
                previousSliderValue = minBid - 30;
                bidSlider.value = (float) minBid - 30;
            }
        }

        public string BidText(int amount)
        {
            string bidAmountText = amount.ToString();

            if (amount == 42)
            {
                bidAmountText = "1M";
            }
            else if (amount == 43)
            {
                bidAmountText = "2M";
            }

            return bidAmountText;
        }

        public void SliderValueChanged(float value)
        {
            if (value > previousSliderValue)
            {
                Increase();
            }
            else if (value < previousSliderValue)
            {
                Decrease();
            }

            previousSliderValue = value;
        }
    }

}