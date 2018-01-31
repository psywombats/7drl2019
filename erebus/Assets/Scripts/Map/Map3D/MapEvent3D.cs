using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;

public class MapEvent3D : MapEvent {

    public Vector3 PositionPx {
        get { return transform.position; }
        private set { gameObject.transform.localPosition = PositionPx; }
    }
    
    protected override void SetScreenPositionToMatchTilePosition() {
        transform.localPosition = new Vector3(Position.x, transform.localPosition.y, -1.0f * Position.y);
    }
    
    protected override void SetDepth() {
        float z = Layer.GetComponent<Layer3D>().Z;
        transform.localPosition = new Vector3(gameObject.transform.localPosition.x, z, gameObject.transform.localPosition.z);
    }

}
