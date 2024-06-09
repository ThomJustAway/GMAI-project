using Player;
using RayWenderlich.Unity.StatePatternInUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBoxSensor : MonoBehaviour
{
    [SerializeField] private Character player;
    [SerializeField] private Transform enemy;
    private bool hasTriggerBoxing;
    private void Update()
    {
        if(Vector3.Distance(player.transform.position,enemy.transform.position) < player.BoxingSenseRadius)
        {
            if (hasTriggerBoxing) return;
            int id = player.currentPlayerState.ID;
            if (id == (int)MainState.Movement ||
                (
                id == (int)MainState.Crouch &&
                !player.IsSomethingAbove    
                ) )
            {
                hasTriggerBoxing = true;
                player.SetBoxingState(true);
            }
        }
        else
        {
            if (hasTriggerBoxing)
            {
                player.SetBoxingState(false);
                hasTriggerBoxing = false;
            }
            
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, player.BoxingSenseRadius);
    }
}
