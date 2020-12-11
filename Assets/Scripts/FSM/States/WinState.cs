using UnityEngine;
using System.Collections;
using Domino42;

namespace Assets.Scripts.FSM.States
{
    [CreateAssetMenu(fileName = "WinState", menuName = "Unity-FSM/States/Win", order = 8)]
    public class WinState : AbstractFSMState
    {
        int playerIndex = -1;

        public override void OnEnable()
        {
            base.OnEnable();

            StateType = FSMStateType.WIN;
        }

        public override bool EnterState()
        {
            EnteredState = base.EnterState();

            if (EnteredState)
            {
                Debug.Log("ENTERED WIN STATE");
                
                _domino42.winLoseMenu.WinLoseStart("You Won!!!");
            }

            return EnteredState;
        }

        public override void UpdateState()
        {
            if (EnteredState)
            {
                if (_domino42.winLoseMenu.Next != null)
                {
                    if (_domino42.winLoseMenu.Next == "RESTART")
                    {
                        _domino42.ResetMatch();

                        _fsm.EnterState(FSMStateType.SHUFFLE);
                    }
                }
            }
        }

        public override bool ExitState()
        {
            base.ExitState();

            Debug.Log("EXITING WIN STATE");

            return true;
        }
    }

}