using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Oculus.Interaction
{
    public class BendObject : MonoBehaviour, ITransformer
    {
        private IGrabbable _grabbable;
        
        public void Initialize(IGrabbable grabbable)
        {
            _grabbable = grabbable;
        }
        
        private Mesh mesh;
        private Vector3[] initialMeshVertices;
        private Matrix4x4 _matrix;

        public void BeginTransform()
        {
            mesh = GetComponent<MeshFilter>().mesh;
            initialMeshVertices = mesh.vertices;
        }
        
        public void UpdateTransform()
        {
            var grabA = _grabbable.GrabPoints[0];
            var grabB = _grabbable.GrabPoints[1];

            Vector3 _midPoint = (grabA.position + grabB.position) / 2.0f;
            Vector3 handDirection = (grabB.position - grabA.position).normalized;
            float bendAmount = 1.0f;
            
            BendVertices(bendAmount);
        }
        
        public void EndTransform() { }
        
        public void BendVertices(float offset)
        {
            Vector3[] vertices = mesh.vertices;
            
            for (int i = 0; i < initialMeshVertices.Length; i++)
            {
                vertices[i].y += Mathf.Sin(3.5f * initialMeshVertices[i].x) * 0.1f;
                // vertices[i].y += initialMeshVertices[i].x * 0.1f;
            }
            
            mesh.vertices = vertices;
            
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            
        }
    }
}