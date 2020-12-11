using UnityEngine;
using System.Collections;
using Domino42;

namespace Assets.Scripts.FSM.States
{
    [CreateAssetMenu(fileName = "LoseState", menuName = "Unity-FSM/States/Lose", order = 9)]
    public class LoseState : AbstractFSMState
    {
        int playerIndex = -1;

        public override void OnEnable()
        {
            base.OnEnable();

            StateType = FSMStateType.LOSE;
        }

        public override bool EnterState()
        {
            EnteredState = base.EnterState();

            if (EnteredState)
            {
                Debug.Log("ENTERED LOSE STATE");
                
                _domino42.winLoseMenu.WinLoseStart("You Lost...");
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

            Debug.Log("EXITING LOSE STATE");

            return true;
        }
    }

}