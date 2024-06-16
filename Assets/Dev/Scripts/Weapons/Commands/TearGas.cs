﻿using Dev.Weapons.Guns;
using UnityEngine;

namespace Dev.Weapons
{
    public class TearGas : ExplosiveProjectile
    {
        public void DealDamage()
        {
            Debug.Log($"Deal damage from tear gas with radius {_explosionRadius}");
            ExplodeAndHitPlayers(_explosionRadius);  
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, _explosionRadius);
        }
    }
}