using UnityEngine;
using UnityEditor;

namespace SoundTrack
{
    [CustomEditor(typeof(RoomRegister))]
    public class RoomRegisterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            RoomRegister rr = (RoomRegister)target;

            if (GUILayout.Button("Register Room Data"))
            {
                rr.Clear();
                BakeLevelObjects(rr);
            }
            if (GUILayout.Button("Show Room Object"))
            {
                ShowLevelObjects(rr);
            }
        }

        private void BakeLevelObjects(RoomRegister rr)
        {
            LevelObject[] objs = rr.GetComponentsInChildren<LevelObject>(true);
            
            Undo.RegisterFullObjectHierarchyUndo(rr.gameObject, "Bake LevelObjects");

            foreach (var obj in objs)
            {
                obj.BakeRegister(rr);
                obj.gameObject.SetActive(false);
            }

            EditorUtility.SetDirty(rr);
        }
        private void ShowLevelObjects(RoomRegister rr)
        {
            LevelObject[] objs = rr.GetComponentsInChildren<LevelObject>(true);

            Undo.RegisterFullObjectHierarchyUndo(rr.gameObject, "Unbake LevelObjects");

            foreach (var obj in objs)
            {
                obj.gameObject.SetActive(true);
            }

            EditorUtility.SetDirty(rr);
        }
    }
}
