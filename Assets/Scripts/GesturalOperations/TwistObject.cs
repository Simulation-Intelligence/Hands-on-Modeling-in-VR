using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Oculus.Interaction
{
    public class TwistObject : MonoBehaviour, ITransformer
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
        
        public void UpdateTransform()
        {
            
        }
        
        public void EndTransform() { }
    }
}