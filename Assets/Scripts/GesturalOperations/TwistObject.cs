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
        
        [SerializeField]
        private Axis _rotationAxis = Axis.Up;
        public Axis RotationAxis => _rotationAxis;
        
        private IGrabbable _grabbable;
        
        // vector from the hand at the first grab point to the hand on the second grab point,
        // projected onto the plane of the rotation.
        private Vector3 _previousHandsVectorOnPlane;
        private Transform _pivotTransform = _grabbable.Transform;
        
        private float _relativeAngle = 0.0f;
        private float _constrainedRelativeAngle = 0.0f;
        
        private Pose _worldPivotPose;
        private Quaternion _localRotation;
        
        private Mesh mesh;
        private Vector3[] initialMeshVertices;
        private Matrix4x4 _matrix;
        
        public void Initialize(IGrabbable grabbable)
        {
            _grabbable = grabbable;
        }
        
        public Pose ComputeWorldPivotPose()
        {
            var targetTransform = _grabbable.Transform;

            Vector3 worldPosition = targetTransform.position;
            
            Quaternion worldRotation = targetTransform.parent != null
                ? targetTransform.parent.rotation * _localRotation
                : _localRotation;

            return new Pose(worldPosition, worldRotation);
        }
        
        // Start is called before the first frame update
        public void BeginTransform()
        {
            Vector3 rotationAxis = CalculateRotationAxisInWorldSpace();
            _previousHandsVectorOnPlane = CalculateHandsVectorOnPlane(rotationAxis);
            _relativeAngle = _constrainedRelativeAngle;
        }
        
        public void UpdateTransform()
        {
            Vector3 rotationAxis = CalculateRotationAxisInWorldSpace();
            Vector3 handsVector = CalculateHandsVectorOnPlane(rotationAxis);
            float angleDelta =
                Vector3.SignedAngle(_previousHandsVectorOnPlane, handsVector, rotationAxis);

            float previousAngle = _constrainedRelativeAngle;
            _relativeAngle += angleDelta;
            _constrainedRelativeAngle = _relativeAngle;
            if (_constraints.MinAngle.Constrain)
            {
                _constrainedRelativeAngle =
                    Mathf.Max(_constrainedRelativeAngle, _constraints.MinAngle.Value);
            }

            if (_constraints.MaxAngle.Constrain)
            {
                _constrainedRelativeAngle =
                    Mathf.Min(_constrainedRelativeAngle, _constraints.MaxAngle.Value);
            }

            angleDelta = _constrainedRelativeAngle - previousAngle;

            // Apply this angle rotation about the axis to our transform
            _grabbable.Transform.RotateAround(_pivotTransform.position, rotationAxis, angleDelta);
            
            _previousHandsVectorOnPlane = handsVector;
        }
        
        public void EndTransform() { }

        private Vector3 CalculateRotationAxisInWorldSpace()
        {
            Vector3 worldAxis = Vector3.zero;
            worldAxis[(int)_rotationAxis] = 1f;
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