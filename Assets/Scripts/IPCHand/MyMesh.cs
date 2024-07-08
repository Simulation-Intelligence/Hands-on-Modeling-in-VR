using System.Collections.Generic;
using UnityEngine;

namespace VR3DModeling
{
    public class MyMesh : MonoBehaviour
    {
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        private void Awake()
        {
            Setup();
        }

        public void Enable(bool b)
        {
            gameObject.SetActive(b);
        }

        public void SetMesh(Mesh mesh)
        {
            meshFilter.mesh = mesh;
        }

        public Mesh GetMesh()
        {
            return meshFilter.mesh;
        }

        public void SetMaterial(Material mat)
        {
            meshRenderer.material = mat;
        }

        public void SetMaterial(Material mat, int index)
        {
            meshRenderer.materials[index] = mat;
        }

        public Material GetMaterial(int index)
        {
            return meshRenderer.materials[index];
        }

        public int GetMeshVertexCount()
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponentInChildren<MeshFilter>();
            }
            return meshFilter.sharedMesh.vertexCount;
        }

        public int GetNIndices()
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }
            return (int)meshFilter.sharedMesh.GetIndexCount(0);
        }

        public Vector3[] GetVertices()
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent <MeshFilter>();
            }
            return meshFilter.sharedMesh.vertices;
        }

        public int[] GetIndices()
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }
            return meshFilter.sharedMesh.GetIndices(0);
        }

        public void SetAll(List<Vector3> vertices, int[] triangles, List<Vector3> normals, List<Vector2> UV0, List<Vector4> tangents)
        {
            meshFilter.mesh.Clear();
            meshFilter.mesh.SetVertices(vertices);
            meshFilter.mesh.triangles = triangles;
            meshFilter.mesh.SetNormals(normals);
            meshFilter.mesh.SetUVs(0, UV0);
            meshFilter.mesh.SetTangents(tangents);

            //meshFilter.mesh.RecalculateNormals();
            //meshFilter.mesh.RecalculateTangents();
        }

        public void SetLists(Vector3[] vertices, int[] triangles, List<Vector2> UV0)
        {
            meshFilter.mesh.Clear();
            meshFilter.mesh.SetVertices(vertices);
            meshFilter.mesh.triangles = triangles;
            meshFilter.mesh.SetUVs(0, UV0);

            meshFilter.mesh.RecalculateNormals();
            meshFilter.mesh.RecalculateTangents();
            meshFilter.mesh.RecalculateBounds();
        }

        public void SetVertices(ref Vector3[] vertices, ref Bounds b)
        {
            meshFilter.mesh.vertices = vertices;
            meshFilter.mesh.bounds = b;
            RefreshMesh();
        }
        
        public void RefreshMesh()
        { 
            meshFilter.mesh.RecalculateNormals();    
        }

        public void CopyMesh(MyMesh meshToCopy)
        {
            Mesh otherMesh = meshToCopy.meshFilter.mesh;

            meshFilter.mesh.vertices = otherMesh.vertices;
            meshFilter.mesh.triangles = otherMesh.triangles;
            meshFilter.mesh.uv = otherMesh.uv;
            meshFilter.mesh.normals = otherMesh.normals;
            meshFilter.mesh.colors = otherMesh.colors;
            meshFilter.mesh.tangents = otherMesh.tangents;

            //CopyMaterials(meshToCopy);
        }

        private void CopyMaterials(MyMesh meshToCopy)
        {
            int numberOfMaterials = meshToCopy.meshRenderer.materials.Length;
            for (int i = 0; i < numberOfMaterials; i++)
            {
                meshRenderer.material.CopyMatchingPropertiesFromMaterial(meshToCopy.meshRenderer.materials[i]);
            }
        }
        
        public void SetVisibility(bool visible)
        {
            meshRenderer.enabled = visible;
        }

        private void Reset()
        {
            Setup();
        }

        public void Setup()
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponentInChildren<MeshFilter>();
                
                if (meshFilter == null)
                {
                    meshFilter = gameObject.AddComponent<MeshFilter>();
                }
            }

            if (meshRenderer == null)
            {
                meshRenderer = GetComponentInChildren<MeshRenderer>();
                
                if (meshRenderer == null)
                {
                    meshRenderer = gameObject.AddComponent<MeshRenderer>();
                }
            }
        }
    
        public Vector3 ComputeTriangleNormal(int triangleId)
        {
            return meshFilter.mesh.normals[triangleId];
        }
    }
}