using Assets.RW.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (other.gameObject.TryGetComponent(out IDamageable component))
        {
            if (LayerMask.LayerToName(other.gameObject.layer) != "Player" &&
                !hitColliders.Contains(other))
            {
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
