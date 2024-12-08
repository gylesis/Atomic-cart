using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Dev.BotsLogic;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Dev.Weapons.Guns;
using DG.Tweening;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Dev.Utils
{
    public static class Extensions
    {
        public static class AtomicCart
        {
            /// <summary>
            /// Getting distance between ShootPos and point in front of the wall with offset
            /// </summary>
            /// <returns></returns>
            public static float GetBulletMaxDistanceClampedByWalls(Vector2 originPos, Vector2 shootDirection,
                                                                   float bulletMaxDistance, float bulletOverlapRadius)
            {
                GameSettings gameSettings = GameSettingsProvider.GameSettings;

                RaycastHit2D raycast = Physics2D.CircleCast(originPos, bulletOverlapRadius, shootDirection,
                    bulletMaxDistance, gameSettings.WeaponObstaclesDetectLayers);

                if (raycast == false) return bulletMaxDistance;

                Vector2 hitPoint = raycast.point;

                hitPoint -= shootDirection * gameSettings.WeaponHitDetectionOffset;
                float distance = (originPos - hitPoint).magnitude;

                return distance;
            }

            public static Vector2 GetAimPosClampedByWalls(Vector2 originPos, Vector2 direction, float bulletMaxDistance, float bulletOverlapRadius)
            {
                var contactFilter = new ContactFilter2D();

                RaycastHit2D[] hits = new RaycastHit2D[10];

                Physics2D.defaultPhysicsScene.CircleCast(originPos, bulletOverlapRadius, direction, bulletMaxDistance, contactFilter, hits);

                Vector2 targetPos = originPos;
                
                foreach (var hit in hits.OrderBy(x => x.distance))
                {
                    if (hit.collider == null) continue;
                    
                    Vector2 hitPoint = hit.point;

                    if ((hitPoint - originPos).sqrMagnitude < GameSettingsProvider.GameSettings.Radius) continue;
                    
                    targetPos = hitPoint - direction * GameSettingsProvider.GameSettings.WeaponHitDetectionOffset;
                    break;
                }   

                return targetPos;
            }


            public static Vector3 GetSpawnPosByTeam(TeamSide teamSide)
            {
                var spawnPoints = LevelService.Instance.CurrentLevel.GetSpawnPointsByTeam(teamSide);

                SpawnPoint spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

                return spawnPoint.transform.position;
            }
        }

        public static void SetPositionZ(this Transform transform, float newZ)
        {
            var position = transform.position;
            position.z = newZ;
            transform.position = position;
        }
        
         /*
        public static void SetPositionZ(this Transform transform, float newX = default, float newY = float.PositiveInfinity, float newZ = float.PositiveInfinity)
        {
            var position = transform.position;
            position.x = newX == default ? transform.position.x : newX;
            position.y = newY;
            position.z = newZ;
            transform.position = position;
        }
        */
        
        
        public static bool IsPointInCameraView(this Camera camera, Vector3 worldPosition, float offset = 0)
        {
            Vector3 viewportPosition = camera.WorldToViewportPoint(worldPosition);
            
            return viewportPosition.z > 0 && 
                   viewportPosition.x >= -offset && viewportPosition.x <= 1 + offset && // Width offset
                   viewportPosition.y >= -offset && viewportPosition.y <= 1 + offset;   // Height offset
        }

        public static NetworkId ToNetworkId(this PlayerRef playerRef)
        {
            return new NetworkId
            {
                Raw = (uint)Mathf.Clamp(playerRef.GetHashCode(), 0, uint.MaxValue)
            };
        }

        public static void Delay(float seconds, CancellationToken token, Action onContinue)
        {
            UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: token)
                .ContinueWith(onContinue).Forget();
        }

        public static async Task<object> InvokeAsync(this MethodInfo @this, object obj, params object[] parameters)
        {
            Task task = (Task)@this.Invoke(obj, parameters);
            await task.ConfigureAwait(false);
            PropertyInfo resultProperty = task.GetType().GetProperty("Result");
            return resultProperty.GetValue(task);
        }

        public static void RotateTo(this Transform transform, Vector2 targetPos)
        {
            Vector2 direction = ((Vector3)targetPos - transform.position).normalized;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (angle < 0) 
                angle += 360;

            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            transform.rotation = targetRotation;
        }
        
        public static void RotateTowardsDirection(this Transform transform, Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (angle < 0) 
                angle += 360;

            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            transform.rotation = targetRotation;
        }
        
        

        public static int GetRandom(int maxNum, params int[] expectNums)
        {
            List<int> nums = new List<int>();

            for (int i = 0; i < maxNum; i++)
            {
                if (expectNums.Length == 0)
                {
                    nums.Add(i);
                }
                else
                {
                    bool exists = expectNums.ToList().Exists(x => x == i);

                    if (exists == false)
                    {
                        nums.Add(i);
                    }
                }
            }

            return nums[Random.Range(0, nums.Count)];
        }

        public static int GetNextRandom(int maxNum, params int[] expectNums)
        {
            List<int> nums = new List<int>();

            for (int i = 0; i < maxNum; i++)
            {
                if (expectNums.Length == 0)
                {
                    nums.Add(i);
                }
                else
                {
                    bool exists = expectNums.ToList().Exists(x => x == i);

                    if (exists == false)
                    {
                        nums.Add(i);
                    }
                }
            }

            return nums.First();
        }

        public static bool OverlapSphereLagCompensate(NetworkRunner runner, Vector3 pos, float radius,
                                                      LayerMask layerMask, out List<LagCompensatedHit> hits)
        {
            hits = new List<LagCompensatedHit>();

            runner.LagCompensation.OverlapSphere(pos, radius, runner.LocalPlayer,
                hits, layerMask);

            return hits.Count > 0;
        }

        public static bool OverlapCircle(NetworkRunner runner, Vector3 pos, float radius,
                                         out List<Collider2D> colliders)
        {
            colliders = new List<Collider2D>();

            var contactFilter2D = new ContactFilter2D();
            // contactFilter2D.layerMask = layerMask;
            contactFilter2D.useTriggers = true;

            runner.GetPhysicsScene2D().OverlapCircle(pos, radius, contactFilter2D, colliders);

            return colliders.Count > 0;
        }


        public static bool OverlapCircleExcludeWalls(NetworkRunner runner, Vector2 pos, float radius,
                                                     out List<Collider2D> targets)
        {
            targets = new List<Collider2D>();

            bool hasAnyTargets = OverlapCircle(runner, pos, radius, out var potentialTargets);
            if (hasAnyTargets == false) return false;

            var physics = runner.GetPhysicsScene2D();

            var contactFilter = new ContactFilter2D();
            // contactFilter2D.layerMask = layerMask;
            contactFilter.useTriggers = true;

            RaycastHit2D[] hits = new RaycastHit2D[10];

            foreach (var target in potentialTargets)
            {
                if (!target.TryGetComponent<IDamageable>(out var damageable)) continue;
                if (damageable.DamageId == DamagableType.Obstacle) continue;

                Vector2 direction = ((Vector2)target.transform.position - pos).normalized;

                physics.CircleCast(pos, 0.05f, direction, radius, contactFilter, hits);
                bool hasWallOnPath = false;

                foreach (var hit in hits.OrderBy(x => x.distance))
                {
                    Collider2D hitCollider = hit.collider;
                    if (hitCollider == null) continue;

                    var tryGetComponent = hitCollider.TryGetComponent<IDamageable>(out var dmgl);

                    if (tryGetComponent == false) continue;
                    if (hitCollider == target) break;

                    if (dmgl.DamageId is DamagableType.Obstacle)
                    {
                        //AtomicLogger.Log($"Can't get {target.name} because of wall", hitCollider);
                        hasWallOnPath = true;
                        break;
                    }
                }

                if (!hasWallOnPath)
                {
                    targets.Add(target);
                    //Debug.DrawRay(pos, direction * radius, Color.blue, 1);
                    //AtomicLogger.Log($"Success hit {target.transform.name}", target.gameObject);
                }
                else
                {
                    //if(circleCast > 0)
                    //Debug.DrawLine(pos, hits[0].point, Color.yellow, 1);
                }
            }

            return targets.Count > 0;
        }

        public static void SetAlpha(this Image image, float targetAlpha)
        {
            Color color = image.color;
            color.a = targetAlpha;
            image.color = color;
        }

        public static void SetAlpha(this CanvasGroup canvasGroup, float targetAlpha)
        {
            float alpha = canvasGroup.alpha;
            alpha = targetAlpha;
            canvasGroup.alpha = alpha;
        }

        public static int RandomBetween(params int[] nums)
        {
            return nums[Random.Range(0, nums.Length)];
        }

        public static void DoBounceScale(this Transform transform, Vector3 startScale, Action onFinish = null)
        {
            Sequence sequence = DOTween.Sequence();

            Vector3 originScale = startScale;
            Vector3 targetScale = originScale * 1.4f;

            sequence
                .Append(transform.DOScale(targetScale, 0.5f))
                .Append(transform.DOScale(originScale, 0.3f))
                .SetEase(Ease.OutBack)
                .OnComplete((() => onFinish?.Invoke()));
        }

        public static Vector3 RandomPointInBounds(this Bounds bounds)
        {
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }

        public static class ParabolicMovement
        {
            public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
            {
                Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

                Vector3 mid = Vector3.Lerp(start, end, t);

                return new Vector3(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
            }

            public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
            {
                Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

                Vector2 mid = Vector2.Lerp(start, end, t);

                return new Vector2(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t));
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="transform"></param>
            /// <param name="targetPos"></param>
            /// <param name="duration"></param>
            /// <param name="height"></param>
            /// <param name="onMoveComplete"></param>
            /// <param name="onMoveUpdate"> 0 to 1</param>
            public static void MoveParabolic(Transform transform, Vector3 targetPos, float duration, float height = 3f,
                                             Action onMoveComplete = null, Action<float> onMoveUpdate = null)
            {
                Vector3 startPos = transform.position;

                DOVirtual.Float(0, 1, duration, (value =>
                {
                    Vector3 pos = Parabola(startPos, targetPos, height, value);
                    transform.transform.position = pos;

                    onMoveUpdate?.Invoke(value);
                })).OnComplete((() => { onMoveComplete?.Invoke(); }));
            }

            public static void MoveParabolic(Transform transform, Transform target, float duration, float height = 3f,
                                             Action onMoveComplete = null, Action<float> onMoveUpdate = null)
            {
                Vector3 startPos = transform.position;

                DOVirtual.Float(0, 1, duration, (value =>
                {
                    Vector3 targetPos = target.position;

                    Vector3 pos = Parabola(startPos, targetPos, height, value);
                    transform.transform.position = pos;
                    onMoveUpdate?.Invoke(value);
                })).OnComplete((() => { onMoveComplete?.Invoke(); }));
            }
        }
    }
}