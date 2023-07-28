using Dev.Infrastructure;
using Fusion;
using UnityEngine;

namespace Dev.Weapons
{
    public class Turret : NetworkContext
    {
        [SerializeField] private WeaponController _weaponController;
        [Networked] private NetworkBool AllowToShoot { get; set; }

        [ContextMenu(nameof(ChangeShootState))]
        private void ChangeShootState()
        {
            AllowToShoot = !AllowToShoot;
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false) return;

            if (AllowToShoot == false) return;

            _weaponController.TryToFire(transform.right);
        }
    }
}