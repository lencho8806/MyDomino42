using UnityEngine;
using System.Collections;
using Domino42;

namespace Assets.Scripts.FSM.States
{
    [CreateAssetMenu(fileName = "BidState", menuName = "Unity-FSM/States/Bid", order = 3)]
    public class BidState : AbstractFSMState
    {
        int playerIndex = -1;

        public override void OnEnable()
        {
            base.OnEnable();

            StateType = FSMStateType.BID;
        }

        public override bool EnterState()
        {
            EnteredState = base.EnterState();

            if (EnteredState)
            {
                Debug.Log("ENTERED BID STATE");

                int dealerIndex = _domino42.players.FindIndex(player => player.IsDealer);
                playerIndex = -1;
                
                for (int i = (dealerIndex + 1); i < ((dealerIndex + 1) + 4); i++)
                {
                    if (_domino42.players[i % 4].BidComplete == false)
                    {
                        playerIndex = i % 4;
                        break;
                    }
                }
                
                if (playerIndex == -1)
                {
                    // why? go to the next section
                    _fsm.EnterState(FSMStateType.TRUMP);
                }
                if (playerIndex == 0)
                {
                    // Player bid
                    _domino42.bidMenu.BidStart();
                }
                else
                {
                    // AI bid
                    //_domino42.Bid(playerIndex);
                }
            }

            return EnteredState;
        }

        public override void UpdateState()
        {
            if (EnteredState)
            {
                if (_domino42.players[playerIndex].BidAmount != null)
                {
                    if (_domino42.players.Exists(player => player.BidAmount == null))
                    {
                        _fsm.EnterState(FSMStateType.BID);
                    }
                    else
                    {
                        _fsm.EnterState(FSMStateType.TRUMP);
                    }
                }
            }
        }

        public override bool ExitState()
        {
            base.ExitState();

            Debug.Log("EXITING BID STATE");

            return true;
        }
    }

}