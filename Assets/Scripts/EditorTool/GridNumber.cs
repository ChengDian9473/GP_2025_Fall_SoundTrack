// GPT code

using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class TilemapCoordOverlay : MonoBehaviour
{
    public Tilemap tilemap;

    [Header("顯示選項")]
    public bool show = true;
    [Tooltip("只顯示有貼圖的格子；關閉則顯示整個 bounds")]
    public bool onlyOccupiedTiles = true;
    [Tooltip("每隔幾格顯示一個標籤（1=每格都顯示）")]
    [Min(1)] public int step = 1;
    [Range(8, 28)] public int fontSize = 12;
    public Color textColor = Color.white;
    public Vector3 worldOffset = new Vector3(0, 0, 0); // 微調標籤位置

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!show || tilemap == null) return;

        // 避免在 Game 視窗顯示（只在 Scene）
        if (SceneView.currentDrawingSceneView == null) return;

        // 以編輯器 Handles 畫字
        Handles.color = textColor;
        var style = new GUIStyle(EditorStyles.label) {
            fontSize = fontSize,
            normal = { textColor = textColor },
            alignment = TextAnchor.MiddleCenter
        };

        // 決定要遍歷哪些格子
        BoundsInt bounds = tilemap.cellBounds;

        int sx = bounds.xMin, ex = bounds.xMax;
        int sy = bounds.yMin, ey = bounds.yMax;

        // 太大的 Tilemap 會很花資源，做個防呆
        int maxCount = Mathf.Clamp((ex - sx) * (ey - sy), 0, 20000);
        if (maxCount >= 20000)
        {
            Handles.Label(tilemap.transform.position, "<Tilemap 很大，請縮小範圍或提高 step>", style);
            return;
        }

        for (int y = sy; y < ey; y += step)
        {
            for (int x = sx; x < ex; x += step)
            {
                var cell = new Vector3Int(x, y, 0);
                if (onlyOccupiedTiles && !tilemap.HasTile(cell)) continue;

                Vector3 world = tilemap.GetCellCenterWorld(cell) + worldOffset;
                // 讓標籤永遠在最前方一些，避免被擋住
                world.z = Camera.current != null ? Camera.current.transform.position.z + 0.1f : world.z;

                Handles.Label(world, $"({x},{y})", style);
            }
        }
    }
#endif
}
