using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Tiled2Unity
{
    [System.Serializable]
    public class LinkedTileset : System.Object
    {
        public int firstGid;
        public Tileset tileset;
    }

    public class TiledMap : MonoBehaviour
    {
        public enum MapOrientation
        {
            Orthogonal,
            Isometric,
            Staggered,
            Hexagonal,
        }

        public enum MapStaggerAxis
        {
            X,
            Y,
        }

        public enum MapStaggerIndex
        {
            Odd,
            Even,
        }

        public MapOrientation Orientation = MapOrientation.Orthogonal;
        public MapStaggerAxis StaggerAxis = MapStaggerAxis.X;
        public MapStaggerIndex StaggerIndex = MapStaggerIndex.Odd;
        public int HexSideLength = 0;

        public int NumLayers = 0;
        public int NumTilesWide = 0;
        public int NumTilesHigh = 0;
        public int TileWidth = 0;
        public int TileHeight = 0;
        public float ExportScale = 1.0f;

        // Note: Because maps can be isometric and staggered we simply can't multiply tile width (or height) by number of tiles wide (or high) to get width (or height)
        // We rely on the exporter to calculate the width and height of the map
        public int MapWidthInPixels = 0;
        public int MapHeightInPixels = 0;

        // Background color could be used to set the camera clear color to get the same effect as in Tiled
        public Color BackgroundColor = Color.black;

        // All tilesets referenced by this map
        public List<LinkedTileset> Tilesets;

        public Tiled2Unity.Layer GetTileLayerNamed(string layerName)
        {
            foreach (Transform childTransform in transform)
            {
                GameObject gameObject = childTransform.gameObject;
                Tiled2Unity.Layer layer = gameObject.GetComponent<TileLayer>();
                if (layer != null)
                {
                    return layer;
                }
            }
            return null;
        }

        public TiledProperty GetPropertyForTile(string propertyName, TileLayer layer, int x, int y)
        {
            if (x < 0 || y < 0 || x >= NumTilesWide || y >= NumTilesHigh)
            {
                return null;
            }
            int tileId = layer.TerrainIds[y * NumTilesWide + x];
            if (tileId <= 0)
            {
                return null;
            }
            LinkedTileset tileset = GetTilesetForTileId(tileId);
            return tileset.tileset.PropertyForTile(tileId - tileset.firstGid, propertyName);
        }

        public LinkedTileset GetTilesetForTileId(int tileId)
        {
            // really that should be a dictionary return type
            int bestRelativeGid = int.MaxValue;
            LinkedTileset bestTileset = null;
            foreach (LinkedTileset tileset in Tilesets)
            {
                int relativeGid = tileId - tileset.firstGid;
                if (relativeGid < 0 || relativeGid > bestRelativeGid)
                {
                    continue;
                }
                bestTileset = tileset;
                bestRelativeGid = relativeGid;
            }
            return bestTileset;
        }

        public float GetMapWidthInPixelsScaled()
        {
            return this.MapWidthInPixels * this.transform.lossyScale.x * this.ExportScale;
        }

        public float GetMapHeightInPixelsScaled()
        {
            return this.MapHeightInPixels * this.transform.lossyScale.y * this.ExportScale;
        }

        public Rect GetMapRect()
        {
            Vector2 pos_w = this.gameObject.transform.position;
            float width = this.MapWidthInPixels;
            float height = this.MapHeightInPixels;
            return new Rect(pos_w.x, pos_w.y - height, width, height);
        }

        public Rect GetMapRectInPixelsScaled()
        {
            Vector2 pos_w = this.gameObject.transform.position;
            float widthInPixels = GetMapWidthInPixelsScaled();
            float heightInPixels = GetMapHeightInPixelsScaled();
            return new Rect(pos_w.x, pos_w.y - heightInPixels, widthInPixels, heightInPixels);
        }

        public bool AreTilesStaggered()
        {
            // Hex and Iso Staggered maps both use "staggered" tiles
            return this.Orientation == MapOrientation.Staggered || this.Orientation == MapOrientation.Hexagonal;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 pos_w = this.gameObject.transform.position;
            Vector3 topLeft = Vector3.zero + pos_w;
            Vector3 topRight = new Vector3(GetMapWidthInPixelsScaled(), 0) + pos_w;
            Vector3 bottomRight = new Vector3(GetMapWidthInPixelsScaled(), -GetMapHeightInPixelsScaled()) + pos_w;
            Vector3 bottomLeft = new Vector3(0, -GetMapHeightInPixelsScaled()) + pos_w;

            // To make gizmo visible, even when using depth-shader shaders, we decrease the z depth by the number of layers
            float depth_z = -1.0f * this.NumLayers;
            pos_w.z += depth_z;
            topLeft.z += depth_z;
            topRight.z += depth_z;
            bottomRight.z += depth_z;
            bottomLeft.z += depth_z;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }
    }
}
