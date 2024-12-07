using System.Collections.Generic;
using Dev.CartLogic;
using UnityEngine;

namespace Dev.BotsLogic.GOAP
{
    public class BotsAI
    {


        private void Init()
        {
            var bots = new List<Bot>();

            IBotAction[] actions = new IBotAction[]{};

            BotGoal killPlayer = new BotGoal(actions, "Kill Player");
        }
        
        
    }


    public interface IBotAction
    {
        void Execute();
        bool CanExecute();
        float Cost { get; }
    }


    public class MoveToAction : BotAction<Vector3>
    {
        public MoveToAction(Vector3 data, Bot bot) : base(data, bot) { }

        public override void Execute()
        {
            Bot.Move(Data);
        }

        public override bool CanExecute()
        {
            return Bot.AllowToMove;
        }

        public override float Cost => (Bot.transform.position - Data).sqrMagnitude;
    }

    public class DragCartAction : BotAction<int>
    {
        private CartService _cartService;

        public DragCartAction(int data, Bot bot, CartService cartService) : base(data, bot)
        {
            _cartService = cartService;
        }
        
        public override void Execute()
        {
            
        }

        public override bool CanExecute() => Bot.GetTeamSide() != _cartService.DragTeamSide;

        public override float Cost { get; }
    }
    
    
    public class BotGoal
    {
        private IBotAction[] _actions;
        private int _currentGoal;
        public string GoalName { get; private set; }

        public BotGoal(IBotAction[] actions, string goalName)
        {
            GoalName = goalName;
            _actions = actions;
        }

        public IBotAction GetAction()
        {
            return _actions[_currentGoal];
        }
        
    }
    
    
    public class BotMemory
    {
        
    }
    
}