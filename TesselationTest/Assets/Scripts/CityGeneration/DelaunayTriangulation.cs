// Version 2023
//  (Updates: supports different root positions)
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;
using System.Drawing;
using System.Linq;

namespace Demo
{
    public class DelaunayTriangulation : MonoBehaviour
    {
        [Header("Grid Parameters")]
        public int rows = 10;
        public int columns = 10;
        public int rowWidth = 10;
        public int columnWidth = 10;

        [Header("Debugging")]
        [SerializeField] bool debugLines = false;

        [HideInInspector] public bool useSeed = false;

        RandomGenerator rnd;

        List<Vector2> vertices = new List<Vector2>();
        public List<Triangle> triangles = new List<Triangle>();
        public List<Edge> edges = new List<Edge>();

        void Start()
        {
            rnd = GetComponent<RandomGenerator>();

            //Generate();
        }

        void Update()
        {
            //if (Input.GetKeyDown(KeyCode.G))
            //{
            //    DestroyChildren();
            //    Generate();
            //}

            if (!debugLines)
            {
                return;
            }

            foreach (Vector2 point in vertices)
            {
                Vector3 position = new Vector3(point.x, 0, point.y);
                Debug.DrawRay(position, Vector3.up * 10, UnityEngine.Color.green);
            }

            foreach(Triangle triangle in triangles)
            {
                DebugTriangle(triangle);
            }
        }

        public void Execute()
        {
            DestroyChildren();
            Generate();
        }

        void DestroyChildren()
        {
            //for (int i = 0; i < transform.childCount; i++)
            //{
            //    Destroy(transform.GetChild(i).gameObject);
            //}

            //Empty lists
            vertices.Clear();
            triangles.Clear();
            edges.Clear();
        }

        public void Generate()
        {
            if (useSeed && rnd != null)
            {
                rnd.ResetRandom();
            }

            GenerateVertices();

            TriangulateVertices(vertices);

            GetAllEdges();
        }

        void GenerateVertices()
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    int randPosX = rnd.Next(columnWidth - 1);
                    int randPosY = rnd.Next(rowWidth - 1);

                    float posX = col * columnWidth + randPosX;
                    float posY = row * rowWidth + randPosY;

                    // Place a random point in each grid cell
                    vertices.Add(new Vector2(posX, posY));
                }
            }
        }

        Triangle SuperTriangle()
        {
            //Generate a triangle that contains all Delaunay points

            //top left
            Vector2 v1 = new Vector2(-((columns * columnWidth) / 2), rows * rowWidth);

            //top right
            Vector2 v2 = new Vector2(columns * columnWidth * 1.5f, rows * rowWidth);

            //bottom
            Vector2 v3 = new Vector2((columns * columnWidth) / 2, -rows * rowWidth);

            Triangle superTriangle = new Triangle(v1, v2, v3);

            return superTriangle;
        }

        

        void TriangulateVertices(List<Vector2> vertices)
        {
            // Create a triangle that contains all vertices
            Triangle superTriangle = SuperTriangle();
            triangles.Add(superTriangle);

            // Triangulate each vertex
            foreach (Vector2 vertex in vertices) {
                triangles = AddVertex(vertex, triangles);
            }

            triangles = triangles.Where(triangle =>
                !SharesVertexWithSuperTriangle(triangle, superTriangle)
            ).ToList();
        }

        bool SharesVertexWithSuperTriangle(Triangle t, Triangle super)
        {
            return t.v1 == super.v1 || t.v1 == super.v2 || t.v1 == super.v3 ||
                   t.v2 == super.v1 || t.v2 == super.v2 || t.v2 == super.v3 ||
                   t.v3 == super.v1 || t.v3 == super.v2 || t.v3 == super.v3;
        }

        List<Triangle> AddVertex(Vector2 vertex, List<Triangle> triangles)
        {
            List<Edge> edges = new List<Edge>();

            List<Triangle> newTriangles = new List<Triangle>();
            foreach (Triangle triangle in triangles) {
                if (triangle.InCircumcircle(vertex))
                {
                    edges.Add(new Edge(triangle.v1, triangle.v2));
                    edges.Add(new Edge(triangle.v2, triangle.v3));
                    edges.Add(new Edge(triangle.v3, triangle.v1));
                }
                else
                {
                    newTriangles.Add(triangle);
                }
            }

            //Get unique edges
            edges = UniqueEdges(edges);

            // Create new triangles from the unique edges and new vertex
            foreach (Edge edge in edges) {
                newTriangles.Add(new Triangle(edge.v1, edge.v2, vertex));
            }

            return newTriangles;
        }

        public static List<Edge> UniqueEdges(List<Edge> edges) {
            List<Edge> uniqueEdges = new List<Edge>();

            for (int i = 0; i < edges.Count; ++i)
            {
                bool isUnique = true;

                // See if edge is unique
                for (var j = 0; j < edges.Count; ++j)
                {
                    if (i != j && edges[i].Equals(edges[j]))
                    {
                        isUnique = false;
                        break;
                    }
                }

                // Edge is unique, add to unique edges array
                if (isUnique)
                {
                    uniqueEdges.Add(edges[i]);
                }
            }

            return uniqueEdges;
        } 
        
        void GetAllEdges()
        {
            edges.Clear();

            foreach (Triangle triangle in triangles)
            {
                TryAddEdge(new Edge(triangle.v1, triangle.v2), triangle);
                TryAddEdge(new Edge(triangle.v2, triangle.v3), triangle);
                TryAddEdge(new Edge(triangle.v3, triangle.v1), triangle);
            }
        }

        void TryAddEdge(Edge newEdge, Triangle triangle)
        {
            // Check if edges share the triangle
            foreach (Edge edge in edges)
            {
                if (edge.Equals(newEdge))
                {
                    edge.AddTriangle(triangle);
                    return;
                }
            }

            // add new edge
            newEdge.AddTriangle(triangle);
            edges.Add(newEdge);
        }

        public static void DebugTriangle(Triangle triangle)
        {
            DebugEdge(triangle.v1, triangle.v2);
            DebugEdge(triangle.v1, triangle.v3);
            DebugEdge(triangle.v2, triangle.v3);
        }

        public static void DebugEdge(Vector2 v1, Vector2 v2)
        {
            Vector3 startPos = new Vector3(v1.x, 0, v1.y);
            Vector3 endPos = new Vector3(v2.x, 0, v2.y);
            Debug.DrawLine(startPos, endPos, UnityEngine.Color.red);
        }

        public static void DebugEdge(Edge edge)
        {
            Vector3 startPos = new Vector3(edge.v1.x, 0, edge.v1.y);
            Vector3 endPos = new Vector3(edge.v2.x, 0, edge.v2.y);
            Debug.DrawLine(startPos, endPos, UnityEngine.Color.red);
        }
    }

    public class Edge
    {
        public Vector2 v1;
        public Vector2 v2;
        public Triangle t1;
        public Triangle t2;

        public Edge(Vector2 v1, Vector2 v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }

        public void AddTriangle(Triangle triangle)
        {
            if (t1 == null)
                t1 = triangle;
            else if (t2 == null && t1 != triangle)
                t2 = triangle;
        }

        public bool Equals(Edge other)
        {
            return (v1.Equals(other.v1) && v2.Equals(other.v2)) || (v1.Equals(other.v2) && v2.Equals(other.v1));
        }
    }

    public class Triangle
    {
        public Vector2 v1;
        public Vector2 v2;
        public Vector2 v3;

        public Triangle(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

        public bool InCircumcircle(Vector2 vertex)
        {
            float ax = v1.x - vertex.x;
            float ay = v1.y - vertex.y;
            float bx = v2.x - vertex.x;
            float by = v2.y - vertex.y;
            float cx = v3.x - vertex.x;
            float cy = v3.y - vertex.y;

            float det = (ax * ax + ay * ay) * (bx * cy - cx * by)
                      - (bx * bx + by * by) * (ax * cy - cx * ay)
                      + (cx * cx + cy * cy) * (ax * by - bx * ay);

            return det < 0;
        }
    }
}