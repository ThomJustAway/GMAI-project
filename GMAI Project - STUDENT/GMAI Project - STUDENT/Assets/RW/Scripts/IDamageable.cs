using System.Collections;
using UnityEngine;

namespace Assets.RW.Scripts
{
    public interface IDamageable 
    {
        void TakeDamage(object sender, int damage);
    }
}