using System;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using LitJson;
using Smart2DWaypoints;
using UnityEditor;
using UnityEngine;

namespace Smart2DWaypointsEditor
{
    [CustomEditor(typeof(AllPath))]
    public class ExportAllPathEditor : Editor
    {
        private AllPath _allPath;

        public void OnEnable()
        {
            _allPath = target as AllPath;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export Client", GUILayout.Width(100)))
            {
                this.ExportClient();
            }

            if (GUILayout.Button("Export Server", GUILayout.Width(100)))
            {
                this.ExportServer();
            }

            if (GUILayout.Button("Force Load", GUILayout.Width(100)))
            {
                if (_allPath.textAsset)
                {
                    this.LoadClient(_allPath.textAsset);
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var textAsset =
                (TextAsset) EditorGUILayout.ObjectField("Config File", _allPath.textAsset, typeof(TextAsset), false);
            if (this._allPath.transform.childCount == 0 && textAsset != null && _allPath.textAsset != textAsset)
            {
                _allPath.textAsset = textAsset;
                this.LoadClient(textAsset);
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

//        public void ExportClient()
//        {
//            var filename = "Assets/art/game/fish/Track/fishpath_test.json";
//            TextWriter tw = new StreamWriter(filename);
//            var index = 0;
//            tw.Write("[");
//            tw.WriteLine();
//            for (int i = 0; i < this._allPath.transform.childCount; i++)
//            {
//                var child = this._allPath.transform.GetChild(i);
//                if (child && child.name == (i + 1).ToString())
//                {
//                    if (index > 0)
//                    {
//                        tw.Write(",");
//                        tw.WriteLine();
//                    }
//                    var path = child.GetComponent<DOTweenPath>();
//                    if (path == null) continue;
//                    float distance = 0f;
//                    for (int j = 2; j < path.path.wpLengths.Length; j++)
//                    {
//                        distance += path.path.wpLengths[j];
//                    }
//                    string jsonStr = JsonMapper.ToJson(new PathServer{id = path.name, distance = distance});
//                    tw.Write(jsonStr);
//                    index++;
//                }
//            }
//
//            tw.WriteLine();
//            tw.Write("]");
//            tw.Flush();
//            tw.Close();
//        }
//
//        public void ExportServer()
//        {
//            var filename = "Assets/art/game/fish/Track/fishPathServer.json";
//            TextWriter tw = new StreamWriter(filename);
//            for (int i = 0; i < this._allPath.transform.childCount; i++)
//            {
//                var child = this._allPath.transform.GetChild(i);
//                if (child && child.name == (i + 1).ToString())
//                {
//                    var path = child.GetComponent<Path>();
//                    if (path == null) continue;
//                    string jsonStr = JsonMapper.ToJson(new PathDistance(path.name, path.GetOnceDistance()));
//                    tw.Write(jsonStr);
//                    tw.WriteLine();
//                }
//            }
//            tw.Flush();
//            tw.Close();
//        }

        public void LoadClient(TextAsset textAsset)
        {
            var children = new List<Transform>();
            for (int i = 0; i < this._allPath.transform.childCount; i++)
            {
                children.Add(this._allPath.transform.GetChild(i));
            }
            foreach (var item in children)
            {
                item.parent = null;
                DestroyImmediate(item.gameObject);
            }
            
            var json = JsonMapper.ToObject(textAsset.text);
            if (json == null) return;
            var pz = this._allPath.transform.localPosition.z;
            foreach (JsonData jsonData in json)
            {
                var child = new GameObject();
                child.transform.parent = this._allPath.transform;
                child.transform.localScale = new Vector3(1, 1, 1);
                child.transform.localPosition = Vector3.zero;
                child.SetActive(true);
                var doTweenPath = child.AddComponent<DOTweenPath>();
                child.name = jsonData["id"].ToString();
                doTweenPath.updateType = UpdateType.Fixed;
                doTweenPath.easeType = Ease.Linear;
                doTweenPath.pathMode = PathMode.Sidescroller2D;
                doTweenPath.isSpeedBased = true;
                doTweenPath.showWpLength = true;
                doTweenPath.showIndexes = true;
                doTweenPath.pathType = PathType.CatmullRom;
                doTweenPath.pathResolution = 20;
                foreach (JsonData positionJson in jsonData["position"])
                {
                    float x = float.Parse(positionJson["x"].ToString());
                    float y = float.Parse(positionJson["y"].ToString());
                    float z = float.Parse(positionJson["z"].ToString());
                    doTweenPath.wps.Add(new Vector3(x, y, Math.Abs(z) < 0.000001f?pz:z));
                }
            }
        }

        public void ExportClient()
        {
            var filename = "Assets/art/game/fish/Track/fishpath.json";
            TextWriter tw = new StreamWriter(filename);
            try
            {
                var index = 0;
                tw.Write("[");
                tw.WriteLine();
                for (int i = 0; i < this._allPath.transform.childCount; i++)
                {
                    var child = this._allPath.transform.GetChild(i);
                    if (child && child.name == (i + 1).ToString())
                    {
                        if (index > 0)
                        {
                            tw.Write(",");
                            tw.WriteLine();
                        }

                        var path = child.GetComponent<DOTweenPath>();
                        if (path == null) continue;
                        float distance = 0f;
                        for (int j = 2; j < path.path.wpLengths.Length; j++)
                        {
                            distance += path.path.wpLengths[j];
                        }

                        List<MovePoint3> movePoint3s = new List<MovePoint3>();
                        for (var j = 0; j < path.wps.Count; j++)
                        {
//                        if (path.wps[j] == Vector3.zero && j == 0)
//                        {
//                            continue;
//                        }
                            movePoint3s.Add(new MovePoint3 {x = path.wps[j].x, y = path.wps[j].y, z = Math.Abs(path.wps[j].z) < 0.000001f?this._allPath.transform.localPosition.z:path.wps[j].z});
                        }

                        var o = new PathClient
                        {
                            id = path.name, distance = distance,
                            position = movePoint3s,
                        };
                        string jsonStr = JsonMapper.ToJson(o);
                        tw.Write(jsonStr);
                        index++;
                    }
                }

                tw.WriteLine();
                tw.Write("]");
                tw.Flush();
            }
            catch (Exception)
            {
            }
            finally
            {
                tw.Close();
            }
            
        }

        public void ExportServer()
        {
            var filename = "Assets/art/game/fish/Track/fishPathServer.xml";
            TextWriter tw = new StreamWriter(filename);
            tw.Write(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>");
            tw.WriteLine();
            tw.Write(@"<objects>");
            tw.WriteLine();
            try
            {
                for (int i = 0; i < this._allPath.transform.childCount; i++)
                {
                    var child = this._allPath.transform.GetChild(i);
                    if (child && child.name == (i + 1).ToString())
                    {
                        var path = child.GetComponent<DOTweenPath>();
                        if (path == null) continue;
                        float distance = 0f;
                        for (int j = 2; j < path.path.wpLengths.Length; j++)
                        {
                            distance += path.path.wpLengths[j];
                        }
                        tw.Write($@"	<sample class=""ww.sample.PathSample"" sid=""{path.name}"" allLifeTime=""{Math.Floor(distance+0.5)}"" allStopTime=""0""/>");
                        tw.WriteLine();
                    }
                }
                tw.Write(@"</objects>");
                tw.Flush();
            }
            catch (Exception)
            {
                
            }
            finally
            {
                tw.Close();
            }
            
        }
    }
}