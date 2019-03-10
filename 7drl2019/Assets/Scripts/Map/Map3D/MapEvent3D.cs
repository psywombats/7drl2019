using UnityEngine;

//[ExecuteInEditMode]
public class MapEvent3D : MapEvent {

    public override Vector3 TileToWorldCoords(Vector2Int position) {
        return new Vector3(position.x, map.terrain.HeightAt(position), position.y);
    }

    public static Vector2Int WorldPositionTileCoords(Vector3 pos) {
        return new Vector2Int(
            Mathf.RoundToInt(pos.x) * OrthoDir.East.Px3DX(),
            Mathf.RoundToInt(pos.z) * OrthoDir.North.Px3DZ());
    }

    public override void SetScreenPositionToMatchTilePosition() {
        transform.localPosition = new Vector3(location.x, map.terrain.HeightAt(location), location.y);
        positionPx = transform.localPosition;
    }

    public override Vector3 InternalPositionToDisplayPosition(Vector3 position) {
        return position;
    }

    public override void SetDepth() {
        // our global height is identical to the height of the parent layer
        if (map != null) {
            transform.localPosition = new Vector3(
                gameObject.transform.localPosition.x,
                map.terrain.HeightAt(location),
                gameObject.transform.localPosition.z);
        }
    }

    public override float CalcTilesPerSecond() {
        return tilesPerSecond;
    }

    public override void Update() {
        base.Update();
        if (!Application.isPlaying) {
            location = WorldPositionTileCoords(transform.localPosition);
            Vector2 sizeDelta = GetComponent<RectTransform>().sizeDelta;
            size = new Vector2Int(
                Mathf.RoundToInt(sizeDelta.x),
                Mathf.RoundToInt(sizeDelta.y));
        }
        SetDepth();
    }

    protected override void DrawGizmoSelf() {
        if (GetComponent<Map3DHandleExists>() != null) {
            return;
        }
        Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.5f);
        Gizmos.DrawCube(new Vector3(
                transform.position.x + size.x * OrthoDir.East.Px3DX() / 2.0f,
                transform.position.y,
                transform.position.z + size.y * OrthoDir.North.Px3DZ() / 2.0f),
            new Vector3((size.x - 0.1f), 0.002f, (size.y - 0.1f)));
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(new Vector3(
                transform.position.x + size.x * OrthoDir.East.Px3DX() / 2.0f,
                transform.position.y,
                transform.position.z + size.y * OrthoDir.North.Px3DZ() / 2.0f),
            new Vector3((size.x - 0.1f), 0.002f, (size.y - 0.1f)));
    }
}
