using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestHandLoader : MonoBehaviour
{
    public GameObject leftHandPrefab;
    public GameObject rightHandPrefab;

    private SkinnedMeshRenderer meshRendererLeft;
    private SkinnedMeshRenderer meshRendererRight;

    private Material materialLeft;
    private Material materialRight;
    public Material handMaterial;
    
    void Start()
    {
        if (leftHandPrefab)
        {
            meshRendererLeft = leftHandPrefab.GetComponent<SkinnedMeshRenderer>();
            if (meshRendererLeft)
            {
                meshRendererLeft.material = handMaterial;
                materialLeft = meshRendererLeft.material;
            }

            leftHandPrefab.GetComponent<OVRMeshRenderer>().enabled = true;
            leftHandPrefab.GetComponent<SkinnedMeshRenderer>().enabled = true;
            leftHandPrefab.GetComponent<OVRMesh>().enabled = true;
        }
        
        if (rightHandPrefab)
        {
            meshRendererRight = rightHandPrefab.GetComponent<SkinnedMeshRenderer>();
            if (meshRendererRight)
            {
                meshRendererRight.material = handMaterial;
                materialRight = meshRendererRight.material;
            }

            rightHandPrefab.GetComponent<OVRMeshRenderer>().enabled = true;
            rightHandPrefab.GetComponent<SkinnedMeshRenderer>().enabled = true;
            rightHandPrefab.GetComponent<OVRMesh>().enabled = true;
        }    
    }
    
    void Update()
    {
        
    }    
}