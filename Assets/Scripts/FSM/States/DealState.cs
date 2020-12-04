using UnityEngine;
using System.Collections;

namespace Assets.Scripts.FSM.States
{
    [CreateAssetMenu(fileName = "DealState", menuName = "Unity-FSM/States/Deal", order = 2)]
    public class DealState : AbstractFSMState
    {
        public override void OnEnable()
        {
            base.OnEnable();

            StateType = FSMStateType.DEAL;
        }

        public override bool EnterState()
        {
            EnteredState = base.EnterState();

            if (EnteredState)
            {
                Debug.Log("ENTERED DEAL STATE");

                _domino42.Deal();
            }

            return EnteredState;
        }

        public override void UpdateState()
        {
            if (EnteredState)
            {
                if (_domino42.IsDealing == false)
                {
                    _fsm.EnterState(FSMStateType.BID);
                }
            }
        }

        public override bool ExitState()
        {
            base.ExitState();

            Debug.Log("EXITING DEAL STATE");

            return true;
        }
    }

}