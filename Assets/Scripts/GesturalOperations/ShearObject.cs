using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Oculus.Interaction
{
    public class ShearObject : MonoBehaviour, ITransformer
    {
        private Quaternion _activeRotation;
        private Vector3 _initialLocalScale;
        private float _initialDistance;
        private float _initialScale = 1.0f;
        private float _activeScale = 1.0f;
        
        private Pose _previousGrabPointA;
        private Pose _previousGrabPointB;
        private Pose _grabDeltaInLocalSpace;
        private IGrabbable _grabbable;

        private Mesh mesh;
        private Matrix4x4 _matrix;
        
        public void Initialize(IGrabbable grabbable)
        {
            _grabbable = grabbable;
        }
        
        // Start is called before the first frame update
        public void BeginTransform()
        {
            var grabA = _grabbable.GrabPoints[0];
            var grabB = _grabbable.GrabPoints[1];
            
            // Initialize transformer ratotion
            Vector3 diff = grabB.position - grabA.position;
            
            _activeRotation = Quaternion.LookRotation(diff, Vector3.up).normalized;
            _initialDistance = diff.magnitude;
            
            _activeScale = _grabbable.Transform.localScale.x;
            _initialScale = _activeScale;
            
            _initialLocalScale = _grabbable.Transform.localScale / _initialScale;
            Debug.Log("The _initialLocalScale is： " + _initialLocalScale);
            
            _previousGrabPointA = new Pose(grabA.position, grabA.rotation);
            _previousGrabPointB = new Pose(grabB.position, grabB.rotation);
        }
        
        // Update is called once per frame
        public void UpdateTransform()
        {
            var grabA = _grabbable.GrabPoints[0];
            var grabB = _grabbable.GrabPoints[1];
            var targetTransform = _grabbable.Transform;
            
            // Use the centroid of our grabs as the transformation center
            var initialCenter = Vector3.Lerp(_previousGrabPointA.position, _previousGrabPointB.position, 0.5f);
            var targetCenter = Vector3.Lerp(grabA.position, grabB.position, 0.5f);
            
        }
        
        public void EndTransform() { }
        
        public void MatrixTransform(Vector3 trans, Quaternion rotation, Vector3 scale)
        {
            _matrix.SetTRS(trans, rotation, scale);
            Debug.Log(_matrix);
            UpdateMeshVertices();
        }
                
        public void UpdateMeshVertices()
        {
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = _matrix.MultiplyPoint3x4(vertices[i]);
            }
            mesh.vertices = vertices;
            
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
    }
}

// public class MeshShearer : MonoBehaviour
// {
//     public float shearFactorY = 0.5f;
//     public float shearFactorZ = 0.5f;

//     void Start()
//     {
//         Mesh mesh = GetComponent<MeshFilter>().mesh;
//         Vector3[] vertices = mesh.vertices;

//         Matrix4x4 shearMatrix = Matrix4x4.identity;
//         shearMatrix.m01 = shearFactorY; // Shearing along X-axis with respect to Y
//         shearMatrix.m02 = shearFactorZ; // Shearing along X-axis with respect to Z

//         for (int i = 0; i < vertices.Length; i++)
//         {
//             vertices[i] = shearMatrix.MultiplyPoint3x4(vertices[i]);
//         }

//         mesh.vertices = vertices;
//         mesh.RecalculateBounds();
//         mesh.RecalculateNormals();
//     }
// }