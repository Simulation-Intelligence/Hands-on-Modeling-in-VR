using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace VR3DModeling
{
    public class OVRHandMeshRenderer : MonoBehaviour
    {
        public interface IOVRMeshRendererDataProvider
        {
            MeshRendererData GetMeshRendererData();
        }

        public struct MeshRendererData
        {
            public bool IsDataValid { get; set; }
            public bool IsDataHighConfidence { get; set; }
            public bool ShouldUseSystemGestureMaterial { get; set; }
        }

        public enum ConfidenceBehavior
        {
            None,
            ToggleRenderer,
        }

        public enum SystemGestureBehavior
        {
            None,
            SwapMaterial,
        }

        [SerializeField]
        private IOVRMeshRendererDataProvider _dataProvider;

        [SerializeField]
        private OVRHandMeshInitialization _ovrHandMesh;

        [SerializeField]
        private OVRSkeleton _ovrSkeleton;

        [SerializeField]
        private ConfidenceBehavior _confidenceBehavior = ConfidenceBehavior.ToggleRenderer;

        [SerializeField]
        private SystemGestureBehavior _systemGestureBehavior = SystemGestureBehavior.SwapMaterial;

        [SerializeField]
        private Material _systemGestureMaterial = null;

        private Material _originalMaterial = null;

        private SkinnedMeshRenderer _skinnedMeshRenderer;
       
        private ObjExporter _objExporter = new ObjExporter();

        private string objFilePath;
        
        public bool IsInitialized { get; private set; }
        public bool IsDataValid { get; private set; }
        public bool IsDataHighConfidence { get; private set; }
        public bool ShouldUseSystemGestureMaterial { get; private set; }

        private void Awake()
        {
            if (_dataProvider == null)
            {
                _dataProvider = GetComponent<IOVRMeshRendererDataProvider>();
            }

            if (_ovrHandMesh == null)
            {
                _ovrHandMesh = GetComponent<OVRHandMeshInitialization>();
            }

            if (_ovrSkeleton == null)
            {
                _ovrSkeleton = GetComponent<OVRSkeleton>();
            }
        }

        private void Start()
        {
            if (_ovrHandMesh == null)
            {
                // disable if no mesh configured
                this.enabled = false;
                return;
            }

            if (ShouldInitialize())
            {
                Initialize();
            }
        }

        private bool ShouldInitialize()
        {
            //if (IsInitialized)
            //{
            //    return false;
            //}

            if ((_ovrHandMesh == null) || ((_ovrHandMesh != null) && !_ovrHandMesh.IsInitialized) ||
                ((_ovrSkeleton != null) && !_ovrSkeleton.IsInitialized))
            {
                // do not initialize if mesh or optional skeleton are not initialized
                return false;
            }

            return true;
        }
        
        private void Initialize()
        {
            _skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            if (!_skinnedMeshRenderer)
            {
                _skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
            }

            _skinnedMeshRenderer.sharedMesh = _ovrHandMesh.Mesh;
            _originalMaterial = _skinnedMeshRenderer.sharedMaterial;

            // Export the triangular hand mesh into an .obj file
            if (_ovrHandMesh.GetMeshType().ToString() == "HandLeft")
            {
                objFilePath = "Assets/Scripts/HandMeshes/LeftHandMesh.obj";
            }
            else if (_ovrHandMesh.GetMeshType().ToString() == "HandRight")
            {
                objFilePath = "Assets/Scripts/HandMeshes/RightHandMesh.obj";
            }
            
            if (!string.IsNullOrEmpty(objFilePath) && !_objExporter.isFileExist(objFilePath))
            {
                Debug.Log("Export the triangular hand mesh into an .obj file");
                _objExporter.ExportMeshAsObj(_ovrHandMesh.Mesh, objFilePath);
            }

            // Render the triangular hand mesh
            if ((_ovrSkeleton != null))
            {
                int numSkinnableBones = _ovrSkeleton.GetCurrentNumSkinnableBones();
                var bindPoses = new Matrix4x4[numSkinnableBones];
                var bones = new Transform[numSkinnableBones];
                var localToWorldMatrix = transform.localToWorldMatrix;
                
                for (int i = 0; i < numSkinnableBones && i < _ovrSkeleton.Bones.Count; ++i)
                {
                    bones[i] = _ovrSkeleton.Bones[i].Transform;
                    bindPoses[i] = _ovrSkeleton.BindPoses[i].Transform.worldToLocalMatrix * localToWorldMatrix;
                }

                _ovrHandMesh.Mesh.bindposes = bindPoses;
                _skinnedMeshRenderer.bones = bones;
                _skinnedMeshRenderer.updateWhenOffscreen = true;
            }

            IsInitialized = true;
            //Debug.Log(_skinnedMeshRenderer.bones.Length); // 19
            //for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
            //{
            //    if (i==5 || i==10)
            //    {
            //        Debug.Log("index = " + i);
            //        Debug.Log(_skinnedMeshRenderer.bones[i].name);
            //        Debug.Log(_skinnedMeshRenderer.bones[i].transform.position);
            //    }
            //}    
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (ShouldInitialize())
            {
                Initialize();
                //Debug.Log("Updating OVRHandMeshRenderer");
            }
#endif
            
            IsDataValid = false;
            IsDataHighConfidence = false;
            ShouldUseSystemGestureMaterial = false;

            if (IsInitialized)
            {
                bool shouldRender = false;

                if (_dataProvider != null)
                {
                    var data = _dataProvider.GetMeshRendererData();

                    IsDataValid = data.IsDataValid;
                    IsDataHighConfidence = data.IsDataHighConfidence;
                    ShouldUseSystemGestureMaterial = data.ShouldUseSystemGestureMaterial;

                    shouldRender = data.IsDataValid && data.IsDataHighConfidence;
                }

                if (_confidenceBehavior == ConfidenceBehavior.ToggleRenderer)
                {
                    if (_skinnedMeshRenderer != null && _skinnedMeshRenderer.enabled != shouldRender)
                    {
                        _skinnedMeshRenderer.enabled = shouldRender;
                    }
                    
                    _skinnedMeshRenderer.enabled = true;
                }
                
                if (_systemGestureBehavior == SystemGestureBehavior.SwapMaterial)
                {
                    if (_skinnedMeshRenderer != null)
                    {
                        if (ShouldUseSystemGestureMaterial && _systemGestureMaterial != null &&
                            _skinnedMeshRenderer.sharedMaterial != _systemGestureMaterial)
                        {
                            _skinnedMeshRenderer.sharedMaterial = _systemGestureMaterial;
                        }
                        else if (!ShouldUseSystemGestureMaterial && _originalMaterial != null &&
                                 _skinnedMeshRenderer.sharedMaterial != _originalMaterial)
                        {
                            _skinnedMeshRenderer.sharedMaterial = _originalMaterial;
                        }
                    }
                }
            }
        }
    }
}