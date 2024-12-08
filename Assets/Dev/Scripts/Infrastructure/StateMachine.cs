using System;
using System.Collections.Generic;
using System.Linq;
using Dev.Utils;
using UniRx;

namespace Dev.Infrastructure
{
    public class StateMachine<TState> where TState : IState
    {
        private readonly Dictionary<Type, TState> _states;
        private IState _currentState;

        public Subject<string> Changed = new Subject<string>();
        public int CurrentStateId => _states.Values.ToList().IndexOf(_states[_currentState.GetType()]);

        private List<Transition> _transitions = new List<Transition>();
        

        public StateMachine(params TState[] states)
        {
            _states = states.ToDictionary(x => x.GetType());
        }

        public void ChangeState<TChangeState>(bool withOverride = false) where TChangeState : TState
        {
           // if ((_currentState != null && typeof(TChangeState) == _currentState.GetType()) || !withOverride) return;

            TState state = _states[typeof(TChangeState)];
            
            _currentState?.Exit();

            Changed.OnNext(typeof(TState).ToString());

            //AtomicLogger.Log($"Changed state to {state.GetType()}");

            _currentState = state;
 
            _currentState?.Enter();
        }

        public void FixedNetworkTick()
        {
            if(_currentState is IFixedNetworkTickState state)
                state.FixedNetworkTick();
        }

        public void Tick()
        {
            if(_currentState is ITickState tickState)
                tickState.Tick();
                
          
            /*foreach (var transition in _transitions)
            {
                if(transition.ToStateType == _currentState) continue;
                
                if(transition.IsSucceed() && transition.Priority == int.MaxValue)
                    ChangeState(transition.ToStateType);
            }*/
        }

        public void Exit()
        {
            _currentState?.Exit();
            _currentState = null;
        }

        public void AddTransition(Transition transition)
        {
            _transitions.Add(transition);
        }
        
        public void ReEnterState<TChangeState>() where TChangeState : TState
        {
            TState state = _states[typeof(TChangeState)];
            _currentState?.Exit();

            // Debug.Log($"ReEnter state to {state.GetType()}");

            _currentState = state;

            _currentState?.Enter();
        }

        public void ChangeState(IState state)
        {
            if (state == _currentState) return;

            //Debug.Log($"Changed state to {state.GetType()}");

            _currentState?.Exit();

            _currentState = (TState)state;

            _currentState?.Enter();
        }
    }

    public struct Transition
    {
        public Type ToStateType { get; }
        public int Priority { get; }
        
        private readonly Func<bool> _condition;

        public Transition(Type toStateType, Func<bool> condition, int priority)
        {   
            Priority = priority;
            _condition = condition;
            ToStateType = toStateType;
        }

        public bool IsSucceed()
        {
            return _condition.Invoke();
        }
    }
}