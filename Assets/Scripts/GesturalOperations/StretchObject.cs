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
            
            // Our transformer rotation is based off our previously saved rotation
            Quaternion initialRotation = _activeRotation;
            
            // The base rotation is based on the delta in vector rotation between grab points
            Vector3 initialVector = _previousGrabPointB.position - _previousGrabPointA.position;
            Vector3 targetVector = grabB.position - grabA.position;
            Quaternion baseRotation = Quaternion.FromToRotation(initialVector, targetVector);
            
            // Any local grab point rotation contributes 50% of its rotation to the final transformation
            // If both grab points rotate the same amount locally, the final result is a 1-1 rotation
            Quaternion deltaA = grabA.rotation * Quaternion.Inverse(_previousGrabPointA.rotation);
            Quaternion halfDeltaA = Quaternion.Slerp(Quaternion.identity, deltaA, 1.0f);
            
            Quaternion deltaB = grabB.rotation * Quaternion.Inverse(_previousGrabPointB.rotation);
            Quaternion halfDeltaB = Quaternion.Slerp(Quaternion.identity, deltaB, 1.0f);
            
            // Apply all the rotation deltas
            Quaternion baseTargetRotation = baseRotation * halfDeltaA * halfDeltaB * initialRotation;

            // Normalize the rotation
            Vector3 upDirection = baseTargetRotation * Vector3.up;
            Quaternion targetRotation = Quaternion.LookRotation(targetVector, upDirection).normalized;

            // Save this target rotation as our active rotation state for future updates
            _activeRotation = targetRotation;
            
            // Stretch scale logic
            float activeDistance = targetVector.magnitude;
            if(Mathf.Abs(activeDistance) < 0.0001f) activeDistance = 0.0001f;
            
            float scalePercentage = activeDistance / _initialDistance;
            float previousScale = _activeScale;
            _activeScale = _initialScale * scalePercentage;
            
            var nextScale = _activeScale * _initialLocalScale;
            _activeScale = nextScale.x / _initialLocalScale.x;
            
            // Apply the positional delta initialCenter -> targetCenter and the
            // rotational delta initialRotation -> targetRotation to the target transform
            Vector3 worldOffsetFromCenter = targetTransform.position - initialCenter;

            Vector3 offsetInTargetSpace = Quaternion.Inverse(initialRotation) * worldOffsetFromCenter;
            offsetInTargetSpace /= previousScale;

            Quaternion rotationInTargetSpace = Quaternion.Inverse(initialRotation) * targetTransform.rotation;

            targetTransform.position = (targetRotation * (_activeScale * offsetInTargetSpace)) + targetCenter;
            targetTransform.rotation = targetRotation * rotationInTargetSpace;
            
            // Unform scale 
            // targetTransform.localScale = nextScale;
            
            // Single axis scale
            targetTransform.localScale = new Vector3(
                targetTransform.localScale.x,
                nextScale.y,
                targetTransform.localScale.z
            );
            
            _previousGrabPointA = new Pose(grabA.position, grabA.rotation);
            _previousGrabPointB = new Pose(grabB.position, grabB.rotation);
        }
        
        public void EndTransform() { }
    }
}

