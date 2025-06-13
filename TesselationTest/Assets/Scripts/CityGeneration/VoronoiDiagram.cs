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
using UnityEngine.Analytics;
using static UnityEngine.Rendering.DebugUI.Table;

namespace Demo
{
    public class VoronoiDiagram : MonoBehaviour
    {
        [Header("Generation parameters")]
        public int buildingMinHeight;
        public int buildingMaxHeight;
        public float distanceFromStreet = 2f;
        public float streetWidth = 0.5f;
        public float buildDelaySeconds = 0.1f;
        [SerializeField] bool generateBuildings = false;

        [Header("Prefabs")]
        public GameObject[] buildingPrefabs;
        public GameObject roadPrefab;

        [Header("Debugging")]
        [SerializeField] bool debugLines = false;

        [Header("Other")]
        [SerializeField] bool useSeed = false;

        RandomGenerator rnd;
        DelaunayTriangulation delaunay;

        List<Vector2> vertices = new List<Vector2>();
        List<Edge> edges = new List<Edge>();
        List<Edge> debugEdges = new List<Edge>();
        List<Vector2> buildingPositions = new List<Vector2>();

        void Start()
        {
            rnd = GetComponent<RandomGenerator>();
            delaunay = GetComponent<DelaunayTriangulation>();

            if (delaunay != null) {
                delaunay.useSeed = useSeed;
            }

            Generate();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                DestroyChildren();
                Generate();
            }

            if (delaunay != null)
            {
                delaunay.useSeed = useSeed;
            }

            if (!debugLines)
            {
                return;
            }

            foreach (Vector2 point in vertices)
            {
                Vector3 position = new Vector3(point.x, 0, point.y);
                Debug.DrawRay(position, Vector3.up * 10, UnityEngine.Color.green);
            }

            foreach (Edge edge in edges)
            {
                DelaunayTriangulation.DebugEdge(edge);
            }

            foreach (Edge edge in debugEdges)
            {
                DelaunayTriangulation.DebugEdge(edge);
            }

        }

        void DestroyChildren()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            vertices.Clear();
            edges.Clear();
            debugEdges.Clear();
            buildingPositions.Clear();
        }

        public void Generate()
        {
            if (delaunay == null) {
                return;
            }

            //Generate delaunay triangulation
            delaunay.Execute();

            //Store the center of each triangle
            //GenerateVertices();

            //Genereate the Voronoi edges
            GenerateEdges();

            //Spawn roads along the edges
            GenerateRoads();

            GenerateBuildings();
        }

        void GenerateVertices()
        {
            List<Triangle> triangles = delaunay.triangles;

            foreach (Triangle triangle in triangles)
            {
                vertices.Add(GetTriangleCenter(triangle));
            }
        }

        Vector2 GetTriangleCenter(Triangle triangle)
        {
            return GetCircumcenter(triangle.v1, triangle.v2, triangle.v3);
        }

        Vector2 GetCircumcenter(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float d = 2 * (v1.x * (v2.y - v3.y) +
                           v2.x * (v3.y - v1.y) +
                           v3.x * (v1.y - v2.y));

            float ux = ((v1.sqrMagnitude * (v2.y - v3.y) +
                         v2.sqrMagnitude * (v3.y - v1.y) +
                         v3.sqrMagnitude * (v1.y - v2.y)) / d);

            float uy = ((v1.sqrMagnitude * (v3.x - v2.x) +
                         v2.sqrMagnitude * (v1.x - v3.x) +
                         v3.sqrMagnitude * (v2.x - v1.x)) / d);

            return new Vector2(ux, uy);
        }

        void GenerateEdges()
        {
            foreach (Edge edge in delaunay.edges)
            {
                if (edge.t1 != null && edge.t2 != null)
                {
                    Vector2 v1 = GetTriangleCenter(edge.t1);
                    Vector2 v2 = GetTriangleCenter(edge.t2);

                    edges.Add(new Edge(v1, v2));
                }
            }

            edges = DelaunayTriangulation.UniqueEdges(edges);
        }

        void GenerateRoads()
        {
            if (!generateBuildings)
            {
                return;
            }

            foreach(Edge edge in edges)
            {
                Vector2 direction = edge.v2 - edge.v1;
                float length = direction.magnitude;

                Vector2 midPoint = edge.v1 + direction * 0.5f;
                Vector3 position = new Vector3(midPoint.x, 0, midPoint.y);

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.Euler(0, -angle, 0);

                GameObject street = Instantiate(roadPrefab, position, rotation, transform);

                street.transform.localScale = new Vector3(length, 0.2f, streetWidth);
            }
        }

        void GenerateBuildings()
        {
            foreach(Edge edge in edges)
            {
                //Get a random point along the vector
                float t = (float)(rnd.Rand.NextDouble());
                t = Mathf.Clamp(t, 0.2f, 0.8f);

                Vector2 randPoint = Vector2.Lerp(edge.v1, edge.v2, t); 

                //Choose to the left or to the right of the edge
                int whichDirection = rnd.Next(2);
                Vector2 direction;
                Vector2 edgeVector = edge.v2 - edge.v1;

                if(whichDirection == 0)
                {
                    direction = new Vector2(edgeVector.y, -edgeVector.x).normalized;
                }
                else
                {
                    direction = new Vector2(-edgeVector.y, edgeVector.x).normalized;
                }

                Vector2 spawnPoint = randPoint + (direction * distanceFromStreet);

                //Check if it overlaps with edges (streets) and other buildings
                if (!CanSpawnThere(spawnPoint)) {
                    continue;
                }

                vertices.Add(spawnPoint);

                debugEdges.Add(new Edge(randPoint, spawnPoint));

                if (generateBuildings)
                {
                    GameObject newBuilding = GenerateBuilding(spawnPoint);

                    Vector3 lookAtVector = new Vector3(randPoint.x, 0, randPoint.y);
                    newBuilding.transform.LookAt(lookAtVector);
                    newBuilding.transform.Rotate(new Vector3(0, 180, 0));

                    buildingPositions.Add(spawnPoint);
                }
            }
        }

        GameObject GenerateBuilding(Vector2 point)
        {
            Vector3 spawnPos = new Vector3(point.x, 0, point.y);

            // Create a new building, chosen randomly from the prefabs:
            int buildingIndex = rnd.Next(buildingPrefabs.Length);
            GameObject newBuilding = Instantiate(buildingPrefabs[buildingIndex], transform);

            // Place it in the grid:
            newBuilding.transform.localPosition = spawnPos;

            // If the building has a Base (grammar) component, launch the grammar and apply properties:
            Base baseBuilding = newBuilding.GetComponent<Base>();
            if (baseBuilding != null) {
                baseBuilding.Width = rnd.Next(2) + 2;
                baseBuilding.Depth = 2;
                baseBuilding.MinHeight = buildingMinHeight;
                baseBuilding.MaxHeight = buildingMaxHeight;

                baseBuilding.Generate();

                return newBuilding;
            }

            // If the building has a Shape (grammar) component, launch the grammar:
            Shape shape = newBuilding.GetComponent<Shape>();

            if (shape != null)
            {
                shape.Generate(buildDelaySeconds);
            }

            return newBuilding;
        }

        bool CanSpawnThere(Vector2 spawnPoint)
        {
            float overlapRadius = 1f;

            foreach(Edge edge in edges)
            {
                if(DistanceFromPointToEdge(spawnPoint, edge) <= overlapRadius)
                {
                    return false;
                }
            }

            foreach (Vector2 pos in buildingPositions) {
                float dist = (pos - spawnPoint).magnitude;

                if(dist <= overlapRadius * 2)
                {
                    return false;
                }
            }

            return true;
        }

        private float DistanceFromPointToEdge(Vector2 point, Edge edge)
        {
            Vector2 ab = edge.v2 - edge.v1;
            Vector2 ap = point - edge.v1;

            float t = Vector2.Dot(ap, ab) / ab.sqrMagnitude;
            t = Mathf.Clamp01(t);

            Vector2 closestPoint = edge.v1 + t * ab;

            return Vector2.Distance(point, closestPoint);
        }
    }
}