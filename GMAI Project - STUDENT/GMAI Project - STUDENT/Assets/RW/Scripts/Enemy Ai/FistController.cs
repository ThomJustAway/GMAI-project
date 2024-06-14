using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fist controller just control the collider active state
/// when it is playing an animation.
/// </summary>
public class FistController : MonoBehaviour
{
    [SerializeField] Collider leftFistCollider;
    [SerializeField] Collider rightFistCollider;

    private void Start()
    {
        leftFistCollider.enabled = false;
        rightFistCollider.enabled = false;
    }
    //you can see the being used in the animation event for the punching
    //animation for both player and enemy.
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