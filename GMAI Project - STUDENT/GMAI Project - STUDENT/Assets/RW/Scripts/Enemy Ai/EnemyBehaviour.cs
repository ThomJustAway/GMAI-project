using Assets.RW.Scripts;
using Assets.RW.Scripts.Enemy_Ai;
using Assets.RW.Scripts.Enemy_Ai.Enemy_FSm;
using PGGE.Patterns;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour, IDamageable
{
    //walk running idle, roaming, sleeping, Attacking, 
    [SerializeField] float walkingSpeed = 1.5f;
    [SerializeField] float runningSpeed = 3f;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] int health = 50;
    [SerializeField]Animator animator;
    [SerializeField] EnemyVision vision;
    Collider bodyCollider;
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

    [Header("Fighting")]
    [SerializeField] float minReactionTime = 0.1f;
    [SerializeField] float maxReactionTime = 0.2f;
    [SerializeField] Collider leftHand;
    [SerializeField] Collider rightHand;

    [Header("Hurting")]
    [SerializeField] float maxStunTime = 0.1f;
    [SerializeField] float minStunTime = 0.2f;
    int speedHash = Animator.StringToHash("Speed");
    int fightingHash = Animator.StringToHash("PrepareFight");
    int punchHash = Animator.StringToHash("Punch");
    int hookHash = Animator.StringToHash("Hook");
    int hurtHash = Animator.StringToHash("Hurt");
    int dieHash = Animator.StringToHash("Die");
    int defendHash = Animator.StringToHash("Block");

    [Header("Defend")]
    [Range(0, 1)]
    [SerializeField]
    float probabilityToDefend = 0.5f;
    public NavMeshAgent Agent { get => agent; }
    public float RoamingRadius { get => roamingRadius;}
    public float ProbabilityToKeepRoaming { get => probabilityToKeepRoaming; }
    public EnemyVision Vision { get => vision; }
    public float AttackingRadius { get => attackingRadius; }
    public float MinReactionTime { get => minReactionTime; set => minReactionTime = value; }
    public float MaxReactionTime { get => maxReactionTime; set => maxReactionTime = value; }
    public float RotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }
    public float MaxStunTime { get => maxStunTime; set => maxStunTime = value; }
    public float MinStunTime { get => minStunTime; set => minStunTime = value; }
    public Animator Animator { get => animator; set => animator = value; }

    //FSM
    FSM enemyBehaviour;
    private void Start()
    {
        ToggleHandCollider(false);
        agent = GetComponent<NavMeshAgent>();
        bodyCollider = GetComponent<Collider>();
        enemyBehaviour = new();
        enemyBehaviour.Add(new EnemyRoamingState(enemyBehaviour,this));
        enemyBehaviour.Add(new EnemyChasingState(enemyBehaviour,this));
        enemyBehaviour.Add(new EnemyAttackingState(enemyBehaviour,this));
        enemyBehaviour.Add(new EnemyHurtState(enemyBehaviour, this));
        enemyBehaviour.Add(new EnemyDeathState(enemyBehaviour, this));
        enemyBehaviour.Add(new EnemyDefenceState(enemyBehaviour, this));
        enemyBehaviour.SetCurrentState((int)EnemyStates.Roaming);
    }

    private void Update()
    {
        //print($"Enemy state {enemyBehaviour.GetCurrentState()}");
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

    public void SetFightingAnimation(bool value)
    {
        animator.SetBool(fightingHash, value);
    }

    public void TriggerPunch()
    {
        animator.SetTrigger(punchHash);
    }

    public void TriggerHook()
    {
        animator.SetTrigger(hookHash);
    }

    public void ToggleHandCollider(bool value)
    {
        leftHand.enabled = value;
        rightHand.enabled = value;  
    }

    public void TakeDamage(object sender, int damage)
    {
        int curId = enemyBehaviour.GetCurrentState().ID;
        if (curId != (int)EnemyStates.Hurting &&
            curId != (int)EnemyStates.Defend)
        {
            if (CanDefend())
            {
                animator.SetTrigger(defendHash);
                enemyBehaviour.SetCurrentState((int)EnemyStates.Defend);
                return;
            }//will block the attack

            health -= damage;
            if(health < 0)
            {
                health = 0;
                bodyCollider.enabled = false;
                animator.SetTrigger(dieHash);
                enemyBehaviour.SetCurrentState((int)EnemyStates.Death);
            }
            else
            {
                animator.SetTrigger(hurtHash);
                enemyBehaviour.SetCurrentState((int)EnemyStates.Hurting);
            }
        }

        bool CanDefend()
        {
            float randPos = Random.value;
            return randPos <= probabilityToDefend;
        }
    }
}
