using Assets.Scripts.FSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Domino42
{
    public enum ExecutionState
    {
        NONE,
        ACTIVE,
        COMPLETED,
        TERMINATED
    }

    public enum FSMStateType
    {
        SHUFFLE,
        DEAL,
        BID,
        TRUMP,
        PLAY,
        ROUND_WINNER,
        SET_WINNER,
        //PLAYER1_TURN,
        //PLAYER2_TURN,
        //PLAYER3_TURN,
        //PLAYER4_TURN,
        WIN,
        LOSE
    };

    public abstract class AbstractFSMState : ScriptableObject
    {
        protected FiniteStateMachine _fsm;
        protected Game _domino42;

        public ExecutionState ExecutionState { get; protected set; }
        public FSMStateType StateType { get; protected set; }
        public bool EnteredState { get; protected set; }

        public virtual void OnEnable()
        {
            ExecutionState = ExecutionState.NONE;
        }

        public virtual bool EnterState()
        {
            bool successDomine42 = true;

            ExecutionState = ExecutionState.ACTIVE;

            successDomine42 = _domino42 != null;

            return successDomine42;
        }

        public abstract void UpdateState();

        public virtual bool ExitState()
        {
            ExecutionState = ExecutionState.COMPLETED;

            return true;
        }

        public virtual void SetExecutingFSM(FiniteStateMachine fsm)
        {
            if (fsm != null)
            {
                _fsm = fsm;
            }
        }

        public virtual void SetDomine42(Game domino42)
        {
            if (domino42 != null)
            {
                _domino42 = domino42;
            }
        }
    }

}