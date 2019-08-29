using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using LitJson;
using Smart2DWaypoints;

namespace Smart2DWaypointsEditor
{
    public static class MyGUIStyles
    {
        private static GUIStyle m_line = null;

        public static GUIStyle EditorLine
        {
            get
            {
                if (m_line == null)
                {
                    m_line = new GUIStyle("box");
                    m_line.border.top = m_line.border.bottom = 1;
                    m_line.margin.top = m_line.margin.bottom = 1;
                    m_line.padding.top = m_line.padding.bottom = 1;
                }

                return m_line;
            }
        }
    }
    
    [CustomEditor(typeof(Formation))]
    public class FormationEditor : Editor
    {
        private Formation _formation;

        public void OnEnable()
        {
            _formation = target as Formation;
        }
        
        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            var textAsset = _formation.textAsset;
            _formation.textAsset = (TextAsset)EditorGUILayout.ObjectField("Config File",
                textAsset, typeof(TextAsset), false);
            if (_formation.members.Count == 0 && _formation.textAsset != null || textAsset != _formation.textAsset)
            {
                _formation.RemoveChildren();
                try
                {
                    var data = JsonMapper.ToObject<FormationSerializeData>(_formation.textAsset.text);
                    if (data != null)
                    {
                        _formation.Type = data.type == 1;
                        int i = 0;
                        foreach (var item in data.offset)
                        {
                            var member = _formation.CreateMember(i, _formation.Type?item.id:_formation.MemberId, 
                                new Vector3(item.x, item.y, item.z), 
                                new Vector3(item.scaleX, item.scaleY, item.scaleZ));
                            _formation.members.Add(member);
                            i++;
                        }

                        _formation.name = data.teamId.ToString();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var oldMode = _formation.Type;
            _formation.Type = EditorGUILayout.Toggle("指定模型或不指定模型", _formation.Type);
            EditorGUILayout.EndHorizontal();
            if (oldMode != _formation.Type)
            {
                _formation.RemoveAll();
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+", GUILayout.Width(40)))
            {
                _formation.AddMember();
            }
            if (GUILayout.Button("Export", GUILayout.Width(80)))
            {
                Export();
            }
            _formation.MemberId = this._formation.Type ? EditorGUILayout.IntField("Default ID", _formation.MemberId) : 0;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            for (int i = 0; i < _formation.members.Count; i++)
            {
                Member member = _formation.members[i];
                EditorGUILayout.BeginHorizontal();
                member.Transform = (Transform) EditorGUILayout.ObjectField(member.Transform, typeof(Transform), true,
                    GUILayout.Width(120));
                if (_formation.Type)
                {
                    var oldId = member.Id;
                    member.Id = EditorGUILayout.IntField("ID", oldId);
                    if (oldId != member.Id)
                    {
                        member = _formation.ChangeMember(member);
                        if (member != null)
                        {
                            _formation.members[i] = member;
                        }
                    }
                }
                if (GUILayout.Button("-", GUILayout.Width(40)))
                {
                    _formation.RemoveMember(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            EditorUtility.SetDirty(_formation);
        }
        
        
        private void Export()
        {
            if (_formation != null)
            {
                var filename = "Assets/art/game/fish/Formation/team" + _formation.name + ".bytes";
                TextWriter tw = new StreamWriter(filename);
                string jsonStr = JsonMapper.ToJson(_formation.GetFormationData());
                tw.Write(jsonStr);
                tw.Flush();
                tw.Close();
                if (_formation.textAsset == null)
                {
                    _formation.textAsset = AssetDatabase.LoadAssetAtPath(filename, typeof(TextAsset)) as TextAsset;
                }
                EditorUtility.SetDirty(_formation);
            }
        }
    }
}