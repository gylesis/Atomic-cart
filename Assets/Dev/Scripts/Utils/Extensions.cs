using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Dev.Utils
{
    public static class Extensions
    {
        private static Dictionary<int, Sequence> Sequences = new Dictionary<int, Sequence>();

        public static async Task<object> InvokeAsync(this MethodInfo @this, object obj, params object[] parameters)
        {
            Task task = (Task)@this.Invoke(obj, parameters);
            await task.ConfigureAwait(false);
            PropertyInfo resultProperty = task.GetType().GetProperty("Result");
            return resultProperty.GetValue(task);
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

        public static void SetAlpha(this Image image, float alpha)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
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