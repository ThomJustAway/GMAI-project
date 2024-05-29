using System;


namespace BehaviourTreeImplementation
{
    //what kind of actions the leaf can take.
    public interface IAction
    {
        Status Action();
        public void Reset()
        {
            //noop
        }
    }
    
    /// <summary>
    /// This make the leaf node check for condition
    /// will return success if it manage to condition satify
    /// else will return a fail result (could be running or failed).
    /// </summary>
    public class Condition : IAction
    {
        private Func<bool> completionCondition;
        private Status failResult;
        public Condition(Func<bool> completionCondition)
        {
            this.completionCondition = completionCondition;
            failResult = Status.Failed;
        }

        public Condition(Func<bool> completionCondition, Status failResult)
        {
            this.completionCondition = completionCondition;
            this.failResult = failResult;
        }

        public Status Action()
        {
            if(completionCondition()) { return Status.Success; }
            return failResult;
        }
    }
    /// <summary>
    /// custom function if the feature
    /// is quite simple and doesn't need a class.
    /// </summary>
    public class CustomFunc : IAction
    {
        private Func<Status> customFunc;

        public CustomFunc(Func<Status> customFunc)
        {
            this.customFunc = customFunc;
        }

        public Status Action()
        {
            return customFunc();
        }
    }
    /// <summary>
    /// A simple leaf node that just return success until a certain
    /// period of time has pass.
    /// </summary>
    public class WaitFor : IAction
    {
        private float elapseTime;
        private float waitTime;

        public WaitFor(float waitTime)
        {
            this.elapseTime = 0;
            this.waitTime = waitTime;
        }

        public Status Action()
        {
            if(elapseTime >= waitTime)
            {
                elapseTime = 0;
                return Status.Success;
            }
            elapseTime += waitTime;
            return Status.Running;
        }
    }
}