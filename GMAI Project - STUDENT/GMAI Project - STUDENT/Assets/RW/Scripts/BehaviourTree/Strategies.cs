using Player;
using RayWenderlich.Unity.StatePatternInUnity;
using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

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

    public class Condition : IStrategy {
        readonly Func<bool> predicate;
        
        public Condition(Func<bool> predicate) {
            this.predicate = predicate;
        }
        
        public Node.Status Process() => predicate() ? Node.Status.Success : Node.Status.Failure;
    }

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
            if (Vector3.Distance(entity.position, targetPosition) < agent.stoppingDistance)
            {
                CreateTargetPosition();
                return Node.Status.Success;
            }
            
            Debug.DrawLine(entity.position, targetPosition,Color.red);

            agent.SetDestination(targetPosition);
            entity.LookAt(targetPosition.With(y: entity.position.y));
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
            {
                Vector3 randPos = UnityEngine.Random.insideUnitSphere *
                    UnityEngine.Random.Range(1f, wanderRadius);
                //get a radius
                agent.CalculatePath(randPos, path);
                if(path.status != NavMeshPathStatus.PathInvalid)
                {
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

    public class Wave : IStrategy
    {
        readonly Character player;
        readonly Transform entity;
        readonly Animator animator;
        bool hasWave = false;

        public Wave(Character player, Transform entity, Animator animator)
        {
            this.player = player;
            this.entity = entity;
            this.animator = animator;
        }

        public Node.Status Process()
        {
            if(player.currentPlayerState.ID == (int)MainState.Wave &&
            Vector3.Distance(player.transform.position, entity.position) < 10f
            )
            {
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
                return Node.Status.Success;
            }

        }
    }

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

}
