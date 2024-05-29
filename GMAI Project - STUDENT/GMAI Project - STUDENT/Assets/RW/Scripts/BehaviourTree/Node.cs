using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTreeImplementation
{

    /// <summary>
    /// A sequence Node is a composite node that will check all the children.
    /// It returns success if all the node can run properly. else it would stop 
    /// excuting the other node if a node runs as either running or fail.
    /// </summary>
    public class SequenceNode : CompositeNode
    {
        public SequenceNode(string name) : base(name)
        {
        }

        public override Status Execute()
        {
            Debug.Log($"At sequence {Name} node");
            //starts from left to right
            if (children == null || children?.Count == 0)
            {//check if can start the operation
                Debug.LogError($"Please create a children for {Name}");
                return Status.Failed;
            }
            //run through all the node.
            for(; currentChild < children.Count; currentChild++)
            {
                Status selectedStatus = children[currentChild].Execute();
                if (selectedStatus == Status.Success)
                {
                    if(currentChild == children.Count - 1)
                    {
                        //if its the final node, then return success
                        currentChild = 0;
                        return Status.Success;
                    }
                    continue;
                }
                else
                {
                    //else return with the coresponding status.
                    return selectedStatus;
                }
            }
            //if something wrong
            Debug.LogWarning($"Something wrong with node {Name}");
            return Status.Failed;
        }
    }

    /// <summary>
    /// A selector node is a composite node that will check all the children
    /// it has. if a child is failed to execute, then it will move on to the next children
    /// it would stop running and return the corresponding result if it hits running or success.
    /// </summary>
    public class SelectorNode : CompositeNode
    {
        public SelectorNode(string name) : base(name)
        {
        }

        public override Status Execute()
        {
            Debug.Log($"At selector {Name} node");

            //start from left to right to decide which state is good
            if (children == null || children?.Count == 0)
            {//check if can start the operation
                Debug.LogError($"Please create a children for {Name}");
                return Status.Failed;
            }
            //wil go through all the children
            for(; currentChild < children.Count; currentChild++)
            {
                var state = children[currentChild].Execute();
                if (state == Status.Failed)
                {//it fails if all the child have failed
                    if( currentChild == children.Count - 1) { return Status.Failed; }
                }
                else
                {
                    //else stop if it recieve any other state.
                    return state;
                }
            }

            Debug.LogWarning($"Something wrong with node {Name}");
            return Status.Failed;
        }

    }

    //a composite node is a node that will consist of multiple node.
    //it is a simple class for other composite nodes like selector and sequence node.
    public abstract class CompositeNode : IExecutable
    {
        protected int currentChild;
        protected List<IExecutable> children;

        protected CompositeNode(string name)
        {
            Name = name;
            this.children = new();
        }

        public string Name { get ; set ; }

        public abstract Status Execute();

        public void AddChild(IExecutable child)
        {
            children.Add(child);
        }

        public void AddChild(List<IExecutable> childList)
        {
            children = childList;
        }

        public void AddName(string name)
        {
            Name = name;
        }

        public void Reset()
        {
            currentChild = 0;
        }
    }

    //each node will implement the IExecutable 
    //as each node needs to execute in order 
    //to know what status the node is in.
    public interface IExecutable
    {
        public string Name { get; set; }

        public Status Execute();
    }    
    
    //what status the node is in. only three which are success/failed/running
    public enum Status
    {
        Failed,
        Running,
        Success
    }
}