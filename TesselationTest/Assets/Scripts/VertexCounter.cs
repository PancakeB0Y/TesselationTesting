using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VertexCounter : MonoBehaviour
{
    int vertexCount = 0;
    int triangleCount = 0;

    int CountVertices(GameObject go)
    {
        int goVertexCount = 0;

        MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf != null && mf.sharedMesh != null)
            {
                goVertexCount += mf.sharedMesh.vertexCount;
            }
        }

        return goVertexCount;
    }

    int CountTriangles(GameObject go)
    {
        int goTriangleCount = 0;

        MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf != null && mf.sharedMesh != null)
            {
                goTriangleCount += mf.sharedMesh.triangles.Length / 3;
            }
        }

        return goTriangleCount;
    }

    void CountAllVerticesInScene()
    {
        int currentVertexCount = 0;

        List<GameObject> allGameObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None).ToList();

        for (int i = allGameObjects.Count - 1; i > 0; i--)
        {
            GameObject go = allGameObjects[i];

            if (go.CompareTag("Ignore"))
            {
                allGameObjects.RemoveAt(i);
            }
        }

        foreach (GameObject go in allGameObjects)
        {
            currentVertexCount += CountVertices(go);
        }

        vertexCount = currentVertexCount;
    }

    void CountAllTrianglesInScene()
    {
        int currentTriangleCount = 0;

        // all game object in the scene
        List<GameObject> allGameObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None).ToList();

        // remove unwanted game objects with the "Ignore" tag
        for (int i = allGameObjects.Count - 1; i > 0; i--)
        {
            GameObject go = allGameObjects[i];

            if (go.CompareTag("Ignore"))
            {
                allGameObjects.RemoveAt(i);
            }
        }

        foreach (GameObject go in allGameObjects)
        {
            currentTriangleCount += CountTriangles(go);
        }

        triangleCount = currentTriangleCount;
    }

    public int GetVertexCount()
    {
        CountAllVerticesInScene();

        return vertexCount;
    }

    public int GetTriangleCount()
    {
        CountAllTrianglesInScene();

        return triangleCount;
    }
}
