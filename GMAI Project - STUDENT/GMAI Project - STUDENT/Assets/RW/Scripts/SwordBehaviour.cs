using Assets.RW.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple script to control the sword behaviour and how it will
/// damage the enemies.
/// </summary>
public class SwordBehaviour : MonoBehaviour
{
    [SerializeField] int damage = 5;
    int playerLayer;
    [SerializeField]
    BoxCollider hitbox;
    List<Collider> hitColliders;

    private void Start()
    {
        playerLayer = LayerMask.GetMask("Player"); 
    }
    private void OnTriggerEnter(Collider other)
    {
        //if it hits another collider, it would try and see what gameobject it is
        if (other.gameObject.TryGetComponent(out IDamageable component))
        {
            if (LayerMask.LayerToName(other.gameObject.layer) != "Player" &&
                !hitColliders.Contains(other))
            {
                //if can get idamageabl component and is not player, just give it damage.
                component.TakeDamage(this,damage);
                hitColliders.Add(other);
            }
        }
    }

    

    public void SetHitBox(bool value)
    {
        hitColliders = new();
        hitbox.enabled = value;
    }
}
