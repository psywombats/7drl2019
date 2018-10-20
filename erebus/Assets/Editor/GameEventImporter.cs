using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiled2Unity;
using System;

[Tiled2Unity.CustomTiledImporter]
public class GameEventImporter : TiledImporter {

    override public void HandleCustomProperties(UnityEngine.GameObject prefab, IDictionary<string, string> props) {
        if (prefab.GetComponent<RuntimeTmxObject>() != null) {
            //string type = prefab.GetComponent<RuntimeTmxObject>().TmxType.ToLower();
            //switch (type) {
            //    case "chest":
            //        Populate<ChestEvent>(prefab, props);
            //        break;
            //}
        }
    }
}
