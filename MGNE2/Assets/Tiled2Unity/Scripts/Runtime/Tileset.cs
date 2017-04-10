using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tiled2Unity
{
    [Serializable]
    public class TileProperties : System.Object
    {
        public int tileId;
        public List<TiledProperty> properties;
    }

    public class Tileset : ScriptableObject
    {
        public string tilesetName;
        public List<TileProperties> properties;

        private Dictionary<int, List<TiledProperty>> propertiesDictionary;

        public TiledProperty PropertyForTile(int tileId, string propertyName)
        {
            if (propertiesDictionary == null)
            {
                ConstructLookup();
            }
            if (propertiesDictionary.ContainsKey(tileId))
            {
                List<TiledProperty> tileProperties = propertiesDictionary[tileId];
                foreach (TiledProperty property in tileProperties) {
                    if (property.key == propertyName) {
                        return property;
                    }
                }
            }
            return null;
        }

        private void ConstructLookup() {
            propertiesDictionary = new Dictionary<int, List<TiledProperty>>();
            foreach (TileProperties tileProperties in properties) {
                propertiesDictionary[tileProperties.tileId] = tileProperties.properties;
            }
        }
    }
}