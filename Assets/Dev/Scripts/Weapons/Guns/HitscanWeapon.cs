using UnityEngine;

namespace Dev.Weapons.Guns
{
    public abstract class HitscanWeapon : Weapon
    {
        [SerializeField] protected float _lenght;
        [SerializeField] protected LayerMask _layerMask;
    }
}