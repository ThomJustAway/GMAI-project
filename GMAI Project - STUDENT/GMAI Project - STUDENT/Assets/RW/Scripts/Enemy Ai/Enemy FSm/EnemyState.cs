using PGGE.Patterns;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.RW.Scripts.Enemy_Ai.Enemy_FSm
{
    public class EnemyState : FSMState
    {
        protected EnemyBehaviour enemyAgent;

        public EnemyState(FSM fsm, EnemyBehaviour enemyAgent) : base(fsm)
        {
            this.enemyAgent = enemyAgent;
        }
    }

    public class EnemyRoamingState : EnemyState
    {
        NavMeshAgent agent;
        
        public EnemyRoamingState(FSM fsm, EnemyBehaviour enemyAgent) : base(fsm, enemyAgent)
        {
            agent = enemyAgent.Agent;
            mId = (int)EnemyStates.Roaming;
        }

        public override void Enter()
        {
            enemyAgent.SetWalkingSpeed();
            PlanNewDestination();
        }

        public override void Update()
        {
            //change this later
            enemyAgent.DisplayMovementAnimation(0.5f);

            if (agent.remainingDistance < agent.stoppingDistance)
            {//has already reach the target destination
                if(DecideCanRoam())
                {
                    PlanNewDestination();
                }
                else
                {
                    //todo switch to a random boring state
                }
            }

        }
        
        private bool DecideCanRoam()
        {
            float randNum = Random.value;
            if(randNum <= enemyAgent.ProbabilityToKeepRoaming) 
            { 
                return true;
            }
            return false;
        }

        private void PlanNewDestination()
        {
            if(agent.hasPath)
            {
                agent.ResetPath();
            }

            NavMeshPath path = new NavMeshPath();
            while (true)
            {
                Vector3 randPos = Random.insideUnitSphere * Random.Range(1, enemyAgent.RoamingRadius);
                
                agent.CalculatePath(randPos,path);
                if(path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetPath(path);
                    return;
                }
            }

        }

    }

    

    public enum EnemyStates
    {
        Roaming,
    }

}