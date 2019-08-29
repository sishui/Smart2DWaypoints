using Smart2DWaypoints;
using UnityEditor;

namespace Smart2DWaypointsEditor
{
    [CustomEditor(typeof(RigidbodyMover))]
    public class RigidbodyMoverEditor : WaypointsMoverEditor
    {
        protected override void DrawIsAlignToDirection()
        {
            base.DrawIsAlignToDirection();

            RigidbodyMover rigidbodyMover = _waypointsMover as RigidbodyMover;
            rigidbodyMover.RotationSpeed = EditorGUILayout.FloatField("Rotation speed", rigidbodyMover.RotationSpeed);
        }
    }
}