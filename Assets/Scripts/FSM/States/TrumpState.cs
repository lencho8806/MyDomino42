using UnityEngine;
using System.Collections;
using System.Linq;
using Domino42;

namespace Assets.Scripts.FSM.States
{
    [CreateAssetMenu(fileName = "TrumpState", menuName = "Unity-FSM/States/Trump", order = 4)]
    public class TrumpState : AbstractFSMState
    {
        int playerBidIndex = -1;

        public override void OnEnable()
        {
            base.OnEnable();

            StateType = FSMStateType.TRUMP;
        }

        public override bool EnterState()
        {
            EnteredState = base.EnterState();

            if (EnteredState)
            {
                Debug.Log("ENTERED TRUMP STATE");

                int maxBid = -100; // as to not interfere with pass (-1)
                playerBidIndex = -1;
                for (int i = 0; i < _domino42.players.Count; i++)
                {
                    if (_domino42.players[i].BidAmount > maxBid)
                    {
                        maxBid = _domino42.players[i].BidAmount.Value;
                        playerBidIndex = i;
                    }
                }

                _domino42.CurrentBidAmount = maxBid;
                _domino42.WhoBid = playerBidIndex;

                if (playerBidIndex == -1)
                {
                    // why? go to the next section
                    _fsm.EnterState(FSMStateType.TRUMP);
                }
                if (playerBidIndex == 0)
                {
                    // Player trump
                    _domino42.trumpMenu.TrumpStart();
                }
                else
                {
                    // AI trump
                    _domino42.SetTrump(playerBidIndex);
                }
            }

            return EnteredState;
        }

        public override void UpdateState()
        {
            if (EnteredState)
            {
                if (_domino42.players[playerBidIndex].Trump != null)
                {
                    _domino42.InitialPlayerTurn = playerBidIndex;

                    _fsm.EnterState(FSMStateType.PLAY);
                }
            }
        }

        public override bool ExitState()
        {
            base.ExitState();

            Debug.Log("EXITING TRUMP STATE");

            return true;
        }
    }

}