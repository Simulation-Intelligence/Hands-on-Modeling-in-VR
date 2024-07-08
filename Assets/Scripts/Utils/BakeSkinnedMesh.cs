using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static OVRPlugin;

public class BakeSkinnedMesh : MonoBehaviour
{
    private Matrix4x4[] bindPoses;
    private Vector3[] vertices;
    private Vector4[] tangents;
    private Vector3[] normals;
    private BoneWeight[] boneWeights;
    private int[] triangles;

    private Transform[] bones;
    private SkinnedMeshRenderer targetSkinnedMeshRenderer;

    // Extract information from the skinned mesh renderer component
    public void ExtractBindPose()
    {
        targetSkinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        
        if (targetSkinnedMeshRenderer && targetSkinnedMeshRenderer.sharedMesh)
        {
            bindPoses = targetSkinnedMeshRenderer.sharedMesh.bindposes;
            vertices = targetSkinnedMeshRenderer.sharedMesh.vertices;
            boneWeights = targetSkinnedMeshRenderer.sharedMesh.boneWeights;
            normals = targetSkinnedMeshRenderer.sharedMesh.normals;
            tangents = targetSkinnedMeshRenderer.sharedMesh.tangents;
            triangles = targetSkinnedMeshRenderer.sharedMesh.triangles;
            bones = targetSkinnedMeshRenderer.bones;

            Debug.Log("SHOW bindPoses");
            for (int i = 0; i < bindPoses.Length; i++)
            {
                Debug.Log(bindPoses[i]);
            }
            for (int i = 0; i < vertices.Length; i++)
            {
                Debug.Log(vertices[i]);
            }
            for (int i = 0; i < triangles.Length; i++)
            {
                Debug.Log(triangles[i]);
            }
            Debug.Log("SHOW bones positions");
            for (int i = 0; i < bones.Length; i++)
            {
                Debug.Log(bones[i].position);
            }
        }
        else
        {
            Debug.Log("There is no Skinned Mesh Renderer to bake mesh");
        }
    }
    
    // Calculate skinny matrices, every joint has a skinny matric that transforms the vertices from bind pose to the current pose
    // Bind pose is the matrix that transforms bones from mesh space to local space -> LocalToWorldMatrix * bindPose
    private Matrix4x4[] CalculateSkinningMatrices()
    {
        Matrix4x4[] SkinningMatrices = new Matrix4x4[bindPoses.Length];

        for (int i = 0; i < bindPoses.Length; i++)
        {
            Transform bone = bones[i];
            Matrix4x4 currentBoneWorldTransformationMatrix;

            if (bone)
            {
                currentBoneWorldTransformationMatrix = bone.localToWorldMatrix;
            }
            else
            {
                currentBoneWorldTransformationMatrix = targetSkinnedMeshRenderer.transform.localToWorldMatrix * bindPoses[i].inverse;
            }

            SkinningMatrices[i] = currentBoneWorldTransformationMatrix * bindPoses[i];
        }

        return SkinningMatrices;
    }

    // Bake the current bind pose
    private void BakeOnCurrentPose()
    {
        
    }
}