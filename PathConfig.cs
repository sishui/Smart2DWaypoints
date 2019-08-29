using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Smart2DWaypoints
{
    public class PathConfig
    {
        public List<MovePoint> MovePoints;
    }

    public class PathConfigSerializable
    {
        public List<MovePointSerializable> MovePoints;
    }
}