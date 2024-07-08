using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Oculus.Interaction
{
    public class StretchObject : MonoBehaviour, ITransformer
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
            
            // Initialize transformer ratotion
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
            
            // Use the centroid of our grabs as the transformation center
            var initialCenter = Vector3.Lerp(_previousGrabPointA.position, _previousGrabPointB.position, 0.5f);
            var targetCenter = Vector3.Lerp(grabA.position, grabB.position, 0.5f);

            Quaternion initialRotation = _activeRotation;
            
            Vector3 initialVector = _previousGrabPointB.position - _previousGrabPointA.position;
            Vector3 targetVector = grabB.position - grabA.position;
            
            Quaternion baseRotation = Quaternion.FromToRotation(initialVector, targetVector);

            Quaternion deltaA = grabA.rotation * Quaternion.Inverse(_previousGrabPointA.rotation);
            Quaternion halfDeltaA = Quaternion.Slerp(Quaternion.identity, deltaA, 0.5f);
            
            Quaternion deltaB = grabB.rotation * Quaternion.Inverse(_previousGrabPointB.rotation);
            Quaternion halfDeltaB = Quaternion.Slerp(Quaternion.identity, deltaB, 0.5f);
            
            Quaternion baseTargetRotation = baseRotation * halfDeltaA * halfDeltaB * initialRotation;
            
            Vector3 upDirection = baseTargetRotation * Vector3.up;
            Quaternion targetRotation = Quaternion.LookRotation(targetVector, upDirection).normalized;

            _activeRotation = targetRotation;
            
            float activeDistance = targetVector.magnitude;
            if(Mathf.Abs(activeDistance) < 0.0001f) activeDistance = 0.0001f;

            float scalePercentage = activeDistance / _initialDistance;
            float previousScale = _activeScale;
            _activeScale = _initialScale * scalePercentage;
            
            var nextScale = _activeScale * _initialLocalScale;
            _activeScale = nextScale.x / _initialLocalScale.x;
            
            Vector3 worldOffsetFromCenter = targetTransform.position - initialCenter;
            
            Vector3 offsetInTargetSpace = Quaternion.Inverse(initialRotation) * worldOffsetFromCenter;
            offsetInTargetSpace /= previousScale;
            
            Quaternion rotationInTargetSpace = Quaternion.Inverse(initialRotation) * targetTransform.rotation;
            
            _previousGrabPointA = new Pose(grabA.position, grabA.rotation);
            _previousGrabPointB = new Pose(grabB.position, grabB.rotation);
            
            // update position, rotation, and localScale
            // targetTransform.position = (targetRotation * (_activeScale * offsetInTargetSpace)) + targetCenter;
            // targetTransform.rotation = targetRotation * rotationInTargetSpace;            
            // targetTransform.localScale = nextScale;
            
            Vector3 trans = (targetRotation * (_activeScale * offsetInTargetSpace)) + targetCenter;
            Quaternion rotation = targetRotation * Quaternion.Inverse(initialRotation);
            var nextStretchScale = scalePercentage * _initialLocalScale;
            MatrixTransform(trans, rotation, nextStretchScale);
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