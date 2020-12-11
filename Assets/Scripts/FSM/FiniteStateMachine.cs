using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Domino42;

namespace Assets.Scripts.FSM
{
    public class FiniteStateMachine : MonoBehaviour
    {
        AbstractFSMState _currentState;
        public AbstractFSMState CurrentState { get { return _currentState; } }

        [SerializeField]
        List<AbstractFSMState> _vallidStates;
        Dictionary<FSMStateType, AbstractFSMState> _fsmStates;

        public void Awake()
        {
            _currentState = null;

            _fsmStates = new Dictionary<FSMStateType, AbstractFSMState>();

            Game domino42 = this.GetComponent<Game>();
            
            foreach (AbstractFSMState state in _vallidStates)
            {
                state.SetExecutingFSM(this);
                state.SetDomine42(domino42);
                _fsmStates.Add(state.StateType, state);
            }
        }

        public void Start()
        {
            EnterState(FSMStateType.SHUFFLE);
        }

        public void Update()
        {
            if (_currentState != null)
            {
                _currentState.UpdateState();
            }
        }

        #region STATE MANAGEMNET

        public void EnterState(AbstractFSMState nextState)
        {
            if (nextState == null) return;

            if (_currentState != null)
            {
                _currentState.ExitState();
            }

            _currentState = nextState;

            _currentState.EnterState();
        }

        public void EnterState(FSMStateType stateType)
        {
            if (_fsmStates.ContainsKey(stateType))
            {
                AbstractFSMState nextState = _fsmStates[stateType];

                EnterState(nextState);
            }
        }

        #endregion 
    }
}
