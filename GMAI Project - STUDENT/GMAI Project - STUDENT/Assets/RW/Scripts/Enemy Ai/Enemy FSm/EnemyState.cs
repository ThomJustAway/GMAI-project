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

            if(Vector3.Distance(enemyAgent.transform.position,player.transform.position) < enemyAgent.AttackingRadius)
            {
                mFsm.SetCurrentState((int)EnemyStates.Fighting);
            }
        }

        public override void Exit()
        {
            player = null;
        }

    }

    public class EnemyAttackingState : EnemyState
    {
        EnemyVision vision;
        GameObject player;
        public EnemyAttackingState(FSM fsm, EnemyBehaviour enemyAgent) : base(fsm, enemyAgent)
        {
            mId = (int)EnemyStates.Fighting;
            vision = enemyAgent.Vision;
        }

        int playerLayerMask = LayerMask.NameToLayer("Player");
        float elapseTime = 0f;
        float reactionTime = 0f;
        public override void Enter()
        {
            //enemyAgent.ToggleHandCollider(true);
            foreach (var t in vision.visibles)
            {
                if (t.layer == playerLayerMask)
                {
                    player = t;
                    enemyAgent.SetFightingAnimation(true);
                    break;
                }
            }
            DecideOnReactionTime();
            
        }

        public override void Update()
        {
            if (!vision.visibles.Contains(player) ||
                !IsPlayerCloser())
            {
                mFsm.SetCurrentState((int)EnemyStates.Chasing);
                return;
            }
            FaceToPlayer();
            while (elapseTime < reactionTime)
            {
                elapseTime += Time.deltaTime;
                return;
            }
            //if there is no more reaction time
            DecideOnReactionTime();
            elapseTime = 0f;
            DecideOnAttack();
        }

        bool IsPlayerCloser()
        {
            return Vector3.Distance(enemyAgent.transform.position, 
                player.transform.position) <= enemyAgent.AttackingRadius;
        }

        void DecideOnReactionTime()
        {
            reactionTime = Random.Range(enemyAgent.MinReactionTime, enemyAgent.MaxReactionTime);
        }

        void FaceToPlayer()
        {
            Vector3 targetDirectionVec = player.transform.position - enemyAgent.transform.position;
            Quaternion targetDirection = Quaternion.LookRotation(targetDirectionVec);
            Quaternion curDirection = player.transform.rotation;
            player.transform.rotation = Quaternion.Lerp(curDirection, 
                targetDirection, Time.deltaTime * enemyAgent.RotationSpeed);
        }

        void DecideOnAttack()
        {
            int randAttack = Random.Range(0, 2);

            switch(randAttack)
            {
                case 0:
                    enemyAgent.TriggerPunch();
                    break;
                case 1:
                    enemyAgent.TriggerHook();
                    break;
                default:
                    enemyAgent.TriggerPunch();
                    break;
            }
        }
        
        public override void Exit()
        {
            //enemyAgent.ToggleHandCollider(false);
            enemyAgent.SetFightingAnimation(false);
            elapseTime = 0f;
            player = null;
        }
    }

    public class EnemyHurtState : EnemyState
    {
        float stunTime;
        float elapseTime;
        public EnemyHurtState(FSM fsm, EnemyBehaviour enemyAgent) : base(fsm, enemyAgent)
        {
            mId = (int)EnemyStates.Hurting;
        }

        public override void Enter()
        {
            elapseTime = 0f;
            DecideStunTime();
        }

        public override void Update()
        {
            while(elapseTime < stunTime)
            {
                elapseTime += Time.deltaTime;
                return;
            }
            mFsm.SetCurrentState(PreviousState);
        }

        void DecideStunTime()
        {
            stunTime = Random.Range(enemyAgent.MinStunTime, enemyAgent.MaxStunTime);
        }

    }

    public class EnemyDeathState : EnemyState
    {
        public EnemyDeathState(FSM fsm, EnemyBehaviour enemyAgent) : base(fsm, enemyAgent)
        {
            mId = (int)EnemyStates.Death;
        }
    }

    public enum EnemyStates
    {
        Roaming,
        Chasing,
        Fighting,
        Hurting,
        Death
    }

}