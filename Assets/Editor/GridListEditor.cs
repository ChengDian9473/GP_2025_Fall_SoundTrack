// Assets/Editor/GridListDrawer.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SoundTrack
{
    [CustomPropertyDrawer(typeof(GridList))]
    public class GridListDrawer : PropertyDrawer
    {
        bool foldTools = true;
        int cx = 0, cy = 0, radius = 0;
        int px = 0, py = 0;
        int ax = 0, ay = 0, bx = 0, by = 0;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight; // 使用自動 layout

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var listProp = property.FindPropertyRelative("items");
            // if (listProp == null || !listProp.isArray)
            // {
            //     EditorGUI.HelpBox(position, "GridList 需要 public List<GridPos> items;", MessageType.Error);
            //     return;
            // }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // --- Items（縮排 1 格）---
            int old = EditorGUI.indentLevel;
            EditorGUI.indentLevel = old + 1;
            EditorGUILayout.PropertyField(listProp, new GUIContent("Items"), true);
            EditorGUI.indentLevel = old;

            EditorGUILayout.Space(2);

            // --- 精準對齊的 Foldout Header ---
            EditorGUI.indentLevel = old + 1; // 與 Items 同縮排
            var headerRect = EditorGUILayout.GetControlRect(false);
            headerRect = EditorGUI.IndentedRect(headerRect);   // 套用縮排偏移

            foldTools = EditorGUI.BeginFoldoutHeaderGroup(headerRect, foldTools, new GUIContent("Grid Tools"));
            if (foldTools)
            {
                EditorGUI.indentLevel = old;
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        px = EditorGUILayout.IntField("x", px);
                        py = EditorGUILayout.IntField("y", py);
                    }
                    if (GUILayout.Button("Add One Grid"))
                        AddMerged(listProp, GridOps.Of((px, py)));

                    EditorGUILayout.Space();
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        ax = EditorGUILayout.IntField("Xmin", ax);
                        bx = EditorGUILayout.IntField("Xmax", bx);
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        ay = EditorGUILayout.IntField("Ymin", ay);
                        by = EditorGUILayout.IntField("Ymax", by);
                    }

                    int sx = Mathf.Min(ax, ay);  // startX
                    int ex = Mathf.Max(ax, ay);  // endX
                    int sy = Mathf.Min(bx, by);  // startY
                    int ey = Mathf.Max(bx, by);  // endY

                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Create Rect"))
                            AddMerged(listProp, GridOps.Rect(sx, sy, ex - sx + 1, ey - sy + 1));
                        if (GUILayout.Button("Remove Rect"))
                        {
                            var cur = ReadAll(listProp);
                            var toRemove = GridOps.Rect(sx, sy, ex - sx + 1, ey - sy + 1);
                            var result = cur.Where(p => !toRemove.Any(r => r.x == p.x && r.y == p.y)).ToList();
                            ReplaceAll(listProp, result);
                        }
                    }
                    
                    cx = EditorGUILayout.IntField("Center X", cx);
                    cy = EditorGUILayout.IntField("Center Y", cy);
                    radius = EditorGUILayout.IntField("Radius", radius);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Create Diamond"))
                        {
                            var pts = MakeDiamond(cx, cy, radius);
                            AddMerged(listProp, pts);
                        }

                        if (GUILayout.Button("Remove Diamond"))
                        {
                            var cur = ReadAll(listProp);
                            var toRemove = MakeDiamond(cx, cy, radius);

                            var removeSet = new HashSet<(int, int)>(toRemove.Select(p => (p.x, p.y)));
                            var result = cur.Where(p => !removeSet.Contains((p.x, p.y))).ToList();

                            ReplaceAll(listProp, result);
                        }
                    }


                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Check"))
                        {
                            var cur = ReadAll(listProp);
                            var dedup = GridOps.Union(cur);
                            ReplaceAll(listProp, dedup);
                        }
                        if (GUILayout.Button("Clear"))
                            ReplaceAll(listProp, new List<GridPos>());
                    }
                }
                EditorGUI.indentLevel = old + 1;
            }
            EditorGUI.EndFoldoutHeaderGroup();
            EditorGUI.indentLevel = old;

            EditorGUILayout.EndVertical();
            EditorGUI.EndProperty();

        }

        // ===== Helpers =====

        List<GridPos> ReadAll(SerializedProperty listProp)
        {
            var so = listProp.serializedObject;
            so.Update(); // <-- 關鍵：先更新快照

            var res = new List<GridPos>(listProp.arraySize);
            for (int i = 0; i < listProp.arraySize; i++)
            {
                var el = listProp.GetArrayElementAtIndex(i);
                res.Add(new GridPos
                {
                    x = el.FindPropertyRelative("x").intValue,
                    y = el.FindPropertyRelative("y").intValue
                });
            }
            return res;
        }
        List<GridPos> MakeDiamond(int cx, int cy, int r)
        {
            var pts = new List<GridPos>();
            for (int dx = -r; dx <= r; dx++)
            {
                int remain = r - Mathf.Abs(dx);
                for (int dy = -remain; dy <= remain; dy++)
                {
                    pts.Add(new GridPos { x = cx + dx, y = cy + dy });
                }
            }
            return pts;
        }

        void AddMerged(SerializedProperty listProp, IEnumerable<GridPos> toAdd)
        {
            var so = listProp.serializedObject;
            var targetObj = so.targetObject;

            so.Update();

            Undo.RecordObject(targetObj, "Add Grid Points");

            var merged = GridOps.Union(ReadAll(listProp), toAdd);

            ReplaceAll(listProp, merged);

            EditorUtility.SetDirty(targetObj);
        }

        void ReplaceAll(SerializedProperty listProp, List<GridPos> pts)
        {
            var so = listProp.serializedObject;
            var targetObj = so.targetObject;

            so.Update();

            Undo.RecordObject(targetObj, "Edit Grid Points");

            listProp.arraySize = pts.Count;
            for (int i = 0; i < pts.Count; i++)
            {
                var el = listProp.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("x").intValue = pts[i].x;
                el.FindPropertyRelative("y").intValue = pts[i].y;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(targetObj);
        }
    }
}
#endif
