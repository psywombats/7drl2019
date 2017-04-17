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
            int startIndex = tmxPath.LastIndexOf("Maps/") + "Maps/".Length;
            string mapPath = tmxPath.Substring(startIndex, tmxPath.Length - (startIndex + ".tmx".Length));
            string xmlPath = "Assets/Tiled2Unity/Imported/" + mapPath + ".tiled2unity.xml";
            string parentDirectory = "";
            if (mapPath.LastIndexOf('/') >= 0) {
                parentDirectory = mapPath.Substring(0, mapPath.LastIndexOf('/'));
                System.IO.Directory.CreateDirectory("Assets/Tiled2Unity/Imported/" + parentDirectory);
            }

            List<string> args = new List<string>();

            Environment.SetEnvironmentVariable("TILED2UNITY_TMXPATH", tmxPath);
            Environment.SetEnvironmentVariable("TILED2UNITY_UNITYDIR", Application.dataPath + "/Tiled2Unity");

            args.Insert(0, tmxPath);
            args.Insert(1, Application.dataPath + "/Tiled2Unity");
            args.Insert(2, "--depth-buffer");
            args.Insert(3, "--parent-directory=" + parentDirectory);

            Tiled2UnityLite.Run(args.ToArray());
            
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(xmlPath);
        }
    }
}
