using UnityEngine;
using System.Collections;
using Domino42;

namespace Assets.Scripts.FSM.States
{
    [CreateAssetMenu(fileName = "PlayState", menuName = "Unity-FSM/States/Play", order = 5)]
    public class PlayState : AbstractFSMState
    {
        int playerIndex = -1;

        public override void OnEnable()
        {
            base.OnEnable();

            StateType = FSMStateType.PLAY;
        }

        public override bool EnterState()
        {
            EnteredState = base.EnterState();

            if (EnteredState)
            {
                Debug.Log("ENTERED PLAY STATE");

                playerIndex = -1;
                for (int i = _domino42.InitialPlayerTurn; i < (_domino42.InitialPlayerTurn + 4); i++)
                {
                    if (_domino42.players[i % 4].TurnComplete == false)
                    {
                        playerIndex = i % 4;
                        break;
                    }
                }

                _domino42.CurrentPlayerTurn = playerIndex;

                if (playerIndex == -1)
                {
                    //determine Game winner
                    _fsm.EnterState(FSMStateType.ROUND_WINNER);
                }
                if (playerIndex == 0)
                {
                    // Player bid
                    //_domino42.bidMenu.BidStart();
                }
                else
                {
                    // AI bid
                    _domino42.SetPlay(playerIndex);
                }
            }

            return EnteredState;
        }

        public override void UpdateState()
        {
            if (EnteredState)
            {
                if (_domino42.players[playerIndex].TurnComplete)
                {
                    if (_domino42.players.Exists(player => player.TurnComplete == false))
                    {
                        // Hand not finished
                        _fsm.EnterState(FSMStateType.PLAY);
                    }
                    else
                    {
                        _fsm.EnterState(FSMStateType.ROUND_WINNER);
                    }
                }
            }
        }

        public override bool ExitState()
        {
            base.ExitState();

            Debug.Log("EXITING PLAY STATE");

            return true;
        }
    }

}