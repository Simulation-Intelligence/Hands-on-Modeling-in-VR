using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Oculus.Interaction
{
    public class TwistObject : MonoBehaviour, ITransformer
    {
        public enum Axis{
            Right = 0,
            Up = 1,
            Forward = 2
        }
        
        private Axis _rotationAxis = Axis.Up;        

        private IGrabbable _grabbable;
        
        // vector from the hand at the first grab point to the hand on the second grab point,
        // projected onto the plane of the rotation.
        private Vector3 _initialHandsVectorOnPlane;
        private Vector3 _previousHandsVectorOnPlane;
        private Transform _pivotTransform = null;
        
        private float _relativeAngle = 0.0f;
        private float _constrainedRelativeAngle = 0.0f;
        
        private Pose _worldPivotPose;
        private Quaternion _localRotation;

        private float minY = float.MaxValue;
        private float maxY = float.MinValue;
        
        private Mesh mesh;
        private Vector3[] initialMeshVertices;
        private Matrix4x4 _matrix;
        private Vector3 _initialLocalScale;
        
        public void Initialize(IGrabbable grabbable)
        {
            _grabbable = grabbable;
        }
        
        // Start is called before the first frame update
        public void BeginTransform()
        {
            mesh = GetComponent<MeshFilter>().mesh;
            initialMeshVertices = mesh.vertices;
            _initialLocalScale = _grabbable.Transform.localScale;

            // find the minimum and maximum y-values of the vertices
            foreach (Vector3 vertex in initialMeshVertices)
            {
                if (vertex.y < minY) minY = vertex.y;
                if (vertex.y > maxY) maxY = vertex.y;
            }
            
            // use the _grabbable.Transform as the _pivotTransform
            _pivotTransform = _grabbable.Transform;

            Vector3 rotationAxis = CalculateRotationAxisInWorldSpace();
            _previousHandsVectorOnPlane = CalculateHandsVectorOnPlane(rotationAxis);
            _relativeAngle = _constrainedRelativeAngle;
        }
        
        public void UpdateTransform()
        {
            Vector3 rotationAxis = CalculateRotationAxisInWorldSpace();
            Vector3 handsVector = CalculateHandsVectorOnPlane(rotationAxis);
            float angleDelta = Vector3.SignedAngle(_previousHandsVectorOnPlane, handsVector, rotationAxis);
            
            float previousAngle = _constrainedRelativeAngle;
            _relativeAngle += angleDelta;
            _constrainedRelativeAngle = _relativeAngle;
            
            // if (_constraints.MinAngle.Constrain)
            // {
            //     _constrainedRelativeAngle =
            //         Mathf.Max(_constrainedRelativeAngle, _constraints.MinAngle.Value);
            // }

            // if (_constraints.MaxAngle.Constrain)
            // {
            //     _constrainedRelativeAngle =
            //         Mathf.Min(_constrainedRelativeAngle, _constraints.MaxAngle.Value);
            // }
            
            angleDelta = _constrainedRelativeAngle - previousAngle;
            
            // Vector3 trans = new Vector3(0, 0, 0);
            // Apply this angle rotation about the axis to our transform
            // Quaternion rotation = Quaternion.AngleAxis(_relativeAngle, Vector3.up);
            // var scale = new Vector3(1, 1, 1);
            // MatrixTransform(trans, rotation, scale);
            // UpdateMeshVertices();
            TwistVertices();
            
            // Apply this angle rotation about the axis to our transform
            // _grabbable.Transform.RotateAround(_pivotTransform.position, rotationAxis, angleDelta);
            
            _previousHandsVectorOnPlane = handsVector;
        }
        
        public void EndTransform() { }
        
        public void MatrixTransform(Vector3 trans, Quaternion rotation, Vector3 scale)
        {
            // include translation, rotation, and scale
            _matrix.SetTRS(trans, rotation, scale);
        }
        
        public void TwistVertices()
        {
            Vector3[] vertices = mesh.vertices;
            
            for (int i = 0; i < initialMeshVertices.Length; i++)
            {
                float fraction = (vertices[i].y - minY) / (maxY - minY);
                float twistAngle = fraction * _relativeAngle * 0.5f;
                
                Matrix4x4 matrix = Matrix4x4.Rotate(Quaternion.Euler(0, twistAngle, 0));
                vertices[i] = matrix.MultiplyPoint3x4(initialMeshVertices[i]);
            }

            mesh.vertices = vertices;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
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

        private Vector3 CalculateRotationAxisInWorldSpace()
        {
            Vector3 worldAxis = Vector3.zero;
            worldAxis[(int)_rotationAxis] = 1f;
            
            // convert the local axis worldAxis into world space
            return _pivotTransform.TransformDirection(worldAxis);
        }

        private Vector3 CalculateHandsVectorOnPlane(Vector3 planeNormal)
        {
            Vector3[] grabPointsOnPlane =
            {
                Vector3.ProjectOnPlane(_grabbable.GrabPoints[0].position, planeNormal),
                Vector3.ProjectOnPlane(_grabbable.GrabPoints[1].position, planeNormal),
            };

            return grabPointsOnPlane[1] - grabPointsOnPlane[0];
        }
    }
}