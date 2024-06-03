using Assets.RW.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFist : MonoBehaviour
{
    [SerializeField] Transform enemy;

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        
        if(other.transform == enemy) return;//ignore this two

        if(other.TryGetComponent<IDamageable>(out var component))
        {
            component.TakeDamage(this, 100);
        }
    }
}
