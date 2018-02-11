using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "BGMIndexData", menuName = "Data/BGMIndexData")]
public class BGMIndexData : GenericIndex<BGMData> {

}

[Serializable]
public class BGMData : GenericDataObject {

    public AudioClip track;

}
