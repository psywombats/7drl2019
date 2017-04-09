using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace Tiled2Unity
{
    partial class ImportTiled2Unity
    {
        public void TmxImported(string tmxPath)
        {
            List<string> args = new List<string>();

            Environment.SetEnvironmentVariable("TILED2UNITY_TMXPATH", tmxPath);
            Environment.SetEnvironmentVariable("TILED2UNITY_UNITYDIR", Application.dataPath + "/Tiled2Unity");

            args.Insert(0, tmxPath);
            args.Insert(1, Application.dataPath + "/Tiled2Unity");

            Tiled2UnityLite.Run(args.ToArray());

            string mapName = tmxPath.Substring(tmxPath.LastIndexOf('/') + 1, tmxPath.Length - tmxPath.LastIndexOf(".tmx"));
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset("Assets/Tiled2Unity/Imported/" + mapName + ".tiled2unity.xml");
        }
    }
}
