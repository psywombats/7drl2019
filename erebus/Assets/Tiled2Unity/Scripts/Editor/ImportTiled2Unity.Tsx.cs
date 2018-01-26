using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml.Linq;

namespace Tiled2Unity
{
    partial class ImportTiled2Unity
    {
        public void TsxImported(string tsxPath)
        {
            // hardcoded asset path? blah
            string tilesetName = tsxPath.Substring(tsxPath.LastIndexOf('/') + 1, tsxPath.LastIndexOf(".tsx") - tsxPath.LastIndexOf('/') - 1);
            string assetPath = "Assets/Tiled2Unity/Tilesets/" + tilesetName + ".asset";

            Tileset tileset = ScriptableObject.CreateInstance<Tileset>();
            tileset.tilesetName = tilesetName;
            tileset.properties = new List<TileProperties>();
            XDocument document = XDocument.Load(tsxPath);
            XElement tilesetXml = document.Element("tileset");

            foreach (XElement tileXml in tilesetXml.Descendants("tile"))
            {
                XElement propertiesXml = tileXml.Element("properties");
                if (propertiesXml != null)
                {
                    TileProperties tileProperties = new TileProperties();
                    tileset.properties.Add(tileProperties);
                    tileProperties.tileId = int.Parse(tileXml.Attribute("id").Value);
                    tileProperties.properties = new List<TiledProperty>();
                    foreach (XElement propertyXml in propertiesXml.Descendants("property"))
                    {
                        TiledProperty property = new TiledProperty();
                        tileProperties.properties.Add(property);
                        property.key = propertyXml.Attribute("name").Value;
                        property.value = propertyXml.Attribute("value").Value;
                    }
                }
            }

            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(tileset, assetPath);
        }
    }
}
