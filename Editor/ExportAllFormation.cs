using System.Collections.Generic;
using System.IO;
using LitJson;
using Smart2DWaypointsEditor;
using UnityEditor;
using UnityEngine;

namespace Smart2DWaypoints
{
    [CustomEditor(typeof(AllFormation))]
    public class ExportAllFormation : Editor
    {
        private AllFormation _allFormation;
        public void OnEnable()
        {
            _allFormation = target as AllFormation;
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
                if (_allFormation.textAsset)
                {
                    this.LoadClient(_allFormation.textAsset);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var textAsset =
                (TextAsset) EditorGUILayout.ObjectField("Config File", _allFormation.textAsset, typeof(TextAsset), false);
            if (this._allFormation.transform.childCount == 0 && textAsset != null && _allFormation.textAsset != textAsset)
            {
                _allFormation.textAsset = textAsset;
                this.LoadClient(textAsset);
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        public void ExportClient()
        {
            var filename = "Assets/art/game/fish/Formation/formation.json";
            TextWriter tw = new StreamWriter(filename);
            var index = 0;
            tw.Write("[");
            tw.WriteLine();
            for (int i = 0; i < this._allFormation.transform.childCount; i++)
            {
                var child = this._allFormation.transform.GetChild(i);
                if (child && child.name == (i + 1).ToString())
                {
                    var formation = child.GetComponent<Formation>();
                    if (formation == null) continue;
                    string jsonStr = JsonMapper.ToJson(formation.GetFormationData());
                    if (index > 0)
                    {
                        tw.Write(",");
                        tw.WriteLine();
                    }
                    tw.Write(jsonStr);
                    index++;
                }
            }
            tw.WriteLine();
            tw.Write("]");
            tw.Flush();
            tw.Close();
        }

        public void ExportServer()
        {
            var filename = "Assets/art/game/fish/Formation/teamServer.json";
            TextWriter tw = new StreamWriter(filename);
            for (int i = 0; i < this._allFormation.transform.childCount; i++)
            {
                var child = this._allFormation.transform.GetChild(i);
                if (child && child.name == (i + 1).ToString())
                {
                    var formation = child.GetComponent<Formation>();
                    if (formation == null) continue;
                    string jsonStr = JsonMapper.ToJson(formation.GetFormationData());
                    tw.Write(jsonStr);
                    tw.WriteLine();
                }
            }
            tw.Flush();
            tw.Close();
        }

        private void LoadClient(TextAsset textAsset)
        {
            var children = new List<Transform>();
            for (int i = 0; i < this._allFormation.transform.childCount; i++)
            {
                children.Add(this._allFormation.transform.GetChild(i));
            }
            foreach (var item in children)
            {
                var formation = item.GetComponent<Formation>();
                formation.RemoveAll();
                item.parent = null;
                DestroyImmediate(item.gameObject);
            }
            
            var json = JsonMapper.ToObject(textAsset.text);
            if (json == null) return;
            foreach (JsonData jsonData in json)
            {
                var child = new GameObject();
                child.transform.parent = this._allFormation.transform;
                child.transform.localScale = new Vector3(1, 1, 1);
                child.transform.localPosition = Vector3.zero;
                child.SetActive(true);
                var formation = child.AddComponent<Formation>();
                child.name = jsonData["teamId"].ToString();
                formation.Type = int.Parse(jsonData["type"].ToString()) == 1;
                int i = 0;
                foreach (JsonData item in jsonData["offset"])
                {
                    float x = float.Parse(item["x"].ToString());
                    float y = float.Parse(item["y"].ToString());
                    float z = float.Parse(item["z"].ToString());
                    float scaleX = float.Parse(item["scaleX"].ToString());
                    float scaleY = float.Parse(item["scaleY"].ToString());
                    float scaleZ = float.Parse(item["scaleZ"].ToString());
                    var member = formation.CreateMember(i, formation.Type?int.Parse(item["id"].ToString()):formation.MemberId, 
                        new Vector3(x, y, z), 
                        new Vector3(scaleX, scaleY, scaleZ));
                    formation.members.Add(member);
                    i++;
                }
            }
        }
    }
}