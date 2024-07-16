using UnityEngine;

namespace VR3DModeling
{
    //public class BoneVisualization
    //{
    //    private GameObject BoneGO;
    //    private Transform BoneBegin;
    //    private Transform BoneEnd;
    //    private LineRenderer Line;
    //    private Material RenderMaterial;
    //    private const float LINE_RENDERER_WIDTH = 0.002f;

    //    public BoneVisualization(GameObject rootGO,
    //        Material renderMat,
    //        Transform begin,
    //        Transform end)
    //    {
    //        RenderMaterial = renderMat;

    //        BoneBegin = begin;
    //        BoneEnd = end;

    //        BoneGO = new GameObject(begin.name);
    //        BoneGO.transform.SetParent(rootGO.transform, false);

    //        Line = BoneGO.AddComponent<LineRenderer>();
    //        Line.sharedMaterial = RenderMaterial;
    //        Line.useWorldSpace = true;
    //        Line.positionCount = 2;

    //        Line.SetPosition(0, BoneBegin.position);
    //        Line.SetPosition(1, BoneEnd.position);

    //        Line.startWidth = LINE_RENDERER_WIDTH;
    //        Line.endWidth = LINE_RENDERER_WIDTH;
    //    }

    //    public void Update(float scale, bool shouldRender)
    //    {
    //        Line.SetPosition(0, BoneBegin.position);
    //        Line.SetPosition(1, BoneEnd.position);

    //        Line.startWidth = LINE_RENDERER_WIDTH;
    //        Line.endWidth = LINE_RENDERER_WIDTH;
            
    //        Line.enabled = shouldRender;
    //        Line.sharedMaterial = RenderMaterial;
    //    }
    //}
}