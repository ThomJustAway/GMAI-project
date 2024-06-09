using Assets.RW.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fist : MonoBehaviour
{
    [SerializeField] Transform holder;

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        
        if(other.transform == holder) return;//ignore this two

        if(other.TryGetComponent<IDamageable>(out var component))
        {
            component.TakeDamage(this, 100);
        }
    }
}
