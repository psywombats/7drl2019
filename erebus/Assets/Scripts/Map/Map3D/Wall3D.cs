using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall3D : MonoBehaviour {

    public TileMeshRenderer North;
    public TileMeshRenderer South;
    public TileMeshRenderer East;
    public TileMeshRenderer West;

    public List<TileMeshRenderer> GetAllSides() {
        return new List<TileMeshRenderer>(new TileMeshRenderer[] {North, East, South, West });
    }
}
