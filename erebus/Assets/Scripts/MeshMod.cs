using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
[AddComponentMenu("")]
public class MeshMod : MonoBehaviour
{
    private bool applied = false;

    [NonSerialized]
    Thread thread;
    [NonSerialized]
    bool isThreadRunning;
    [NonSerialized]
    Vector3[] threadVertices;
    [NonSerialized]
    Vector3[] threadNormals;
    [NonSerialized]
    Vector2[] threadUvs;
    [NonSerialized]
    Color[] threadColors;
    [NonSerialized]
    int[] threadTriangles;
    [NonSerialized]
    MeshMod[] meshMods;

    // Temp values for threading
    public void Update()
    {
        if (isThreadRunning && thread != null)
        {
            if (!thread.IsAlive)
            {
                isThreadRunning = false;
                pushChangesToMesh();
            }
        }
    }

    private void pushChangesToMesh()
    {

        // Push the changes to the mesh
        Mesh newMesh = new Mesh();
        newMesh.vertices = threadVertices;
        newMesh.normals = threadNormals;
        newMesh.uv = threadUvs;
        newMesh.colors = threadColors;
        newMesh.triangles = threadTriangles;
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();
        
        MeshEdit meshEdit = gameObject.GetComponent<MeshEdit>();
        meshEdit.meshModified = newMesh;
        meshEdit.pushModifiedMeshToGameObject();
    }

    public void OnValidate()
    {
        applyAllMods();
    }

    public void applyAllMods()
    {
    }

    private void applyAllModsInThread(List<Action> modifierFunctions)
    {
        for (int i = 0; i < modifierFunctions.Count; i++)
        {
            modifierFunctions[i]();
        }
    }
    
    public void OnDestroy()
    {
        this.enabled = false;
        GetComponent<MeshEdit>().applyModifiers(false);
    }

    public virtual void apply(
        ref Vector3[] meshVertices,
        ref Vector3[] meshNormals,
        ref Vector2[] meshUvs,
        ref Color[] meshColours,
        ref int[] meshTriangles)
    {
        applied = true;
    }

    protected bool applyModifier(bool forceApply)
    {
        return ((!applied) || forceApply);
    }
    /*
    protected void applyNormalised(Mesh mesh)
    {

        Vector3[] vertsNormalised = new Vector3[newVertices.Length];
        Vector3 p = gameObject.transform.position;
        Quaternion r = gameObject.transform.rotation;
        Vector3 s = gameObject.transform.lossyScale;
        if (s.x != 0 && s.y != 0 && s.z != 0)
        {
            for (int i = 0; i < newVertices.Length; i++)
            {
                Vector3 v = newVertices[i];
                v -= p;

                v = Quaternion.Inverse(r) * v;

                v.x /= s.x;
                v.y /= s.y;
                v.z /= s.z;

                vertsNormalised[i] = v;
            }
            mesh.vertices = vertsNormalised;
        }
    }
    */
    protected void flipNormals(Mesh mesh)
    {
        int[] newTriangles = new int[mesh.triangles.Length];
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            newTriangles[i] = mesh.triangles[i];
            newTriangles[i + 1] = mesh.triangles[i + 2];
            newTriangles[i + 2] = mesh.triangles[i + 1];
        }
        mesh.triangles = newTriangles;
    }

}
#endif
