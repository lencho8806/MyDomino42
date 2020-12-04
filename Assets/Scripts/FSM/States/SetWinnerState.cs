using UnityEngine;
using System.Collections;

namespace Assets.Scripts.FSM.States
{
    [CreateAssetMenu(fileName = "SetWinnerState", menuName = "Unity-FSM/States/SetWinner", order = 7)]
    public class SetWinnerState : AbstractFSMState
    {
        public override void OnEnable()
        {
            base.OnEnable();

            StateType = FSMStateType.SET_WINNER;
        }

        public override bool EnterState()
        {
            EnteredState = base.EnterState();

            if (EnteredState)
            {
                _domino42.SetWinner();
            }

            return EnteredState;
        }

        public override void UpdateState()
        {
            if (EnteredState)
            {
                if (_domino42.SetComplete != null)
                {
                    if (_domino42.SetComplete == true)
                    {
                        if (_domino42.SetScoreUs >= 3)
                        {
                            _fsm.EnterState(FSMStateType.WIN);
                        }
                        else if (_domino42.SetScoreThem >= 3)
                        {
                            _fsm.EnterState(FSMStateType.LOSE);
                        }
                        else
                        {
                            _domino42.ResetSet();

                            _fsm.EnterState(FSMStateType.SHUFFLE);
                        }
                    }
                    else
                    {
                        _domino42.ResetRound();

                        _fsm.EnterState(FSMStateType.PLAY);
                    }
                }
            }
        }

        public override bool ExitState()
        {
            base.ExitState();

            Debug.Log("EXITING SET WINNER STATE");

            return true;
        }
    }

}