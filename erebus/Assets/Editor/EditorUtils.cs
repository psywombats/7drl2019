using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class EditorUtils {

    public static string NameFromPath(string path) {
        char[] splitters = { '/' };
        string[] split = path.Split(splitters);
        string name = split[split.Length - 1];
        name = name.IndexOf('.') > 0 ? name.Substring(0, name.IndexOf('.')) : name;
        return name;
    }

    public static string LocalDirectoryFromPath(string path) {
        List<string> components = new List<string>(path.Split(new char[] { '/' }));
        components.RemoveAt(components.Count - 1);
        return string.Join("/", components);
    }

    public static Vector2Int GetPreprocessedImageSize(TextureImporter importer) {
        object[] args = new object[2] { 0, 0 };
        MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
        mi.Invoke(importer, args);
        return new Vector2Int((int)args[0], (int)args[1]);
    }

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
}
