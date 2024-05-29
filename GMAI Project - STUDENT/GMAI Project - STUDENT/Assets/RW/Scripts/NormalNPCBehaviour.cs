using BehaviourTreeImplementation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NormalNPCBehaviour : MonoBehaviour
{
    [SerializeField]
    float searchRadius;
    [SerializeField]
    Transform targetPos;
    NavMeshAgent agent;
    BehaviourTree tree;

    public float SearchRadius { get => searchRadius; set => searchRadius = value; }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        tree = new BehaviourTree();

        
    }

}
