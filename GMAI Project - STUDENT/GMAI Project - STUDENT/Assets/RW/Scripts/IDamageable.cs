using System.Collections;
using UnityEngine;

namespace Assets.RW.Scripts
{
    //a simple interface to record who does damage.
    public interface IDamageable 
    {
        void TakeDamage(object sender, int damage);
    }

    //a simple abstract interface for player to fight the enemy.
    public interface IBoxingEnemy
    {
        Transform transform { get; }
        bool IsPlayerEnemy { get;}
        bool IsDead { get;  }

    }
}