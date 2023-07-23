using System.Collections.Generic;
using Dev.Infrastructure;
using Fusion;
using UnityEngine;

namespace Dev.Weapons.Guns
{
    [RequireComponent(typeof(NetworkRigidbody2D))]
    public abstract class Projectile : NetworkContext
    {
        [SerializeField] private NetworkRigidbody2D _networkRigidbody2D;

        private Vector3 _moveDirection;
        private float _force;

        public void Init(Vector3 moveDirection, float force)
        {
            _force = force;
            _moveDirection = moveDirection;

            transform.up = _moveDirection;
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false) return;

            _networkRigidbody2D.Rigidbody.velocity = _moveDirection * _force * Runner.DeltaTime;
        }

        protected bool OverlapSphere(Vector3 pos, float radius, LayerMask layerMask, out List<LagCompensatedHit> hits)
        {
            hits = new List<LagCompensatedHit>();

            Runner.LagCompensation.OverlapSphere(pos, radius, Object.InputAuthority,
                hits, layerMask);

            return hits.Count > 0;
        }

        protected void ExplodePlayer(Vector3 pos, float radius, float explosionForcePower,
            bool needToCheckWalls = false)
        {
            var overlapSphere = OverlapSphere(pos, radius, LayerMask.NameToLayer("Player"), out var hits);

            // Debug.Log($"Hits count {hits.Count}");

            if (overlapSphere)
            {
                foreach (LagCompensatedHit hit in hits)
                {
                    // Debug.Log($"Hit {hit.GameObject.name}", hit.GameObject);

                    var player = hit.GameObject.GetComponent<Player>();

                    PlayerRef owner = Object.InputAuthority;
                    PlayerRef target = player.Object.InputAuthority;

                    if (target == owner) continue;

                    // player.Damaged?.Invoke(owner, target);

                    ApplyForceToPlayer(player, explosionForcePower);
                }
            }
        }

        protected void ApplyForceToPlayer(Player player, float forcePower)
        {
            var forceDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(0f, 1f));
            forceDirection.Normalize();

            Debug.DrawRay(player.transform.position, forceDirection * 2, Color.blue, 5f);

            player.Rigidbody.AddForce(forceDirection * forcePower, ForceMode2D.Impulse);
        }
    }
}