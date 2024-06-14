using Assets.RW.Scripts;
using RayWenderlich.Unity.StatePatternInUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple script for the merton (creature)
/// when the creature is attacking the player.
/// </summary>
public class MertonAttack : MonoBehaviour
{
    BoxCollider hitBox;
    int playerLayer;
    List<Collider> hitColliders;
    [SerializeField] int damage = 5;


    private void Start()
    {
        hitBox = GetComponent<BoxCollider>();
        playerLayer = LayerMask.GetMask("Player");

    }

    private void OnTriggerEnter(Collider other)
    {
        //will see if the player is within the collider that was entered in the trigger.
        if (other.gameObject.TryGetComponent(out IDamageable component))
        {
            if (LayerMask.LayerToName(other.gameObject.layer) == "Player" &&
                !hitColliders.Contains(other))
            {
                //if it is, then make sure that the player does take damage.
                component.TakeDamage(this, damage);
                hitColliders.Add(other);
            }
        }
    }

    public void SetHitBox(bool value)
    {
        hitColliders = new();
        hitBox.enabled = value;
    }
}
