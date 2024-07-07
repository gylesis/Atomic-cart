using Dev.Infrastructure;
using Dev.PlayerLogic;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev.Weapons
{
    public abstract class AbilityCastCommand
    {
        public bool AllowToCast { get; protected set; } = true;
        public AbilityType AbilityType { get; private set; }

        public Subject<AbilityType> AbilityRecharged { get; } = new Subject<AbilityType>();
        
        protected NetworkRunner _runner;
        protected TeamSide TeamSide => _owner.TeamSide;
        
        protected SessionPlayer _owner;

        protected AbilityCastCommand(NetworkRunner runner, AbilityType abilityType, SessionPlayer owner)
        {
            _owner = owner;
            AbilityType = abilityType;
            _runner = runner;
        }

        protected T Spawn<T>(T prefab, Vector3 pos, PlayerRef inputAuthority, NetworkRunner.OnBeforeSpawned onBeforeSpawned) where T : NetworkContext
        {   
            T isntance = _runner.Spawn(prefab: prefab, position: pos, inputAuthority: inputAuthority, onBeforeSpawned: (
                (runner, o) =>
                {
                    DependenciesContainer.Instance.Inject(o.gameObject);
                    onBeforeSpawned(runner, o);
                })).GetComponent<T>();
            
            return isntance;
        }

        public virtual void Process(Vector3 pos)
        {
            AllowToCast = false;
        }

        public virtual void Reset()
        {
            AllowToCast = true;
            
            AbilityRecharged.OnNext(AbilityType);
        }
    }
}