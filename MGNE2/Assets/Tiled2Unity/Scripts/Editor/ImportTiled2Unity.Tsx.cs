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
            string tilesetName = tsxPath.Substring(tsxPath.LastIndexOf('/') + 1, tsxPath.Length - tsxPath.LastIndexOf(".tsx") + 1);
            string assetPath = "Assets/Tiled2Unity/Tilesets/" + tilesetName + ".asset";

            Tileset tileset = ScriptableObject.CreateInstance<Tileset>();
            tileset.TilesetName = tilesetName;
            tileset.Properties = new TilesetProperties();
            XDocument document = XDocument.Load(tsxPath);
            XElement tilesetXml = document.Element("tileset");

            foreach (XElement tileXml in tilesetXml.Descendants("tile"))
            {
                XElement propertiesXml = tileXml.Element("properties");
                if (propertiesXml != null)
                {
                    int id = int.Parse(tileXml.Attribute("id").Value);
                    List<TiledProperty> properties = new List<TiledProperty>();
                    tileset.Properties[id] = properties;
                    foreach (XElement propertyXml in propertiesXml.Descendants("property"))
                    {
                        TiledProperty property = new TiledProperty();
                        properties.Add(property);
                        property.Key = propertyXml.Attribute("name").Value;
                        property.Value = propertyXml.Attribute("value").Value;
                    }
                }
            }

            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(tileset, assetPath);
        }
    }
}
