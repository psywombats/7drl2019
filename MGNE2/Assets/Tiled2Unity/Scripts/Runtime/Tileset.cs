using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tiled2Unity
{
    [Serializable]
    public class TilesetProperties : SerializableDictionary<int, List<TiledProperty>> { }

    public class Tileset : ScriptableObject
    {
        public string TilesetName;
        public TilesetProperties Properties;
    }
}