/*
 The purpose of this script is to create cross-section material instances
 and - in case of capped sections - to scale the capped section prefabs to fit the model GameObject.

 The script uses threading for axis aligned bound box calculation
 */
#define USE_JOB_THREADS
//#define PHUONG_THREADS
using System.Collections.Generic;
using Unity.Collections;
using System.Collections;
#if  USE_JOB_THREADS
using Unity.Jobs;
#endif
using UnityEngine;
using System.IO;
using System.Linq;
//using MathGeoLib;
#if PHUONG_THREADS
using Threading;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

using System;

namespace WorldSpaceTransitions
{
    [System.Serializable]
    public enum BoundsOrientation { objectOriented, worldOriented, arbitrary };


    [ExecuteInEditMode]

    public class SectionSetup : MonoBehaviour
    {
        //[Tooltip("Reassign after geometry change")]
        [HideInInspector]
        public GameObject model;
        [SerializeField]
        [HideInInspector]
        protected GameObject currentModel;
        [SerializeField]
        [HideInInspector]
        protected Bounds bounds;
        [HideInInspector]
        public BoundsOrientation boundsMode = BoundsOrientation.worldOriented;
        [HideInInspector]
        public bool accurateBounds = true;
        //[HideInInspector]
        //public bool useMathGeo = true;
        [SerializeField]
        [HideInInspector]
        protected bool previousAccurate;
        [SerializeField]
        [HideInInspector]
        protected BoundsOrientation boundsModePrevious;
        [SerializeField]
        [HideInInspector]
        public string newMatsPath = "";
        string renderPlipelineSuffix = "";
        List<string> createdMaterials;
        [SerializeField]
        [HideInInspector]
        private bool materialsCreated = false;

        protected static string renderPipelineAssetName = "";

#if USE_JOB_THREADS
        NativeArray<Bounds> orientedBoundsResult;
        protected NativeArray<Matrix4x4> mmatrices;
        protected NativeArray<Vector3> mvertices;
        protected NativeArray<int> mcounts;
        //Job Handles
        protected OrientedBounds.BoundsJob boundsJob;
        protected JobHandle boundsJobHandle;
#endif
#if PHUONG_THREADS
        private bool mainThreadUpdated = true;
#endif
        private Dictionary<Material, Material> materialsToreplace;
        [HideInInspector]
        public List<ShaderSubstitute> shaderSubstitutes;
        protected bool recalculate = false;

        [System.Serializable]
        public struct ShaderSubstitute
        {
#if UNITY_EDITOR
            [ReadOnly]
#endif
            public Shader original;
            public Shader substitute;


            public ShaderSubstitute(Shader orig, Shader subst)
            {
                original = orig; substitute = subst;
            }
        }

#if UNITY_EDITOR
        /*
        private readonly Queue<Action> _actionQueue = new Queue<Action>();
        public Queue<Action> ActionQueue
        {
            get
            {
                lock (Async.GetLock("ActionQueue"))
                {
                    return _actionQueue;
                }
            }
        }
        */
        private void Start()
        {
            Setup();
        }

        protected virtual void OnValidate()
        {
            Debug.Log("onvalidate");
            if (Application.isPlaying) return;
            Setup();
        }
#endif
        protected virtual void Setup()
        {
            Debug.Log("[ExecuteInEditMode] => SectionSetup.Setup()");
            //Debug.Log(UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.GetType().Name);
            //Debug.Log(UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.defaultShader.name);
            try
            {
                if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline == null)
                {
                    renderPipelineAssetName = "";
                }
                else
                {
                    renderPipelineAssetName = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.GetType().Name;
                }
            }
            catch { }
            Debug.Log(renderPipelineAssetName);

#if PHUONG_THREADS
            if (!mainThreadUpdated) return;
#endif

#if UNITY_EDITOR
            if (model)
            {
                string mpath = "WorldSpaceTransitions/crossSection/NewMaterials" + renderPlipelineSuffix;
                string ppath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(model);
                if (ppath != "") mpath = ppath;
                //Debug.Log("model | " + mpath);
                string dirname = Path.GetDirectoryName(mpath);
                newMatsPath = Path.Combine(dirname, "Materials");
            }
#endif

            ISizedSection csc = GetComponent<ISizedSection>();
            if (csc == null) return;
            if (model)
            {
                switch (boundsMode)
                {
                    case BoundsOrientation.objectOriented:
                        transform.rotation = model.transform.rotation;
                        break;
                    case BoundsOrientation.worldOriented:
                        transform.rotation = Quaternion.identity;
                        break;
                    default:
                        // code block
                        break;
                }
                //Debug.Log((model != currentModel).ToString() + " | " + (accurateBounds != previousAccurate).ToString() + " | " + (boundsMode != boundsModePrevious).ToString());
                if (model != currentModel || accurateBounds != previousAccurate || boundsMode != boundsModePrevious || recalculate)
                {
                    bounds = GetBounds(model, boundsMode);

                    csc.Size(bounds, model, boundsMode);

                    if (accurateBounds) AccurateBounds(model, boundsMode);
                    if (!accurateBounds)
                    {
                        currentModel = model;
                        previousAccurate = accurateBounds;
                        boundsModePrevious = boundsMode;
                    }
                }
            }
            else
            {
                currentModel = null;
            }

            Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, UnityEngine.Vector3.one);
            Shader.SetGlobalMatrix("_WorldToObjectMatrix", m.inverse);
            //hide the box when no model assigned
            foreach (Transform tr in transform)
            {
                //tr.gameObject.SetActive(model);
                try { tr.GetComponent<Renderer>().enabled = model; }
                catch { }
                try { tr.GetComponent<Collider>().enabled = model; }
                catch { }
            }
        }

#if UNITY_EDITOR
        public virtual string CheckShaders()
        {
            //if (GraphicsSettings.renderPipelineAsset.name == "LightweightRenderPipelineAsset") return;
            List<Shader> shaderList = new List<Shader>();
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                Material[] mats = r.sharedMaterials;
                foreach (Material m in mats)
                {
                    Shader sh = m.shader;
                    if (!shaderList.Contains(sh)) shaderList.Add(sh);
                }
            }

            string shaderKeywordNeeded = "CLIP_PLANE";
            if (GetComponent<CappedSectionCorner>()) shaderKeywordNeeded = "CLIP_CORNER";
            if (GetComponent<CappedSectionBox>()) shaderKeywordNeeded = "CLIP_BOX";

            shaderSubstitutes.Clear();

            string shaderInfo = "";

            foreach (Shader sh in shaderList)
            {
                bool isKeywordSupported = false;
                Shader substitute = getSubstitute(sh, renderPipelineAssetName, shaderKeywordNeeded, out isKeywordSupported);
                if (substitute != null)
                {
                    shaderSubstitutes.Add(new ShaderSubstitute(sh, substitute));
                    if (!isKeywordSupported) shaderInfo += "Add " + shaderKeywordNeeded + " keyword to " + substitute.name + " shader \n";
                }
                else
                {
                    if (!isKeywordSupported) shaderInfo += "Add " + shaderKeywordNeeded + " keyword to " + sh.name + " shader \n";
                }
                //keywordSupport = keywordSupport && isKeywordSupported;
            }
            if (shaderInfo == "")
                shaderInfo = "check o.k.; all shaders support " + shaderKeywordNeeded + " keyword";
            if (shaderSubstitutes.Count > 0) shaderInfo = "Create and assign section materials using the below shader substitutes. You can change the suggested substitutes to other crossSection shaders";
            return shaderInfo;
        }
#endif

        public void CreateSectionMaterials()
        {
            Dictionary<Shader, Shader> shadersToreplace = new Dictionary<Shader, Shader>();
            foreach (ShaderSubstitute ssub in shaderSubstitutes)
            {
                shadersToreplace.Add(ssub.original, ssub.substitute);
            }
            materialsToreplace = new Dictionary<Material, Material>();
#if UNITY_EDITOR
            Undo.SetCurrentGroupName("crossSection material assign");
            int group = Undo.GetCurrentGroup();
            //Undo.RegisterFullObjectHierarchyUndo(model, "crossSection material assign");
            createdMaterials = new List<string>();

#endif
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                Material[] mats = r.sharedMaterials;
                Material[] newMats = new Material[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    Shader sh = mats[i].shader;
                    if (shadersToreplace.ContainsKey(sh))
                    {
                        if (!materialsToreplace.ContainsKey(mats[i]))
                        {
                            Material newMaterial;
#if UNITY_EDITOR
                            string mpath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(model);
                            //Debug.Log("model | " + mpath);
                            string newName = mats[i].name + "_cs" + renderPlipelineSuffix;
                            if (!Directory.Exists(newMatsPath)) 
                            { 
                                DirectoryInfo di = Directory.CreateDirectory(newMatsPath);
                            }
                            string materialPath = Path.Combine(newMatsPath, newName + ".mat");

                            newMaterial = (Material)AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));
                            if (newMaterial == null)
                            {
#endif
                                newMaterial = new Material(mats[i]);
#if UNITY_EDITOR
                                newMaterial.name = newName;
                                Debug.Log(materialPath);
                                AssetDatabase.CreateAsset(newMaterial, materialPath);
                                createdMaterials.Add(materialPath);
                                Undo.RegisterCreatedObjectUndo(newMaterial, "");
                            }
#endif
                            newMaterial.shader = shadersToreplace[mats[i].shader];
                            if ((GetComponent<ISizedSection>() != null) && (renderPipelineAssetName == ""))
                            {
                                newMaterial.SetFloat("_StencilPassFront", 6);
                                newMaterial.SetFloat("_StencilPassBack", 7);
                                newMaterial.SetFloat("_CapPrepareZWrite", 0);
                                newMaterial.SetFloat("_CapPrepareZTest", 8);
                                newMaterial.SetFloat("_Cull", 2);//Back
                            }
                            else
                            {
                                newMaterial.SetFloat("_Cull", 0);//Off _DoubleSidedEnable UniversalRenderPipelineAsset
                            }
                            if(renderPipelineAssetName == "HDRenderPipelineAsset")
                            {
                                newMaterial.SetFloat("_DoubleSidedEnable", (GetComponent<ISizedSection>() != null)? 0: 1);
                            }
                            materialsToreplace.Add(mats[i], newMaterial);
                        }

                        newMats[i] = materialsToreplace[mats[i]];
                    }
                    else
                    {
                        newMats[i] = mats[i];
                    }
                }
#if UNITY_EDITOR
                Undo.RegisterCompleteObjectUndo(r, "");
                r.materials = newMats;
#endif
            }
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(this, "");
            materialsCreated = true;
            Undo.CollapseUndoOperations(group);
            Undo.undoRedoPerformed += MaterialsUndoCallback;
#endif
        }


        public void SetModel(GameObject _model)
        {
            //if (Application.isPlaying) accurateBounds = false;
            model = _model;
            Setup();
        }
        public static Bounds GetBounds(GameObject go)
        {
            return GetBounds(go, BoundsOrientation.worldOriented);
        }

        public static Bounds GetBounds(GameObject go, BoundsOrientation boundsMode)
        {
            Quaternion quat = go.transform.rotation;//object axis AABB

            Bounds bounds = new Bounds();


            if (boundsMode != BoundsOrientation.worldOriented) go.transform.rotation = Quaternion.identity;

            MeshRenderer[] mrenderers = go.GetComponentsInChildren<MeshRenderer>();
            SkinnedMeshRenderer[] smrenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();
            List<Renderer> allRenderers = new List<Renderer>();
            allRenderers.AddRange(mrenderers);
            allRenderers.AddRange(smrenderers);
            //Debug.Log(renderers.Length.ToString() + " | " + mrenderers.Length.ToString());
            if (allRenderers.Count > 0)
            {
                for (int i = 0; i < allRenderers.Count; i++)
                {
                    Bounds i_bounds = allRenderers[i].bounds;
                    //Accomodate for mesh animation
                    if (allRenderers[i].GetType() == typeof(SkinnedMeshRenderer)) i_bounds.Expand(1.3f);
                    if (i == 0)
                    {
                        bounds = i_bounds;
                    }
                    else
                    {
                        bounds.Encapsulate(i_bounds);
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog("CrossSection message", "The object contains no meshRenderers!\n- please reassign", "Continue");
#endif
            }

            UnityEngine.Vector3 localCentre = go.transform.InverseTransformPoint(bounds.center);
            go.transform.rotation = quat;
            bounds.center = go.transform.TransformPoint(localCentre);

            return bounds;
        }

        protected void AccurateBounds(GameObject go, BoundsOrientation boundsMode)
        {

            MeshFilter[] meshes = go.GetComponentsInChildren<MeshFilter>();
            ISizedSection csc = GetComponent<ISizedSection>();
#if UNITY_EDITOR
            if (meshes.Length == 0)
            {
                EditorUtility.DisplayDialog("CrossSection message", "The object contains no meshes!\n- please reassign", "Continue");
            }
#endif
            VertexData[] vertexData = new VertexData[meshes.Length];
#if USE_JOB_THREADS
            List<Vector3> vertList = new List<Vector3>();
            Matrix4x4[] matrs = new Matrix4x4[meshes.Length];
            int[] vcounts = new int[meshes.Length];
#endif
            //
            for (int i = 0; i < meshes.Length; i++)
            {
                Mesh ms = meshes[i].sharedMesh;
                vertexData[i] = new VertexData(ms.vertices, meshes[i].transform.localToWorldMatrix);
#if USE_JOB_THREADS
                vertList.AddRange(ms.vertices);
                vcounts[i] = ms.vertices.Length;
                matrs[i] = meshes[i].transform.localToWorldMatrix;
#endif
            }
            Vector3 v1;
            Vector3 v2;
            Vector3 v3;
            switch (boundsMode)
            {
                case BoundsOrientation.objectOriented:
                    v1 = go.transform.right;
                    v2 = go.transform.up;
                    v3 = go.transform.forward;
                    break;
                case BoundsOrientation.worldOriented:
                    v1 = Vector3.right;
                    v2 = Vector3.up;
                    v3 = Vector3.forward;
                    break;
                default:
                    v1 = transform.right;
                    v2 = transform.up;
                    v3 = transform.forward;
                    break;
            }

#if USE_JOB_THREADS
            mvertices = new NativeArray<Vector3>(vertList.ToArray(), Allocator.Persistent);
            mmatrices = new NativeArray<Matrix4x4>(matrs, Allocator.Persistent);
            mcounts = new NativeArray<int>(vcounts, Allocator.Persistent);

            Bounds[] b = new Bounds[1];
            orientedBoundsResult = new NativeArray<Bounds>(b, Allocator.Persistent);

            //Creating a job and assigning the variables within the Job
            boundsJob = new OrientedBounds.BoundsJob()
            {
                result = orientedBoundsResult,
                vertices = mvertices,
                vcounts = mcounts,
                matrices = mmatrices,
                _v1 = v1,
                _v2 = v2,
                _v3 = v3
            };

            //Setup of the job handle
            boundsJobHandle = boundsJob.Schedule();
            //if (Application.isPlaying)
            //{
                StartCoroutine(WaitForBoundBoxThread());
            //}

#endif

#if PHUONG_THREADS
            Async.Run(() =>
            {
                mainThreadUpdated = false;
                Debug.Log("thread start");
                bounds = OrientedBounds.OBB(vertexData, v1, v2, v3);
            }).ContinueInMainThread(() =>
            {
                Debug.Log("back to main thread");
                if (csc != null) csc.Size(bounds, go, boundsMode);
                currentModel = model;
                previousAccurate = accurateBounds;
                boundsModePrevious = boundsMode;
                enabled = false;
                enabled = true;
                recalculate = false;
                //mainThreadUpdated = true;
            });
#endif
#if !PHUONG_THREADS && !USE_JOB_THREADS
            bounds = OrientedBounds.OBB(vertexData, v1, v2, v3);
            Vector3 localCentre = go.transform.InverseTransformPoint(bounds.center);
            bounds.center = go.transform.TransformPoint(localCentre);
            if (csc != null) csc.Size(bounds, go, boundsMode);
#endif
        }

#if UNITY_EDITOR && USE_JOB_THREADS
        protected virtual void Update()
        {
            if (Application.isPlaying) return;
            if (!recalculate) return;
            
            boundsJobHandle.Complete();
            Debug.Log("back to main thread ");
            UpdateThreadResult ();
        }
#endif
#if USE_JOB_THREADS
        protected virtual void UpdateThreadResult()
        {
            bounds = boundsJob.result[0];
            ISizedSection csc = GetComponent<ISizedSection>();
            if (csc != null) csc.Size(bounds, model, boundsMode);
            currentModel = model;
            previousAccurate = accurateBounds;
            boundsModePrevious = boundsMode;
            if (mvertices.IsCreated) mvertices.Dispose();
            if (mmatrices.IsCreated) mmatrices.Dispose();
            if (mcounts.IsCreated) mcounts.Dispose();
            //enabled = false;
            //enabled = true;
            recalculate = false;
        }

        public IEnumerator WaitForBoundBoxThread()
        {
            //while (!boundsJobHandle.IsCompleted)
            //yield return null;
            boundsJobHandle.Complete(); 
            if (boundsJobHandle.IsCompleted == false)
                yield return new WaitForJobCompleted(boundsJobHandle, isUsingTempJobAllocator: true);
            if (boundsJobHandle.IsCompleted) UpdateThreadResult();
        }
#endif

#if UNITY_EDITOR
        protected Shader getSubstitute(Shader shader, string pipelineAssetName, string keyword, out bool hasKeyword)
        {
            //Let's define crossSection shader is that containing any of these keywords
            List<string> cs_keywords = new List<string>() { "CLIP_PLANE", "CLIP_BOX", "CLIP_CORNER" };
            List<string> keywordList = shader.keywordSpace.keywordNames.ToList();
            hasKeyword = false;
            if (keywordList.Contains(keyword)) hasKeyword = true;
            if (keywordList.Intersect(cs_keywords).Any()) return null;

            string substituteName = "";
            string defaultshaderName;

            switch (pipelineAssetName)
            {
                case "UniversalRenderPipelineAsset":
                    if (shader.name.Contains("Graphs"))
                    {
                        substituteName = shader.name.Replace("Shader Graphs/", "CrossSectionGraphs/");
                    }
                    else
                    {
                        substituteName = shader.name.Replace("Universal Render Pipeline/", "CrossSectionURP/");
                    }
                    defaultshaderName = "CrossSectionURP/Lit";
                    renderPlipelineSuffix = "_urp";
                    break;
                case "HDRenderPipelineAsset":
                    if (shader.name.Contains("Graphs"))
                    {
                        substituteName = shader.name.Replace("Shader Graphs/", "CrossSectionGraphs/");
                    }
                    else
                    {
                        substituteName = shader.name.Replace("HDRP/", "CrossSectionHDRP/");
                    }
                    defaultshaderName = "CrossSectionHDRP/Lit";
                    renderPlipelineSuffix = "_hdrp";
                    break;
                default:
                    string sname = shader.name.Replace("Legacy Shaders/", "");
                    if (sname.Contains("Transparent/VertexLit"))
                    {
                        sname = sname.Replace("Transparent/VertexLit", "Transparent/Specular");
                    }
                    else if (sname=="Standard")
                    {
                        sname = "Standard/Base";
                    }
                    else if (sname == "Standard (Specular Setup)")
                    {
                        sname = "Standard (Specular Setup)/Base";
                    }
                    else if (sname == "Autodesk Interactive")
                    {
                        sname = "Autodesk Interactive/Base";
                    }
                    else
                    {
                        sname = "Standard/Base";
                    }
                    substituteName = "CrossSection/" + sname;
                    defaultshaderName = "CrossSection/Standard/Base";
                    if (keyword == "CLIP_BOX"|| GetComponent<ISizedSection>()!=null)
                    {
                        substituteName = "CrossSection/" + sname.Replace("Base", "CapPrepare");
                        defaultshaderName = "CrossSection/Standard/CapPrepare";
                    }

                    break;
            }

            /*
            //methods to get list of global and local shader keywords are internal and private, only way to call them is via reflection
            var getKeywordsMethod = typeof(ShaderUtil).GetMethod("GetShaderGlobalKeywords", BindingFlags.Static | BindingFlags.NonPublic);
            string[] keywords;

            //check the object shaders for keywords
            keywords = (string[])getKeywordsMethod.Invoke(null, new object[] { shader });
            keywordList = new List<string>(keywords);
            Debug.Log(keywordList[0]);
            hasKeyword = keywordList.Contains(keyword);
            //
            */

            Shader newShader;
            if (Shader.Find(substituteName))
            {
                newShader = Shader.Find(substituteName);
            }
            else
            {
                newShader = Shader.Find(defaultshaderName);
            }
            if (!newShader) Debug.Log(pipelineAssetName + " | " + substituteName + " | " + defaultshaderName + " | alert");
            keywordList = newShader.keywordSpace.keywordNames.ToList();
            if (keywordList.Contains(keyword)) hasKeyword = true;
            return newShader;
        }
#endif
        private void OnEnable()
        {
            //Mulithreading used in bound box calculations
#if PHUONG_THREADS
            mainThreadUpdated = true;
#endif
#if USE_JOB_THREADS
            Bounds[] b = new Bounds[1];
            orientedBoundsResult = new NativeArray<Bounds>(b, Allocator.Persistent);
#endif
            //boundsModePrevious = boundsMode;
        }
        void OnDrawGizmos()
        {
            // Your gizmo drawing thing goes here if required...
            // Update the main thread after the bound box calculations
#if UNITY_EDITOR && PHUONG_THREADS
            // Ensure continuous Update calls.
            if (!Application.isPlaying && !mainThreadUpdated)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
#endif
        }

        public virtual void RecalculateBounds()
        {
            recalculate = true;
            Setup();
        }


        private void OnDisable()
        {
#if USE_JOB_THREADS
            if (Application.isPlaying) return;
            // make sure to Dispose any NativeArrays when you're done
            if (orientedBoundsResult.IsCreated) orientedBoundsResult.Dispose();
            if (mvertices.IsCreated) mvertices.Dispose();
            if (mmatrices.IsCreated) mmatrices.Dispose();
            if (mcounts.IsCreated) mcounts.Dispose();
#endif
        }

        private void OnDestroy()
        {
#if USE_JOB_THREADS
            // make sure to Dispose any NativeArrays when you're done
            if (orientedBoundsResult.IsCreated) orientedBoundsResult.Dispose();
            if (mvertices.IsCreated) mvertices.Dispose();
            if (mmatrices.IsCreated) mmatrices.Dispose();
            if (mcounts.IsCreated) mcounts.Dispose();
#endif
        }
        public string NewMatsPath
        {
            get
            {
                return newMatsPath;
            }
            set
            {
                newMatsPath = value;
            }
        }
#if UNITY_EDITOR
        void MaterialsUndoCallback()
        {
            // code for the action to take on Undo
            if (materialsCreated) return;
            AssetDatabase.Refresh();
            List<string> failed = new List<string>();
            if(createdMaterials.Count==0) Debug.Log("no material creations to undo");
            AssetDatabase.MoveAssetsToTrash(createdMaterials.ToArray(), failed);
            if(failed.Count>0) Debug.LogWarning("couldn't delete " + String.Join(", ", failed));
            AssetDatabase.Refresh();
            Undo.undoRedoPerformed -= MaterialsUndoCallback;
        }
#endif
    }
}
