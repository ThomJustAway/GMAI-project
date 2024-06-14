using Assets.RW.Scripts;
using Player;
using RayWenderlich.Unity.StatePatternInUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// a script to trigger when the player is close to a boxing enemy.
/// </summary>
public class PlayerBoxSensor : MonoBehaviour
{
    [SerializeField] private Character player;
    private bool hasTriggerBoxing;
    private IBoxingEnemy selectedEnemy = null;

    private void Update()
    {
        if (selectedEnemy != null)
        {
            //check if the enemy is still within the area or if the player should box
            DetermineIfCanBox();
        }
        else
        {
            //will consistenly check for the enemy
            Collider[] enemiesNearby = Physics.OverlapSphere(transform.position, player.BoxingSenseRadius)
                .Where(collider => collider.GetComponent<IBoxingEnemy>() != null).ToArray();

            if (enemiesNearby.Length == 0) return;

            //if there is a enemy, then just check which one is the closest.
            float minValue = float.MaxValue;
            foreach (var enemy in enemiesNearby)
            {
                float distance = Vector3.Distance(player.transform.position, enemy.transform.position);
                if (distance < minValue)
                {
                    minValue = distance;
                    selectedEnemy = enemy.GetComponent<IBoxingEnemy>();
                }
            }
        }


        
    }

    private void DetermineIfCanBox()
    {
        if (Vector3.Distance(player.transform.position, selectedEnemy.transform.position) < player.BoxingSenseRadius &&
            !selectedEnemy.IsDead)
        {//if within range and is not dead
            if (hasTriggerBoxing) return;
            int id = player.currentPlayerState.ID;
            if (id == (int)MainState.Movement ||
                (
                id == (int)MainState.Crouch &&
                !player.IsSomethingAbove
                ))
            {
                hasTriggerBoxing = true;
                player.SetBoxingState(true);
            }
        }
        else
        {//stop the player from fighting
            if (hasTriggerBoxing)
            {
                player.SetBoxingState(false);
                hasTriggerBoxing = false;
            }
            selectedEnemy = null;
        }
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, player.BoxingSenseRadius);
    }
}
