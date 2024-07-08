using System.IO;
using System.Text;
using UnityEngine;

namespace VR3DModeling
{
    public class ObjExporter : MonoBehaviour
    {
        public enum HandType
        {
            None = OVRPlugin.MeshType.None,
            HandLeft = OVRPlugin.MeshType.HandLeft,
            HandRight = OVRPlugin.MeshType.HandRight,
        }
        
        [SerializeField]
        [Tooltip("Change the hand type to store the mesh into .obj file.")]
        private HandType _handType = HandType.None;
        
        // The filepath to store the .obj file
        private static string _leftHandMeshFilePath = "Assets/Scripts/HandMeshes/LeftHandMesh.obj";
        private static string _rightHandMeshFilePath = "Assets/Scripts/HandMeshes/RightHandMesh.obj";
        
        private string _objFilePath;
        private Mesh _mesh;
        
        private void Awake()
        {
            if (_handType.ToString() != "None")
            {
                if (isFileExist(_leftHandMeshFilePath) && isFileExist(_rightHandMeshFilePath))
                {
                    Debug.Log("Hand meshes already exist!");
                }
                else
                {
                    SkinnedMeshRenderer skinnedMesh = GetComponent<SkinnedMeshRenderer>();
                    _mesh = skinnedMesh.sharedMesh;

                    // Export the triangular hand mesh into an .obj file
                    if (_handType.ToString() == "HandLeft")
                    {
                        _objFilePath = _leftHandMeshFilePath;
                        ExportMeshAsObj(_mesh, _objFilePath);

                        Debug.Log("Already exported the left triangle hand mesh into an .obj file");
                    }
                    else if (_handType.ToString() == "HandRight")
                    {
                        _objFilePath = _rightHandMeshFilePath;
                        ExportMeshAsObj(_mesh, _objFilePath);

                        Debug.Log("Already Exported the right triangle hand mesh into an .obj file");
                    }
                }
            }    
        }

        public bool isFileExist(string filePath)
        {
            if (File.Exists(filePath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public void ExportMeshAsObj(Mesh mesh, string objFilePath)
        {
            // Initialize StringBuilder for the OBJ file format
            StringBuilder stringBuilder = new StringBuilder();

            // Write vertices
            foreach (Vector3 v in mesh.vertices)
            {
                stringBuilder.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
            }
            stringBuilder.Append("\n");

            // Write normals
            foreach (Vector3 n in mesh.normals)
            {
                stringBuilder.Append(string.Format("n {0} {1} {2}\n", n.x, n.y, n.z));
            }
            stringBuilder.Append("\n");

            // Write uv cooridinates
            foreach (Vector2 uv in mesh.uv)
            {
                stringBuilder.Append(string.Format("vt {0} {1}\n", uv.x, uv.y));
            }
            
            // Write triangles (faces)
            for (int material = 0; material < mesh.subMeshCount; material++)
            {
                stringBuilder.Append("\n");
                int[] triangles = mesh.GetTriangles(material);
                
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    stringBuilder.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                                         triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
                }
            }

            // Write the OBJ data to a file
            File.WriteAllText(objFilePath, stringBuilder.ToString());
        }
    }
}