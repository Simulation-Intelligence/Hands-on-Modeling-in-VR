using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VR3DModeling
{
    public class IPCHandBehaviour : MonoBehaviour
    {
        public enum HandType { Left, Right };

        [Range(1, 2)]
        public int nHands = 1;

        private static IPCHandBehaviour Instance;

        private OVRCameraRig oculusCameraRig;

        private OVRHand[] oculusHands;
        
        private OVRSkeleton[] oculusSkeletons;
        
        private List<GameObject>[] oculusBones;
        
        private List<HandInstance> hands;
        private HandInstance leftHand, rightHand;
        
        private List<TetHandMesh> tetHandMeshes;
        private TetHandMesh leftTetHandMesh, rightTetHandMesh;

        public IPCHandPrefabs prefabs;
        
        private void Awake()
        {
            // Singleton 
            if (Instance)
            {
                Destroy(this);
                Debug.Log("There can only be one IPCHandBehaviour in the scene.");
                return;
            }
            else
            {
                Instance = this;
            }

            // Initialize prefabs
            if (prefabs == null)
            {
                prefabs = Resources.Load<IPCHandPrefabs>("IPCHandPrefabs");
            }
            
        }

        void Start()
        {
            // Initialize one hand or two hands
            InitializeHands();
            //InitializeMesh();
            InitializeFEMMesh();

            // Setup Oculus Tracking
            oculusBones = new List<GameObject>[2];
            oculusBones[0] = new List<GameObject>();
            oculusBones[1] = new List<GameObject>();
            
            oculusCameraRig = GameObject.Find("OVRCameraRig").GetComponent<OVRCameraRig>();
            
            oculusHands = new OVRHand[]
            {
                GameObject.Find("OVRCameraRig/TrackingSpace/LeftHandAnchor/OVRHandPrefab").GetComponent<OVRHand>(),
                GameObject.Find("OVRCameraRig/TrackingSpace/RightHandAnchor/OVRHandPrefab").GetComponent<OVRHand>()
            };
            
            oculusSkeletons = new OVRSkeleton[]
            {
                GameObject.Find("OVRCameraRig/TrackingSpace/LeftHandAnchor/OVRHandPrefab").GetComponent<OVRSkeleton>(),
                GameObject.Find("OVRCameraRig/TrackingSpace/RightHandAnchor/OVRHandPrefab").GetComponent<OVRSkeleton>()
            };
            
            for (int i = 0; i < 2; i++)
            {
                Debug.Log("Number of bones in each hand skeleton is: " + oculusSkeletons[i].GetCurrentNumBones());
                for (int j = 0; j < oculusSkeletons[i].GetCurrentNumBones(); j++)
                {
                    GameObject bone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    bone.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
                    bone.name = "QUESTBone_" + i + "_" + j;
                    oculusBones[i].Add(bone);
                }
            }    

            // Set initial bone positions and rotations
            for (int i = 0; i < 2; i++)
            {
                IList<OVRBone> sbones = oculusSkeletons[i].Bones;

                for (int j = 0; j < sbones.Count; j++)
                {
                    oculusBones[i][j].transform.parent = null;
                    oculusBones[i][j].transform.position = sbones[j].Transform.position;
                    oculusBones[i][j].transform.rotation = sbones[j].Transform.rotation;

                    Vector3 bonePos = sbones[j].Transform.position;
                    Quaternion boneRot = sbones[j].Transform.rotation;
                }
            }
        }
        
        void InitializeHands()
        {
            hands = new List<HandInstance>();
            tetHandMeshes = new List<TetHandMesh>();
            
            for (int i = 0; i < nHands; i++)
            {
                //HandInstance hand = new HandInstance();
                GameObject handInstanceGo = Instantiate(prefabs.prefab_handInstance) as GameObject;
                handInstanceGo.name = "Hand Instance " + i;
                handInstanceGo.transform.SetParent(transform);
                
                HandInstance hand = handInstanceGo.GetComponent<HandInstance>();
                hand.Initialize(this, i);
                hands.Add(hand);

                TetHandMesh tetHandMesh = new TetHandMesh(i);
                tetHandMeshes.Add(tetHandMesh);
            }
            
            // Semantic variables for left and right hands
            if (nHands == 2)
            {
                leftHand = hands[0];
                rightHand = hands[1];
            }
            else
            {
                // Use the right hand as the default dominant hand
                rightHand = hands[0];
            }
        }
        
        void InitializeMesh()
        {
            for (int i = 0; i < nHands; i++)
            {
                HandInstance hand = hands[i];

                hand.nVerticesSkin = tetHandMeshes[i].GetNumberOfVertices;
                hand._vertices = new Vector3[hand.nVerticesSkin];
                hand._UV0 = new List<Vector2>();

                // Vertices

                // Triangles

                // Set mesh
                hand.mesh.SetLists(hand._vertices, hand._triangles, hand._UV0);
            }
        }
        
        void InitializeFEMMesh()
        {
            for (int i = 0; i < nHands; i++)
            {
                HandInstance hand = hands[i];

                hand.nVerticesFEM = tetHandMeshes[i].GetNumberOfVertices;
                hand._verticesFEM = new Vector3[hand.nVerticesFEM];
                hand._UV0FEM = new List<Vector2>();
                
                // Vertices
                for (int j = 0; j < hand.nVerticesFEM; j++)
                {
                    float x = tetHandMeshes[i].GetTetVerts[j * 3];
                    float y = tetHandMeshes[i].GetTetVerts[j * 3 + 1];
                    float z = tetHandMeshes[i].GetTetVerts[j * 3 + 2];

                    hand._verticesFEM[j] = new Vector3(x, y, z);
                }

                // Triangles
                hand._trianglesFEM = tetHandMeshes[i].CalculateTetTriIds();
                
                // Set FEM mesh
                hand.meshFEM.SetLists(hand._verticesFEM, hand._trianglesFEM, hand._UV0FEM);
            }
        }
        
        void Update()
        {
            MyUpdate();    
        }
        
        void MyUpdate()
        {

        }    
    }
}