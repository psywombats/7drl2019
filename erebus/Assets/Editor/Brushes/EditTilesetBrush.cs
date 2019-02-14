using System.Linq;
using UnityEngine;

namespace UnityEditor {

    [CustomGridBrush(true, false, false, "Edit Tileset Brush")]
    public class EditTilesetBrush : GridBrush {

        public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position) {
            base.Paint(grid, brushTarget, position);
            TilesetOverlay overlay = brushTarget.GetComponentInParent<TilesetOverlay>();
            overlay.CheckForChanges();
        }

        public override void Erase(GridLayout grid, GameObject brushTarget, Vector3Int position) {
            // no op
        }

        public override void FloodFill(GridLayout grid, GameObject brushTarget, Vector3Int position) {
            base.FloodFill(grid, brushTarget, position);
            TilesetOverlay overlay = brushTarget.GetComponentInParent<TilesetOverlay>();
            overlay.CheckForChanges();
        }

        public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt position) {
            base.BoxFill(gridLayout, brushTarget, position);
            TilesetOverlay overlay = brushTarget.GetComponentInParent<TilesetOverlay>();
            overlay.CheckForChanges();
        }
    }

    [CustomEditor(typeof(EditTilesetBrush))]
    public class EditTilesetBrushEditor : GridBrushEditorBase {
        public override GameObject[] validTargets {
            get {
                return FindObjectsOfType<TilesetOverlay>().Select(x => x.overlay.gameObject).ToArray();
            }
        }
    }
}

