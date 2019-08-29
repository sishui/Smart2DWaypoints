using UnityEngine;

namespace Smart2DWaypoints
{
    [RequireComponent(typeof(RandomWaypoint))]
    [RequireComponent(typeof(LineRenderer))]
    public class RandomWaypointRenderer : MonoBehaviour
    {
        private RandomWaypoint _randomWaypoint;
        private LineRenderer _lineRenderer;

        public void Awake()
        {
            _randomWaypoint = GetComponent<RandomWaypoint>();
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.sortingOrder = 10;

            Vector3[] points = _randomWaypoint.GetEllipsePoints();
            _lineRenderer.SetVertexCount(points.Length);
            for (int i = 0; i < points.Length; i++)
                _lineRenderer.SetPosition(i, points[i]);
        }
    }
}