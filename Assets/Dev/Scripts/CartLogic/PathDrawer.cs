using UnityEngine;

namespace Dev
{
    public class PathDrawer : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;

        public void DrawPath(Vector3[] points)
        {
            _lineRenderer.positionCount = points.Length;

            _lineRenderer.SetPositions(points);
        }

        public void Dispose()
        {
            _lineRenderer.positionCount = 0;
        }
        
    }
}