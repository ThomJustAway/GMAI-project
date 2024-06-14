using Assets.RW.Scripts;
using Pathfinding.BehaviourTrees;
using Player;
using RayWenderlich.Unity.StatePatternInUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//the creature behaviour tree and how I implement it
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
        //get component
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        //start making the behaviour tree 
        tree = new BehaviourTree("Metalon BT");

        //the priority selector will consistenly check the highest priority node
        //when the process is being run.
        var action = new PrioritySelector("Actions");

        #region death sequence
        var deathSequence = new Sequence("death sequence",150);
        deathSequence.AddChild(new Leaf("IsDead", new Condition(() => isDead)));
        deathSequence.AddChild(new Leaf("Do Death animation", new ActionStrategy(
            () => {
                //will basically show the animation of the merton
                animator.ResetTrigger("Take Damage");
                animator.SetTrigger("Die");
                agent.ResetPath();

                //afterward, it would create a new behaviour tree that will do nothing to stop
                //it from doing anything.
                tree = new BehaviourTree("Nothing");

                tree.AddChild(new Leaf("nothing", new ActionStrategy(() => { })));
            })));
        action.AddChild(deathSequence);
        #endregion

        #region aggro sequence
        var aggroSequence = new Sequence("Aggro", 125);
        //check if mad at player.
        aggroSequence.AddChild(new Leaf("is mad at player", new Condition(() => IsMadAtPlayer)));
        var beCloseToPlayer = new PrioritySelector("Close to player");
        beCloseToPlayer.AddChild(new Leaf("is close to player", new Condition(() =>
        Vector3.Distance(transform.position, player.transform.position) < 2f)));
        //if this fail, make sure to move close to the player
        beCloseToPlayer.AddChild(new Leaf("chase player", new ChasePlayer(transform, agent, player.transform, this)));
        aggroSequence.AddChild(beCloseToPlayer);
        aggroSequence.AddChild(new Leaf("Attack Player", new ActionStrategy(() =>
        {
            //will play the attack animation.
            animator.SetTrigger(weaponHash);
            weapon.SetHitBox(true);
        })));
        aggroSequence.AddChild(new Leaf("wait for attack to end", new WaitForAttackToFinish(animator, weapon)));
        //add it into the action node.
        action.AddChild(aggroSequence);
        #endregion

        #region wave at player sequence
        //though in the document, I added a bunch of leaf in order to wave,
        // I compress all the logic into one leaf node called wave since I thought it would be
        //easier that way.
        action.AddChild(new Leaf("Wave at player", new Wave(player,transform,animator,this), 100));
        #endregion

        #region idle strategy
        //the random selector will randomly do one of the children and see if it run success or fail.
        var nothingToDoStrategy = new RandomSelector("Idle strategy" , 50);
        nothingToDoStrategy.AddChild(new Leaf("Wandering", new WanderAround(transform,agent,this)));
        nothingToDoStrategy.AddChild(new Leaf("Idle", new WaitStrategy(5f)));
        action.AddChild(nothingToDoStrategy);
        #endregion


        tree.AddChild(action);
    }

    private void Update()
    {
        tree.Process();
        //tree.PrintTree();
    }

    #region extra methods
    int posXAnimation = Animator.StringToHash("MovX");
    int posZAnimation = Animator.StringToHash("MovZ");
    int weaponHash = Animator.StringToHash("Stab");

    public void PlayWalkingAnimation(Vector3 point)
    {
        //check the forward of the merton
        Vector3 targetDir = (point - transform.position).normalized;
        //afterwards, determine the horizontal and vertical weights for the animation
        float horizontalMovement = Vector3.Dot(transform.right , targetDir);
        float verticalMovement = Vector3.Dot(transform.forward, targetDir);

        //then set it on the animator to show it walking
        animator.SetFloat(posXAnimation, horizontalMovement);
        animator.SetFloat(posZAnimation, verticalMovement);
    }

    public void PlayRunningAnimation(Vector3 point)
    {
        Vector3 targetDir = (point - transform.position).normalized;
        //afterwards, determine the horizontal and vertical weights for the animation
        float horizontalMovement = Vector3.Dot(transform.right, targetDir);
        float verticalMovement = Vector3.Dot(transform.forward, targetDir);
        //show the animation based on the weights.
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
    #endregion 
}
