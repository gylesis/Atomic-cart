using System.IO.Compression;
using Dev.Infrastructure;
using Fusion;
using UnityEngine;

namespace Dev.Weapons
{
    public class Turret : NetworkContext
    {
        [SerializeField] private WeaponController _weaponController;
        
        [Networked] private NetworkBool AllowToShoot { get; set; } = true;

        [SerializeField] private Vector2 _shootDirection = Vector2.right;
        

        [ContextMenu(nameof(ChangeShootState))]
        private void ChangeShootState()
        {
            AllowToShoot = !AllowToShoot;
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false) return;

            if (AllowToShoot == false) return;

            _shootDirection = transform.right;
            
            _weaponController.TryToFire(transform.right);
        }
    }
}