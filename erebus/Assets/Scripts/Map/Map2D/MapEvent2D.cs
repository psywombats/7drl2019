using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class MapEvent2D : MapEvent {

    public Vector2 PositionPx2D {
        get { return new Vector2(gameObject.transform.position.x, gameObject.transform.position.y); }
        private set { gameObject.transform.position = new Vector3(value.x, value.y, gameObject.transform.position.z); }
    }

    public static IntVector2 GridLocationTileCoords(BoundsInt gridPosition) {
        return new IntVector2(gridPosition.x, -1 * (gridPosition.y + 1));
    }

    public static IntVector2 WorldPositionTileCoords(Vector3 pos) {
        return new IntVector2(
            Mathf.RoundToInt(pos.x / Map.TileSizePx) * OrthoDir.East.Px2DX(), 
            Mathf.RoundToInt(pos.y / Map.TileSizePx) * OrthoDir.North.Px2DY());
    }

    public override void Update() {
        base.Update();
        if (Application.isEditor) {
            position = WorldPositionTileCoords(transform.position);
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
        return new Vector3(
            positionPx.x + dir.Px2DX() * Map.TileSizePx / Map.UnityUnitScale,
            positionPx.y + dir.Px2DY() * Map.TileSizePx / Map.UnityUnitScale,
            DepthForPosition(position + dir.XY()));
    }

    public override void SetScreenPositionToMatchTilePosition() {
        Vector2 transform = new Vector2(Map.TileSizePx, Map.TileSizePx);
        transform.x = transform.x * OrthoDir.East.Px2DX();
        transform.y = transform.y * OrthoDir.North.Px2DY();
        PositionPx2D = Vector2.Scale(position, transform);
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
            float z = DepthForPosition(position);
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, z);
        }
    }

    private float DepthForPosition(Vector2 position) {
        return transform.parent.position.z - (position.y / (parent.size.y)) * 0.1f;
    }

    private void DrawGizmoSelf() {
        if (GetComponent<CharaEvent>() == null || GetComponent<CharaEvent>().GetAppearance() == null) {
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.5f);
            Gizmos.DrawCube(new Vector3(
                    PositionPx2D.x + size.x * Map.TileSizePx * OrthoDir.East.Px2DX() / 2.0f,
                    PositionPx2D.y + size.y * Map.TileSizePx * OrthoDir.North.Px2DY() / 2.0f,
                    transform.position.z - 0.001f),
                new Vector3((size.x - 0.1f) * Map.TileSizePx, (size.y - 0.1f) * Map.TileSizePx, 0.002f));
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(new Vector3(
                    PositionPx2D.x + size.x * Map.TileSizePx * OrthoDir.East.Px2DX() / 2.0f,
                    PositionPx2D.y + size.y * Map.TileSizePx * OrthoDir.North.Px2DY() / 2.0f,
                    transform.position.z - 0.001f),
                new Vector3((size.x - 0.1f) * Map.TileSizePx, (size.y - 0.1f) * Map.TileSizePx, 0.002f));
        }
    }
}
