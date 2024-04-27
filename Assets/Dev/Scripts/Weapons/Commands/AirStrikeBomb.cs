using Dev.Effects;
using Dev.PlayerLogic;
using Dev.Weapons.Guns;
using UnityEngine;

namespace Dev.Weapons
{
    public class AirStrikeBomb : ExplosiveProjectile
    {
        [SerializeField] private float _explosionRadius;
        
        private TeamSide _ownerTeam;

        public void Init(TeamSide teamSide)
        {
            _ownerTeam = teamSide;
        }
        
        public void StartDetonate()
        {
            ExplodeAndHitPlayers(_explosionRadius);  
            
            FxController.Instance.SpawnEffectAt("landmine_explosion", transform.position);

            ToDestroy.OnNext(this);
        }
        
    }
}