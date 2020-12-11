using UnityEngine;
using System.Collections;
using Domino42;

namespace Assets.Scripts.FSM.States
{
    [CreateAssetMenu(fileName = "RoundWinnerState", menuName = "Unity-FSM/States/RoundWinner", order = 6)]
    public class RoundWinnerState : AbstractFSMState
    {
        public override void OnEnable()
        {
            base.OnEnable();

            StateType = FSMStateType.ROUND_WINNER;
        }

        public override bool EnterState()
        {
            EnteredState = base.EnterState();

            if (EnteredState)
            {
                Debug.Log("ENTERED ROUND WINNER STATE");
                
                _domino42.RoundWinner();
            }

            return EnteredState;
        }

        public override void UpdateState()
        {
            if (EnteredState)
            {
                if (_domino42.RoundComplete)
                {
                    _fsm.EnterState(FSMStateType.SET_WINNER);
                }
            }
        }

        public override bool ExitState()
        {
            base.ExitState();

            Debug.Log("EXITING ROUND WINNER STATE");

            return true;
        }
    }

}