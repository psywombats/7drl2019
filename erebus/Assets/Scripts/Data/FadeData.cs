using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "FadeData", menuName = "Data/FadeData")]
public class FadeData : GenericDataObject {

    public Texture2D transitionMask;
    public bool invert;
    public bool flipHorizontal;
    public bool flipVertical;
    public float delay;
    [Range(0.0f, 1.0f)] public float softEdgePercent;

}
