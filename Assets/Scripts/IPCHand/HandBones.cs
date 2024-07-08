using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VR3DModeling
{
    public class HandBones : MonoBehaviour
    {
        private TetHandMesh tetHandMesh;
        private Dictionary<int, int> vertexControl = new Dictionary<int, int>();

        public enum HandType
        {
            None = OVRPlugin.MeshType.None,
            HandLeft = OVRPlugin.MeshType.HandLeft,
            HandRight = OVRPlugin.MeshType.HandRight,
        }
        
        [SerializeField]
        private HandType _handType = HandType.None;
        private int _handIndex = -1;

        public bool RenderHandSkeleton = false;
        public bool RenderHandMesh = false;
        public bool RenderBoneVertices = true;
        
        [SerializeField]
        private List<Transform> jointTransforms = new List<Transform>();
        public IList<Transform> Joints => jointTransforms;

        private List<BoneVisualization> boneVisualizations;
        
        private void Awake()
        {
            if (_handType.ToString() != "None")
            {
                if (_handType.ToString() == "HandLeft")
                {
                    _handIndex = 0;
                }    
                else if (_handType.ToString() == "HandRight")
                {
                    _handIndex = 1;
                }

                // Get the tetrahedra hand mesh
                tetHandMesh = new TetHandMesh(_handIndex);

                // Calculate all the vertices on the bones
                //GetBoneVertices(RenderBoneVertices);

                // Render bones that assigned in Unity Editor
                if (RenderHandSkeleton)
                {
                    RenderSkeleton();
                }

                // Render hand mesh
                if (RenderHandMesh)
                {
                    RenderMesh();
                }
            }
            else
            {
                Debug.Log("Hand type has not been specified.");
            }
        }
        
        private void GetBoneVertices(bool RenderBoneVertices)
        {
            var tetVerts = GenerateMeshVertices(tetHandMesh.GetTetVerts);

            for (int i = 0; i < jointTransforms.Count; i++)
            {
                Vector3 start = jointTransforms[i].position;
                Vector3 end = jointTransforms[i].parent.position;

                if (start == end)
                    continue;

                int bone_id = i;
                for (int j = 0; j < tetHandMesh.GetTetIds.Length; j += 4)
                {
                    var tet = new int[]
                    {
                        tetHandMesh.GetTetIds[j + 0],
                        tetHandMesh.GetTetIds[j + 1],
                        tetHandMesh.GetTetIds[j + 2],
                        tetHandMesh.GetTetIds[j + 3]
                    };

                    bool fix_tri_points = true;
                    if (!fix_tri_points)
                    {
                        // fix the points belong the the intersected tetrahedron
                        if (IsSegmentIntersectingTetrahedron(start, end, tet))
                        {
                            foreach (var tet_v in tet)
                            {
                                if (!vertexControl.ContainsKey(tet_v))
                                {
                                    // only controlled by one specific bone
                                    vertexControl[tet_v] = bone_id;
                                }
                                else if (vertexControl[tet_v] != bone_id)
                                {
                                    // controlled by multiple bones
                                    vertexControl[tet_v] = -1;
                                }
                            }
                        }
                    }
                    else
                    {
                        // fix the points belong to the intersected triangles
                        Vector3 tet_v0 = tetVerts[tet[0]];
                        Vector3 tet_v1 = tetVerts[tet[1]];
                        Vector3 tet_v2 = tetVerts[tet[2]];
                        Vector3 tet_v3 = tetVerts[tet[3]];

                        // triangles in the tet
                        var triangles = new List<Vector3[]>()
                        {
                            new Vector3[] {tet_v0, tet_v1, tet_v2},
                            new Vector3[] {tet_v0, tet_v3, tet_v1},
                            new Vector3[] {tet_v1, tet_v3, tet_v2},
                            new Vector3[] {tet_v0, tet_v2, tet_v3}
                        };

                        // index in the tet
                        var tet_index = new List<int[]>()
                        {
                            new int[] {tet[0], tet[1], tet[2]},
                            new int[] {tet[0], tet[3], tet[1]},
                            new int[] {tet[1], tet[3], tet[2]},
                            new int[] {tet[0], tet[2], tet[3]}
                        };

                        for (int k = 0; k < triangles.Count; k++)
                        {
                            var triangle = triangles[k];
                            var tri_index = tet_index[k];

                            if (IsRayIntersectingTriangle(start, end, triangle[0], triangle[1], triangle[2]))
                            {
                                foreach (var index in tri_index)
                                {
                                    if (!vertexControl.ContainsKey(index))
                                    {
                                        // only controlled by one specific bone
                                        vertexControl[index] = bone_id;
                                    }
                                    else if (vertexControl[index] != bone_id)
                                    {
                                        // controlled by multiple bones
                                        vertexControl[index] = -1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // Render bone vertices
            if (RenderBoneVertices)
            {
                foreach (var vert in tetVerts)
                {
                    // All the vertices in the tetrahedral mesh
                    GameObject sp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sp.transform.position = vert;
                    sp.transform.localScale = new Vector3(0.0005f, 0.0005f, 0.0005f);
                }
            }
            
            int num = 0;
            foreach (var vertex in vertexControl)
            {
                if (vertex.Value != -1)
                {
                    //Debug.Log(vertex.Value);
                    int v_i = vertex.Key;
                    GameObject sphere1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere1.transform.position = tetVerts[v_i];
                    sphere1.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

                    Renderer renderer = sphere1.GetComponent<Renderer>();
                    renderer.material.color = GetColorForBone(vertex.Value);
                } 
                else
                {
                    //Debug.Log(vertex.Value);
                    num++;
                }
            }

            Debug.Log("There are " + vertexControl.Count + " vertices are controlled by the skeleton");
            Debug.Log(num + " vertices are controlled by multiple bones");
        }

        private Color GetColorForBone(int id)
        {
            Random.InitState(id);
            return new Color(Random.value, Random.value, Random.value);
        }
        
        private bool IsSegmentIntersectingTetrahedron(Vector3 start, Vector3 end, int[] tet)
        {
            // Check if the segment intersects with the tetrahedron
            var tetVerts = GenerateMeshVertices(tetHandMesh.GetTetVerts);

            var tet_v0 = tetVerts[tet[0]];
            var tet_v1 = tetVerts[tet[1]];
            var tet_v2 = tetVerts[tet[2]];
            var tet_v3 = tetVerts[tet[3]];

            return IsRayIntersectingTriangle(start, end, tet_v0, tet_v1, tet_v2) ||
                   IsRayIntersectingTriangle(start, end, tet_v0, tet_v3, tet_v1) ||
                   IsRayIntersectingTriangle(start, end, tet_v1, tet_v3, tet_v2) ||
                   IsRayIntersectingTriangle(start, end, tet_v0, tet_v2, tet_v3);
        }

        private bool IsLineSegmentIntersectingTriangle(Vector3 ve0, Vector3 ve1, Vector3 vt0, Vector3 vt1, Vector3 vt2)
        {
            // This function is not used
            if (IsRayIntersectingTriangle(ve0, ve1, vt0, vt1, vt2))
            {
                float sign_0 = Mathf.Sign(SignedVolumeOfTetrahedron(ve0, vt0, vt1, vt2));
                float sign_1 = Mathf.Sign(SignedVolumeOfTetrahedron(ve1, vt0, vt1, vt2));

                // Opposite signs mean the segment intersects the triangle
                if (sign_0 != sign_1) { return true; }

                return false;
            }
            else
            {
                return false;
            }
        }

        private bool IsRayIntersectingTriangle(Vector3 ve0, Vector3 ve1, Vector3 vt0, Vector3 vt1, Vector3 vt2)
        {
            Vector3 rayVector = ve1 - ve0;
            Vector3 outIntersectionPoint = Vector3.zero;

            Vector3 edge1 = vt1 - vt0;
            Vector3 edge2 = vt2 - vt0;

            // Check if the ray intersects the triangle's plane
            Vector3 normal = Vector3.Cross(edge1, edge2);
            if (Vector3.Dot(normal, ve0 - vt0) * Vector3.Dot(normal, ve1 - vt0) > 0.0f)
            {
                return false;
            }

            Vector3 h = Vector3.Cross(rayVector, edge2);
            float det = Vector3.Dot(edge1, h);
            
            // If determinant is near zero, ray lies in plane of triangle otherwise not
            if (Mathf.Abs(det) < Mathf.Epsilon)
            {
                return false;
            }

            float invDet = 1.0f / det;

            // Calculate u parameter
            float u = invDet * Vector3.Dot(ve0 - vt0, h);
            if (u < 0.0f || u > 1.0f)
            {
                return false;
            }

            // Calculate v parameter
            Vector3 q = Vector3.Cross(ve0 - vt0, edge1);
            float v = invDet * Vector3.Dot(rayVector, q);
            if (v < 0.0f || u + v > 1.0f)
            {
                return false;
            }
            
            float t = invDet * Vector3.Dot(edge2, q);
            if (t > Mathf.Epsilon)
            {
                outIntersectionPoint = ve0 + rayVector * t;
                return true;
            }
            
            return false;
        }

        private float SignedVolumeOfTetrahedron(Vector3 q, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            // Computes the signed volume of a series of tetrahedrons defined by the vertices
            float volume = (1.0f / 6.0f) * Vector3.Dot(Vector3.Cross(p2 - p1, p3 - p1), q - p1);
            return volume;
        }

        private void RenderMesh()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (!meshFilter)
                meshFilter = gameObject.AddComponent<MeshFilter>();

            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (!meshRenderer)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.SetVertices(GenerateMeshVertices(tetHandMesh.GetTetVerts));
            mesh.triangles = tetHandMesh.CalculateTetTriIds();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
        }
        
        private List<Vector3> GenerateMeshVertices(float[] verts)
        {
            List<Vector3> vertices = new();

            for (int i = 0; i < verts.Length; i += 3)
            {
                Vector3 v = new(verts[i], verts[i + 1], verts[i + 2]);
                vertices.Add(v);
            }

            return vertices;
        }
        
        private void RenderSkeleton()
        {
            // Render tetrahedran vertices of all bones
            boneVisualizations = new List<BoneVisualization>();

            GameObject _skeletonGO = new GameObject("SkeletonRenderer");
            _skeletonGO.transform.SetParent(transform, false);

            Material _skeletonMaterial = new Material(Shader.Find("Diffuse"));
            _skeletonMaterial.color = Color.red;

            for (int i = 0; i < jointTransforms.Count; i++)
            {
                var boneVis = new BoneVisualization(
                    _skeletonGO,
                    _skeletonMaterial,
                    jointTransforms[i],
                    jointTransforms[i].parent
                );
                    
                boneVisualizations.Add(boneVis);

                // Render white spheres on the bones' positions
                GameObject start = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GameObject end = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                start.transform.position = jointTransforms[i].position;
                end.transform.position = jointTransforms[i].parent.position;
                start.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                end.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            }
        }
    }
}
