using Player;
using RayWenderlich.Unity.StatePatternInUnity;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Printing;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

//This is found in this video: https://www.youtube.com/watch?v=lusROFJ3_t8&pp=ygUYZ2l0IGFtZW5kIGJlaGF2aW91ciB0cmVl
namespace Pathfinding.BehaviourTrees {
    public interface IStrategy {
        Node.Status Process();

        void Reset() {
            // Noop
        }
    }

    public class ActionStrategy : IStrategy {
        readonly Action doSomething;
        
        public ActionStrategy(Action doSomething) {
            this.doSomething = doSomething;
        }
        
        public Node.Status Process() {
            doSomething();
            return Node.Status.Success;
        }
    }
    /// <summary>
    /// a simple condition node that check a condition
    /// return success if true. else fail.
    /// </summary>
    public class Condition : IStrategy {
        readonly Func<bool> predicate;
        
        public Condition(Func<bool> predicate) {
            this.predicate = predicate;
        }
        
        public Node.Status Process() => predicate() ? Node.Status.Success : Node.Status.Failure;
    }
    //not used dont look
    public class PatrolStrategy : IStrategy
    {
        readonly Transform entity;
        readonly NavMeshAgent agent;
        readonly List<Transform> patrolPoints;
        readonly float patrolSpeed;
        int currentIndex;
        bool isPathCalculated;

        public PatrolStrategy(Transform entity, NavMeshAgent agent, List<Transform> patrolPoints, float patrolSpeed = 2f)
        {
            this.entity = entity;
            this.agent = agent;
            this.patrolPoints = patrolPoints;
            this.patrolSpeed = patrolSpeed;
        }

        public Node.Status Process()
        {
            if (currentIndex == patrolPoints.Count) return Node.Status.Success;

            var target = patrolPoints[currentIndex];
            agent.SetDestination(target.position);
            entity.LookAt(target.position.With(y: entity.position.y));

            if (isPathCalculated && agent.remainingDistance < 0.1f)
            {
                currentIndex++;
                isPathCalculated = false;
            }

            if (agent.pathPending)
            {
                isPathCalculated = true;
            }

            return Node.Status.Running;
        }

        public void Reset() => currentIndex = 0;
    }
    //not used dont look
    public class MoveToTarget : IStrategy {
        readonly Transform entity;
        readonly NavMeshAgent agent;
        readonly Transform target;
        bool isPathCalculated;

        public MoveToTarget(Transform entity, NavMeshAgent agent, Transform target) {
            this.entity = entity;
            this.agent = agent;
            this.target = target;
        }

        public Node.Status Process() {
            if (Vector3.Distance(entity.position, target.position) < 1f) {
                return Node.Status.Success;
            }
            
            agent.SetDestination(target.position);
            entity.LookAt(target.position.With(y:entity.position.y));

            if (agent.pathPending) {
                isPathCalculated = true;
            }
            return Node.Status.Running;
        }

        public void Reset() => isPathCalculated = false;
    }
    /// <summary>
    /// A node that will complete once it reach a planned point
    /// </summary>
    public class WanderAround : IStrategy
    {
        readonly Transform entity;
        readonly NavMeshAgent agent;
        readonly NormalNPCBehaviour merton;
        Vector3 targetPosition;

        bool isPathCalculated; 
        float wanderRadius = 10f;

        public WanderAround(Transform entity, NavMeshAgent agent, NormalNPCBehaviour merton)
        {
            this.entity = entity;
            this.agent = agent;
            CreateTargetPosition();
            this.merton = merton;
        }

        public Node.Status Process()
        {
            merton.SetToWalkingSpeed();
            if (Vector3.Distance(entity.position, targetPosition) < agent.stoppingDistance)
            {
                //once it reach a destination, create a new destination and wait
                //and see if the creature should continue to wander around.
                CreateTargetPosition();
                return Node.Status.Success;
            }
            
            Debug.DrawLine(entity.position, targetPosition,Color.red);
            //consistently set the destination to the target position (since we dont know if we will continue
            //this action
            agent.SetDestination(targetPosition);
            //entity.LookAt(targetPosition.With(y: entity.position.y));
            merton.PlayWalkingAnimation(targetPosition);

            if (agent.pathPending)
            {
                isPathCalculated = true;
            }
            return Node.Status.Running;
        }

        void CreateTargetPosition() 
        { 
            NavMeshPath path = new NavMeshPath();
            while (true)
            {//will consistently check if a point within the wandering radius can
                Vector3 randPos = UnityEngine.Random.insideUnitSphere *
                    UnityEngine.Random.Range(1f, wanderRadius);
                //get a radius
                //check if the path is viable
                agent.CalculatePath(randPos, path);
                if(path.status == NavMeshPathStatus.PathComplete)
                {//if it is, set the target position to that position.
                    
                    targetPosition = randPos.With(y: entity.position.y);
                    return;
                }
            }
        }

        public void Reset()
        {
            isPathCalculated = false;
            merton.StopmovementAnimation();

        }
    }
    /// <summary>
    /// Chase player is when the merton is angry at the player.
    /// </summary>
    public class ChasePlayer : IStrategy
    {
        readonly Transform entity;
        readonly Transform player;
        readonly NavMeshAgent agent;
        readonly NormalNPCBehaviour merton;

        bool isPathCalculated;
        float wanderRadius = 10f;

        public ChasePlayer(Transform entity, NavMeshAgent agent, Transform player, NormalNPCBehaviour merton)
        {
            this.entity = entity;
            this.agent = agent;
            this.merton = merton;
            this.player = player;
        }

        public Node.Status Process()
        {
            //will make sure it would set the navmesh agent speed to the approriate settings.
            merton.SetToRunningSpeed();

            if (Vector3.Distance(entity.position, player.position) < 2f)
            {//if the merton is close to the player, return success
                return Node.Status.Success;
            }

            //show that the merton is running toward the player.
            merton.PlayRunningAnimation(player.position);
            //continuously set the destination to the player destination
            agent.SetDestination(player.position);
            if (agent.pathPending)
            {
                isPathCalculated = true;
            }
            return Node.Status.Running;
        }


        public void Reset()
        {
            isPathCalculated = false;
            merton.StopmovementAnimation();

        }
    }

    /// <summary>
    /// the wave leaf node would wave back to the player
    /// if the player wave. else it would just do it own thing.
    /// </summary>
    public class Wave : IStrategy
    {
        readonly Character player;
        readonly Transform entity;
        readonly Animator animator;
        readonly NormalNPCBehaviour npc;
        bool hasWave = false;
        public Wave(Character player, Transform entity, Animator animator , NormalNPCBehaviour npc)
        {
            this.player = player;
            this.entity = entity;
            this.animator = animator;
            this.npc = npc;
        }

        public Node.Status Process()
        {
            if(player.subStatePlayerState.ID == (int)Substate.Wave &&
            Vector3.Distance(player.transform.position, entity.position) < 10f &&
            !npc.IsMadAtPlayer
            ) 
            {//if the player is nearby and wave at merton, it would wave back
                if (!hasWave)
                {
                    hasWave = true;
                    animator.SetTrigger("Wave");
                }
                return Node.Status.Running;
            }
            else
            {
                hasWave = false;
                return Node.Status.Failure;
            }

        }
    }
    /// <summary>
    /// Will wait a certain amount of seconds
    /// before return success. return running while running.
    /// </summary>
    public class WaitStrategy : IStrategy
    {
        float elapseTime = 0;
        float duration;

        public WaitStrategy(float duration)
        {
            this.duration = duration;
        }

        public Node.Status Process()
        {
            while (elapseTime < duration)
            {
                elapseTime += Time.deltaTime;
                return Node.Status.Running;
            }
            elapseTime = 0f;
            return Node.Status.Success;
        }
    }

    /// <summary>
    /// a leaf node that will wait for the attack animation to finish
    /// </summary>
    public class WaitForAttackToFinish : IStrategy
    {
        Animator animator;
        MertonAttack weapon;
        int weaponHash = Animator.StringToHash("Stab");

        public WaitForAttackToFinish(Animator animator, MertonAttack weapon)
        {
            this.animator = animator;
            this.weapon = weapon;
        }

        public Node.Status Process()
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Stab Attack"))
            {
                //if the attack animation failed, then reset the hit box and return success
                weapon.SetHitBox(false);
                return Node.Status.Success;
            }
            //else continue to wait.
            return Node.Status.Running;

        }

        public void Reset()
        {
            weapon.SetHitBox(false);
            animator.ResetTrigger(weaponHash);
        }
    }
}
