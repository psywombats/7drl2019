using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("")]
public class MeshEdit : MonoBehaviour
{
    [System.Serializable]
    public class UVData : System.Object
    {
        [SerializeField, HideInInspector]
        public string name;
        [SerializeField, HideInInspector]
        public int texWidth;
        [SerializeField, HideInInspector]
        public int texHeight;
        [SerializeField, HideInInspector]
        public int tileWidth;
        [SerializeField, HideInInspector]
        public int tileHeight;
        [SerializeField, HideInInspector]
        public int tileOutline;
        // Used for temp storage in editor functions
        [SerializeField, HideInInspector]
        public Vector2[] _newUvs;
        public Vector2[] newUvs
        {
            get
            {
                if (_newUvs == null)
                {
                    _newUvs = new Vector2[vertCount];
                    for (int i = 0; i < _newUvs.Length; i++)
                    {
                        getNeutralCoord(i, ref _newUvs[i].x, ref _newUvs[i].y);
                    }
                }
                else if (_newUvs.Length != vertCount)
                {
                    Vector2[] newArray = new Vector2[vertCount];

                    for (int i = 0; i < newArray.Length; i++)
                    {
                        if (i >= _newUvs.Length)
                        {
                            getNeutralCoord(i, ref newArray[i].x, ref newArray[i].y);
                        }
                        else
                        {
                            newArray[i] = _newUvs[i];
                        }
                    }

                    _newUvs = newArray;
                }

                return _newUvs;
            }
            set
            {
                _newUvs = value;
            }
        }
        [SerializeField, HideInInspector]
        public Vector2[] _uvs;
        public Vector2[] uvs
        {
            get
            {
                float tx = 1.0f / (texWidth / tileWidth);
                float ty = 1.0f / (texHeight / tileHeight);
                if (_uvs == null)
                {
                    _uvs = new Vector2[vertCount];

                    for (int i = 0; i < _uvs.Length; i++)
                    {
                        getNeutralCoord(i, ref _uvs[i].x, ref _uvs[i].y);
                    }
                }
                else if (_uvs.Length != vertCount)
                {
                    Vector2[] newArray = new Vector2[vertCount];

                    for (int i = 0; i < newArray.Length; i++)
                    {
                        if (i >= _uvs.Length)
                        {
                            getNeutralCoord(i, ref newArray[i].x, ref newArray[i].y);
                        }
                        else
                        {
                            newArray[i] = _uvs[i];
                        }
                    }

                    _uvs = newArray;
                }

                return _uvs;
            }
            set
            {
                _uvs = value;
            }
        }

        public Vector2[] defaultUVs;

        public void getNeutralCoord(int i, ref float x, ref float y)
        {
            float tx = (float)tileOutline / texWidth;
            float ty = 1.0f - (float)tileOutline / texHeight;

            float tw = (float)tileWidth / texWidth;
            float th = (float)tileHeight / texHeight;


            if (defaultUVs != null && i < defaultUVs.Length)
            {
                x = defaultUVs[i].x;
                y = defaultUVs[i].y;
            }
            else
            {
                if (i % 4 == 0)
                {
                    x = tx;
                    y = ty;
                }
                else if (i % 4 == 1)
                {
                    x = tx + tw;
                    y = ty;
                }
                else if (i % 4 == 2)
                {
                    x = tx;
                    y = ty - th;
                }
                else if (i % 4 == 3)
                {
                    x = tx + tw;
                    y = ty - th;
                }
            }

        }
        [SerializeField, HideInInspector]
        private int[] _uvAnimationLength;
        public int[] uvAnimationLength
        {
            get
            {
                if (_uvAnimationLength == null)
                {
                    _uvAnimationLength = new int[vertCount];
                }
                else if (_uvAnimationLength.Length != vertCount)
                {
                    int[] newArray = new int[vertCount];
                    for (int i = 0; i < newArray.Length; i++)
                    {
                        if (i >= _uvAnimationLength.Length)
                        {
                            newArray[i] = 0;
                        }
                        else
                        {
                            newArray[i] = _uvAnimationLength[i];
                        }
                    }
                    _uvAnimationLength = newArray;
                }
                return _uvAnimationLength;
            }
            set
            {
                _uvAnimationLength = value;
            }
        }
        [SerializeField, HideInInspector]
        public int[] _uvAnimationStartPosition;
        public int[] uvAnimationStartPosition
        {
            get
            {
                if (_uvAnimationStartPosition == null)
                {
                    _uvAnimationStartPosition = new int[vertCount];
                }
                else if (_uvAnimationStartPosition.Length != vertCount)
                {
                    int[] newArray = new int[vertCount];
                    for (int i = 0; i < newArray.Length; i++)
                    {
                        if (i >= _uvAnimationStartPosition.Length)
                        {
                            newArray[i] = 0;
                        }
                        else
                        {
                            newArray[i] = _uvAnimationStartPosition[i];
                        }
                    }
                    _uvAnimationStartPosition = newArray;
                }
                return _uvAnimationStartPosition;
            }
            set
            {
                _uvAnimationStartPosition = value;
            }
        }
        [SerializeField, HideInInspector]
        public int[] _uvAnimationIndex;
        public int[] uvAnimationIndex
        {
            get
            {
                if (_uvAnimationIndex == null)
                {
                    _uvAnimationIndex = new int[vertCount];
                }
                else if (_uvAnimationIndex.Length != vertCount)
                {
                    int[] newArray = new int[vertCount];
                    for (int i = 0; i < newArray.Length; i++)
                    {
                        if (i >= _uvAnimationIndex.Length)
                        {
                            newArray[i] = 0;
                        }
                        else
                        {
                            newArray[i] = _uvAnimationIndex[i];
                        }
                    }
                    _uvAnimationIndex = newArray;
                }
                return _uvAnimationIndex;
            }
            set
            {
                _uvAnimationIndex = value;
            }
        }
        [SerializeField, HideInInspector]
        public int[] _uvAnimationQuadPoint;
        public int[] uvAnimationQuadPoint
        {
            get
            {
                if (_uvAnimationQuadPoint == null)
                {
                    _uvAnimationQuadPoint = new int[vertCount];
                }
                else if (_uvAnimationQuadPoint.Length != vertCount)
                {
                    int[] newArray = new int[vertCount];
                    for (int i = 0; i < newArray.Length; i++)
                    {
                        if (i >= _uvAnimationQuadPoint.Length)
                        {
                            newArray[i] = 0;
                        }
                        else
                        {
                            newArray[i] = _uvAnimationQuadPoint[i];
                        }
                    }
                    _uvAnimationQuadPoint = newArray;
                }
                return _uvAnimationQuadPoint;
            }
            set
            {
                _uvAnimationQuadPoint = value;
            }
        }
        [SerializeField, HideInInspector]
        int animationSpeed;
        [SerializeField, HideInInspector]
        int animationTimer;
        [SerializeField, HideInInspector]
        public int vertCount;

        public UVData(string name, int texWidth, int texHeight, int tileWidth, int tileHeight, int tileOutline, int vertCount, Vector2[] defaultUVs = null)
        {
            if (defaultUVs != null)
            {
                this.defaultUVs = defaultUVs;
            }
            this.name = name;
            this.texWidth = texWidth;
            this.texHeight = texHeight;
            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            this.tileOutline = tileOutline;
            animationSpeed = 4;
            animationTimer = 0;

            this.vertCount = vertCount;
        }

        public void resizeUVSpace(int texWidth, int texHeight, int tileWidth, int tileHeight, int tileOutline)
        {
            // TODO: When the same texture is updated, compare the previous texture width/height and re-size the UVs so that the texture isn't warped.
            if (texWidth != 0 && texHeight != 0 && tileWidth != 0 && tileHeight != 0 &&
                (texWidth != this.texWidth || texHeight != this.texHeight || tileOutline != this.tileOutline ||
                tileWidth != this.tileWidth || tileHeight != this.tileHeight))
            {
                float xPixel = 1.0f / texWidth;
                float yPixel = 1.0f / texHeight;

                float wRatio = this.texWidth / (float)texWidth;
                float hRatio = this.texHeight / (float)texHeight;
                for (int i = 0; i < uvs.Length; i++)
                {

                    // Remove original outline from original uv coordinate
                    float x = uvs[i].x * this.texWidth;
                    float y = (1.0f - uvs[i].y) * this.texHeight;

                    float newX = 0;
                    float newY = 0;

                    float thisTotalTileWidth = (this.tileWidth + this.tileOutline * 2);

                    float thisTotalTileHeight = (this.tileHeight + this.tileOutline * 2);

                    float midX = (int)(x / thisTotalTileWidth) * thisTotalTileWidth + (thisTotalTileWidth / 2.0f);
                    float midY = (int)(y / thisTotalTileHeight) * thisTotalTileHeight + (thisTotalTileHeight / 2.0f);

                    float xCoord = (int)(x / (this.tileWidth + this.tileOutline * 2));
                    float yCoord = (int)(y / (this.tileHeight + this.tileOutline * 2));

                    float newMidX = xCoord * (tileWidth + tileOutline * 2) + (tileWidth / 2.0f + tileOutline);
                    float newMidY = yCoord * (tileHeight + tileOutline * 2) + (tileHeight / 2.0f + tileOutline);

                    if (x < midX)
                    {
                        x -= this.tileOutline;
                        newX = newMidX - tileWidth / 2.0f;
                    }
                    else
                    {
                        x += this.tileOutline;
                        newX = newMidX + tileWidth / 2.0f;
                    }
                    if (y > midY)
                    {
                        y -= this.tileOutline;
                        newY = newMidY + tileHeight / 2.0f;
                    }
                    else
                    {
                        y += this.tileOutline;
                        newY = newMidY - tileHeight / 2.0f;
                    }

                    newX = newX / texWidth;
                    newY = newY / texHeight;
                    uvs[i].x = newX;
                    uvs[i].y = 1.0f - newY;

                    newUvs[i].x = newX;
                    newUvs[i].y = 1.0f - newY;

                    /*

                    float tileX = this.texWidth - uvs[i].x * this.texWidth;
                    float newTileX = (texWidth - tileX) / texWidth;
                    float tileY = this.texHeight - uvs[i].y * this.texHeight;
                    float newTileY = (texHeight - tileY) / texHeight;

                    uvs[i].x = newTileX;
                    uvs[i].y = newTileY;*/
                }
                this.texHeight = texHeight;
                this.texWidth = texWidth;
                this.tileWidth = tileWidth;
                this.tileHeight = tileHeight;
                this.tileOutline = tileOutline;

            }


        }

        public void resizeUVLength(int vertCount, int[] newVerts = null)
        {
            Console.WriteLine("ResizeUVLength() Called...");
            Vector2[] resizedUVs = new Vector2[vertCount];

            this.vertCount = vertCount;

            Vector2[] updateV2 = uvs;
            updateV2 = newUvs;
            int[] update = uvAnimationLength;
            update = uvAnimationQuadPoint;
            update = uvAnimationIndex;
            update = uvAnimationStartPosition;

            if (newVerts != null)
            {
                Console.WriteLine("-uvs changed");
                Console.WriteLine("-newUvs changed");
                for (int i = 0; i < newVerts.Length; i++)
                {
                    uvs[vertCount - newVerts.Length + i] = uvs[newVerts[i]];
                    newUvs[vertCount - newVerts.Length + i] = newUvs[newVerts[i]];
                }
            }
        }

        public Vector2 getCoordsOfTile(Rect rect, int tileWidth, int tileHeight, int texWidth, int texHeight, int tileOutline)
        {
            Vector2 coords = new Vector2(0, 0);
            float unitsX = (tileWidth + tileOutline * 2);
            float unitsY = (tileHeight + tileOutline * 2);
            coords.x = (rect.x * tileWidth - tileOutline) / unitsX;
            coords.y = (rect.y * tileHeight - tileOutline) / unitsY;

            return coords;
        }
        public Rect getTile(int x, int y, int tileWidth, int tileHeight, int texWidth, int texHeight, int tileOutline)
        {
            Rect tile = new Rect(0, 0, 0, 0);

            tile.x = (x * (tileWidth + tileOutline * 2) + tileOutline) / texWidth;
            tile.y = (y * (tileHeight + tileOutline * 2) + tileOutline) / texHeight;
            tile.width = (tileWidth) / texWidth;
            tile.height = (tileHeight) / texHeight;

            return tile;
        }

        public void update(Mesh mesh)
        {
            if (uvAnimationLength == null ||
                uvAnimationLength.Length != uvs.Length)
            {
                return;
            }
            if (uvAnimationIndex == null ||
                uvAnimationIndex.Length != uvs.Length)
            {
                return;
            }
            if (uvAnimationStartPosition == null ||
                uvAnimationStartPosition.Length != uvs.Length)
            {
                return;
            }

            if (uvs != null &&
                uvAnimationLength != null &&
                uvAnimationIndex != null &&
                uvAnimationStartPosition != null)
            {
                
                if (uvs.Length != mesh.vertices.Length)
                {
                    uvAnimationLength = null;
                    uvAnimationIndex = null;
                    uvAnimationQuadPoint = null;
                    uvAnimationStartPosition = null;
                    uvs = null;
                    newUvs = null;
                    return;
                }

                animationTimer++;

                if (animationTimer > animationSpeed)
                {
                    float xUnit = 1.0f / (texWidth / (tileWidth + tileOutline * 2));
                    float yUnit = 1.0f / (texHeight / (tileHeight + tileOutline * 2));

                    animationTimer = 0;

                    newUvs = new Vector2[uvs.Length];
                    for (int i = 0; i < uvs.Length; i++)
                    {
                        newUvs[i] = uvs[i];

                        if (uvAnimationLength[i] > 1)
                        {
                            uvAnimationIndex[i]++;
                            if (uvAnimationIndex[i] >= uvAnimationLength[i])
                            {
                                int tilesPerRow = texWidth / (tileWidth + tileOutline * 2);
                                int tilesPerColumn = texHeight / (tileHeight + tileOutline * 2);
                                float x = (uvAnimationStartPosition[i] % tilesPerRow) / (float)tilesPerRow + tileOutline / (float)texWidth;
                                float y = (uvAnimationStartPosition[i] / tilesPerRow) / (float)tilesPerColumn + tileOutline / (float)texHeight;



                                // This stretches out the quads to fit the UV tile
                                float qOffsetX = 0;
                                if (uvAnimationQuadPoint[i] == 1 || uvAnimationQuadPoint[i] == 2)
                                {
                                    qOffsetX = xUnit - (tileOutline * 2 / (float)texWidth);
                                }
                                float qOffsetY = 0;
                                if (uvAnimationQuadPoint[i] == 0 || uvAnimationQuadPoint[i] == 1)
                                {
                                    qOffsetY = yUnit - (tileOutline * 2 / (float)texHeight);
                                }


                                newUvs[i].x = x + qOffsetX;
                                newUvs[i].y = y + qOffsetY;

                                uvAnimationIndex[i] = 0;
                            }
                            else
                            {
                                // qOffset is the tile Width offset from the root position of the UV
                                float qOffset = 0;
                                if (uvAnimationQuadPoint[i] == 1 || uvAnimationQuadPoint[i] == 2)
                                {
                                    qOffset = xUnit - (tileOutline * 2 / (float)texWidth);
                                }

                                newUvs[i].x += xUnit;

                                if (newUvs[i].x - qOffset >= 1.0f)
                                {
                                    newUvs[i].x = qOffset + tileOutline / (float)texWidth;
                                    newUvs[i].y -= yUnit;
                                }
                            }
                        }
                    }

                    mesh.uv = newUvs;
                    uvs = newUvs;
                }
            }
        }

        public void intialise(int length)
        {

            if (uvAnimationLength == null ||
                uvAnimationLength.Length != length)
            {
                uvAnimationLength = new int[length];
            }
            if (uvAnimationStartPosition == null ||
                uvAnimationStartPosition.Length != length)
            {
                uvAnimationStartPosition = new int[length];
            }
            if (uvAnimationIndex == null ||
                uvAnimationIndex.Length != length)
            {
                uvAnimationIndex = new int[length];
            }
            if (uvAnimationQuadPoint == null ||
                uvAnimationQuadPoint.Length != length)
            {
                uvAnimationQuadPoint = new int[length];
            }
        }



        public void removeVert(int index)
        {
            if (newUvs != null && newUvs.Length > 0)
            {
                Vector2[] newUvs2 = new Vector2[newUvs.Length - 1];
                Vector2[] uvs2 = new Vector2[uvs.Length - 1];
                bool hasAnimInfo = false;

                int[] uvAnimationLength2 = new int[0];
                int[] uvAnimationStartPosition2 = new int[0];
                int[] uvAnimationIndex2 = new int[0];
                int[] uvAnimationQuadPoint2 = new int[0];
                if (uvAnimationLength != null && uvAnimationLength.Length > 0)
                {
                    hasAnimInfo = true;
                    uvAnimationLength2 = new int[uvAnimationLength.Length - 1];
                    uvAnimationStartPosition2 = new int[uvAnimationStartPosition.Length - 1];
                    uvAnimationIndex2 = new int[uvAnimationIndex.Length - 1];
                    uvAnimationQuadPoint2 = new int[uvAnimationQuadPoint.Length - 1];
                }
                int c = 0;
                for (int i = 0; i < uvs.Length; i++)
                {
                    if (i == index)
                    {
                        i++;
                    }
                    if (i < uvs.Length)
                    {
                        if (hasAnimInfo)
                        {
                            newUvs2[c] = newUvs[i];
                            uvs2[c] = uvs[i];
                            uvAnimationLength2[c] = uvAnimationLength[i];
                            uvAnimationStartPosition2[c] = uvAnimationStartPosition[i];
                            uvAnimationIndex2[c] = uvAnimationIndex[i];
                            uvAnimationQuadPoint2[c] = uvAnimationQuadPoint[i];
                        }
                        c++;
                    }
                }

                newUvs = newUvs2;
                uvs = uvs2;
                uvAnimationLength = uvAnimationLength2;
                uvAnimationStartPosition = uvAnimationStartPosition2;
                uvAnimationIndex = uvAnimationIndex2;
                uvAnimationQuadPoint = uvAnimationQuadPoint2;
                vertCount--;
            }
        }
    }

    public class Triangle
    {
        public Vector3 n;
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
        public Vector3 center;

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;

            // If the points are very small, they may require scaling to ensure they're not rounded to zero during the cross product process.
            Vector3 bc = (b - a) * 1000;
            Vector3 ab = (c - a) * 1000;

            n = Vector3.Cross(bc, ab);

            n = n.normalized;

            recalculateCenter();
        }

        public void scale(Vector3 s)
        {
            a.x *= s.x;
            a.y *= s.y;
            a.z *= s.z;

            b.x *= s.x;
            b.y *= s.y;
            b.z *= s.z;

            c.x *= s.x;
            c.y *= s.y;
            c.z *= s.z;

            recalculateCenter();
        }

        public void translate(Vector3 t)
        {
            a += t;
            b += t;
            c += t;

            recalculateCenter();
        }

        public void rotate(Vector3 r)
        {
            a = Quaternion.Euler(r) * a;
            b = Quaternion.Euler(r) * b;
            c = Quaternion.Euler(r) * c;
            n = Quaternion.Euler(r) * n;

            recalculateCenter();
        }
        public void rotate(Quaternion r)
        {
            a = r * a;
            b = r * b;
            c = r * c;
            n = r * n;

            recalculateCenter();
        }


        public void rotate(Vector3 r, Vector3 origin)
        {
            translate(-origin);
            a = Quaternion.Euler(r) * a;
            b = Quaternion.Euler(r) * b;
            c = Quaternion.Euler(r) * c;
            n = Quaternion.Euler(r) * n;

            recalculateCenter();

            translate(origin);
        }

        public void recalculateCenter()
        {
            center = (a + b + c) / 3;
        }

        public string ToString(string format = "f4")
        {
            return ("a: " + a.ToString(format) + " b: " + b.ToString(format) + " c: " + c.ToString(format) + " n: " + n.ToString(format));
        }
    }

    [System.Serializable]
    public class ListWrapper
    {
        [SerializeField, HideInInspector]
        public List<int> list;

        public ListWrapper()
        {
            list = new List<int>();
        }

        public ListWrapper(int[] array)
        {
            list = new List<int>(array);
        }

        public ListWrapper(List<int> list)
        {
            this.list = list;
        }

        public void Add(int item)
        {
            list.Add(item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }
        public void Remove(int item)
        {
            list.Remove(item);
        }

        public bool Contains(int item)
        {
            return list.Contains(item);
        }

        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        public void Clear()
        {
            list.Clear();
        }
    }
    /*
    [System.Serializable]
    public class QuickMesh
    {
        [NonSerialized]
        private Mesh _mesh;

        public Mesh mesh
        {
            get
            {
                if (_mesh == null)
                {
                    _mesh = new Mesh();
                }

                _mesh.vertices = vertices;
                _mesh.normals = normals;
                _mesh.uv = uvs;
                _mesh.colors = colours;
                _mesh.triangles = triangles;

                return mesh;
            }
        }

        [SerializeField, HideInInspector]
        public Vector3[] vertices;
        [SerializeField, HideInInspector]
        public Vector3[] normals;
        [SerializeField, HideInInspector]
        public Vector2[] uvs;
        [SerializeField, HideInInspector]
        public Color[] colours;
        [SerializeField, HideInInspector]
        public int[] triangles;
    }*/

    // UV Information
    [SerializeField, HideInInspector]
    public List<UVData> _uvMaps;
    public List<UVData> uvMaps
    {
        get
        {
            if (_uvMaps == null)
            {
                _uvMaps = new List<UVData>();
            }
            return _uvMaps;
        }
        set
        {
            _uvMaps = value;
        }
    }

    [SerializeField, HideInInspector]
    public bool hasDefaultUVs;
    [SerializeField, HideInInspector]
    public Vector2[] defaultUVs;

    [SerializeField, HideInInspector]
    public int currentUVMap;
    [SerializeField, HideInInspector]
    public int lastTilesetUsed;

    [SerializeField, HideInInspector]
    public bool isTilesetRefreshRequired;

    // Geometry Information
    [SerializeField, HideInInspector]
    public Mesh _mesh;
    [SerializeField, HideInInspector]
    public Mesh mesh
    {
        get
        {
            if (_mesh == null)
            {
                pushNewGeometry();
            }

            return _mesh;
        }
        set
        {
            _mesh = value;
        }
    }
    [SerializeField, HideInInspector]
    public Mesh meshModified;
    [SerializeField, HideInInspector]
    public List<Vector3> verts;
    [SerializeField, HideInInspector]
    public List<Color> colours;
    [SerializeField, HideInInspector]
    public List<Vector3> faceNormals;
    [SerializeField, HideInInspector]
    public List<Vector3> vertNormals;
    [SerializeField, HideInInspector]
    public List<int> tris;
    [SerializeField, HideInInspector]
    public List<int> quads;
    [SerializeField, HideInInspector]
    public List<ListWrapper> connectedVerts;
    [HideInInspector]
    public const float minimumScale = 0.00001f;


    // Editor Information
    [SerializeField, HideInInspector]
    public int currentEditMode = 0;

    private bool isSelected = false;
    
    public enum DrawMode { GameView, WireFrame, WireFrameOpaque, Texture, VertexColour }
    [HideInInspector]
    DrawMode drawMode = DrawMode.GameView;

    [HideInInspector]
    private int vertMode;
    [HideInInspector]
    private bool[] selectedVerts;
    [HideInInspector]
    private bool[] selectedFaces;
    [SerializeField, HideInInspector]
    public bool isMeshTransparent;

    [HideInInspector]
    public float cutCount;
    [HideInInspector]
    public List<int> facesCut;
    [HideInInspector]
    public List<Vector3> cutsAB;

    public Vector3 quadCenter(int quad)
    {
        return (
            verts[quads[quad * 4 + 0]] +
            verts[quads[quad * 4 + 1]] +
            verts[quads[quad * 4 + 2]] +
            verts[quads[quad * 4 + 3]]) / 4;
    }

#if UNITY_EDITOR
    public void setVertMode(int vertMode, bool[] selectedVerts, bool[] selectedFaces)
    {
        this.vertMode = vertMode;
        if (vertMode == 0)
        {
            this.selectedVerts = selectedVerts;
        }
        else if (vertMode == 1)
        {
            this.selectedFaces = selectedFaces;
        }
    }

    
    [SerializeField, HideInInspector]
    private Material _material;
    private Material material
    {
        get
        {
            if (_material == null)
            {
                _material = new Material(Shader.Find("Particles/Standard Unlit"));
            }
            return _material;
        }
        set
        {
            _material = value;
        }
    }
    


    void Update()
    {
        if (updateSelectedStates())
        {
            isSelected = true;

            if (currentEditMode != 0)
            {
                Tools.hidden = true;
            }
            else
            {
                checkTransforms();
                Tools.hidden = false;
            }
        }
        
        if (uvMaps != null &&
            currentUVMap >= 0 &&
            currentUVMap < uvMaps.Count)
        {
            uvMaps[currentUVMap].update(mesh);
        }


    }


    public void setToEditorPosition()
    {
        editorPosition = transform.position;
        editorScale = transform.localScale;
        editorRotation = transform.rotation;
    }

    public bool updateSelectedStates()
    {
        bool isSelected = !(Selection.activeTransform == null ||
            Selection.activeTransform.gameObject != gameObject);
        if (!isSelected)
        {
            this.isSelected = false;

            vertMode = -1;
            selectedFaces = null;
            selectedVerts = null;

            cutsAB = null;
            cutCount = 0;
            facesCut = null;

            currentEditMode = 0;
            checkTransforms();

            isMeshTransparent = false;

        }
        GetComponent<MeshRenderer>().enabled = !isMeshTransparent || currentEditMode == 0 || currentEditMode == 2;

        return isSelected;
    }


    public void setDrawMode(DrawMode drawMode)
    {
        if (this.drawMode != drawMode)
        {
            this.drawMode = drawMode;

            if (drawMode == DrawMode.GameView)
            {
                GetComponent<MeshRenderer>().enabled = true;
            }
            else if (drawMode == DrawMode.WireFrame)
            {
                GetComponent<MeshRenderer>().enabled = false;
            }

        }
    }

    [SerializeField, HideInInspector]
    public int drawNormals = 0;
    [SerializeField, HideInInspector]
    public int paintMode = 0;
    [SerializeField, HideInInspector]
    public float normalLength = 0.5f;
    Color orange = new Color(0.9f, 0.25f, 0.08f);

    
    public void OnDrawGizmos()
    {
        if (updateSelectedStates())
        {
            if (quads != null && quads.Count > 0)
            {
                Gizmos.color = Color.black;
                float r = 1.5f;
                float d;
                Vector2 sp;
                Vector3 spR, spL;
                //Debug.Log("Time since last frame: " + Time.deltaTime);

                for (int i = 0; i < quads.Count; i += 4)
                {
                    if (currentEditMode == 1 && vertMode == 1 && selectedFaces != null)
                    {
                        if (selectedFaces[i / 4])
                        {
                            Gizmos.color = Color.white;
                        }
                        else
                        {
                            Gizmos.color = Color.black;
                        }
                        Vector3 q = (
                            verts[quads[i + 0]] +
                            verts[quads[i + 1]] +
                            verts[quads[i + 2]] +
                            verts[quads[i + 3]]) * 0.25f;

                        d = Vector3.Distance(SceneView.lastActiveSceneView.camera.transform.position, q);
                        sp = HandleUtility.WorldToGUIPoint(verts[i]);
                        spR = HandleUtility.GUIPointToWorldRay(verts[i] + new Vector3(r, 0, 0)).GetPoint(d);
                        spL = HandleUtility.GUIPointToWorldRay(verts[i] + new Vector3(-r, 0, 0)).GetPoint(d);
                        //Graphics.DrawMesh(cube, q, Quaternion.identity, material, 0);
                        Gizmos.DrawCube(q, Vector3.one * Vector3.Distance(spR, spL));
                    }
                    if (currentEditMode == 3 && paintMode == 1)
                    {
                        Vector3 q = (
                            verts[quads[i + 0]] +
                            verts[quads[i + 1]] +
                            verts[quads[i + 2]] +
                            verts[quads[i + 3]]) * 0.25f;

                        d = Vector3.Distance(SceneView.lastActiveSceneView.camera.transform.position, q);
                        sp = HandleUtility.WorldToGUIPoint(verts[i]);
                        spR = HandleUtility.GUIPointToWorldRay(verts[i] + new Vector3(r, 0, 0)).GetPoint(d);
                        spL = HandleUtility.GUIPointToWorldRay(verts[i] + new Vector3(-r, 0, 0)).GetPoint(d);

                        Gizmos.color = (colours[i + 0] + colours[i + 1] + colours[i + 2] + colours[i + 3]) / 4.0f;
                        Gizmos.DrawCube(q, Vector3.one * Vector3.Distance(spR, spL));
                    }
                    Gizmos.color = Color.black;
                    Gizmos.DrawLine(verts[quads[i + 2]], verts[quads[i + 0]]);
                    Gizmos.DrawLine(verts[quads[i + 0]], verts[quads[i + 1]]);
                    Gizmos.DrawLine(verts[quads[i + 3]], verts[quads[i + 1]]);
                    Gizmos.DrawLine(verts[quads[i + 3]], verts[quads[i + 2]]);
                }

                if ((vertMode == 0 && selectedVerts != null) || (currentEditMode == 3 && paintMode == 0))
                {
                    for (int i = 0; i < verts.Count; i++)
                    {
                        if (selectedVerts != null && selectedVerts.Length > i && selectedVerts[i])
                        {
                            Gizmos.color = Color.white;
                        }
                        else
                        {
                            Gizmos.color = Color.black;
                        }
                        d = Vector3.Distance(SceneView.lastActiveSceneView.camera.transform.position, verts[i]);
                        sp = HandleUtility.WorldToGUIPoint(verts[i]);
                        spR = HandleUtility.GUIPointToWorldRay(verts[i] + new Vector3(r, 0, 0)).GetPoint(d);
                        spL = HandleUtility.GUIPointToWorldRay(verts[i] + new Vector3(-r, 0, 0)).GetPoint(d);
                        //Graphics.DrawMesh(cube, verts[i], Quaternion.identity, material, 0);

                        if (currentEditMode == 3)
                        {
                            Gizmos.color = colours[i];
                        }
                        Gizmos.DrawCube(verts[i], Vector3.one * Vector3.Distance(spR, spL));
                    }

                }
                if (drawNormals > 0)
                {
                    if (drawNormals == 1)
                    {
                        Gizmos.color = Color.green;
                        for (int i = 0; i < quads.Count; i += 4)
                        {
                            Vector3 c = (verts[quads[i + 0]] + verts[quads[i + 1]] + verts[quads[i + 2]] + verts[quads[i + 3]]) / 4.0f;

                            Gizmos.DrawLine(c, c + faceNormals[i / 4] * normalLength);
                        }
                    }
                    else if (drawNormals == 2)
                    {
                        Gizmos.color = Color.blue;
                        for (int i = 0; i < vertNormals.Count; i++)
                        {
                            Gizmos.DrawLine(verts[i], verts[i] + vertNormals[i].normalized * normalLength);
                        }
                    }
                    else if (drawNormals == 3)
                    {

                        Gizmos.color = Color.magenta;

                        for (int i = 0; i < quads.Count; i += 4)
                        {
                            int tA = (i / 4) * 2 * 3;
                            int tB = ((i / 4) * 2 + 1) * 3;
                            Vector3 ab = verts[tris[tA + 1]] - verts[tris[tA + 0]];
                            Vector3 ac = verts[tris[tA + 2]] - verts[tris[tA + 0]];
                            Vector3 dc = verts[tris[tB + 1]] - verts[tris[tB + 0]];
                            Vector3 db = verts[tris[tB + 2]] - verts[tris[tB + 0]];

                            Vector3 a = Vector3.Cross(ab, ac);
                            Vector3 b = Vector3.Cross(dc, db);
                            Vector3 c = (verts[tris[tA + 0]] + verts[tris[tA + 1]] + verts[tris[tA + 2]]) / 3.0f;
                            Gizmos.DrawLine(c, c + a.normalized * normalLength);
                            c = (verts[tris[tB + 0]] + verts[tris[tB + 1]] + verts[tris[tB + 2]]) / 3.0f;
                            Gizmos.DrawLine(c, c + b.normalized * normalLength);
                        }
                    }
                }

                if (cutsAB != null)
                {
                    CreateLineMaterial();
                    // Apply the line material
                    lineMaterial.SetPass(0);

                    GL.PushMatrix();
                    // Set transformation matrix for drawing to
                    // match our transform
                    GL.MultMatrix(Matrix4x4.identity);

                    GL.PopMatrix();

                    GL.Begin(GL.LINES);
                    GL.Color(new Color(1.0f, 0.0f, 0.0f, 0.5f));
                    float cuts = Mathf.Floor(cutCount);
                    Vector3 a;
                    Vector3 b;
                    for (int i = 0; i < facesCut.Count; i++)
                    {
                        for (int ii = 0; ii < cuts; ii++)
                        {
                            float sep = 1.0f / (cuts + 1.0f);
                            float ratio = sep * (ii + 1);

                            a = cutsAB[i * 4 + 0] + (cutsAB[i * 4 + 1] - cutsAB[i * 4 + 0]) * ratio;
                            b = cutsAB[i * 4 + 2] + (cutsAB[i * 4 + 3] - cutsAB[i * 4 + 2]) * ratio;

                            GL.Vertex3(a.x, a.y, a.z);
                            GL.Vertex3(b.x, b.y, b.z);
                        }

                    }
                    if (tracker != null)
                    {
                        for (int i = 0; i < tracker.Count; i += 2)
                        {
                            GL.Color(new Color(0.0f, 0.0f, 1.0f, 1.0f));
                            GL.Vertex3(tracker[i].x, tracker[i].y, tracker[i].z);
                            GL.Color(new Color(0.0f, 1.0f, 0.0f, 1.0f));
                            GL.Vertex3(tracker[i + 1].x, tracker[i + 1].y, tracker[i + 1].z);
                        }
                    }
                    GL.End();
                    Gizmos.color = Color.red;
                    /*
                    float cuts = Mathf.Floor(cutCount);
                    for (int i = 0; i < facesCut.Count; i++)
                    {
                        for (int ii = 0; ii < cuts; ii++)
                        {
                            float sep = 1.0f / (cuts + 1.0f);
                            float ratio = sep * (ii + 1);

                            Gizmos.DrawLine(
                                cutsAB[i * 4 + 0] + (cutsAB[i * 4 + 1] - cutsAB[i * 4 + 0]) * ratio,
                                cutsAB[i * 4 + 2] + (cutsAB[i * 4 + 3] - cutsAB[i * 4 + 2]) * ratio);
                        }
                    }
                    */
                }
                Gizmos.color = Color.white;
            }

        }


        if (Selection.activeGameObject == gameObject)
            if (isDebugMode)
                for (int i = 0; i < verts.Count; i++)
                {
                    int q = quads.IndexOf(i) / 4;
                    Vector3 qOffset = quadCenter(q) - verts[i];
                    Handles.color = Color.white;
                    string list = i + " {";
                    for (int ii = 0; ii < connectedVerts[i].Count; ii++)
                    {
                        list += connectedVerts[i].list[ii].ToString();
                        if (ii < connectedVerts[i].Count - 1)
                        {
                            list += ", ";
                        }
                    }
                    list += "}";

                    Handles.Label(verts[i] + qOffset * 0.25f, list);
                }

        #region GL Drawing
        if (isSelected)
        {
            CreateLineMaterial();
            // Apply the line material
            lineMaterial.SetPass(0);

            GL.PushMatrix();
            // Set transformation matrix for drawing to
            // match our transform
            GL.MultMatrix(Matrix4x4.identity);

            if (currentEditMode == 1 && vertMode == 1 && selectedFaces != null)
            {
                // Draw lines
                int index = 0;
                for (int i = 0; i < selectedFaces.Length; i++)
                {
                    GL.Begin(GL.TRIANGLE_STRIP);
                    GL.Color(new Color(0.95f, 0.95f, 0.95f, 0.6f));
                    if (selectedFaces[i])
                    {
                        index = i * 4;
                        GL.Vertex3(verts[quads[index + 0]].x, verts[quads[index + 0]].y, verts[quads[index + 0]].z);
                        GL.Vertex3(verts[quads[index + 1]].x, verts[quads[index + 1]].y, verts[quads[index + 1]].z);
                        GL.Vertex3(verts[quads[index + 2]].x, verts[quads[index + 2]].y, verts[quads[index + 2]].z);
                        GL.Vertex3(verts[quads[index + 3]].x, verts[quads[index + 3]].y, verts[quads[index + 3]].z);
                    }
                    GL.End();
                }
            }
            else if (selectedVerts != null)
            {
                for (int i = 0; i < selectedVerts.Length; i += 4)
                {
                    if (selectedVerts[i + 0] &&
                        selectedVerts[i + 1] &&
                        selectedVerts[i + 2] &&
                        selectedVerts[i + 3])
                    {
                        GL.Begin(GL.TRIANGLE_STRIP);
                        GL.Color(new Color(0.95f, 0.95f, 0.95f, 0.6f));

                        GL.Vertex3(verts[i + 0].x, verts[i + 0].y, verts[i + 0].z);
                        GL.Vertex3(verts[i + 1].x, verts[i + 1].y, verts[i + 1].z);
                        GL.Vertex3(verts[i + 2].x, verts[i + 2].y, verts[i + 2].z);
                        GL.Vertex3(verts[i + 3].x, verts[i + 3].y, verts[i + 3].z);

                        GL.End();
                    }
                }
            }
            GL.PopMatrix();
        }
        #endregion
    }

    static Material lineMaterial;
    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // lineMaterial.SetOverrideTag("RenderType", "Overdraw");
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
            lineMaterial.SetInt("_ZTest", 0);
        }
    }



    [SerializeField]
    public bool isDebugMode = false;

    [SerializeField, HideInInspector]
    private Vector3 editorPosition;
    [SerializeField, HideInInspector]
    private Vector3 editorScale;
    [SerializeField, HideInInspector]
    private Quaternion editorRotation;

    public void getQuads()
    {
        // DO NOT reference the mesh that belongs to the MeshEdit object
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh meshCopy = Mesh.Instantiate(meshFilter.sharedMesh) as Mesh;
        Mesh mesh = meshFilter.sharedMesh;

        quads = new List<int>();
        faceNormals = new List<Vector3>();

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            int[] coords = { -1, -1, -1, -1 };
            coords[0] = mesh.triangles[i];
            coords[1] = mesh.triangles[i + 1];
            coords[2] = mesh.triangles[i + 2];
            coords[3] = findAdjacentTriPoint(mesh.triangles, mesh.vertices, ref coords[1], ref coords[2], ref coords[0]);

            bool isNewQuad = true;

            for (int ii = 0; ii < 4; ii++)
            {
                if (quads.Contains(coords[ii]))
                {
                    isNewQuad = false;
                    break;
                }
            }

            if (isNewQuad)
            {
                Vector3 n = Vector3.zero;
                for (int ii = 0; ii < 4; ii++)
                {
                    quads.Add(coords[ii]);
                    n += vertNormals[coords[ii]];
                }
                n /= 4;
                faceNormals.Add(n);
            }
        }
    }


    public void recalculateSelectedNormals(Mesh mesh, bool[] updatedVerts, bool[] updatedFaces)
    {
        mesh.RecalculateNormals();

        Vector3 p = gameObject.transform.position;
        Quaternion r = gameObject.transform.rotation;
        Vector3 s = gameObject.transform.lossyScale;
        /* TODO: This NEEEDS to be more efficient
        for (int i = 0; i < updatedVerts.Length; i++)
        {
            if (updatedVerts[i])
            {
                Vector3 n = mesh.normals[i];
                int sharedCount = 1;
                for (int ii = 0; ii < mesh.normals.Length; ii++)
                {
                    if (i != ii)
                    {
                        if (mesh.vertices[i] == mesh.vertices[ii])
                        {
                            n += mesh.normals[ii];
                            sharedCount++;
                        }
                    }
                }
                n /= sharedCount;

                n.x *= s.x;
                n.y *= s.y;
                n.z *= s.z;
                n = r * n;
                vertNormals[i] = n;
            }
        }
        mesh.normals = vertNormals.ToArray();
        */

        for (int i = 0; i < quads.Count; i += 4)
        {
            if (updatedFaces[i / 4])
            {
                Vector3 n = Vector3.zero;

                int tA = (i / 4) * 2 * 3;
                int tB = ((i / 4) * 2 + 1) * 3;
                Vector3 ab = verts[tris[tA + 1]] - verts[tris[tA + 0]];
                Vector3 ac = verts[tris[tA + 2]] - verts[tris[tA + 0]];
                Vector3 dc = verts[tris[tB + 1]] - verts[tris[tB + 0]];
                Vector3 db = verts[tris[tB + 2]] - verts[tris[tB + 0]];


                Vector3 a = Vector3.Cross(ab, ac);
                Vector3 b = Vector3.Cross(dc, db);
                n = a + b;
                faceNormals[i / 4] = n.normalized;
            }
        }
    }

    public void createTrianglesFromWorldMesh(bool isForced = false)
    {
        Mesh m = gameObject.GetComponent<MeshFilter>().sharedMesh;

        Vector3 p = gameObject.transform.position;
        Quaternion r = gameObject.transform.rotation;
        Vector3 s = gameObject.transform.lossyScale;

        if (p != editorPosition ||
            r != editorRotation ||
            s != editorScale ||
            vertNormals == null ||
            verts == null ||
            vertNormals.Count != verts.Count ||
            isForced)
        {
            editorPosition = p;
            editorRotation = r;
            editorScale = s;
            verts = null;
            quads = null;
        }

        if (verts == null || verts.Count != m.vertices.Length ||
            colours == null || colours.Count != m.colors.Length)
        {
            verts = new List<Vector3>();
            vertNormals = new List<Vector3>();
            colours = new List<Color>();
            s.x = Mathf.Max(Mathf.Abs(s.x), minimumScale) * Mathf.Sign(s.x);
            s.y = Mathf.Max(Mathf.Abs(s.y), minimumScale) * Mathf.Sign(s.y);
            s.z = Mathf.Max(Mathf.Abs(s.z), minimumScale) * Mathf.Sign(s.z);
            for (int i = 0; i < m.vertices.Length; i++)
            {
                Vector3 v = m.vertices[i];
                v.x *= s.x;
                v.y *= s.y;
                v.z *= s.z;
                v = r * v;
                v += p;
                verts.Add(v);

                Vector3 n = m.normals[i];
                n.x *= s.x;
                n.y *= s.y;
                n.z *= s.z;
                n = r * n;
                vertNormals.Add(n);

                colours.Add(m.colors[i]);
            }
        }

        if (tris == null || tris.Count != m.triangles.Length)
        {
            tris = new List<int>();
            for (int i = 0; i < m.triangles.Length; i++)
            {
                tris.Add(m.triangles[i]);
            }
        }

        for (int i = 0; i < uvMaps.Count; i++)
        {
            uvMaps[i].vertCount = verts.Count;
        }

        if (quads == null || faceNormals == null)// || quads.Count != verts.Count || faceNormals.Count != verts.Count / 4)
        {
            getQuads();
        }

        if (connectedVerts == null || connectedVerts.Count != verts.Count)
        {
            connectedVerts = new List<ListWrapper>();
            for (int i = 0; i < verts.Count; i++)
            {
                List<int> sharedVerts = new List<int>();
                for (int j = 0; j < verts.Count; j++)
                {
                    if (j != i)
                    {
                        if (verts[i] == verts[j])
                        {
                            sharedVerts.Add(j);
                        }
                    }
                }
                connectedVerts.Add(new ListWrapper(sharedVerts));
            }
        }
    }

    List<Vector3> tracker;

    public void getFirstFaceInLoop(ref int face, ref int a, ref int b, ref int c, ref int d, List<int> facesChecked = null)
    {
        if (facesChecked == null)
        {
            tracker = new List<Vector3>();
            facesChecked = new List<int>();
        }
        int connectedFace = -1;

        c = a;
        d = b;

        facesChecked.Add(face);
        // This could be made more efficient by only calling adjacent faces (via connectedVerts)
        for (int i = 0; i < quads.Count; i += 4)
        {
            if (i != face * 4)
            {
                bool foundA = false;
                bool foundB = false;

                int fA = -1;
                int fB = -1;
                int fC = -1;
                int fD = -1;
                for (int ii = 0; ii < 4; ii++)
                {
                    if (!foundA && connectedVerts[quads[i + ii]].Contains(quads[face * 4 + a]))
                    {

                        fA = ii;
                        foundA = true;
                    }
                    else if (!foundB && connectedVerts[quads[i + ii]].Contains(quads[face * 4 + b]))
                    {
                        fB = ii;
                        foundB = true;
                    }
                }

                if (foundA && foundB)
                {
                    connectedFace = i / 4;

                    getOppositeSideOfQuadRelative(connectedFace, fA, fB, out fC, out fD);

                    a = fC;
                    b = fD;

                    if (facesChecked.Contains(connectedFace))
                    {

                        return;
                    }
                    else
                    {
                        face = connectedFace;
                        getFirstFaceInLoop(ref face, ref a, ref b, ref c, ref d, facesChecked);
                    }

                    break;
                }
            }
        }

        return;
    }

    public void getLoopCut(int face, int a, int b, ref List<int> facesCut, ref List<Vector3> cutsAB)
    {
        int connectedFace = -1;

        int c = -1;
        int d = -1;

        getOppositeSideOfQuadRelative(face, a, b, out c, out d);

        facesCut.Add(face);
        cutsAB.Add(verts[quads[face * 4 + a]]);
        cutsAB.Add(verts[quads[face * 4 + b]]);
        cutsAB.Add(verts[quads[face * 4 + c]]);
        cutsAB.Add(verts[quads[face * 4 + d]]);

        for (int i = 0; i < quads.Count; i += 4)
        {
            if (i != face * 4)
            {
                bool foundA = false;
                bool foundB = false;

                int fA = -1;
                int fB = -1;
                int fC = -1;
                int fD = -1;
                for (int ii = 0; ii < 4; ii++)
                {
                    if (!foundA && connectedVerts[quads[i + ii]].Contains(quads[face * 4 + a]))
                    {

                        fA = ii;
                        foundA = true;
                    }
                    else if (!foundB && connectedVerts[quads[i + ii]].Contains(quads[face * 4 + b]))
                    {
                        fB = ii;
                        foundB = true;
                    }
                }

                if (foundA && foundB)
                {
                    connectedFace = i / 4;

                    getOppositeSideOfQuadRelative(connectedFace, fA, fB, out fC, out fD);


                    c = fC;
                    d = fD;

                    if (facesCut.Contains(connectedFace))
                    {

                        // We've found the start of the loop
                        return;
                    }
                    else
                    {
                        getLoopCut(connectedFace, c, d, ref facesCut, ref cutsAB);
                    }

                    break;
                }
            }
        }

        return;
    }


    public void flipFaces(bool[] selectedFaces)
    {
        //Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
        int[] newTriangles = new int[mesh.triangles.Length];
        for (int i = 0; i < newTriangles.Length; i++)
        {
            newTriangles[i] = mesh.triangles[i];
        }
        for (int i = 0; i < newTriangles.Length; i += 3)
        {
            int face = i / 6;
            if (selectedFaces[face])
            {
                int temp = newTriangles[i + 1];
                newTriangles[i + 1] = newTriangles[i + 2];
                newTriangles[i + 2] = temp;

                faceNormals[face] = faceNormals[face] * -1;

                temp = tris[i + 1];
                tris[i + 1] = tris[i + 2];
                tris[i + 2] = temp;
            }
        }
        for (int i = 0; i < quads.Count; i += 4)
        {
            if (selectedFaces[i / 4])
            {
                int temp = quads[i + 1];
                quads[i + 1] = quads[i + 2];
                quads[i + 2] = temp;


            }
        }

        mesh.triangles = newTriangles;
        recalculateNormals(mesh);

        pushLocalMeshToGameObject();
    }


    public void flipSaddling(bool[] selectedFaces)
    {
        //Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
        int[] newTriangles = new int[mesh.triangles.Length];
        for (int i = 0; i < newTriangles.Length; i++)
        {
            newTriangles[i] = mesh.triangles[i];
        }

        for (int i = 0; i < selectedFaces.Length; i++)
        {
            if (selectedFaces[i])
            {
                // Rotate the indexes of the quad
                int temp = quads[i * 4 + 0];
                quads[i * 4 + 0] = quads[i * 4 + 1];
                quads[i * 4 + 1] = quads[i * 4 + 3];
                quads[i * 4 + 3] = quads[i * 4 + 2];
                quads[i * 4 + 2] = temp;

                int t = i * 6;

                newTriangles[t + 0] = quads[i * 4 + 0];
                newTriangles[t + 1] = quads[i * 4 + 1];
                newTriangles[t + 2] = quads[i * 4 + 2];
                newTriangles[t + 3] = quads[i * 4 + 3];
                newTriangles[t + 4] = quads[i * 4 + 2];
                newTriangles[t + 5] = quads[i * 4 + 1];

                tris[t + 0] = quads[i * 4 + 0];
                tris[t + 1] = quads[i * 4 + 1];
                tris[t + 2] = quads[i * 4 + 2];
                tris[t + 3] = quads[i * 4 + 3];
                tris[t + 4] = quads[i * 4 + 2];
                tris[t + 5] = quads[i * 4 + 1];
            }
        }

        mesh.triangles = newTriangles;
        recalculateNormals(mesh);

        pushLocalMeshToGameObject();
    }

    /* Disconnects all links in a connection group to the selected vert
     */
    private void disconnect(int referenceVert, int disconnectedVert)
    {
        for (int i = 0; i < connectedVerts[referenceVert].Count; i++)
        {
            connectedVerts[connectedVerts[referenceVert].list[i]].Remove(disconnectedVert);
        }
        connectedVerts[disconnectedVert].Clear();
    }

    private int findAdjacentTriPoint(int[] triangles, Vector3[] verts, ref int a, ref int b, ref int cOther, int checkIndex = 0)
    {
        int tri = 0;
        bool aFound = false;
        bool bFound = false;
        bool cFound = false;
        int c = -1;

        for (int i = 0; i < triangles.Length; i++)
        {
            if (triangles[i] == a)
            {
                aFound = true;
            }
            else if (triangles[i] == b)
            {
                bFound = true;
            }
            else if (triangles[i] != cOther)
            {
                cFound = true;
                c = triangles[i];
            }

            if (aFound && bFound && cFound)
            {
                break;
            }

            tri++;
            if (tri > 2)
            {
                tri = 0;

                aFound = false;
                bFound = false;
                cFound = false;

                c = -1;
            }
        }
        if (c == -1 && checkIndex < 2)
        {
            int temp = cOther;
            cOther = a;
            a = b;
            b = temp;

            c = findAdjacentTriPoint(triangles, verts, ref a, ref b, ref cOther, checkIndex + 1);
        }
        return c;
    }

    public bool isShared(int vertA, int vertB)
    {
        return (connectedVerts[vertA].Contains(vertB) ||
            connectedVerts[vertB].Contains(vertA));
    }

    /// <summary>
    /// Extrude a face around the perimiter of the selected area.
    /// Returns the new array of selected vertices
    /// </summary>
    /// <param name="selectedVerts"></param>
    /// <param name="selectedFaces"></param>
    /// <returns></returns>
    public bool[] extrude(List<int> selectedEdges, List<int> faceRefs, bool[] selectedVerts, bool[] selectedFaces, Vector3 direction)
    {
        //SelectedEdges:
        // ...[][a, b, c, d][]...
        //FaceRefs
        // ...[][f1, f2][]...
        // a and b connect to a selected edge, c and d don't, c and d can be -1, meaning there is no face across from the selected face
        // f1 is the selected face and f2 is the opposite face, f2 can be -1
        // if f1 isn't selected, the edge has no face

        #region Save the connections existing between selected edges









        DateTime start = DateTime.Now;
        int newQuads = selectedEdges.Count / 4;
        int vertexCount = verts.Count;

        List<int>[] originalConnections = new List<int>[selectedEdges.Count];



        /* There are three types of link we need to connect to make sure the new faces are properly attached to the rest of the mesh
         * 1. Top->Down: Where the vert leading an extrusion is linked to the original mesh, pass that down to the bottom of the new extrusion (ignoring verts from selected faces)
         * 2. Down->Up : The vert leading the extrusion described previously, that must be passed to the upward portion of the new face
         * 3. Lateral  : The connections of each new face around the perimeter of the selected area
         *  
         *              ／
         *            ／
         *     ＼   ／                              0 ／ |
         *        ＼ 1   Selected Face             ／ 2
         *        |3 ＼                         ／   
         *        |     ＼                   ／
         *        |       edge            ／
         *        |           ＼  0 1  ／
         *        |              ＼ ／
         *        |              2 | 3
         *        |                |
         *        |                |
         *                         |
         *                         |
         * 
         */

        for (int i = 0; i < selectedEdges.Count; i++)
        {
            originalConnections[i] = new List<int>();
        }
        // Top-Down and Down-Up Connections
        for (int i = 0; i < selectedEdges.Count; i++)
        {
            // Save the Top-Down connections
            if (i % 4 < 2)
            {
                int topVert = selectedEdges[i];
                int topQuad = quads.IndexOf(topVert) / 4;
                for (int j = 0; j < connectedVerts[topVert].Count; j++)
                {
                    // For each of the verts linked to the leading point of the new face
                    // Save all non-selected connections to later apply them to the lower side of the new face
                    int connected = connectedVerts[topVert].list[j];
                    if (connected < vertexCount)
                    {
                        // If the vert belongs to a selected face, ignore it!
                        int quad = quads.IndexOf(connected) / 4;
                        if (!selectedFaces[quad] &&
                            connected != topVert) // <- for safety
                        {
                            originalConnections[i + 2].Add(connected);
                            // Reverse connection
                            connectedVerts[connected].Add(vertexCount + (i + 2));
                        }
                        else if (selectedFaces[quad])
                        {
                            // Other Down-Up connections
                            originalConnections[i].Add(connected);
                            // Reverse connection
                            connectedVerts[connected].Add(vertexCount + i);
                        }
                    }
                }
                // Finally, check to see if the leading face is selected! If it isn't, it's an edge extrusion where the top face must be added
                if (!selectedFaces[topQuad])
                {
                    originalConnections[i + 2].Add(topVert);
                    // Reverse connection
                    connectedVerts[topVert].Add(vertexCount + (i + 2));
                }

                if (selectedFaces[topQuad])
                {
                    // Save the main Down-Up connection
                    originalConnections[i].Add(topVert);
                    // Reverse connection
                    connectedVerts[topVert].Add(vertexCount + i);
                }
            }
        }

        // Lateral connections
        // If either top/bottom vert on a given side share a vert with a new edge, connect it on the top and bottom
        for (int i = 0; i < selectedEdges.Count; i++)
        {
            if (i % 4 > 1)
            {
                // Verts on underside of extrusion
                int iAlt = i;
                if (selectedEdges[i] < 0) { iAlt -= 2; }
                for (int ii = 0; ii < selectedEdges.Count; ii++)
                {
                    if (ii % 4 > 1)
                    {
                        if (i != ii)
                        {
                            int iiAlt = ii;
                            if (selectedEdges[ii] < 0) { iiAlt -= 2; }

                            // Checks if the faces that this edge was extruded from had any connections
                            if (isShared(selectedEdges[iAlt], selectedEdges[iiAlt]))
                            {
                                originalConnections[i].Add(vertexCount + ii);
                            }

                            // For the case of a lone plane being extruded and not having it's corners sewn together:
                            // If the underside has no connection and it was extruded from the same face, then sew them pre-emptively
                            if (selectedEdges[i] == -1 && selectedEdges[ii] == -1)
                            {
                                // Find which face the edge was extruded from originally, if they're the same then stitch the new face (lagging section)
                                //int faceI = / 4;
                                //int faceII =  / 4;
                                if (selectedEdges[ii - 2] == selectedEdges[i - 2])
                                {
                                    originalConnections[i].Add(vertexCount + ii);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Verts leading the extrusion
                for (int ii = 0; ii < selectedEdges.Count; ii++)
                {
                    if (ii % 4 < 2)
                    {
                        if (i != ii)
                        {
                            if (isShared(selectedEdges[i], selectedEdges[ii]) ||
                                selectedEdges[i] == selectedEdges[ii])
                            {
                                originalConnections[i].Add(vertexCount + ii);
                            }
                        }
                    }
                }
            }
        }

        DateTime after = DateTime.Now;

        #endregion

        #region Sever existing connections along the perimeter of the selection
        start = DateTime.Now;
        for (int i = 0; i < selectedEdges.Count; i++)
        {
            int leadingFace = faceRefs[(i / 4) * 2];
            // If the extrusion doesn't involve a face, no cuts need to be made
            if (selectedEdges[i] >= 0 &&
                leadingFace >= 0 &&
                selectedFaces[leadingFace])
            {
                // Selected nodes
                // The first two verts of each edge are a part of the selected face, so they need to be severed from any non-selected face.
                for (int ii = 0; ii < connectedVerts[selectedEdges[i]].Count; ii++)
                {
                    int q = quads.IndexOf(connectedVerts[selectedEdges[i]].list[ii]) / 4;
                    // We added a bunch of connections to verts that don't exist yet, so ignore any verts that are over the number of verts
                    if (q >= 0 &&
                        connectedVerts[selectedEdges[i]].list[ii] < vertexCount)
                    {
                        if (i % 4 < 2)
                        {
                            // Selected faces
                            if (!selectedFaces[q])
                            {
                                connectedVerts[connectedVerts[selectedEdges[i]].list[ii]].Remove(selectedEdges[i]);

                                connectedVerts[selectedEdges[i]].RemoveAt(ii);
                                ii--;
                            }
                        }
                        else
                        {
                            // Non-selected faces
                            if (selectedFaces[q])
                            {
                                connectedVerts[connectedVerts[selectedEdges[i]].list[ii]].Remove(selectedEdges[i]);
                                connectedVerts[selectedEdges[i]].RemoveAt(ii);
                                ii--;
                            }
                        }
                    }
                }
            }
        }
        after = DateTime.Now;

        #endregion

        #region Create new quads along the perimeter of the selection
        start = DateTime.Now;
        for (int i = 0; i < selectedEdges.Count; i += 4)
        {

            int f = faceRefs[i / 2];

            bool isFaceSelected = false;
            if (f >= 0)
            {
                isFaceSelected = selectedFaces[f];
            }
            int c = selectedEdges[i + 2];
            int d = selectedEdges[i + 3];
            if (c < 0 || d < 0)
            {
                c = selectedEdges[i + 0];
                d = selectedEdges[i + 1];
            }
            addQuadBetweenFaces(selectedEdges[i], selectedEdges[i + 1], selectedEdges[i + 2], selectedEdges[i + 3], direction, f, isFaceSelected);

        }
        after = DateTime.Now;

        #endregion

        #region Add saved connections to the new set of verts
        start = DateTime.Now;
        for (int i = 0; i < verts.Count - vertexCount; i++)
        {
            for (int j = 0; j < originalConnections[i].Count; j++)
            {
                if (!connectedVerts[vertexCount + i].Contains(originalConnections[i][j]))
                {
                    connectedVerts[vertexCount + i].Add(originalConnections[i][j]);
                }
            }
        }
        after = DateTime.Now;

        #endregion

        start = DateTime.Now;
        //shareConnectedVerts();
        after = DateTime.Now;


        // Each of the four vertices that make up a plane will be physically touching 0 or more other vertices from adjacent quads (unless the the quad is a plane)
        // Count the overlapping number for each vert on the selected section, and then compare to the number when moved. If the value is LOWER then a quad must be created using that point
        int[] vertOverlap = new int[selectedVerts.Length];
        int[] selectedVertOverlap = new int[selectedVerts.Length];
        for (int i = 0; i < selectedVerts.Length; i++)
        {
            if (selectedVerts[i])
            {
                for (int ii = 0; ii < selectedVerts.Length; ii++)
                {
                    if (ii != i &&
                        connectedVerts[ii].Contains(i))
                    //verts[ii] == verts[i])
                    {
                        vertOverlap[i]++;

                        if (selectedVerts[ii])
                        {
                            selectedVertOverlap[i]++;
                        }
                    }
                }
            }
        }


        int vertCount = verts.Count;
        bool[] selectedVertsUpdated = new bool[vertCount];


        bool[] centeredSelectedFaces = new bool[selectedVerts.Length];
        for (int i = 0; i < selectedVerts.Length; i++)
        {
            if (selectedVertOverlap[i] == vertOverlap[i])
            {
                selectedVerts[i] = false;
                centeredSelectedFaces[i] = selectedVertOverlap[i] > 0;
            }
        }





        for (int i = 0; i < selectedVertsUpdated.Length; i++)
        {
            if (i < selectedVerts.Length)
            {
                selectedVertsUpdated[i] = selectedVerts[i];
            }
            else
            {
                // By selecting every second new vertice in this pattern, you end up grabbing the face-touching edge that lets you stretch the plane out during extrusion.
                selectedVertsUpdated[i] = ((i - selectedVerts.Length) % 4 == 0 || (i - selectedVerts.Length) % 4 == 1);
                //selectedVertsUpdated[i] = ((i - selectedVerts.Length) % 4 == 0 || (i - selectedVerts.Length) % 4 == 3);
            }
        }
        for (int i = 0; i < selectedFaces.Length; i++)
        {
            if (selectedFaces[i])
            {
                selectedVertsUpdated[i * 4 + 0] = true;
                selectedVertsUpdated[i * 4 + 1] = true;
                selectedVertsUpdated[i * 4 + 2] = true;
                selectedVertsUpdated[i * 4 + 3] = true;
            }
        }

        start = DateTime.Now;
        pushNewGeometry();
        after = DateTime.Now;

        return selectedVertsUpdated;
    }


    public void join(bool[] selectedVerts)
    {
        Vector3 average = Vector3.zero;
        int selectedCount = 0;
        for (int i = 0; i < selectedVerts.Length; i++)
        {
            if (selectedVerts[i])
            {
                average += verts[i];
                selectedCount++;
                for (int ii = 0; ii < selectedVerts.Length; ii++)
                {
                    if (ii != i)
                    {
                        if (!connectedVerts[i].Contains(ii))
                        {
                            connectedVerts[i].Add(ii);
                        }
                    }
                }
            }
        }
        average /= selectedCount;
        for (int i = 0; i < selectedVerts.Length; i++)
        {
            verts[i] = average;
        }
        pushNewGeometry();

        }


    public void delete(bool[] selectedVerts, bool[] selectedFaces)
    {
        for (int i = 0; i < selectedFaces.Length; i++)
        {
            selectedFaces[i] = selectedFaces[i] ||
                (selectedVerts[quads[i * 4 + 0]] ||
                selectedVerts[quads[i * 4 + 1]] ||
                selectedVerts[quads[i * 4 + 2]] ||
                selectedVerts[quads[i * 4 + 3]]);
        }

        bool[] vertDelete = new bool[verts.Count];
        for (int i = 0; i < selectedFaces.Length; i++)
        {
            if (selectedFaces[i])
            {
                for (int ii = 0; ii < 4; ii++)
                {
                    vertDelete[i * 4 + ii] = true;
                }
            }
        }

        bool[] quadDelete = new bool[quads.Count / 4];
        bool[] triDelete = new bool[tris.Count / 3];
        for (int i = vertDelete.Length - 1; i >= 0; i--)
        {
            if (vertDelete[i])
            {
                // Connected Verts
                /*
                for (int j = 0; j < connectedVerts[i].Count; j++)
                { 
                    if (connectedVerts[i][j] == i)
                    {
                        connectedVerts[i].RemoveAt(j);
                        j--;
                    }
                }
                for (int j = 0; j < connectedVerts[i].Count; j++)
                {
                    if (connectedVerts[i][j] > i)
                    {
                        connectedVerts[i][j]--;
                    }
                }*/
                for (int c = 0; c < connectedVerts.Count; c++)
                {
                    while (connectedVerts[c].Contains(i))
                    {
                        connectedVerts[c].Remove(i);
                    }
                    for (int cc = 0; cc < connectedVerts[c].Count; cc++)
                    {
                        if (connectedVerts[c].list[cc] > i)
                        {
                            connectedVerts[c].list[cc]--;
                        }
                    }
                }
                connectedVerts.RemoveAt(i);

                // Colour
                colours.RemoveAt(i);
                // Vert
                verts.RemoveAt(i);
                // Normal at vert
                vertNormals.RemoveAt(i);
                // UVs
                for (int u = 0; u < uvMaps.Count; u++)
                {
                    uvMaps[u].removeVert(i);
                }
                // Quads
                for (int q = 0; q < quads.Count; q += 4)
                {
                    for (int qi = 0; qi < 4; qi++)
                    {
                        if (q + qi >= quads.Count)
                        {

                        }
                        if (quads[q + qi] == i)
                        {
                            quadDelete[q / 4] = true;
                        }
                        else if (quads[q + qi] > i)
                        {
                            quads[q + qi]--;
                        }
                    }
                }
                // Tris
                for (int t = 0; t < tris.Count; t += 3)
                {
                    for (int ti = 0; ti < 3; ti++)
                    {
                        if (tris[t + ti] == i)
                        {
                            triDelete[t / 3] = true;
                        }
                        else if (tris[t + ti] > i)
                        {
                            tris[t + ti]--;
                        }
                    }
                }
            }
        }
        for (int i = quadDelete.Length - 1; i >= 0; i--)
        {
            if (quadDelete[i])
            {
                quads.RemoveAt(i * 4 + 3);
                quads.RemoveAt(i * 4 + 2);
                quads.RemoveAt(i * 4 + 1);
                quads.RemoveAt(i * 4 + 0);
                faceNormals.RemoveAt(i);
            }
        }
        for (int i = triDelete.Length - 1; i >= 0; i--)
        {
            if (triDelete[i])
            {
                tris.RemoveAt(i * 3 + 2);
                tris.RemoveAt(i * 3 + 1);
                tris.RemoveAt(i * 3 + 0);
            }
        }

        for (int u = 0; u < uvMaps.Count; u++)
        {
            uvMaps[u].vertCount = verts.Count;
        }
        pushNewGeometry();
    }

    [SerializeField, HideInInspector]
    public bool transformsApplied;

    public void checkTransforms()
    {
        Vector3 p = gameObject.transform.position;
        Quaternion r = gameObject.transform.rotation;
        Vector3 s = gameObject.transform.lossyScale;

        bool update = false;
        bool updateP = false, updateR = false, updateS = false;
        Vector3 offsetP = Vector3.zero;
        Quaternion offsetR = Quaternion.identity;
        Vector3 offsetS = Vector3.zero;
        if (p != editorPosition)
        {
            offsetP = editorPosition - p;
            editorPosition = p;
            update = true;
            updateP = true;
        }
        if (s != editorScale)
        {
            offsetS.x = s.x / editorScale.x;
            offsetS.y = s.y / editorScale.y;
            offsetS.z = s.z / editorScale.z;
            editorScale = s;
            update = true;
            updateS = true;
        }
        if (r != editorRotation)
        {
            offsetR = editorRotation;
            editorRotation = r;

            update = true;
            updateR = true;
        }
        if (update)
        {
            for (int i = 0; i < verts.Count; i++)
            {
                if (updateP)
                {
                    verts[i] -= offsetP;
                }
                if (updateS)
                {
                    verts[i] -= p;
                    verts[i] = Quaternion.Inverse(r) * verts[i];
                    verts[i] = new Vector3(
                        verts[i].x * offsetS.x,
                        verts[i].y * offsetS.y,
                        verts[i].z * offsetS.z);
                    verts[i] = r * verts[i];
                    verts[i] += p;
                }
                if (updateR)
                {
                    verts[i] -= p;
                    verts[i] = Quaternion.Inverse(offsetR) * verts[i];
                    verts[i] = editorRotation * verts[i];
                    verts[i] += p;
                }
            }

        }
        if ((update || !transformsApplied)) // && !isSelected)
        {
            applyModifiers(true);
        }
    }

    public void applyModifiers(bool forceUpdate)
    {

        // Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
        
        MeshMod[] mods = gameObject.GetComponents<MeshMod>();
        if (mods.Length > 0)
        {
            mods[0].applyAllMods();
        }
        /*
        deepCopyLocalMeshToModifiedMesh();

        for (int i = 0; i < mods.Length; i++)
        {
            if (mods[i].enabled)
            {
                mods[i].apply(meshModified);
            }
        }
        pushModifiedMeshToGameObject();
        */
        transformsApplied = true;
        
    }

    public void deepCopyLocalMeshToModifiedMesh()
    {
        meshModified = new Mesh();
        
        Vector3[] vertices = new Vector3[mesh.vertexCount];
        Vector3[] normals = new Vector3[mesh.normals.Length];
        Vector2[] uvs = new Vector2[mesh.uv.Length];
        int[] triangles = new int[mesh.triangles.Length];
        Color[] colours = new Color[mesh.colors.Length];

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            vertices[i] = (mesh.vertices[i]);
            normals[i] = (mesh.normals[i]);
            uvs[i] = (mesh.uv[i]);
            colours[i] = (mesh.colors[i]);
        }
        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            triangles[i] = (mesh.triangles[i]);
        }

        meshModified.vertices = vertices;
        meshModified.normals = normals;
        meshModified.uv = uvs;
        meshModified.colors = colours;
        meshModified.triangles = triangles;

        meshModified.RecalculateBounds();
        meshModified.RecalculateNormals();
    }

    /// <summary>
    /// Update the mesh to reflect the changes made to vertices on the model.
    /// Updated the mesh vertices, then recalculates normals on both the in game model and the mesh.
    /// </summary>
    /// <param name="updatedVerts"></param>
    /// <param name="updatedFaces"></param>
    public void updateMeshVerts(bool[] updatedVerts = null, bool[] updatedFaces = null)
    {
        //MeshFilter meshFilter = GetComponent<MeshFilter>();
        //Mesh mesh = meshFilter.sharedMesh;
        
        List<Vector3> vertsNormalised = new List<Vector3>();

        Vector3 p = gameObject.transform.position;
        Quaternion r = gameObject.transform.rotation;
        Vector3 s = gameObject.transform.lossyScale;
        s.x = Mathf.Max(Mathf.Abs(s.x), minimumScale) * Mathf.Sign(s.x);
        s.y = Mathf.Max(Mathf.Abs(s.y), minimumScale) * Mathf.Sign(s.y);
        s.z = Mathf.Max(Mathf.Abs(s.z), minimumScale) * Mathf.Sign(s.z);

        for (int i = 0; i < verts.Count; i++)
        {

            Vector3 v = verts[i];
            v -= p;

            v = Quaternion.Inverse(r) * v;

            v.x /= s.x;
            v.y /= s.y;
            v.z /= s.z;

            vertsNormalised.Add(v);
        }

        mesh.vertices = vertsNormalised.ToArray();

        if (updatedVerts == null && updatedFaces == null)
        {
            recalculateNormals(mesh);
        }
        else
        {
            recalculateSelectedNormals(mesh, updatedVerts, updatedFaces);
        }

        mesh.RecalculateBounds();

        pushLocalMeshToGameObject();
    }
    public void shareConnectedVerts()
    {
        for (int i = 0; i < connectedVerts.Count; i++)
        {
            bool isShared = false;

            while (!isShared)
            {
                isShared = true;
                for (int ii = 0; ii < connectedVerts.Count; ii++)
                {
                    if (ii != i)
                    {
                        if (connectedVerts[ii].Contains(i) ||
                            connectedVerts[i].Contains(ii))
                        {
                            if (!connectedVerts[i].Contains(ii))
                            {
                                isShared = false;
                                connectedVerts[i].Add(ii);
                            }
                            if (!connectedVerts[ii].Contains(i))
                            {
                                isShared = false;
                                connectedVerts[ii].Add(i);
                            }

                            for (int j = 0; j < connectedVerts[ii].Count; j++)
                            {
                                if (connectedVerts[ii].list[j] != i &&
                                    !connectedVerts[i].Contains(connectedVerts[ii].list[j]))
                                {
                                    isShared = false;
                                    connectedVerts[i].Add(connectedVerts[ii].list[j]);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    /*
    public List<int> getSharedVerts(int vert, List<int> sharedVerts = null)
    {
        if (sharedVerts == null)
        {
            sharedVerts = new List<int>();
            sharedVerts.Add(vert);
        }
        {
            for (int i = 0; i < connectedVerts[i].Count; i++)
            {
                if (vert != i)
                {
                    getSharedVerts
                }
            }
        }
    }*/

    public void recenterPivot(Vector3 normalisedCenter)
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        for (int i = 0; i < verts.Count; i++)
        {
            if (verts[i].x < minX)
            {
                minX = verts[i].x;
            }
            if (verts[i].y < minY)
            {
                minY = verts[i].y;
            }
            if (verts[i].z < minZ)
            {
                minZ = verts[i].z;
            }
            if (verts[i].x > maxX)
            {
                maxX = verts[i].x;
            }
            if (verts[i].y > maxY)
            {
                maxY = verts[i].y;
            }
            if (verts[i].z > maxZ)
            {
                maxZ = verts[i].z;
            }
        }

        Vector3 centerPoint = new Vector3(
            minX + (maxX - minX) * normalisedCenter.x,
            minY + (maxY - minY) * normalisedCenter.y,
            minZ + (maxZ - minZ) * normalisedCenter.z);

        Vector3 offset = centerPoint - gameObject.transform.position;
        gameObject.transform.position = centerPoint;

        for (int i = 0; i < verts.Count; i++)
        {
            verts[i] -= offset;
        }

        pushNewGeometry();
    }

    // Used for extrusion
    public void addQuadBetweenFaces(int v0, int v1, int v2, int v3, Vector3 n, int faceReference, bool isFaceSelected, bool attachQuads = false)
    {
        int v2Temp = v2;
        int v3Temp = v3;
        if (v2 == -1)
        {
            v2 = v0;
        }
        if (v3 == -1)
        {
            v3 = v1;
        }
        Vector3 nCenter = Vector3.zero;

        if (faceReference >= 0)
        {
            nCenter = quadCenter(faceReference);
        }


        // "n" refers to the wrong face
        int vertCount = verts.Count;

        verts.Add(verts[v0]);
        verts.Add(verts[v1]);
        verts.Add(verts[v2]);
        verts.Add(verts[v3]);

        colours.Add(colours[v0]);
        colours.Add(colours[v1]);
        colours.Add(colours[v2]);
        colours.Add(colours[v3]);

        if (attachQuads)
        {
            if (isFaceSelected)
            {
                connectedVerts[v0].Add(vertCount + 0);
                connectedVerts[v1].Add(vertCount + 1);
                connectedVerts.Add(new ListWrapper(new List<int>(new int[] { v0 })));
                connectedVerts.Add(new ListWrapper(new List<int>(new int[] { v1 })));
            }
            else
            {
                connectedVerts.Add(new ListWrapper(new List<int>(new int[] { })));
                connectedVerts.Add(new ListWrapper(new List<int>(new int[] { })));
            }
            if (v2Temp >= 0 && v3Temp >= 0)
            {
                connectedVerts[v2].Add(vertCount + 2);
                connectedVerts[v3].Add(vertCount + 3);
                connectedVerts.Add(new ListWrapper(new List<int>(new int[] { v2 })));
                connectedVerts.Add(new ListWrapper(new List<int>(new int[] { v3 })));
            }
            else
            {
                connectedVerts[v2].Add(vertCount + 2);
                connectedVerts[v3].Add(vertCount + 3);
                connectedVerts.Add(new ListWrapper(new List<int>(new int[] { })));
                connectedVerts.Add(new ListWrapper(new List<int>(new int[] { })));
            }
        }
        else
        {
            connectedVerts.Add(new ListWrapper(new List<int>(new int[] { })));
            connectedVerts.Add(new ListWrapper(new List<int>(new int[] { })));
            connectedVerts.Add(new ListWrapper(new List<int>(new int[] { })));
            connectedVerts.Add(new ListWrapper(new List<int>(new int[] { })));
        }

        tris.Add(vertCount + 0);
        tris.Add(vertCount + 1);
        tris.Add(vertCount + 2);
        tris.Add(vertCount + 3);
        tris.Add(vertCount + 2);
        tris.Add(vertCount + 1);

        Vector3 a = verts[v0] + n;
        Vector3 b = verts[v1] + n;
        Vector3 c = verts[v2];
        Vector3 ab = b - a;
        Vector3 ac = c - a;
        Vector3 proofNormal = Vector3.Cross(ab, ac);
        if ((v0 == v2 || v1 == v3) &&
            !isFaceSelected)
        {
            float dot = Vector3.Dot(SceneView.lastActiveSceneView.camera.transform.forward, proofNormal);

            Vector3 s = transform.lossyScale;
            if (s.x * s.y * s.z < 0)
            {
                dot *= -1;
            }
            if (dot > 0)
            {
                proofNormal *= -1;
                int tc = tris.Count;
                int temp = tris[tc - 1];
                tris[tc - 1] = tris[tc - 2];
                tris[tc - 2] = temp;

                temp = tris[tc - 4];
                tris[tc - 4] = tris[tc - 5];
                tris[tc - 5] = temp;

            }
        }
        else// if (geometryBasedFlip)
        {
            Vector3 compareNormal = (verts[v1] + verts[v0]) / 2 - nCenter;

            if (Vector3.Dot(proofNormal, compareNormal) < 0)
            {
            }
            Vector3 midPoint = (verts[v0] + n + verts[v1] + n) * 0.5f;
            Vector3 centerOfNewFace = (verts[v0] + n + verts[v1] + n + verts[v2] + verts[v3]) * 0.25f;
            Vector3 between = (centerOfNewFace + nCenter) / 2;
            Vector3 crossReference = Vector3.Cross(midPoint - (nCenter + n), n);
            Vector3 crossNew = Vector3.Cross(midPoint - centerOfNewFace, proofNormal);

            float dot = Vector3.Dot(crossReference, crossNew);

            //Vector3 quessNormal = (vertNormals[v0] + vertNormals[v1] + vertNormals[v2] + n + vertNormals[v3] + n);
            //dot = Vector3.Dot(quessNormal, proofNormal);
            Vector3 s = transform.lossyScale;
            if (s.x * s.y * s.z < 0)
            {
                dot *= -1;
            }
            if (dot > 0)
            {
                proofNormal *= -1;
                int tc = tris.Count;
                int temp = tris[tc - 1];
                tris[tc - 1] = tris[tc - 2];
                tris[tc - 2] = temp;

                temp = tris[tc - 4];
                tris[tc - 4] = tris[tc - 5];
                tris[tc - 5] = temp;

            }
        }

        quads.Add(vertCount + 0);
        quads.Add(vertCount + 1);
        quads.Add(vertCount + 2);
        quads.Add(vertCount + 3);


        vertNormals.Add(proofNormal);
        vertNormals.Add(proofNormal);
        vertNormals.Add(proofNormal);
        vertNormals.Add(proofNormal);

        faceNormals.Add(proofNormal);

        // Resize all data structures representing this mesh to fit the new mesh size.
        int[] newVerts = { v0, v1, v2, v3 };

        int quadReference = (quads.IndexOf(v0) / 4);
        int uv0 = v0, uv1 = v1, uv2 = -1, uv3 = -1;
        getOppositeSideOfQuad(quadReference, uv0, uv1, out uv2, out uv3);

        for (int i = 0; i < uvMaps.Count; i++)
        {
            uvMaps[i].resizeUVLength(vertCount + 4, new int[] { uv0, uv1, uv2, uv3 });//, newVerts);
        }
    }

    public bool isVertCovered(int vert)
    {
        bool isCovered = false;
        Vector3 p = verts[vert];
        Ray r = new Ray(p, (SceneView.lastActiveSceneView.camera.transform.position - p));

        r = HandleUtility.GUIPointToWorldRay(HandleUtility.WorldToGUIPoint(p));
        float d2Vert = Vector3.Distance(r.origin, p);
        Vector3 intersection = Vector3.zero;

        Triangle t = new Triangle(Vector3.zero, Vector3.zero, Vector3.zero);
        for (int i = 0; i < quads.Count; i += 4)
        {
            t.a = verts[quads[i + 0]];
            t.b = verts[quads[i + 1]];
            t.c = verts[quads[i + 2]];
            if (intersectTriangleRay(t, r, out intersection))
            {
                if (Vector3.Distance(intersection, p) > 0.000001f &&
                    Vector3.Distance(intersection, SceneView.lastActiveSceneView.camera.transform.position) < d2Vert)
                {
                    isCovered = true;
                    break;
                }
            }

            t.a = verts[quads[i + 3]];
            t.b = verts[quads[i + 1]];
            t.c = verts[quads[i + 2]];
            if (intersectTriangleRay(t, r, out intersection))
            {
                if (Vector3.Distance(intersection, p) > 0.000001f &&
                    Vector3.Distance(intersection, SceneView.lastActiveSceneView.camera.transform.position) < d2Vert)
                {
                    isCovered = true;
                    break;
                }
            }
        }

        return isCovered;
    }

    public bool isFaceCovered(int face)
    {
        return isFaceCovered(face, quadCenter(face));
    }

    public bool isFaceCovered(int face, Vector3 pointOnFace)
    {
        bool isCovered = false;
        Vector3 p = pointOnFace;
        Ray r = new Ray(SceneView.lastActiveSceneView.camera.transform.position, (p - SceneView.lastActiveSceneView.camera.transform.position).normalized);
        r = HandleUtility.GUIPointToWorldRay(HandleUtility.WorldToGUIPoint(p));
        float dd = (SceneView.lastActiveSceneView.camera.transform.position - p).sqrMagnitude;

        Vector3 intersection = Vector3.zero;

        Triangle t = new Triangle(Vector3.zero, Vector3.zero, Vector3.zero);
        for (int i = 0; i < quads.Count; i += 4)
        {
            if (4 * face != i)
            {
                t.a = verts[quads[i + 0]];
                t.b = verts[quads[i + 1]];
                t.c = verts[quads[i + 2]];
                if (intersectTriangleRay(t, r, out intersection))
                {
                    float d = Vector3.Distance(intersection, SceneView.lastActiveSceneView.camera.transform.position);
                    float d2 = (intersection - SceneView.lastActiveSceneView.camera.transform.position).sqrMagnitude;
                    if (d2 < dd)
                    {
                            isCovered = true;
                            break;
                    }
                }

                t.a = verts[quads[i + 3]];
                t.b = verts[quads[i + 1]];
                t.c = verts[quads[i + 2]];
                if (intersectTriangleRay(t, r, out intersection))
                {
                    float d = Vector3.Distance(intersection, p);

                    float d2 = (intersection - SceneView.lastActiveSceneView.camera.transform.position).sqrMagnitude;
                    if (d2 < dd)
                    {
                            isCovered = true;
                            break;
                    }
                }
            }
        }

        return isCovered;
    }

    // Used for add quad
    public void addQuadBetweenFaces(int v0, int v1, int v2, int v3)
    {
        Vector3 crossA = Vector3.zero;
        Vector3 crossB = Vector3.zero;
        int[] newQuad = { v0, v1, v2, v3 };
        int[] bestQuad = new int[4];
        float crossLength = float.MaxValue;
        for (int i = 0; i < 3; i++)
        {
            crossA = Vector3.Cross(
                verts[newQuad[1]] - verts[newQuad[0]],
                verts[newQuad[2]] - verts[newQuad[0]]);
            crossB = Vector3.Cross(
                verts[newQuad[2]] - verts[newQuad[3]],
                verts[newQuad[1]] - verts[newQuad[3]]);

            if (Vector3.Dot(crossA, crossB) > 0)
            {
                float l = Vector3.Cross(crossA, crossB).magnitude;

                if (l < crossLength)
                {
                    crossLength = l;
                    bestQuad = new int[] { newQuad[0], newQuad[1], newQuad[2], newQuad[3] };
                }
            }

            // Rotate the last three items in newQuad 
            int temp = newQuad[1];
            newQuad[1] = newQuad[2];
            newQuad[2] = newQuad[3];
            newQuad[3] = temp;

        }

        v0 = bestQuad[0];
        v1 = bestQuad[1];
        v2 = bestQuad[2];
        v3 = bestQuad[3];

        Vector3 nCenter = Vector3.zero;


        // "n" refers to the wrong face
        int vertCount = verts.Count;

        verts.Add(verts[v0]);
        verts.Add(verts[v1]);
        verts.Add(verts[v2]);
        verts.Add(verts[v3]);

        colours.Add(colours[v0]);
        colours.Add(colours[v1]);
        colours.Add(colours[v2]);
        colours.Add(colours[v3]);

        connectedVerts[v0].Add(vertCount + 0);
        connectedVerts[v1].Add(vertCount + 1);
        connectedVerts.Add(new ListWrapper(new List<int>(new int[] { v0 })));
        connectedVerts.Add(new ListWrapper(new List<int>(new int[] { v1 })));
        connectedVerts[v2].Add(vertCount + 2);
        connectedVerts[v3].Add(vertCount + 3);
        connectedVerts.Add(new ListWrapper(new List<int>(new int[] { v2 })));
        connectedVerts.Add(new ListWrapper(new List<int>(new int[] { v3 })));
        for (int i = 0; i < 4; i ++)
        {
            int vert = v0;
            if (i == 1) { vert = v1; }
            if (i == 2) { vert = v2; }
            if (i == 3) { vert = v3; }

            for (int ii = 0; ii < connectedVerts[vert].Count; ii++)
            {
                connectedVerts[vertCount + i].Add(connectedVerts[vert].list[ii]);
                connectedVerts[connectedVerts[vert].list[ii]].Add(vertCount + i);
            }
        }
        
        tris.Add(vertCount + 0);
        tris.Add(vertCount + 1);
        tris.Add(vertCount + 2);
        tris.Add(vertCount + 3);
        tris.Add(vertCount + 2);
        tris.Add(vertCount + 1);

        // Flip to face the camera
        Vector3 normal = Vector3.Cross(
            verts[v1] - verts[v0],
            verts[v2] - verts[v0]);
        float dot = Vector3.Dot(SceneView.lastActiveSceneView.camera.transform.forward, normal);
        Vector3 s = transform.lossyScale;
        if (s.x * s.y * s.z < 0)
        {
            dot *= -1;
        }
        if (dot > 0)
        {
            normal *= -1;
            int tc = tris.Count;
            int temp = tris[tc - 1];
            tris[tc - 1] = tris[tc - 2];
            tris[tc - 2] = temp;

            temp = tris[tc - 4];
            tris[tc - 4] = tris[tc - 5];
            tris[tc - 5] = temp;

        }

        quads.Add(vertCount + 0);
        quads.Add(vertCount + 1);
        quads.Add(vertCount + 2);
        quads.Add(vertCount + 3);


        vertNormals.Add(normal);
        vertNormals.Add(normal);
        vertNormals.Add(normal);
        vertNormals.Add(normal);

        faceNormals.Add(normal);

        // Resize all data structures representing this mesh to fit the new mesh size.
        int[] newVerts = { v0, v1, v2, v3 };

        int quadReference = (quads.IndexOf(v0) / 4);
        int uv0 = v0, uv1 = v1, uv2 = -1, uv3 = -1;
        getOppositeSideOfQuad(quadReference, uv0, uv1, out uv2, out uv3);

        for (int i = 0; i < uvMaps.Count; i++)
        {
            uvMaps[i].resizeUVLength(vertCount + 4);//, newVerts);
        }
    }
    public void getOppositeSideOfQuad(int face, int a, int b, out int c, out int d)
    {
        c = a;
        d = b;
        if (quads[face * 4 + 0] == a &&
            quads[face * 4 + 1] == b)
        {
            c = quads[face * 4 + 2];
            d = quads[face * 4 + 3];
        }
        else if (
            quads[face * 4 + 1] == a &&
            quads[face * 4 + 0] == b)
        {
            c = quads[face * 4 + 3];
            d = quads[face * 4 + 2];
        }
        else if (
            quads[face * 4 + 0] == a &&
            quads[face * 4 + 2] == b)
        {
            c = quads[face * 4 + 1];
            d = quads[face * 4 + 3];
        }
        else if (
            quads[face * 4 + 2] == a &&
            quads[face * 4 + 0] == b)
        {
            c = quads[face * 4 + 3];
            d = quads[face * 4 + 1];
        }
        else if (
            quads[face * 4 + 1] == a &&
            quads[face * 4 + 3] == b)
        {
            c = quads[face * 4 + 0];
            d = quads[face * 4 + 2];
        }
        else if (
            quads[face * 4 + 3] == a &&
            quads[face * 4 + 1] == b)
        {
            c = quads[face * 4 + 2];
            d = quads[face * 4 + 0];
        }
        else if (
            quads[face * 4 + 3] == a &&
            quads[face * 4 + 2] == b)
        {
            c = quads[face * 4 + 1];
            d = quads[face * 4 + 0];
        }
        else if (
            quads[face * 4 + 2] == a &&
            quads[face * 4 + 3] == b)
        {
            c = quads[face * 4 + 0];
            d = quads[face * 4 + 1];
        }

    }

    public void getOppositeSideOfQuadRelative(int face, int a, int b, out int c, out int d)
    {
        c = a;
        d = b;
        if (0 == a &&
            1 == b)
        {
            c = 2;
            d = 3;
        }
        else if (
            1 == a &&
            0 == b)
        {
            c = 3;
            d = 2;
        }
        else if (
            0 == a &&
            2 == b)
        {
            c = 1;
            d = 3;
        }
        else if (
            2 == a &&
            0 == b)
        {
            c = 3;
            d = 1;
        }
        else if (
            1 == a &&
            3 == b)
        {
            c = 0;
            d = 2;
        }
        else if (
           3 == a &&
            1 == b)
        {
            c = 2;
            d = 0;
        }
        else if (
            3 == a &&
            2 == b)
        {
            c = 1;
            d = 0;
        }
        else if (
            2 == a &&
            3 == b)
        {
            c = 0;
            d = 1;
        }

    }

    public void addQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 n)
    {
        int vertCount = verts.Count;

        verts.Add(v0);
        verts.Add(v1);
        verts.Add(v2);
        verts.Add(v3);

        colours.Add(Color.white);
        colours.Add(Color.white);
        colours.Add(Color.white);
        colours.Add(Color.white);

        vertNormals.Add(n);
        vertNormals.Add(n);
        vertNormals.Add(n);
        vertNormals.Add(n);

        faceNormals.Add(n);





        // Test the normals, if they don't face outward, flip the triangles. (0, 3 are the verts grabbed)
        Vector3 cross = Vector3.Cross(v1 - v0, v2 - v0);
        if (Vector3.Dot(cross, n) < 0)
        {
            tris.Add(vertCount + 0); // a (v1)
            tris.Add(vertCount + 2); // b (v0)
            tris.Add(vertCount + 1); // c (v2)
        }
        else
        {
            tris.Add(vertCount + 0); // a (v1)
            tris.Add(vertCount + 1); // b (v0)
            tris.Add(vertCount + 2); // c (v2)
        }

        cross = Vector3.Cross(v2 - v3, v1 - v3);
        if (Vector3.Dot(cross, n) < 0)
        {
            tris.Add(vertCount + 3); // a (v1)
            tris.Add(vertCount + 1); // b
            tris.Add(vertCount + 2); // c
        }
        else
        {
            tris.Add(vertCount + 3); // a (v1)
            tris.Add(vertCount + 2); // b
            tris.Add(vertCount + 1); // c
        }

        quads.Add(vertCount + 0);
        quads.Add(vertCount + 1);
        quads.Add(vertCount + 2);
        quads.Add(vertCount + 3);

        // Resize all data structures representing this mesh to fit the new mesh size.
        for (int i = 0; i < uvMaps.Count; i++)
        {
            uvMaps[i].resizeUVLength(vertCount + 4);//, newVerts);
        }
    }

    public void addMesh(Mesh mesh, Vector3 position)
    {
        Vector3 p = gameObject.transform.position;
        Quaternion r = gameObject.transform.rotation;
        Vector3 s = gameObject.transform.lossyScale;

        int newVerts = mesh.vertices.Length;
        int vertCount = verts.Count;
        Vector3 v;
        for (int i = 0; i < newVerts; i++)
        {
            v = mesh.vertices[i] + position;
            // Verts
            verts.Add(v);
            // Colours
            colours.Add(mesh.colors[i]);
            // Vert Normals
            vertNormals.Add(mesh.normals[i]);

            // Quads
            quads.Add(vertCount + i);

            // ConnectedVerts
            ListWrapper connected = new ListWrapper();
            for (int ii = 0; ii < newVerts; ii++)
            {
                if (ii != i)
                {
                    if (mesh.vertices[i] == mesh.vertices[ii])
                    {
                        connected.Add(vertCount + ii);
                    }
                }
            }

            connectedVerts.Add(connected);
        }

        // Tris
        for (int i = 0; i < mesh.triangles.Length; i+=3 )
        {
            tris.Add(mesh.triangles[i + 0] + vertCount);
            tris.Add(mesh.triangles[i + 1] + vertCount);
            tris.Add(mesh.triangles[i + 2] + vertCount);

            // FaceNormals
            if (i % 6 == 0)
            {
                Vector3 ab = mesh.vertices[mesh.triangles[i + 1]] - mesh.vertices[mesh.triangles[i]];
                Vector3 ac = mesh.vertices[mesh.triangles[i + 2]] - mesh.vertices[mesh.triangles[i]];

                faceNormals.Add(Vector3.Cross(ab, ac).normalized);
            }
        }

        // UVs
        for (int ii = 0; ii < uvMaps.Count; ii++)
        {
            uvMaps[ii].resizeUVLength(verts.Count);
        }

        pushNewGeometry();
    }

    // Check for any incompatible data arrays or uninitialised properties in the mesh
    public void verifyMeshData()
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;

        if (verts == null || verts.Count != mesh.vertices.Length)
        {

        }
    }

    public static bool intersectTriangleRay(Triangle t, Ray r, out Vector3 intersection)
    {
        Vector3 rOpposite = r.origin + r.direction * 10000.0f;
        Vector3 n;
        Vector3 intersectionTemp;

        intersection = Vector3.zero;

        n = Vector3.Cross(t.b - t.a, t.c - t.a);
        n = n.normalized;

        float d1 = Vector3.Dot((r.origin - t.a), (n));
        float d2 = Vector3.Dot((rOpposite - t.a), (n));

        if ((d1 * d2) >= 0.0f)
        {
            return false;
        }

        if (d1 == d2)
        {
            return false;
        }

        intersectionTemp = r.origin + (rOpposite - r.origin) * (-d1 / (d2 - d1));

        Vector3 cross = Vector3.Cross(n, (t.b - t.a));
        if (Vector3.Dot(cross, (intersectionTemp - t.a)) < 0.0f)
        {
            return false;
        }

        cross = Vector3.Cross(n, (t.c - t.b));
        if (Vector3.Dot(cross, (intersectionTemp - t.b)) < 0.0f)
        {
            return false;
        }

        cross = Vector3.Cross(n, (t.a - t.c));
        if (Vector3.Dot(cross, (intersectionTemp - t.a)) < 0.0f)
        {
            return false;
        }

        intersection = intersectionTemp;

        return true;
    }

    public static Vector3 closestPoint(Vector3 a, Vector3 b, Vector3 o)
    {
        float d = Vector3.SqrMagnitude(b - a);

        if (d > 0)
        {
            Vector3 ao = o - a;
            Vector3 ab = b - a;
            float dot = Vector3.Dot(ao, ab) / d;
            if (dot <= 0) { return a; }
            else if (dot >= 1) { return b; }
            else
            {
                return a + ab * dot;
            }
        }
        else
        {
            return a;
        }
    }
    public static Vector2 closestPoint(Vector2 a, Vector2 b, Vector2 o, bool constrain = true)
    {
        float d = Vector2.SqrMagnitude(b - a);

        if (d > 0)
        {
            Vector2 ao = o - a;
            Vector2 ab = b - a;
            float dot = Vector2.Dot(ao, ab) / d;

            if (constrain)
            {
                if (dot <= 0) { return a; }
                else if (dot >= 1) { return b; }
            }

            return a + ab * dot;

        }
        else
        {
            return a;
        }
    }

    void OnDisable()
    {
        //Debug.Log("Disabled");
    }

    void OnDestroy()
    {
        //Debug.Log("Destroyed");
    }



    public void OnRenderObject()
    {
       // Debug.Log("OnRenderObject...");
    }
    public void OnPostRender()
    {
        // Debug.Log("OnPostRenderObject...");
    }
#endif







    public void pushNewGeometry()
    {
        List<Vector3> vertsNormalised = new List<Vector3>();

        Vector3 p = gameObject.transform.position;
        Quaternion r = gameObject.transform.rotation;
        Vector3 s = gameObject.transform.lossyScale;
        //checkTransforms();
        for (int i = 0; i < verts.Count; i++)
        {
            Vector3 v = verts[i];
            v -= p;

            v = Quaternion.Inverse(r) * v;

            s.x = Mathf.Max(Mathf.Abs(s.x), minimumScale) * Mathf.Sign(s.x);
            s.y = Mathf.Max(Mathf.Abs(s.y), minimumScale) * Mathf.Sign(s.y);
            s.z = Mathf.Max(Mathf.Abs(s.z), minimumScale) * Mathf.Sign(s.z);

            v.x /= s.x;
            v.y /= s.y;
            v.z /= s.z;

            vertsNormalised.Add(v);
        }

        //MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        //Mesh mesh = meshFilter.sharedMesh;

        if (_mesh == null)
        {
            _mesh = new Mesh();
        }

        _mesh.triangles = new int[0];
        _mesh.vertices = vertsNormalised.ToArray();
        _mesh.normals = vertNormals.ToArray();
        _mesh.triangles = tris.ToArray();
        if (uvMaps.Count > currentUVMap)
        {
            _mesh.uv = uvMaps[currentUVMap].uvs;
        }
        if (_mesh.uv == null)
        {
            _mesh.uv = new Vector2[_mesh.vertices.Length];
        }
        _mesh.colors = colours.ToArray();

        recalculateNormals(_mesh);
        _mesh.RecalculateBounds();

        pushLocalMeshToGameObject();
    }

    public void pushUVData()
    {
        //MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        //Mesh mesh = mf.sharedMesh;
        mesh.uv = uvMaps[currentUVMap].newUvs;
        pushLocalMeshToGameObject();
    }

    public void pushColour()
    {
        //MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        //Mesh mesh = meshFilter.sharedMesh;

        mesh.colors = colours.ToArray();

        pushLocalMeshToGameObject();
    }

    public void pushLocalMeshToGameObject()
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh = this.mesh;
    }
    public void pushModifiedMeshToGameObject()
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh = this.meshModified;
    }

    public void recalculateNormals(Mesh mesh = null)
    {
        if (mesh == null)
        {
            mesh = GetComponent<MeshFilter>().sharedMesh;
        }
        mesh.RecalculateNormals();

        Vector3 p = gameObject.transform.position;
        Quaternion r = gameObject.transform.rotation;
        Vector3 s = gameObject.transform.lossyScale;

        faceNormals = new List<Vector3>();
        for (int i = 0; i < quads.Count; i += 4)
        {
            Vector3 n = Vector3.zero;

            int tA = (i / 4) * 2 * 3;
            int tB = ((i / 4) * 2 + 1) * 3;
            Vector3 ab = verts[tris[tA + 1]] - verts[tris[tA + 0]];
            Vector3 ac = verts[tris[tA + 2]] - verts[tris[tA + 0]];
            Vector3 dc = verts[tris[tB + 1]] - verts[tris[tB + 0]];
            Vector3 db = verts[tris[tB + 2]] - verts[tris[tB + 0]];


            Vector3 a = Vector3.Cross(ab, ac);
            Vector3 b = Vector3.Cross(dc, db);
            n = a + b;
            faceNormals.Add(n.normalized);
        }
    }

}