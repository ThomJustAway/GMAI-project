using Assets.RW.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a simple script for the the fist for both player and 
/// enemy so they can box it
/// </summary>
public class Fist : MonoBehaviour
{
    [SerializeField] Transform holder;
    [SerializeField] int damage = 10;//how much damage to deal when in contact
    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        
        if(other.transform == holder) return;//ignore this two

        if(other.TryGetComponent<IDamageable>(out var component))
        {
            component.TakeDamage(this, damage);
        }
    }
}
