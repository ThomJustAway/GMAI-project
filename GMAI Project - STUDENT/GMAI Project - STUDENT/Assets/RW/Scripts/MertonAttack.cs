using Assets.RW.Scripts;
using RayWenderlich.Unity.StatePatternInUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (other.gameObject.TryGetComponent(out IDamageable component))
        {
            if (LayerMask.LayerToName(other.gameObject.layer) == "Player" &&
                !hitColliders.Contains(other))
            {
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
