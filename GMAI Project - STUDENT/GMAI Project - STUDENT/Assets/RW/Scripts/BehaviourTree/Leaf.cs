using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTreeImplementation
{
    /// <summary>
    /// Leaf node is a node that will run a singluar action
    /// It takes in an IAction to know what kind of action the
    /// leaf is going to do.
    /// </summary>
    public class Leaf : IExecutable
    {
        public string Name { get; set; }
        public IAction actions;

        public Leaf(string name, IAction actions)
        {
            this.actions = actions;
            Name = name;
        }


        public Status Execute()
        {
            //for debuggin
            Debug.Log($"At leaf {Name}");

            return actions.Action();
        }
    }

}