using System;

namespace Dev.BotsLogic.GOAP
{
    public class GoapGoal
    {
        private GoapAction[] _actions;

        public GoapGoal()
        {
            var patrolAction = new GoapAction(bot: new Bot(), enterCondition: new GoapCondition(() =>
            {
                
                return false;
            }), action: bot =>
            {
                
            });
            
        }

        public void Tick()
        {
            foreach (var goapAction in _actions)
            {
                if (goapAction.IsActive)
                {
                   // goapAction.FinishCondition
                }
            }
        }
        
        
    }


    public class GoapAction
    {
        private Bot _bot;
        public GoapCondition EnterCondition { get; set; }
        
        public int Cost { get; private set; }
        
        
        public GoapCondition FinishCondition { get; set; }
        
        public bool IsActive { get; private set; }
        
        public bool IsFinished { get; private set; }

        public GoapAction(Bot bot, GoapCondition enterCondition, Action<Bot> action)
        {
            EnterCondition = enterCondition;
            _bot = bot;
        }

        public void OnStart()
        {
            IsActive = true;
            IsFinished = false;
        }
        
        public void CheckCondition()
        {
            if (EnterCondition.IsSucceed.Invoke()) 
                OnConditionSucceed(_bot);
        }

        public void CheckFinishCondition()
        {
            if (IsActive && FinishCondition.IsSucceed.Invoke())
                FinishAction();
        }
        
        protected void FinishAction()
        {
            IsFinished = true;
        }

        public void OnEnd()
        {
            IsActive = true;
        }

        protected virtual void OnConditionSucceed(Bot bot)
        {
            //Action?.Invoke(_bot);
        }
        
    }


    public class State
    {

        

        public enum StatType
        {
            Int,
            String
        }
    }


    public class GoapCondition
    {
        public Func<bool> IsSucceed { get; set; }

        public GoapCondition(Func<bool> isSucceed)
        {
            IsSucceed = isSucceed;
        }
    }
    
}