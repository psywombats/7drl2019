using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tiled2Unity
{
    [System.Serializable]
    public class TiledProperty : System.Object
    {
        public string key;
        public string value;

        public bool GetBoolValue()
        {
            return value.Length > 0 && value != "false";
        }

        public int GetIntValue()
        {
            return int.Parse(value);
        }

        public float GetFloatValue()
        {
            return float.Parse(value);
        }

        public string GetStringValue()
        {
            return value;
        }

        public Color GetColorValue()
        {
            Color color = Color.cyan;
            ColorUtility.TryParseHtmlString(value, out color);
            return color;
        } 
    }
}
