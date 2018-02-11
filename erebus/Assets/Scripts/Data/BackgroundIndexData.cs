using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "BackgroundIndexData", menuName = "Data/BackgroundIndexData")]
public class BackgroundIndexData : GenericIndex<BackgroundData> {

}

[Serializable]
public class BackgroundData : GenericDataObject {

    public Sprite background;

}
