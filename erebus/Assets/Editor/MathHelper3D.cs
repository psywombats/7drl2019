﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MathHelper3D {

    public static float IntersectTri(Ray ray, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2) {
        const float EPSILON = 0.0000001f;
        Vector3 edge1, edge2, h, s, q;
        float a, f, u, v;
        edge1 = vertex1 - vertex0;
        edge2 = vertex2 - vertex0;
        h = Vector3.Cross(ray.direction, edge2);
        a = Vector3.Dot(edge1, h);

        if (a > -EPSILON && a < EPSILON) {
            return -1.0f;
        }
        f = 1.0f / a;
        s = ray.origin - vertex0;
        u = f * (Vector3.Dot(s, h));
        if (u < 0.0 || u > 1.0) {
            return -1.0f;
        }
        q = Vector3.Cross(s, edge1);
        v = f * Vector3.Dot(ray.direction, q);
        if (v < 0.0 || u + v > 1.0) {
            return -1.0f;
        }

        return f * Vector3.Dot(edge2, q);
    }

    // given some uvs, split them up based on our height
    // because y can sometimes be split across tiles
    // xMode is a hack to just get stuff looking right
    public static Vector2[] AdjustZ(Vector2[] origUVs, Tilemap tileset, float lowerHeight, bool xMode) {
        if (tileset == null) {
            return origUVs;
        }
        float unit = 1.0f / tileset.size.y;
        if (xMode) {
            if (Mathf.Abs(Mathf.Round(lowerHeight) - lowerHeight) > 0.1f) {
                return new Vector2[] {
                    origUVs[0],
                    origUVs[1],
                    new Vector2(origUVs[2].x, origUVs[2].y + unit / 2.0f),
                    new Vector2(origUVs[3].x, origUVs[3].y + unit / 2.0f),
                };
            } else {
                return new Vector2[] {
                    new Vector2(origUVs[0].x, origUVs[0].y - unit / 2.0f),
                    new Vector2(origUVs[1].x, origUVs[1].y - unit / 2.0f),
                    origUVs[2],
                    origUVs[3],
                };
            }
        } else {
            if (Math.Abs(Mathf.Round(lowerHeight) - lowerHeight) < 0.1f) {
                return new Vector2[] {
                    new Vector2(origUVs[2].x, origUVs[2].y),
                    new Vector2(origUVs[0].x, origUVs[0].y - unit / 2.0f),
                    new Vector2(origUVs[3].x, origUVs[3].y),
                    new Vector2(origUVs[1].x, origUVs[1].y - unit / 2.0f),
                };
            } else {
                return new Vector2[] {
                    new Vector2(origUVs[2].x, origUVs[2].y + unit / 2.0f),
                    new Vector2(origUVs[0].x, origUVs[0].y),
                    new Vector2(origUVs[3].x, origUVs[3].y + unit / 2.0f),
                    new Vector2(origUVs[1].x, origUVs[1].y),
                };
            }
        }
    }

    public static float RayDistanceForQuad(List<Vector3> vertices, List<int> tris, Ray ray, TerrainQuad quad) {
        float t1 = IntersectTri(ray,
            vertices[tris[quad.trisIndex + 0]],
            vertices[tris[quad.trisIndex + 1]],
            vertices[tris[quad.trisIndex + 2]]);
        float t2 = IntersectTri(ray,
            vertices[tris[quad.trisIndex + 3]],
            vertices[tris[quad.trisIndex + 4]],
            vertices[tris[quad.trisIndex + 5]]);
        if (t1 > 0.0f && t2 > 0.0f) {
            return t1 < t2 ? t1 : t2;
        } else {
            return t1 > t2 ? t1 : t2;
        }
    }

    public static float GetHeightAtMouse(TerrainQuad relativeToQuad) {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Vector3 midpoint = relativeToQuad.pos + new Vector3(0.5f, 0.0f, 0.5f);
        Plane plane = new Plane(-1.0f * Camera.current.transform.forward, midpoint);
        plane.Raycast(ray, out float enter);

        Vector3 hit = ray.GetPoint(enter);
        float height = Mathf.Round(hit.y * 2.0f) / 2.0f;
        return height > 0 ? height : 0;
    }

    public static void DrawQuads(List<TerrainQuad> selectedQuads, Color color) {
        foreach (TerrainQuad quad in selectedQuads) {
            Vector3 mid = quad.pos + new Vector3(0.5f, -0.25f, 0.5f) + new Vector3(
                quad.normal.x * 0.5f,
                quad.normal.y * 0.25f,
                quad.normal.z * 0.5f);
            Vector3 size = new Vector3(
                1.01f - Mathf.Abs(quad.normal.x),
                0.51f - (Mathf.Abs(quad.normal.y) * 0.5f),
                1.01f - Mathf.Abs(quad.normal.z));
            Handles.color = color;
            Handles.DrawWireCube(mid, size);
        }
    }

    public static List<TerrainQuad> GetQuadsAroundQuad(Dictionary<Vector3, Dictionary<Vector3, TerrainQuad>> quads, 
            TerrainQuad quad, Vector2 selectionSize) {
        List<TerrainQuad> selectedQuads = new List<TerrainQuad>();
        if (quad == null) {
            return selectedQuads;
        }
        int dx = Mathf.RoundToInt(selectionSize.x);
        int dy = Mathf.RoundToInt(selectionSize.y);
        int lowX = -1 * Mathf.CeilToInt(dx / 2.0f) + 1;
        int lowY = -1 * Mathf.CeilToInt(dy / 2.0f) + 1;
        int highX = Mathf.FloorToInt(dx / 2.0f);
        int highY = Mathf.FloorToInt(dy / 2.0f);
        for (int i = lowY; i <= highY; i += 1) {
            for (int j = lowX; j <= highX; j += 1) {
                Vector3 newPos = quad.pos;
                if (quad.normal.x != 0) {
                    newPos = new Vector3(
                        quad.pos.x,
                        quad.pos.y + i * 0.5f,
                        quad.pos.z + j);
                } else if (quad.normal.y != 0) {
                    newPos = new Vector3(
                        quad.pos.x + j,
                        quad.pos.y,
                        quad.pos.z + i);
                } else if (quad.normal.z != 0) {
                    newPos = new Vector3(
                        quad.pos.x + j,
                        quad.pos.y + i * 0.5f,
                        quad.pos.z);
                }
                if (quads.ContainsKey(newPos) && quads[newPos].ContainsKey(quad.normal)) {
                    selectedQuads.Add(quads[newPos][quad.normal]);
                }
            }
        }
        return selectedQuads;
    }

    // assume that q1/q2 share a normal
    public static List<TerrainQuad> GetQuadsInRect(Dictionary<Vector3, Dictionary<Vector3, TerrainQuad>> quads, 
            TerrainQuad q1, TerrainQuad q2) {
        List<TerrainQuad> selectedQuads = new List<TerrainQuad>();
        for (float x = Mathf.Min(q1.pos.x, q2.pos.x); x <= Math.Max(q1.pos.x, q2.pos.x); x += 1) {
            for (float z = Mathf.Min(q1.pos.z, q2.pos.z); z <= Math.Max(q1.pos.z, q2.pos.z); z += 1) {
                for (float y = Mathf.Min(q1.pos.y, q2.pos.y); y <= Math.Max(q1.pos.y, q2.pos.y); y += 0.5f) {
                    selectedQuads.Add(quads[new Vector3(x, y, z)][q1.normal]);
                }
            }
        }
        return selectedQuads;
    }
}
