using PGGE.Patterns;
using Player;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.RW.Scripts.Enemy_Ai.Enemy_FSm
{
    /// <summary>
    /// What a typical enemy state should.
    /// </summary>
    public class EnemyState : FSMState
    {
        protected EnemyBehaviour enemyAgent;

        public EnemyState(FSM fsm, EnemyBehaviour enemyAgent) : base(fsm)
        {
            this.enemyAgent = enemyAgent;
        }
    }
    /// <summary>
    /// A state where the enemy would roam around
    /// as it does not have anything to do.
    /// </summary>
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
            //make sure that the enemy is set to walking.
            enemyAgent.SetWalkingSpeed();
            //decide on the destination.
            PlanNewDestination();
        }

        public override void Update()
        {
            //display the movement animation
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
                    //enable chase state if see player
                    mFsm.SetCurrentState((int)EnemyStates.Chasing);
                    return;
                }
            }

            if (agent.remainingDistance < agent.stoppingDistance)
            {//has already reach the target destination
                PlanNewDestination();
            }

            
        }
        
        //private bool DecideCanRoam()
        //{
        //    float randNum = Random.value;
        //    if(randNum <= enemyAgent.ProbabilityToKeepRoaming) 
        //    { 
        //        return true;
        //    }
        //    return false;
        //}

        private void PlanNewDestination()
        {
            if(agent.hasPath)
            {
                //make sure that the agent has no more path ah
                agent.ResetPath();
            }
            
            NavMeshPath path = new NavMeshPath();
            while (true)
            {
                //will repeat this until a valid point has been found
                Vector3 randPos = Random.insideUnitSphere * Random.Range(1, enemyAgent.RoamingRadius);
                
                //calculate if the point is okay.
                agent.CalculatePath(randPos,path);
                if(path.status == NavMeshPathStatus.PathComplete)
                {
                    //if okay then set that as the path.
                    agent.SetPath(path);
                    return;
                }
            }

        }

    }
    /// <summary>
    /// Chase state is when the enemy found the player
    /// </summary>
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
            //make sure the navmeshagent is at running speed
            enemyAgent.SetRunningSpeed();
            foreach(var t in vision.visibles)
            {
                if (t.layer == playerLayerMask)
                {
                    //retrieve the player from the enemy vision so that
                    //it can start chasing.
                    player = t;
                    break;
                }
            }
        }

        public override void Update() 
        {
            if (!vision.visibles.Contains(player))
            {
                //if the player is not there anymore, then go back to roaming.
                mFsm.SetCurrentState((int)EnemyStates.Roaming);
                return;
            }

            //else consistently set teh destination to the player destination.
            agent.SetDestination(player.transform.position);
            //show that the enemy is chasing the player.
            enemyAgent.DisplayMovementAnimation(1f);

            //if the enemy is close to the player, go to fighting stance to start fighting the enemy.
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
    /// <summary>
    /// Attacking state is when the enemy goes face to face with the player
    /// and box
    /// </summary>
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
            
            //when enter, get the player to know the player location
            foreach (var t in vision.visibles)
            {
                if (t.layer == playerLayerMask)
                {
                    player = t;
                    enemyAgent.SetFightingAnimation(true);
                    break;
                }
            }
            //afterwards, decide on how long the enemy will react.
            DecideOnReactionTime();
            
        }

        public override void Update()
        {
            if (!vision.visibles.Contains(player) ||
                !IsPlayerCloser())
            {
                //if the player is not close or not there, just go chase and see if it go resume to fighting state
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
            //a simple function to make sure that the enemy always face the player.
            Vector3 targetDirectionVec =  player.transform.position - enemyAgent.transform.position ;
            Quaternion targetDirection = Quaternion.LookRotation(targetDirectionVec);
            Quaternion curDirection = enemyAgent.transform.rotation;
            enemyAgent.transform.rotation = Quaternion.Lerp(curDirection, 
                targetDirection, Time.deltaTime * enemyAgent.RotationSpeed);
        }

        //will randomly select a punch to attack the player.
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
            //stop the enemy from being in the fighting animation
            enemyAgent.SetFightingAnimation(false);
            elapseTime = 0f;
            player = null;
        }
    }
    /// <summary>
    /// Hurt state is when the player take damage from the player. It will
    /// countdown how long the enemy should be stun for once it takes damage.
    /// </summary>
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
            }//do a countdown on the decide cool down.
            mFsm.SetCurrentState(PreviousState);
        }
        //randomly decide how long the stun should be.
        void DecideStunTime()
        {
            stunTime = Random.Range(enemyAgent.MinStunTime, enemyAgent.MaxStunTime);
        }

    }
    /// <summary>
    /// death state is just a empty state when the player kills the enemy.
    /// </summary>
    public class EnemyDeathState : EnemyState
    {
        public EnemyDeathState(FSM fsm, EnemyBehaviour enemyAgent) : base(fsm, enemyAgent)
        {
            mId = (int)EnemyStates.Death;
        }
    }
    /// <summary>
    /// Defense state is when the player manage to defend the attack if the player
    /// attack the enemy.
    /// </summary>
    public class EnemyDefenceState : EnemyState
    {
        Animator anim;
        public EnemyDefenceState(FSM fsm, EnemyBehaviour enemyAgent) : base(fsm, enemyAgent)
        {
            mId = (int)EnemyStates.Defend;
            anim = enemyAgent.Animator;
        }
        public override void Update()
        {
            //wait for the animation to complete
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Block") &&
                anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f)
            {
                //once complete, goes back to the original state it was in.
                mFsm.SetCurrentState(PreviousState);
            }
        }
    }

    public enum EnemyStates
    {
        Roaming,
        Chasing,
        Fighting,
        Hurting,
        Death,
        Defend
    }

}