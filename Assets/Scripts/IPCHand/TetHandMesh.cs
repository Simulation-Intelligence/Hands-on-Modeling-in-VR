using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VR3DModeling
{
    public class TetHandMesh
    {
        private int _handIndex;

        private static string LeftTetHandMesh_FilePath = "Assets/Scripts/HandMeshes/TetLeftHandMesh.vtk";
        private static string RightTetHandMesh_FilePath = "Assets/Scripts/HandMeshes/TetRightHandMesh.vtk";
        
        private string filePath;

        private float[] tetVerts;
        private int[] tetIds;
        private int[] tetEdgeIds;

        // Exterior triangles used for rendering in Unity
        private float[] surfaceVerts;
        private List<Vector3> surfaceVertices = new List<Vector3>();
        private List<int> tetSurfaceTriIds = new List<int>();

        // Getters
        public float[] GetTetVerts => tetVerts;
        public int[] GetTetIds => tetIds;
        public int[] GetTetEdgeIds => tetEdgeIds;
        public int[] GetTetSurfaceTriIds => tetSurfaceTriIds.ToArray();

        public int GetNumberOfVertices => tetVerts.Length / 3;
        public int GetNumberOfTetrahedrons => tetIds.Length / 4;
        public int GetNumberOfEdges => tetEdgeIds.Length / 2;
        
        private readonly Color[] colors = new Color[] { Color.red, Color.blue, Color.green, Color.yellow, Color.cyan };

        public TetHandMesh(int handIndex)
        {
            // Specify hand mesh by index
            _handIndex = handIndex;
            
            if (_handIndex != -1)
            {
                if (_handIndex == 0)
                {
                    filePath = LeftTetHandMesh_FilePath;
                }
                else if (_handIndex == 1)
                {
                    filePath = RightTetHandMesh_FilePath;
                }

                if (filePath != null)
                {
                    List<float> vertList = new List<float>();
                    List<int> tetIdList = new List<int>();

                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string line;
                        bool readingPoints = false, readingCells = false;

                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.StartsWith("POINTS"))
                            {
                                readingPoints = true;
                                readingCells = false;

                                // Jump to the next line
                                continue;
                            }
                            else if (line.StartsWith("CELLS"))
                            {
                                readingPoints = false;
                            }
                            else if (line.StartsWith("CONNECTIVITY"))
                            {
                                readingCells = true;
                                continue;
                            }
                            else if (line.StartsWith("CELL_TYPES"))
                            {
                                readingCells = false;
                                break;
                            }

                            // Read data line by line
                            if (readingPoints && !line.StartsWith("CELLS"))
                            {
                                string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string part in parts)
                                {
                                    if (float.TryParse(part, out float parsedValue))
                                        vertList.Add(parsedValue);
                                }
                            }
                            else if (readingCells && !line.StartsWith("CELL_TYPES"))
                            {
                                string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string part in parts)
                                {
                                    if (int.TryParse(part, out int parsedValue))
                                        tetIdList.Add(parsedValue);
                                }
                            }
                        }
                    }

                    tetVerts = vertList.ToArray();
                    Debug.Log("Number of vertices: " + tetVerts.Length / 3);
                    
                    tetIds = tetIdList.ToArray();
                    Debug.Log("Number of tetrahedrons: " + tetIds.Length / 4);

                    //tetSurfaceTriIds = 
                    Debug.Log("Number of triangular surfaces on exterior boundary: " + tetSurfaceTriIds);

                    // Calculate triangles on the exterior boundary of the tetrahedron mesh
                    //CalculateTetSurfaceTriIds();
                }
                else
                {
                    Debug.LogError("Tetrahedra hand mesh has not been created");
                }
            }    
        }

        // Used for testing the visualization
        public Mesh GetMesh()
        {
            Mesh mesh = new Mesh();

            // Render tet hand mesh with interior vertices
            mesh.SetVertices(GenerateMeshVertices(tetVerts));
            mesh.triangles = CalculateTetTriIds();

            // Render tet hand mesh with only exterior vertices
            //mesh.vertices = surfaceVertices.ToArray();
            //mesh.triangles = tetSurfaceTriIds.ToArray();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            
            return mesh;
        }

        // Calculate triangles on the exterior boundary of the tetrahedron mesh
        private void CalculateTetSurfaceTriIds()
        {
            Dictionary<string, int> faceCount = new Dictionary<string, int>();
            Dictionary<string, int> vertexMap = new Dictionary<string, int>();

            for (int i = 0; i < tetIds.Length; i += 4)
            {
                int i0 = tetIds[i + 0];
                int i1 = tetIds[i + 1];
                int i2 = tetIds[i + 2];
                int i3 = tetIds[i + 3];

                // Find the four triangles of this tetrahedron
                var faces = new List<int[]>
            {
                new int[] { i0, i1, i2 },
                new int[] { i0, i3, i1 },
                new int[] { i1, i3, i2 },
                new int[] { i0, i2, i3 },
            };

                // Sort every face and add it to HashMap
                foreach (var face in faces)
                {
                    System.Array.Sort(face);
                    string key = $"{face[0]}, {face[1]}, {face[2]}";

                    if (faceCount.ContainsKey(key))
                        faceCount[key]++;
                    else
                        faceCount[key] = 1;
                }
            }

            int triNums = 0;
            // Get the face that only appears once
            foreach (var entry in faceCount)
            {
                if (entry.Value == 1)
                {
                    triNums++;
                    string[] indices = entry.Key.Split(',');
                    int[] face = new int[3];

                    for (int j = 0; j < 3; j++)
                    {
                        face[j] = int.Parse(indices[j]);
                    }

                    foreach (var index in face)
                    {
                        string vertexKey = $"{tetVerts[index * 3]}, {tetVerts[index * 3 + 1]}, {tetVerts[index * 3 + 2]}";

                        if (!vertexMap.ContainsKey(vertexKey))
                        {
                            vertexMap[vertexKey] = surfaceVertices.Count;
                            surfaceVertices.Add(new Vector3(tetVerts[index * 3], tetVerts[index * 3 + 1], tetVerts[index * 3 + 2]));
                        }

                        tetSurfaceTriIds.Add(vertexMap[vertexKey]);
                    }
                }
            }

            Debug.Log("Number of faces on exterior boundary: " + triNums);
            Debug.Log(tetSurfaceTriIds.Count); // should be triNums * 3
            for (int i = 0; i < tetSurfaceTriIds.Count; i++)
            {
                Debug.Log("id is£º " + tetSurfaceTriIds[i]);
            }
        }

        public int[] CalculateTetTriIds()
        {
            List<int> triangles = new List<int>();

            for (int i = 0; i < tetIds.Length; i += 4)
            {
                int v0 = tetIds[i + 0];
                int v1 = tetIds[i + 1];
                int v2 = tetIds[i + 2];
                int v3 = tetIds[i + 3];

                // Add triangles
                triangles.AddRange(new int[] { v0, v2, v1 });
                triangles.AddRange(new int[] { v0, v3, v2 });
                triangles.AddRange(new int[] { v0, v1, v3 });
                triangles.AddRange(new int[] { v1, v2, v3 });
            }

            return triangles.ToArray();
        }

        private List<Vector3> GenerateMeshVertices(float[] tetVerts)
        {
            List<Vector3> vertices = new();

            for (int i = 0; i < tetVerts.Length; i += 3)
            {
                Vector3 v = new(tetVerts[i], tetVerts[i + 1], tetVerts[i + 2]);
                vertices.Add(v);
            }

            return vertices;
        }
    }
}

