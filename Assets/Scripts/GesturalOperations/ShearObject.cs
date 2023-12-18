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
        private IGrabbable _grabbable;
        
        private Mesh mesh;
        private Vector3[] initialMeshVertices;
        private Matrix4x4 _matrix;
        public enum Axis {X, Y, Z};
        
        public void Initialize(IGrabbable grabbable)
        {
            _grabbable = grabbable;
        }
        
        // Start is called before the first frame update
        public void BeginTransform()
        {
            mesh = GetComponent<MeshFilter>().mesh;
            initialMeshVertices = mesh.vertices;
            
            var grabA = _grabbable.GrabPoints[0];
            var grabB = _grabbable.GrabPoints[1];
            
            Vector3 diff = grabB.position - grabA.position;
                        
            _activeRotation = Quaternion.LookRotation(diff, Vector3.up).normalized;
            _initialDistance = diff.magnitude;
            
            _activeScale = _grabbable.Transform.localScale.x;
            _initialScale = _activeScale;
            
            _initialLocalScale = _grabbable.Transform.localScale / _initialScale;
            
            _previousGrabPointA = new Pose(grabA.position, grabA.rotation);
            _previousGrabPointB = new Pose(grabB.position, grabB.rotation);
        }
        
        // Update is called once per frame
        public void UpdateTransform()
        {
            var grabA = _grabbable.GrabPoints[0];
            var grabB = _grabbable.GrabPoints[1];
            
            var targetTransform = _grabbable.Transform;
            
            Vector3 initialVector = _previousGrabPointB.position - _previousGrabPointA.position;
            Vector3 targetVector = grabB.position - grabA.position;
            
            Vector3 trans = new Vector3(0, 0, 0);
            Quaternion rotation = Quaternion.identity;
            var scale = _initialLocalScale;
            
            MatrixTransform(trans, rotation, scale);
            // shear the object
            float shear_angle1 = 90f - Vector3.Angle(targetVector, Vector3.right);
            float shear_angle2 = 0f;
            ShearVertices(Axis.X, shear_angle1, shear_angle2);
            UpdateMeshVertices();
            
            _previousGrabPointA = new Pose(grabA.position, grabA.rotation);
            _previousGrabPointB = new Pose(grabB.position, grabB.rotation);
        }
        
        public void EndTransform() { }
        
        public void MatrixTransform(Vector3 trans, Quaternion rotation, Vector3 scale)
        {
            // include translation, rotation, and scale
            _matrix.SetTRS(trans, rotation, scale);
        }
        
        public void ShearVertices(Axis axis, float shear_angle1, float shear_angle2)
        {
            switch (axis)
            {
                case Axis.X:
                    _matrix.m01 = Mathf.Tan(shear_angle1 * Mathf.Deg2Rad); // Shearing along X-axis with respect to Y
                    _matrix.m02 = Mathf.Tan(shear_angle2 * Mathf.Deg2Rad); // Shearing along X-axis with respect to Z
                    break;
                case Axis.Y:
                    _matrix.m10 = Mathf.Tan(shear_angle1 * Mathf.Deg2Rad); // Shearing along Y-axis with respect to X
                    _matrix.m12 = Mathf.Tan(shear_angle2 * Mathf.Deg2Rad); // Shearing along Y-axis with respect to Z
                    break;
                case Axis.Z:
                    _matrix.m20 = Mathf.Tan(shear_angle1 * Mathf.Deg2Rad); // Shearing along Z-axis with respect to X
                    _matrix.m21 = Mathf.Tan(shear_angle2 * Mathf.Deg2Rad); // Shearing along Z-axis with respect to Y
                    break;
            } 
        }
        
        public void UpdateMeshVertices()
        {
            Vector3[] vertices = mesh.vertices;
            
            for (int i = 0; i < initialMeshVertices.Length; i++)
            {
                vertices[i] = _matrix.MultiplyPoint3x4(initialMeshVertices[i]);
            }
            mesh.vertices = vertices;
            
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
    }
}