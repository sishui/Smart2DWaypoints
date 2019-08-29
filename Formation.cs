using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Smart2DWaypoints
{
    public class Member
    {
        public int Id;
        public Transform Transform;
    }
    
    public class FormationPositionSerializeData
    {
        public int id;
        public float x;
        public float y;
        public float z;
        public float scaleX;
        public float scaleY;
        public float scaleZ;
    }

    public class FormationSerializeData
    {
        public int teamId;
        public int type;
        public List<FormationPositionSerializeData> offset;
    }

    
    public class Formation : MonoBehaviour
    {
//        public string CfgName;
        public List<Member> members = new List<Member>();
        public int MemberId = 0;
        public bool Type = false;
        public TextAsset textAsset;

        public FormationSerializeData GetFormationData()
        {
            int index = System.Convert.ToInt32(this.name);
            List<FormationPositionSerializeData> offsets = new List<FormationPositionSerializeData>();
            foreach (var member in this.members)
            {
                var data = new FormationPositionSerializeData
                {
                    x = member.Transform.localPosition.x,
                    y = member.Transform.localPosition.y,
                    z = 0,
                    scaleX = Math.Abs(member.Transform.localScale.x),
                    scaleY = Math.Abs(member.Transform.localScale.y),
                    scaleZ = Math.Abs(member.Transform.localScale.z),
                };
                if (this.Type)
                {
                    data.id = member.Id;
                }
                else
                {
                    data.id = -1;
                }
                offsets.Add(data);
            }

            return new FormationSerializeData
            {
                teamId = index,
                type = this.Type ? 1 : 0,
                offset = offsets,
            };
        }
#if UNITY_EDITOR
        public Member CreateMember(int count, int memberId, Vector3 pos, Vector3 scale)
        {
            if (memberId < 0)
            {
                return null;
            }
            string localPath = $"Assets/art/game/fish/Prefab/fish2d/fish{memberId+1:D2}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject));
            if (prefab == null)
            {
                return null;
            }
//            GameObject go = UnityEngine.Object.Instantiate(prefab) as GameObject;
            GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            go.name = count.ToString();
            go.transform.SetParent(this.transform);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = scale;
            return new Member {Id = memberId, Transform = go.transform};
        }

        public Member CreateMember(int count)
        {
            return this.CreateMember(count, this.MemberId, Vector3.zero, Vector3.one);
        }


        public Member AddMember()
        {
            var count = this.members.Count;
            var member = CreateMember(count);
            this.members.Add(member);
            return member;
        }
        
        public Member ChangeMember(Member old)
        {
            if (old == null)
            {
                return null;
            }
            string localPath = $"Assets/art/game/fish/Prefab/fish2d/fish{old.Id+1:D2}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject));
            if (prefab == null)
            {
                return null;
            }
//            GameObject go = UnityEngine.Object.Instantiate(prefab) as GameObject;
            GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (go == null) return null;
            go.transform.SetParent(this.transform);
            go.name = old.Transform.name;
            go.transform.localPosition = new Vector3(old.Transform.localPosition.x, old.Transform.localPosition.y, old.Transform.localPosition.z);
            go.transform.localRotation = new Quaternion(old.Transform.localRotation.x, old.Transform.localRotation.y, old.Transform.localRotation.z, old.Transform.localRotation.w);
            go.transform.localScale = new Vector3(old.Transform.localScale.x, old.Transform.localScale.y, old.Transform.localScale.z);
            Member member = new Member {Id = old.Id, Transform = go.transform};
            old.Id = -1;
            old.Transform.parent = null;
            DestroyImmediate(old.Transform.gameObject);
            return member;
        }
#endif

        public void RemoveMember(int index)
        {
            if (this.members.Count > index)
            {
                var removed = this.members[index];
                this.members.RemoveAt(index);
                for (int i = 0; i < this.members.Count; i++)
                {
                    this.members[i].Transform.name = i.ToString();
                }
                removed.Transform.parent = null;
                DestroyImmediate(removed.Transform.gameObject);
            }
        }
        
        public void RemoveChildren()
        {
            var children = new List<Transform>();
            for (int i = 0; i < this.transform.childCount; i++)
            {
                var child = this.transform.GetChild(i);
                children.Add(child);
            }
            foreach (var member in this.members.ToArray())
            {
                this.members.Remove(member);
                if ( children.IndexOf(member.Transform) != -1)
                {
                    children.Remove(member.Transform);
                }
                member.Transform.parent = null;
                DestroyImmediate(member.Transform.gameObject);
            }

            foreach (var child in children)
            {
                child.parent = null;
                DestroyImmediate(child.gameObject);
            }
        }

        public void RemoveAll()
        {
            this.RemoveChildren();
            this.textAsset = null;
            this.MemberId = 1;
        }
    }
}