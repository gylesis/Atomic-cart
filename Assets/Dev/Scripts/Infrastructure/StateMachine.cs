using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Dev.Infrastructure
{
    public class StateMachine<TState> where TState : IState
    {
        private readonly Dictionary<Type, TState> _states;
        private IState _current;

        public Subject<string> Changed = new Subject<string>();

        public StateMachine(params TState[] states)
        {
            _states = states.ToDictionary(x => x.GetType());
        }

        public void ChangeState<TChangeState>() where TChangeState : TState
        {
            TState state = _states[typeof(TChangeState)];
            _current?.Exit();

            Changed.OnNext(typeof(TState).ToString());

//            Debug.Log($"Changed state to {state.GetType()}");

            _current = state;

            _current?.Enter();
        }

        public int CurrentStateId => _states.Values.ToList().IndexOf(_states[_current.GetType()]);

        public void Exit()
        {
            _current?.Exit();
            _current = null;
        }

        public void ReEnterState<TChangeState>() where TChangeState : TState
        {
            TState state = _states[typeof(TChangeState)];
            _current?.Exit();

            // Debug.Log($"ReEnter state to {state.GetType()}");

            _current = state;

            _current?.Enter();
        }

        public void ChangeState(IState state)
        {
            if (state == _current) return;

            //Debug.Log($"Changed state to {state.GetType()}");

            _current?.Exit();

            _current = (TState)state;

            _current?.Enter();
        }
    }

    public struct Transition
    {
        public IState To { get; }

        private readonly Func<bool> _condition;

        public Transition(IState to, Func<bool> condition)
        {
            _condition = condition;
            To = to;
        }

        public bool IsSucceed()
        {
            return _condition.Invoke();
        }
    }
}