
#if UNITY_EDITOR
using System.Collections.Generic;
using Dev.Utils;
using ModestTree;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dev
{
    public class PathDrawer : MonoBehaviour
    {
            
        [SerializeField] private RailPathStraight _railPrefab;

        [SerializeField] private List<RailPathStraight> _rails;

        [SerializeField] private Transform _railsParent;
        [SerializeField] private Transform _pathPointsParent;
        
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
        
        [ContextMenu(nameof(DrawPath))]
        public void DrawPath()
        {
            ClearPath();

            var pathPoints = _pathPointsParent.GetComponentsInChildren<CartPathPoint>();

            float size = _railPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
                
            for (var index = 0; index < pathPoints.Length; index++)
            {
                CartPathPoint cartPathPoint = pathPoints[index];

                if (index == 0)
                {
                    RailPathStraight railPathStraight = PrefabUtility.InstantiatePrefab(_railPrefab) as RailPathStraight;
                    railPathStraight.transform.position = cartPathPoint.transform.position;
                    railPathStraight.transform.parent = _railsParent;

                    _rails.Add(railPathStraight);
                }
                else
                {
                    RailPathStraight prevRail = _rails[index - 1];
                    prevRail.SecondBone.Rotate2D(cartPathPoint.transform.position);

                    CartPathPoint prevPathPoint = pathPoints[index - 1];

                    float distance = (cartPathPoint.transform.position - prevPathPoint.transform.position).magnitude;

                    int count = (int)(distance / size);

                    Debug.Log($"Sub rails count for path {index - 1} - {index} {count}");
                
                    for (int i = 0; i < count; i++)
                    {
                        float t = (float) i / count;

                        Debug.Log($"T is {t}");
                    
                        Vector3 subRailSpawnPos = Vector3.Lerp(prevPathPoint.transform.position,
                            cartPathPoint.transform.position, t);
                        
                        RailPathStraight subRailPathStraight = PrefabUtility.InstantiatePrefab(_railPrefab) as RailPathStraight;
                        subRailPathStraight.transform.position = subRailSpawnPos;
                    
                        subRailPathStraight.transform.Rotate2D(cartPathPoint.transform.position);

                        subRailPathStraight.transform.parent = _railsParent;
                        
                        _rails.Add(subRailPathStraight);
                    }
                    
                    Debug.Log($"Distance between {index - 1} and {index} is {distance}");
                }
                
                
                    
            }

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

    }
}
#endif