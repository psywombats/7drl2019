using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tiled2Unity
{
    [System.Serializable]
    public class TiledProperty : System.Object
    {
        public string Key;
        public string Value;

        public bool GetBoolValue()
        {
            return Value.Length > 0 && Value != "false";
        }

        public int GetIntValue()
        {
            return int.Parse(Value);
        }

        public float GetFloatValue()
        {
            return float.Parse(Value);
        }

        public string GetStringValue()
        {
            return Value;
        }

        public Color GetColorValue()
        {
            Color color = Color.cyan;
            ColorUtility.TryParseHtmlString(Value, out color);
            return color;
        } 
    }
}
