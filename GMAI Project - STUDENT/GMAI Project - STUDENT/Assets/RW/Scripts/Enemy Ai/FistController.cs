using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FistController : MonoBehaviour
{
    [SerializeField] Collider leftFistCollider;
    [SerializeField] Collider rightFistCollider;

    private void Start()
    {
        leftFistCollider.enabled = false;
        rightFistCollider.enabled = false;
    }

    public void EnableLeftFist()
    {
        leftFistCollider.enabled = true;
    }

    public void DisableLeftFist()
    {
        leftFistCollider.enabled = false;
    }

    public void EnableRightFist()
    {
        rightFistCollider.enabled = true;
    }

    public void DisableRightFist()
    {
        rightFistCollider.enabled = false;
    }
}