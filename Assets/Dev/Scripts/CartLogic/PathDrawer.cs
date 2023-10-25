#if UNITY_EDITOR
using System.Collections.Generic;
using System.Threading.Tasks;
using Dev.Utils;
using ModestTree;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Dev
{
    public class PathDrawer : MonoBehaviour
    {
        [SerializeField] private bool _autoSaveSceneAfterDraw = false;

        [SerializeField] private RailPathStraight _railPrefab;

        [SerializeField] private List<RailPathStraight> _rails;

        [SerializeField] private Transform _railsParent;
        [SerializeField] private Transform _pathPointsParent;

        [SerializeField] private int _subrailsSize = 2;

        [FormerlySerializedAs("_magnitudeDivider")] [SerializeField] private float _magnitudeDivideModifier = 2;
        
        [ContextMenu(nameof(ClearPath))]
        public void ClearPath()
        {
            for (var index = _rails.Count - 1; index >= 0; index--)
            {
                RailPathStraight railPathStraight = _rails[index];
                DestroyImmediate(railPathStraight.gameObject);
            }

            _rails.Clear();
        }


        [ContextMenu(nameof(CorrectRailsPositions))]
        public async void CorrectRailsPositions()
        {
            var pathPoints = _pathPointsParent.GetComponentsInChildren<CartPathPoint>();
            
            for (var index = 1; index < pathPoints.Length; index++)
            {
                await Task.Delay(500);
                CartPathPoint currentPathPoint = pathPoints[index];
                CartPathPoint prevPathPoint = pathPoints[index - 1];
    
                Vector3 directionToCurrentPathPoint = (currentPathPoint.transform.position - prevPathPoint.transform.position);
                float magnitude = directionToCurrentPathPoint.magnitude;
                magnitude /= _magnitudeDivideModifier;

                float rawDistance = Mathf.Ceil(magnitude);

                var centeredPos = currentPathPoint.transform.position + directionToCurrentPathPoint.normalized * rawDistance;

                centeredPos.x = Mathf.Ceil(centeredPos.x);
                centeredPos.y = Mathf.Ceil(centeredPos.y);
                
                currentPathPoint.transform.position = centeredPos;

                for (int i = index + 1; i < pathPoints.Length; i++)
                {
                    CartPathPoint pathPointToMove = pathPoints[i];

                    pathPointToMove.transform.position += directionToCurrentPathPoint.normalized * rawDistance;
                }
                
                
            }
            
            if (_autoSaveSceneAfterDraw)
            {
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }
            else
            {
                EditorUtility.SetDirty(this);
            }
        }
        
        
        [ContextMenu(nameof(DrawPath))]
        public async void DrawPath()
        {
            ClearPath();

            var pathPoints = _pathPointsParent.GetComponentsInChildren<CartPathPoint>();

            float size = _railPrefab.GetComponent<SpriteRenderer>().bounds.size.x;

            for (var index = 1; index < pathPoints.Length; index++)
            {
                await Task.Delay(500);
                CartPathPoint cartPathPoint = pathPoints[index];
                
                /*RailPathStraight prevRail = _rails[index - 1];
                prevRail.SecondBone.Rotate2D(currentCartPathPointPos);*/

                CartPathPoint prevPathPoint = pathPoints[index - 1];

                Vector3 directionToCurrentPathPoint = (cartPathPoint.transform.position - prevPathPoint.transform.position);
                float magnitude = directionToCurrentPathPoint.magnitude;

                int intDistance = (int)Mathf.Floor(magnitude);
                float rawDistance = Mathf.Floor(magnitude);

                var centeredPos = cartPathPoint.transform.position + directionToCurrentPathPoint.normalized * rawDistance;

                cartPathPoint.transform.position = centeredPos;

                directionToCurrentPathPoint = (cartPathPoint.transform.position - prevPathPoint.transform.position);
                magnitude = directionToCurrentPathPoint.magnitude;

                intDistance = (int)Mathf.Floor(magnitude);
                
                int amountPerLenght = _subrailsSize;
                int subrailsCount = (intDistance / amountPerLenght) + 1;

                // int count = (int)(distance / size * _subrailsSize);

                Debug.Log($"Sub rails count for path {index - 1} - {index} {subrailsCount}");

                for (int i = 0; i < subrailsCount; i++)
                {
                    float t = (float)i / subrailsCount;

                    Debug.Log($"T is {t}");

                    Vector3 subRailSpawnPos = Vector3.Lerp(prevPathPoint.transform.position,
                        cartPathPoint.transform.position, t);

                    RailPathStraight subRailPathStraight =
                        PrefabUtility.InstantiatePrefab(_railPrefab) as RailPathStraight;
                    
                    subRailPathStraight.transform.position = subRailSpawnPos;
                    subRailPathStraight.transform.Rotate2D(cartPathPoint.transform.position);
                    subRailPathStraight.transform.parent = _railsParent;

                    _rails.Add(subRailPathStraight);

                    await Task.Delay(300);
                }

                Debug.Log($"Distance between {index - 1} and {index} is raw: {rawDistance}, centered: {intDistance}");
            }

            if (_autoSaveSceneAfterDraw)
            {
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }
            else
            {
                EditorUtility.SetDirty(this);
            }
        }
    }
}
#endif