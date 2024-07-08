using System.Collections.Generic;
using UnityEngine;

public class BindPoseExample : MonoBehaviour
{
    private SkinnedMeshRenderer rend;

    private Animation anim;
    private AnimationCurve curve;
    private AnimationClip clip;

    private BakeSkinnedMesh bakeSkinnedMesh;
    
    private void Awake()
    {
        gameObject.AddComponent<Animation>();
        gameObject.AddComponent<SkinnedMeshRenderer>();
        gameObject.AddComponent<BakeSkinnedMesh>();

        rend = GetComponent<SkinnedMeshRenderer>();
        anim = GetComponent<Animation>();

        // Test bake the skinned mesh and extract the mesh bind pose
        bakeSkinnedMesh = GetComponent<BakeSkinnedMesh>();

        // Construct the mesh
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-1, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(-1, 5, 0),
            new Vector3(1, 5, 0)
        };
        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        
        mesh.triangles = new int[] { 0, 1, 2, 1, 3, 2 };
        mesh.RecalculateNormals();
        rend.material = new Material(Shader.Find("Diffuse"));

        // Assign bone weights to the mesh
        BoneWeight[] weights = new BoneWeight[4];
        weights[0].boneIndex0 = 0;
        weights[0].weight0 = 1;
        weights[1].boneIndex0 = 0;
        weights[1].weight0 = 1;
        weights[2].boneIndex0 = 1;
        weights[2].weight0 = 1;
        weights[3].boneIndex0 = 1;
        weights[3].weight0 = 1;
        mesh.boneWeights = weights;

        // Create bone transorms and bind poses
        // One bone at the bottom and one at the top
        Transform[] bones = new Transform[2];
        Matrix4x4[] bindPoses = new Matrix4x4[2];

        bones[0] = new GameObject("Lower").transform;
        bones[0].parent = transform; // Set the position relative to the parent

        bones[0].localRotation = Quaternion.identity;
        bones[0].localPosition = new Vector3(0, 0, 0);
        
        // bind pose is bone's inverse transform matrix
        bindPoses[0] = bones[0].worldToLocalMatrix * transform.localToWorldMatrix;

        bones[1] = new GameObject("Upper").transform;
        bones[1].parent = transform; // We need the bone to be a child of the GameObject with the SkinnedMeshRenderer
        bones[1].localRotation = Quaternion.identity;
        bones[1].localPosition = new Vector3(0, 5, 0);
        bindPoses[1] = bones[1].worldToLocalMatrix * transform.localToWorldMatrix;

        // Assign the bindPose array to the mesh
        mesh.bindposes = bindPoses;

        // Assign the bones and bind poses to the renderer
        rend.bones = bones;
        rend.sharedMesh = mesh;
    }
    
    private void Start()
    {
        bakeSkinnedMesh.ExtractBindPose();

        // Set the animation to play
        StartAnimation();
    }

    private void StartAnimation()
    {
        curve = new AnimationCurve();
        curve.keys = new Keyframe[]
        {
            new Keyframe(0, 0, 0, 0),
            new Keyframe(1, 2.5f, 0, 0),
            new Keyframe(2, 0, 0, 0)
        };

        clip = new AnimationClip();
        clip.SetCurve("Lower", typeof(Transform), "m_LocalPosition.y", curve);
        clip.legacy = true;

        clip.wrapMode = WrapMode.Loop;
        anim.AddClip(clip, "test");
        anim.Play("test");
    }
}