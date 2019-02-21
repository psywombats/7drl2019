using UnityEngine;

public class MapCamera2D : MapCamera {

    public void LateUpdate() {
        ManualUpdate();
    }

    public override void ManualUpdate() {
        base.ManualUpdate();
        Vector3 targetPos = target.transform.position;
        Vector3 oldPos = GetComponent<Camera>().transform.position;
        Vector3 newPos = new Vector3(
            targetPos.x - OrthoDir.North.XY2D().y * Map.TileSizePx / Map.UnityUnitScale / 2.0f, 
            targetPos.y - OrthoDir.North.XY2D().x * Map.TileSizePx / Map.UnityUnitScale / 2.0f, 
            oldPos.z);
        GetComponent<Camera>().transform.position = newPos;
    }
}
