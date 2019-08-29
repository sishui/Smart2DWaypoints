using System.Collections.Generic;
using UnityEngine;


namespace Smart2DWaypoints
{
    public class MovePointSerializable
    {
        public float x;
        public float y;
        public float Angle;
        public bool IsCurved;
        public bool IsClosed;

        public MovePointSerializable()
        {
        }

        public MovePointSerializable(MovePoint point)
        {
            x = point.Position.x;
            y = point.Position.y;
            Angle = point.Angle;
        }
    }

    [System.Serializable]
    public class MovePoint
    {
        public Vector2 Position;
        public float Angle;

        public MovePoint()
        {
        }

        public MovePoint(MovePoint point)
        {
            Position = point.Position;
            Angle = point.Angle;
        }

        public MovePoint(float x, float y, float angle)
        {
            Position = new Vector2(x, y);
            Angle = angle;
        }

        public MovePoint(Vector2 position, float angle)
        {
            Position = position;
            Angle = angle;
        }
    }

    [System.Serializable]
    public class MovePoint3
    {
        public float x;
        public float y;
        public float z;
    }

    [System.Serializable]
    public class PathServer
    {
        public string id;
        public float distance;
    }
    
    [System.Serializable]
    public class PathClient
    {
        public string id;
        public float distance;
        public List<MovePoint3> position;
    }
}