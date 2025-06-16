using System.Globalization;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VertexCounter), true)]
public class VertexCounterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        VertexCounter targetVertexCounter = (VertexCounter)target;

        if (GUILayout.Button("Count Vertices in Scene"))
        {
            int vertexCount = targetVertexCounter.GetVertexCount();

            string formattedVertexCount = string.Format(CultureInfo.InvariantCulture, "{0:N0}", vertexCount);

            Debug.Log("Vertex count: " + formattedVertexCount);


            int triangleCount = targetVertexCounter.GetTriangleCount();

            string formattedTriangleCount = string.Format(CultureInfo.InvariantCulture, "{0:N0}", triangleCount);

            Debug.Log("Triangle count: " + formattedTriangleCount);
        }

        DrawDefaultInspector();
    }
}
