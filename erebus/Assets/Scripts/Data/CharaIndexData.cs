using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "CharaIndexData", menuName = "Data/VN/CharaIndexData")]
public class CharaIndexData : GenericIndex<CharaData> {

}

[Serializable]
public class CharaData : GenericDataObject {

    public string displayName;
    public Sprite portrait;

}
