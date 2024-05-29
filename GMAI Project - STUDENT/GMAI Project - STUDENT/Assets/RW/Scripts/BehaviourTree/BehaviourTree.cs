using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace BehaviourTreeImplementation
{
    //store the node and leafs
    //inspired by https://www.youtube.com/watch?v=lusROFJ3_t8
    public class BehaviourTree
    {
        //the starting node should be the 
        List<IExecutable> mainBranch;
        //can switch the process of the behaviour tree.
        private SelectionProcess process;
        public delegate void SelectionProcessDelegate();
        public SelectionProcessDelegate Update;
        public enum SelectionProcess
        {
            Sequence,
            Selector
        }

        public BehaviourTree()
        {
            this.mainBranch = new List<IExecutable>();
        }

        public void AddBranch(IExecutable branch)
        {
            mainBranch.Add(branch);
        }

        public void AddBranch(List<IExecutable> branch)
        {
            mainBranch = branch;
        }

        /// <summary>
        /// Will decide if the behaviour tree would decide
        /// its node using sequence or selector.
        /// </summary>
        /// <param name="process">What process the leaf selection should do</param>
        public void SetUpSelection(SelectionProcess process = SelectionProcess.Selector)
        {
            //will decide what decision this behaviour tree should do. sequence or selection
            this.process = process;
            if(process == SelectionProcess.Sequence)
            {
                Update = Sequence;
            }
            else
            {
                Update = Selection;
            }
        }
       
        /// <summary>
        /// For sequence it would run through all the node. if it execute
        /// as success then it will move on to the next node. as it would stop
        /// once it hits running or failed.
        /// </summary>
        private void Sequence()
        {
            for (int i = 0; i < mainBranch.Count; i++)
            {
                Debug.Log("behaviour tree -->");
                var status = mainBranch[i].Execute();
                if (status == Status.Success)
                {
                    continue;
                }
                else
                {
                    return;
                }
            }
        }

        /// <summary>
        /// For selection, it would run through all the node. If it execute 
        /// and runs fail, it would move on to another node to determine if it can execute
        /// the node. it would stop if it hits success or running.
        /// </summary>
        private void Selection()
        {
            for (int i = 0; i < mainBranch.Count; i++)
            {
                var status = mainBranch[i].Execute();
                if (status == Status.Failed)
                {
                    continue;
                }
                else
                {
                    return;
                }
            }
        }
    }
}