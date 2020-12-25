using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Domino42
{
    public class UserInput : MonoBehaviour
    {
        // SHOULD i move all this logic to PlayState...???

        public GameObject prevObjectClicked = null;

        [SerializeField]
        Game domino42;

        // Start is called before the first frame update
        void Start()
        {
            prevObjectClicked = this.gameObject;
        }

        // Update is called once per frame
        void Update()
        {
            if (domino42.CurrGameState == Game.GameState.Play && !domino42.players[domino42.CurrentPlayerTurn].IsAI)
            {
                GetMouseClick();
            }
        }

        void GetMouseClick()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -10));
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit)
                {
                    if (hit.collider.CompareTag("Domino") && domino42.players[domino42.CurrentPlayerTurn].Hand.Exists(domino => domino == hit.collider.name))
                    {
                        if (hit.collider.name == prevObjectClicked.name)
                        {
                            // clicked domino
                            print("Clicked domino");

                            prevObjectClicked = this.gameObject;

                            domino42.SelectDominoFromHand(domino42.CurrentPlayerTurn, hit.collider.name);

                            //domino42.players[0].TurnComplete = true;
                        }
                        else
                        {
                            prevObjectClicked = hit.collider.gameObject;
                        }
                    }
                }
            }
        }
    }
}