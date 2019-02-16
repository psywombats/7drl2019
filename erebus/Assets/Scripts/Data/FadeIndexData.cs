using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "FadeIndexData", menuName = "Data/FadeIndexData")]
public class FadeIndexData : GenericIndex<FadeData> {

}

[Serializable]
public class FadeData : GenericDataObject {

    public Texture2D transitionMask;
    public bool invert;
    public bool flipHorizontal;
    public bool flipVertical;
    public float delay;
    [Range(0.0f, 1.0f)] public float softEdgePercent;

    // copy constructor
    public FadeData(FadeData copySource) {
        transitionMask = copySource.transitionMask;
        invert = copySource.invert;
        flipHorizontal = copySource.flipHorizontal;
        flipVertical = copySource.flipVertical;
        delay = copySource.delay;
        softEdgePercent = copySource.softEdgePercent;
    }
}
