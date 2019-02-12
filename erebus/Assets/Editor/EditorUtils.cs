using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

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

    public static IntVector2 GetPreprocessedImageSize(TextureImporter importer) {
        object[] args = new object[2] { 0, 0 };
        MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
        mi.Invoke(importer, args);
        return new IntVector2((int)args[0], (int)args[1]);
    }
}
