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
        EnemyVision vision;
        public EnemyRoamingState(FSM fsm, EnemyBehaviour enemyAgent) : base(fsm, enemyAgent)
        {
            vision = enemyAgent.Vision;
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


            DecideNextState();
        }


        int playerLayerMask = LayerMask.NameToLayer("Player");
        private void DecideNextState()
        {
            //find if player is nearby
            foreach (var t in vision.visibles)
            {
                if(t.layer == playerLayerMask)
                {
                    //enable chase scene
                    mFsm.SetCurrentState((int)EnemyStates.Chasing);
                    return;
                }
            }

            if (agent.remainingDistance < agent.stoppingDistance)
            {//has already reach the target destination
                if (DecideCanRoam())
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

    public class EnemyChasingState : EnemyState
    {
        NavMeshAgent agent;
        EnemyVision vision;
        GameObject player;

        public EnemyChasingState(FSM fsm, EnemyBehaviour enemyAgent) : base(fsm, enemyAgent)
        {
            vision = enemyAgent.Vision;
            agent = enemyAgent.Agent;
            mId = (int)EnemyStates.Chasing;
        }

        int playerLayerMask = LayerMask.NameToLayer("Player");


        public override void Enter()
        {
            enemyAgent.SetRunningSpeed();
            foreach(var t in vision.visibles)
            {
                if (t.layer == playerLayerMask)
                {
                    player = t; break;
                }
            }

        }

        public override void Update() 
        {
            if (!vision.visibles.Contains(player))
            {
                mFsm.SetCurrentState((int)EnemyStates.Roaming);
                return;
            }

            agent.SetDestination(player.transform.position);
            enemyAgent.DisplayMovementAnimation(1f);
        }

        public override void Exit()
        {
            player = null;
        }

    }

    public class EnemyAttackingState : EnemyState
    {


        public EnemyAttackingState(FSM fsm, EnemyBehaviour enemyAgent) : base(fsm, enemyAgent)
        {
        }
    }

    public enum EnemyStates
    {
        Roaming,
        Chasing
    }

}