using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchObject : MonoBehaviour
{
    public OVRHand leftHand;
    public OVRHand rightHand;
    private bool isLeftHandIndexFingerPinching;
    private bool isRightHandIndexFingerPinching;
    
    // Start is called before the first frame update
    void Start()
    {
    
    }
    
    // Update is called once per frame
    void Update()
    {
        if (leftHand.IsTracked || rightHand.IsTracked)
        {
            isLeftHandIndexFingerPinching = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            isRightHandIndexFingerPinching = rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            
            if (isLeftHandIndexFingerPinching || isRightHandIndexFingerPinching)
            {
                pinchObject();
            }
        }
    }
    
    void pinchObject() {
        if (isLeftHandIndexFingerPinching) {
            transform.position = leftHand.transform.position;
            transform.rotation = Quaternion.LookRotation(transform.position - leftHand.transform.position);
        } else if (isRightHandIndexFingerPinching) {
            transform.position = rightHand.transform.position;
            transform.rotation = Quaternion.LookRotation(transform.position - rightHand.transform.position);
        }
    }
}
