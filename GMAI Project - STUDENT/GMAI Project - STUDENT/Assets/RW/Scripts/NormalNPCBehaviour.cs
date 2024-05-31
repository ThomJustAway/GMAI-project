using Pathfinding.BehaviourTrees;
using Player;
using RayWenderlich.Unity.StatePatternInUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NormalNPCBehaviour : MonoBehaviour
{
    //[SerializeField]
    //float searchRadius;
    [SerializeField]
    Character player;
    NavMeshAgent agent;
    BehaviourTree tree;
    Animator animator;
    BoxCollider attackHitBox;

    //public float SearchRadius { get => searchRadius; set => searchRadius = value; }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        tree = new BehaviourTree("Metalon BT");

        var action = new PrioritySelector("Actions");

        //for interacting with the player
        var interactWithPlayer = new Selector("interact with player" , 100);
        interactWithPlayer.AddChild(new Leaf("Player wave", new Condition(
            () => player.currentPlayerState.ID == (int)MainState.Wave &&
            Vector3.Distance(player.transform.position, transform.position) < 10f
            )));
        interactWithPlayer.AddChild(new Leaf("wave player",new Wave(player,transform,animator) ));
        action.AddChild(interactWithPlayer);

        var nothingToDoStrategy = new RandomSelector("Idle strategy" , 50);
        nothingToDoStrategy.AddChild(new Leaf("Wandering", new WanderAround(transform,agent,this)));
        nothingToDoStrategy.AddChild(new Leaf("Idle", new WaitStrategy(5f)));
        action.AddChild(nothingToDoStrategy);

        tree.AddChild(action);
    }

    private void Update()
    {
        tree.Process();
        tree.PrintTree();
    }


    int posXAnimation = Animator.StringToHash("MovX");
    int posZAnimation = Animator.StringToHash("MovZ");
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
    public void StopmovementAnimation()
    {
        animator.SetFloat(posXAnimation, 0);
        animator.SetFloat(posZAnimation, 0);
    }
}
