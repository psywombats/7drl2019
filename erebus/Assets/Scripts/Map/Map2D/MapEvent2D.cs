using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class MapEvent2D : MapEvent {

    public static Vector2Int GridLocationTileCoords(BoundsInt gridPosition) {
        return new Vector2Int(gridPosition.x, -1 * (gridPosition.y + 1));
    }

    public static Vector2Int WorldPositionTileCoords(Vector3 pos) {
        return new Vector2Int(
            Mathf.RoundToInt(pos.x / Map.TileSizePx) * OrthoDir.East.Px2DX(), 
            Mathf.RoundToInt(pos.y / Map.TileSizePx) * OrthoDir.North.Px2DY());
    }

    public override void Update() {
        base.Update();
        if (!Application.isPlaying) {
            position = WorldPositionTileCoords(transform.position);
            Vector2 sizeDelta = GetComponent<RectTransform>().sizeDelta;
            size = new Vector2Int(
                Mathf.RoundToInt(sizeDelta.x / Map.TileSizePx),
                Mathf.RoundToInt(sizeDelta.y / Map.TileSizePx));
        }
        SetDepth();
    }

    public void OnDrawGizmos() {
        if (Selection.activeGameObject == gameObject) {
            Gizmos.color = Color.red;
        } else {
            Gizmos.color = Color.magenta;
        }
        DrawGizmoSelf();
    }

    public override Vector3 CalculateOffsetPositionPx(OrthoDir dir) {
        float y = positionPx.y + dir.Px2DY() * Map.TileSizePx / Map.UnityUnitScale * OrthoDir.North.Px2DY();
        return new Vector3(
            positionPx.x + dir.Px2DX() * Map.TileSizePx / Map.UnityUnitScale * OrthoDir.East.Px2DX(),
            y,
            DepthForPositionPx(y));
    }

    public override void SetScreenPositionToMatchTilePosition() {
        Vector2 transform = new Vector2(Map.TileSizePx, Map.TileSizePx);
        transform.x = transform.x * OrthoDir.East.Px2DX();
        transform.y = transform.y * OrthoDir.North.Px2DY();
        positionPx = Vector2.Scale(position, transform);
        GetComponent<RectTransform>().sizeDelta = new Vector2(
            size.x * Map.TileSizePx / Map.UnityUnitScale, 
            size.y * Map.TileSizePx / Map.UnityUnitScale);
        GetComponent<RectTransform>().pivot = new Vector2(0.0f, 1.0f);
    }

    public override Vector3 InternalPositionToDisplayPosition(Vector3 position) {
        return new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), position.z);
    }

    public override void SetDepth() {
        if (parent != null) {
            gameObject.transform.localPosition = new Vector3(
                gameObject.transform.localPosition.x,
                gameObject.transform.localPosition.y,
                DepthForPositionPx(gameObject.transform.localPosition.y));
        }
    }

    private float DepthForPositionPx(float y) {
        return (y / (parent.size.y * Map.TileSizePx)) * 0.1f;
    }

    private void DrawGizmoSelf() {
        if (GetComponent<CharaEvent>() == null || GetComponent<CharaEvent>().GetAppearance() == null) {
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.5f);
            Gizmos.DrawCube(new Vector3(
                    transform.position.x + size.x * Map.TileSizePx * OrthoDir.East.Px2DX() / 2.0f,
                    transform.position.y + size.y * Map.TileSizePx * OrthoDir.North.Px2DY() / 2.0f,
                    transform.position.z - 0.001f),
                new Vector3((size.x - 0.1f) * Map.TileSizePx, (size.y - 0.1f) * Map.TileSizePx, 0.002f));
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(new Vector3(
                    transform.position.x + size.x * Map.TileSizePx * OrthoDir.East.Px2DX() / 2.0f,
                    transform.position.y + size.y * Map.TileSizePx * OrthoDir.North.Px2DY() / 2.0f,
                    transform.position.z - 0.001f),
                new Vector3((size.x - 0.1f) * Map.TileSizePx, (size.y - 0.1f) * Map.TileSizePx, 0.002f));
        }
    }
}
