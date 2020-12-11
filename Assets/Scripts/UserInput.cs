using Assets.Scripts.FSM;
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
        FiniteStateMachine fsm;

        // Start is called before the first frame update
        void Start()
        {
            fsm = domino42.GetComponent<FiniteStateMachine>();

            prevObjectClicked = this.gameObject;
        }

        // Update is called once per frame
        void Update()
        {
            if (domino42.CurrentPlayerTurn == 0 && fsm.CurrentState.StateType == FSMStateType.PLAY)
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
                    if (hit.collider.CompareTag("Domino") && domino42.players[0].Hand.Exists(domino => domino == hit.collider.name))
                    {
                        if (hit.collider.name == prevObjectClicked.name)
                        {
                            // clicked domino
                            print("Clicked domino");

                            prevObjectClicked = this.gameObject;

                            domino42.SelectDominoFromHand(0, hit.collider.name);

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