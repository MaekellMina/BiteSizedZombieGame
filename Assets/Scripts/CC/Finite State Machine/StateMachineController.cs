using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Object = System.Object;

namespace cc.FiniteStateMachine
{
    public class StateMachineController
    {
        private IState _currentState;

        private Dictionary<Type, List<Transition>> _transitions = new Dictionary<Type, List<Transition>>();
        private List<Transition> _currentTransitionList = new List<Transition>();
        private List<Transition> _anyTransitionList = new List<Transition>();


        private static List<Transition> EmptyTransitionList = new List<Transition>(0);

        public IState CurrentState { get => _currentState; set => _currentState = value; }

        public void Tic()
        {
            var transition = GetTransition();
            if (transition != null)
                SetState(transition.To);

            _currentState?.Tic();
                
        }

        public void PhysicsTic()
        {
            IPlayerState physicsState = (IPlayerState)_currentState;
            if (physicsState!=null)
                physicsState?.OnPhysicsTic();
        }

        public void SetState(IState state)
        {
            if (state == _currentState) return;

            _currentState?.OnExit();
            _currentState = state;

            _transitions.TryGetValue(_currentState.GetType(), out _currentTransitionList);
            if (_currentTransitionList == null) _currentTransitionList = EmptyTransitionList;

            _currentState.OnEnter();
        }


        public void AddTransition(IState from, IState to, Func<bool> predicate)
        {
            if(_transitions.TryGetValue(from.GetType(),out var transitions) == false)
            {
                transitions = new List<Transition>();
                _transitions[from.GetType()] = transitions;

            }

            transitions.Add(new Transition(to, predicate));

        }
        public void AddTransition(IPlayerState from, IPlayerState to, Func<bool> predicate)
        {
            if (_transitions.TryGetValue(from.GetType(), out var transitions) == false)
            {
                transitions = new List<Transition>();
                _transitions[from.GetType()] = transitions;

            }

            transitions.Add(new Transition(to, predicate));

        }


        public void AddAnyTransition(IState state, Func<bool> predicate)
        {
            _anyTransitionList.Add(new Transition(state, predicate));
        }
        public void AddAnyTransition(IPlayerState state, Func<bool> predicate)
        {
            _anyTransitionList.Add(new Transition(state, predicate));
        }
        public bool IsCurrentState(IState stateToCheck)
        {
            return (_currentState == stateToCheck) ;
        }

        #region Transition Class
        private class Transition
        {
            public Func<bool> Condition { get; }
            public IState To { get; }

            public Transition(IState to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }



        }

        #endregion

        private Transition GetTransition()
        {
            foreach (var transition in _anyTransitionList)
            {
                if (transition.Condition())
                    return transition;
            }

            foreach (var transition in _currentTransitionList)
            {
                if (transition.Condition())
                    return transition;
            }

            return null;
        }
    }
}

