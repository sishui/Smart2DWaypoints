using System;
using System.Linq;
using Smart2DWaypoints;
using UnityEditor;
using UnityEngine;

namespace Smart2DWaypointsEditor
{
    [CustomEditor(typeof(WaypointsMover))]
    public class WaypointsMoverEditor : Editor
    {
        protected WaypointsMover _waypointsMover;

        public void OnEnable()
        {
            _waypointsMover = target as WaypointsMover;

            if (_waypointsMover != null && Mathf.Abs(WaypointsMover.InitSpeed - _waypointsMover.Speed) < 0.00001f)
            {
                _waypointsMover.Speed = 0.2f * Camera.main.orthographicSize;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();

            DrawPathSettings();
            DrawStartWaypointSettings();

            float newSpeed = EditorGUILayout.FloatField("Speed", _waypointsMover.Speed);
            if (Math.Abs(newSpeed - _waypointsMover.Speed) > 0.0000001f)
                _waypointsMover.Speed = newSpeed;
    
            DrawLoopTypeSettings();

            _waypointsMover.IsAlignToDirection = EditorGUILayout.Toggle("Align to direction",
                _waypointsMover.IsAlignToDirection);
            if (_waypointsMover.IsAlignToDirection)
                DrawIsAlignToDirection();

            DrawFlip();
            DrawPositionOffset();
            EditorGUILayout.EndVertical();
            EditorUtility.SetDirty(_waypointsMover);
        }

        private void DrawPathSettings()
        {
            Path newPath = EditorGUILayout.ObjectField("Path", _waypointsMover.Path, typeof(Path), true) as Path;
            if (newPath != _waypointsMover.Path && newPath != null && newPath.Waypoints.Any())
                _waypointsMover.transform.position = newPath.Waypoints.First().position;
            _waypointsMover.Path = newPath;
        }

        private void DrawStartWaypointSettings()
        {
            if (_waypointsMover.Path == null)
                return;

            GUILayout.BeginHorizontal();
            string[] options = _waypointsMover.Path.Waypoints.Select(_ => _.name).ToArray();
            int selectedIndex = _waypointsMover.Path.GetIndex(_waypointsMover.StartWaypoint);
            int newSelectedIndex = EditorGUILayout.Popup("Start waypoint", selectedIndex, options);
            if (newSelectedIndex != selectedIndex)
            {
                _waypointsMover.StartWaypoint = _waypointsMover.Path.Waypoints[newSelectedIndex];
                _waypointsMover.transform.position = _waypointsMover.StartWaypoint.position;
            }

            if (GUILayout.Button("#", GUILayout.Width(20f), GUILayout.Height(15f)))
            {
                EditorGUIUtility.PingObject(_waypointsMover.StartWaypoint);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawLoopTypeSettings()
        {
            GUILayout.BeginVertical("Box");
            _waypointsMover.LoopType = (LoopType) EditorGUILayout.EnumPopup("Loop type", _waypointsMover.LoopType);
            _waypointsMover.Loops = EditorGUILayout.IntField("Loops", _waypointsMover.Loops);
            
            _waypointsMover.IsDestroyWhenCompleted = EditorGUILayout.Toggle(
                "Destroy when completed", _waypointsMover.IsDestroyWhenCompleted);
            GUILayout.EndVertical();
        }

        private void DrawFlip()
        {
            _waypointsMover.IsXFlipEnabled = EditorGUILayout.Toggle("X-Flip", _waypointsMover.IsXFlipEnabled);
            _waypointsMover.IsYFlipEnabled = EditorGUILayout.Toggle("Y-Flip", _waypointsMover.IsYFlipEnabled);
        }

        protected virtual void DrawIsAlignToDirection()
        {
            _waypointsMover.RotationOffset =
                EditorGUILayout.FloatField("Rotation offset", _waypointsMover.RotationOffset);
        }

        private void DrawPositionOffset()
        {
            _waypointsMover.Offset = EditorGUILayout.Vector3Field("Position offset", _waypointsMover.Offset);
        }
    }
}