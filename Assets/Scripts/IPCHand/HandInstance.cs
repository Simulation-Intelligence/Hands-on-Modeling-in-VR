using System.Collections.Generic;
using UnityEngine;

namespace VR3DModeling
{
    public class HandInstance : MonoBehaviour
    {
        public MyMesh mesh, meshFEM;
        
        public int _handIndex = -1;
        
        public IPCHandBehaviour _ipcHandBehaviour;

        public Vector3[] _vertices;
        public int[] _triangles;
        public List<Vector2> _UV0;
        public List<Vector3> _normals;
        public List<Vector4> _tangents;
        
        public Vector3[] _verticesFEM;
        public int[] _trianglesFEM;
        public List<Vector2> _UV0FEM;
        public List<Vector3> _normalsFEM;

        public int nVerticesSkin;
        public int nVerticesFEM;

        private double[] skinVerticesPositions;
        
        public void Initialize(IPCHandBehaviour ipcHandBehaviour, int handIndex)
        {
            _ipcHandBehaviour = ipcHandBehaviour;
            _handIndex = handIndex;
        }

        private void Start()
        {
            // Number of vertices as skin
            // nVerticesSkin = ?;
            // skinVerticesPositions = new double[nVerticesSkin * 3];
            
            // Number of vertices for FEM
            //nVerticesFEM = ?;
        }

        public void ShowSkin(bool on)
        {
            mesh.Enable(on);
        }

        public void UpdateMesh()
        {
            for (int i = 0; i < nVerticesSkin; i++)
            {
                //_vertices[i].x = ;
                //_vertices[i].y = ;
                //_vertices[i].z = ;
                 
            }
        }    

        public void UpdateFEMMesh()
        {
            for (int i = 0; i < nVerticesFEM; i++)
            {
                //_verticesFEM[i] = ;
            }
        }
    }

}