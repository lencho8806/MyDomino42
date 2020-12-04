using UnityEngine;
using System.Collections;

namespace Assets.Scripts.FSM.States
{
    [CreateAssetMenu(fileName = "ShuffleState", menuName = "Unity-FSM/States/Shuffle", order = 1)]
    public class ShuffleState : AbstractFSMState
    {
        public override void OnEnable()
        {
            base.OnEnable();

            StateType = FSMStateType.SHUFFLE;
        }

        public override bool EnterState()
        {
            EnteredState = base.EnterState();

            if (EnteredState)
            {
                Debug.Log("ENTERED SHUFFLE STATE");

                _domino42.Shuffle();

                _fsm.EnterState(FSMStateType.DEAL);
            }

            return EnteredState;
        }

        public override void UpdateState()
        { }

        public override bool ExitState()
        {
            base.ExitState();

            Debug.Log("EXITING SHUFFLE STATE");

            return true;
        }
    }

}