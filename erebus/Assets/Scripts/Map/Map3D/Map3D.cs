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
        TiledMap tiledMap = GetComponent<TiledMap>();
        Map map = GetComponent<Map>();
        for (int i = 0; i < tiledMap.NumLayers; i += 1) {
            Layer layer = map.LayerAtIndex(i);
            if (layer.properties[@"wall"] != null) {
                BuildWallsForLayer((TileLayer)layer);
            }
        }
    }

    private void BuildWallsForLayer(TileLayer layer) {
        TiledMap tiledMap = GetComponent<TiledMap>();
        for (int x = 0; x < tiledMap.NumTilesWide; x += 1) {
            for (int y = 0; y < tiledMap.NumTilesHigh; y += 1) {
                TiledProperty wallSeqProperty = tiledMap.GetPropertyForTile("wall", layer, x, y);
                if (wallSeqProperty == null) {
                    continue;
                }

                string wallSeq = wallSeqProperty.GetStringValue();
                string[] splitSeq = wallSeq.Split(new Char[] { ',' });

                List<int> tileIds = new List<int>();
                int nativeTileId = layer.TerrainIds[x + y * tiledMap.NumTilesWide];
                foreach (string stringId in splitSeq) {
                    tileIds.Add(int.Parse(stringId));
                }

                LinkedTileset linkedTileset = tiledMap.GetTilesetForTileId(nativeTileId);
                Material textureAtlas = Resources.Load<Material>("Materials/" + linkedTileset.tileset.name);
                tileIds.Insert(0, nativeTileId - linkedTileset.firstGid);
                
                foreach (int tileId in tileIds) {

                }
            }
        }
    }
}
