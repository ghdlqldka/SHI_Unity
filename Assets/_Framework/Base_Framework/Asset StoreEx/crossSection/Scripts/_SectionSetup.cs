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
using UnityEngine.Rendering;
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
    [ExecuteInEditMode]

    public class _SectionSetup : SectionSetup
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_SectionSetup]</b></color> {0}";

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            Debug.LogFormat(LOG_FORMAT, "OnValidate");

            if (Application.isPlaying)
                return;

            Setup();
        }
#endif

        protected override void Setup()
        {
            // base.Setup();

            //Debug.Log(UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.GetType().Name);
            //Debug.Log(UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.defaultShader.name);

            if (GraphicsSettings.defaultRenderPipeline != null)
            {
                renderPipelineAssetName = GraphicsSettings.defaultRenderPipeline.GetType().Name;
                Debug.LogFormat(LOG_FORMAT, "Setup(), renderPipelineAssetName : " + renderPipelineAssetName);
            }
            else
            {
                renderPipelineAssetName = "";
            }

#if PHUONG_THREADS
            if (!mainThreadUpdated) return;
#endif
            ISizedSection csc = this.GetComponent<ISizedSection>();
            if (csc == null)
                return;

            if (model != null)
            {
                if (boundsMode == BoundsOrientation.objectOriented)
                {
                    this.transform.rotation = model.transform.rotation;
                }
                else
                {
                    this.transform.rotation = Quaternion.identity;
                }
                
                //Debug.Log((model != currentModel).ToString() + " | " + (accurateBounds != previousAccurate).ToString() + " | " + (boundsMode != boundsModePrevious).ToString());
                if (model != currentModel || accurateBounds != previousAccurate || boundsMode != boundsModePrevious || recalculate)
                {
                    bounds = GetBounds(model, boundsMode);

                    csc.Size(bounds, model, boundsMode);

                    if (accurateBounds)
                        AccurateBounds(model, boundsMode);
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
                try
                {
                    tr.GetComponent<Renderer>().enabled = model;
                }
                catch
                {
                    //
                }
                try
                {
                    tr.GetComponent<Collider>().enabled = model;
                }
                catch
                {
                    //
                }
            }
        }

#if UNITY_EDITOR
        public override string CheckShaders()
        {
            // base.CheckShaders();

            //if (GraphicsSettings.renderPipelineAsset.name == "LightweightRenderPipelineAsset") return;
            List<Shader> shaderList = new List<Shader>();
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                Material[] mats = r.sharedMaterials;
                foreach (Material m in mats)
                {
                    Shader sh = m.shader;
                    if (!shaderList.Contains(sh)) 
                        shaderList.Add(sh);
                }
            }

            string shaderKeywordNeeded = "CLIP_PLANE";
            if (GetComponent<CappedSectionCorner>()) 
                shaderKeywordNeeded = "CLIP_CORNER";
            if (GetComponent<CappedSectionBox>()) 
                shaderKeywordNeeded = "CLIP_BOX";

            shaderSubstitutes.Clear();

            string shaderInfo = "";

            foreach (Shader sh in shaderList)
            {
                bool isKeywordSupported = false;
                Shader substitute = getSubstitute(sh, renderPipelineAssetName, shaderKeywordNeeded, out isKeywordSupported);
                if (substitute != null)
                {
                    shaderSubstitutes.Add(new ShaderSubstitute(sh, substitute));
                    if (!isKeywordSupported) 
                        shaderInfo += "Add " + shaderKeywordNeeded + " keyword to " + substitute.name + " shader \n";
                }
                else
                {
                    if (!isKeywordSupported) 
                        shaderInfo += "Add " + shaderKeywordNeeded + " keyword to " + sh.name + " shader \n";
                }
                //keywordSupport = keywordSupport && isKeywordSupported;
            }
            if (shaderInfo == "")
                shaderInfo = "check o.k.; all shaders support " + shaderKeywordNeeded + " keyword";
            if (shaderSubstitutes.Count > 0) 
                shaderInfo = "Create and assign section materials using the below shader substitutes. You can change the suggested substitutes to other crossSection shaders";

            return shaderInfo;
        }
#endif

        public override void RecalculateBounds()
        {
            Debug.LogFormat(LOG_FORMAT, "RecalculateBounds()");
            // base.RecalculateBounds();

            recalculate = true;
            Setup();
        }

#if UNITY_EDITOR && USE_JOB_THREADS
        protected override void Update()
        {
            // base.Update();

            if (Application.isPlaying)
                return;
            if (!recalculate)
                return;

            boundsJobHandle.Complete();
            Debug.Log("back to main thread ");
            UpdateThreadResult();
        }
#endif

#if USE_JOB_THREADS
        protected override void UpdateThreadResult()
        {
            // base.UpdateThreadResult();

            bounds = boundsJob.result[0];
            ISizedSection csc = GetComponent<ISizedSection>();
            if (csc != null)
                csc.Size(bounds, model, boundsMode);
            currentModel = model;
            previousAccurate = accurateBounds;
            boundsModePrevious = boundsMode;
            if (mvertices.IsCreated)
                mvertices.Dispose();
            if (mmatrices.IsCreated)
                mmatrices.Dispose();
            if (mcounts.IsCreated)
                mcounts.Dispose();

            recalculate = false;
        }
#endif
    }
}
