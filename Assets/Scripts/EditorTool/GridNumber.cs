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
    public bool show = true;
    public bool onlyOccupiedTiles = true;
    public int step = 1;
    public int fontSize = 12;
    public Color textColor = Color.white;

#if UNITY_EDITOR
void OnDrawGizmos()
{
    if (!show || tilemap == null) return;
    if (SceneView.currentDrawingSceneView == null) return;

    // 文字樣式
    var style = new GUIStyle(EditorStyles.label) {
        fontSize = fontSize,
        normal = { textColor = textColor },
        alignment = TextAnchor.MiddleCenter,   // 對 GUI.Label 有效
        clipping = TextClipping.Overflow
    };

    BoundsInt bounds = tilemap.cellBounds;
    int sx = bounds.xMin, ex = bounds.xMax;
    int sy = bounds.yMin, ey = bounds.yMax;

    int maxCount = Mathf.Clamp((ex - sx) * (ey - sy), 0, 20000);
    if (maxCount >= 20000)
    {
        Handles.Label(tilemap.transform.position, "<Tilemap 很大，請縮小範圍或提高 step>", style);
        return;
    }

    Handles.BeginGUI();
    for (int y = sy; y < ey; y += step)
    {
        for (int x = sx; x < ex; x += step)
        {
            var cell = new Vector3Int(x, y, 0);
            if (onlyOccupiedTiles && !tilemap.HasTile(cell)) continue;

            // 世界 → 螢幕座標（Scene 視窗的 GUI 座標）
            Vector3 world = tilemap.GetCellCenterWorld(cell) + worldOffset;
            Vector2 sp = HandleUtility.WorldToGUIPoint(world);

            // 計算字串尺寸，建立置中的矩形
            var content = new GUIContent($"({x},{y})");
            Vector2 size = style.CalcSize(content);

            // （可選）避免字抖動：四捨五入
            float cx = Mathf.Round(sp.x);
            float cy = Mathf.Round(sp.y);

            Rect r = new Rect(cx - size.x * 0.5f, cy - size.y * 0.5f, size.x, size.y);
            GUI.Label(r, content, style);
        }
    }
    Handles.EndGUI();
}
#endif
}
