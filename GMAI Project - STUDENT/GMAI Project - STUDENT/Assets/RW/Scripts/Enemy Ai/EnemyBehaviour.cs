using Assets.RW.Scripts.Enemy_Ai;
using Assets.RW.Scripts.Enemy_Ai.Enemy_FSm;
using PGGE.Patterns;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    //walk running idle, roaming, sleeping, Attacking, 
    [SerializeField] float walkingSpeed = 1.5f;
    [SerializeField] float runningSpeed = 3f;
    [SerializeField]Animator animator;
    [SerializeField] EnemyVision vision;
    NavMeshAgent agent;
    [Header("Roaming")]
    [SerializeField]
    float roamingRadius = 10f;
    [Range(0,1f)]
    [SerializeField]
    float probabilityToKeepRoaming = 1f;

    [Header("Chasing")]
    [SerializeField]
    float attackingRadius = 1f;

    int speedHash = Animator.StringToHash("Speed");
    public NavMeshAgent Agent { get => agent; }
    public float RoamingRadius { get => roamingRadius;}
    public float ProbabilityToKeepRoaming { get => probabilityToKeepRoaming; }
    public EnemyVision Vision { get => vision; }
    public float AttackingRadius { get => attackingRadius; }

    //FSM
    FSM enemyBehaviour;
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyBehaviour = new();
        enemyBehaviour.Add(new EnemyRoamingState(enemyBehaviour,this));
        enemyBehaviour.Add(new EnemyChasingState(enemyBehaviour,this));
        enemyBehaviour.SetCurrentState((int)EnemyStates.Roaming);
    }

    private void Update()
    {
        enemyBehaviour.Update();
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, roamingRadius);
    }

    public void DisplayMovementAnimation(float value)
    {
        animator.SetFloat(speedHash, value);
    }

    public void SetWalkingSpeed()
    {
        agent.speed = walkingSpeed;    
    }

    public void SetRunningSpeed()
    {
        agent.speed = runningSpeed;
    }

}
