using Assets.RW.Scripts;
using Pathfinding.BehaviourTrees;
using Player;
using RayWenderlich.Unity.StatePatternInUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NormalNPCBehaviour : MonoBehaviour , IDamageable
{
    //[SerializeField]
    //float searchRadius;
    [SerializeField]
    Character player;
    NavMeshAgent agent;
    BehaviourTree tree;
    Animator animator;
    BoxCollider attackHitBox;

    [Header("For movement")]
    [SerializeField]
    float walkSpeed;
    [SerializeField]
    float runSpeed;
    [SerializeField]
    int health = 10;
    bool isDead => health <= 0;

    [Header("For attack")]
    [SerializeField]
    MertonAttack weapon;

    public bool IsMadAtPlayer { get => isMadAtPlayer; set => isMadAtPlayer = value; }

    bool hasTakenDamage = false;
    bool isMadAtPlayer = false;

    //public float SearchRadius { get => searchRadius; set => searchRadius = value; }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        tree = new BehaviourTree("Metalon BT");

        var action = new PrioritySelector("Actions");

        //for interacting with the player
        //var interactWithPlayer = new Selector("interact with player" , 100);
        //interactWithPlayer.AddChild(new Leaf("Check if player wave", new Condition(
        //    () => player.currentPlayerState.ID == (int)MainState.Wave &&
        //    Vector3.Distance(player.transform.position, transform.position) < 10f
        //    )));
        //interactWithPlayer.AddChild(new Leaf("wave player",new Wave(player,transform,animator) ));
        var deathSequence = new Sequence("death sequence",150);
        deathSequence.AddChild(new Leaf("IsDead", new Condition(() => isDead)));
        deathSequence.AddChild(new Leaf("Do Death animation", new ActionStrategy(
            () => {
                animator.ResetTrigger("Take Damage");
                animator.SetTrigger("Die");
                agent.ResetPath();
                tree = new BehaviourTree("Nothing");

                tree.AddChild(new Leaf("nothing", new ActionStrategy(() => { })));
            })));
        action.AddChild(deathSequence);

        var aggroSequence = new Sequence("Aggro", 125);
        aggroSequence.AddChild(new Leaf("is mad at player", new Condition(() => IsMadAtPlayer)));
        var beCloseToPlayer = new PrioritySelector("Close to player");
        beCloseToPlayer.AddChild(new Leaf("is close to player", new Condition(() =>
        Vector3.Distance(transform.position, player.transform.position) < 2f)));
        //if this fail, make sure to move close to the player
        beCloseToPlayer.AddChild(new Leaf("chase player", new ChasePlayer(transform, agent, player.transform, this)));
        aggroSequence.AddChild(beCloseToPlayer);
        aggroSequence.AddChild(new Leaf("Attack Player", new ActionStrategy(() =>
        {
            animator.SetTrigger(weaponHash);
            weapon.SetHitBox(true);
        })));
        aggroSequence.AddChild(new Leaf("wait for attack to end", new WaitForAttackToFinish(animator, weapon)));

        action.AddChild(aggroSequence);


        action.AddChild(new Leaf("Wave at player", new Wave(player,transform,animator,this), 100));

        var nothingToDoStrategy = new RandomSelector("Idle strategy" , 50);
        nothingToDoStrategy.AddChild(new Leaf("Wandering", new WanderAround(transform,agent,this)));
        nothingToDoStrategy.AddChild(new Leaf("Idle", new WaitStrategy(5f)));
        action.AddChild(nothingToDoStrategy);

        tree.AddChild(action);
    }

    private void Update()
    {
        tree.Process();
        //tree.PrintTree();
    }


    int posXAnimation = Animator.StringToHash("MovX");
    int posZAnimation = Animator.StringToHash("MovZ");
    int weaponHash = Animator.StringToHash("Stab");

    public void PlayWalkingAnimation(Vector3 point)
    {
        //z as vertical, x as horizontal
        //check the forward of the merton
        Vector3 targetDir = (point - transform.position).normalized;
        float horizontalMovement = Vector3.Dot(transform.right , targetDir);
        float verticalMovement = Vector3.Dot(transform.forward, targetDir);

        animator.SetFloat(posXAnimation, horizontalMovement);
        animator.SetFloat(posZAnimation, verticalMovement);
    }

    public void PlayRunningAnimation(Vector3 point)
    {
        //z as vertical, x as horizontal
        //check the forward of the merton
        Vector3 targetDir = (point - transform.position).normalized;
        float horizontalMovement = Vector3.Dot(transform.right, targetDir);
        float verticalMovement = Vector3.Dot(transform.forward, targetDir);

        animator.SetFloat(posXAnimation, horizontalMovement);
        animator.SetFloat(posZAnimation, verticalMovement * 2);
    }

    public void StopmovementAnimation()
    {
        animator.SetFloat(posXAnimation, 0);
        animator.SetFloat(posZAnimation, 0);
    }

    public void TakeDamage(object sender, int damage)
    {
        if (isDead) return;
        health -= damage;
        animator.SetTrigger("Take Damage");
        if(health < 0)
        {
            health = 0;
        }
        if (sender.GetType() == typeof(SwordBehaviour))
        {
            isMadAtPlayer = true;
        }
    }

    public void SetToWalkingSpeed()
    {
        agent.speed = walkSpeed;
    }

    public void SetToRunningSpeed()
    {
        agent.speed = runSpeed;
    }
}
