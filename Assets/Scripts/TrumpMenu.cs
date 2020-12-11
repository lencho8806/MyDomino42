using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Domino42
{
    public class TrumpMenu : MonoBehaviour
    {
        public static bool GameIsTrump = false;
        private int Trump = 0;

        public Text textAmount;
        public GameObject trumpMenuUI;

        void Start()
        {

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
                case -1:
                    // do nothing... min...
                    break;
                case 0:
                    // set to pass
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
            var domino42 = FindObjectOfType<Game>();
            domino42.players[0].Trump = (Trump)Trump;
            domino42.Trump = (Trump)Trump;

            domino42.trumpText.text = domino42.Trump.ToString();

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
            var domino42 = FindObjectOfType<Game>();

            // get list of nums to determine trump
            List<string> dominoNums = new List<string>();
            domino42.players[0].Hand.ForEach(domino =>
            {
                var dominoSplit = domino.Split('_');
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