using System;
using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;

/**
 * 3D map class for Tiled maps that get converted into dungeon crawl style scenes
 */
[RequireComponent(typeof(TiledMap))]
public class Map3D : TiledInstantiated {

    public override void Populate(IDictionary<string, string> properties) {
        Map map = GetComponent<Map>();
    }
}
