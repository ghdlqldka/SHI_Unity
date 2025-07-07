using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
#if UNITY_EDITOR
namespace WorldSpaceTransitions
{
    [RequireComponent(typeof(SectionSetup))]
    [ExecuteInEditMode]
    public class FitSceneScale : MonoBehaviour
    {
        public void FitScale()
        {
            Debug.Log("fs");
            List<Object> undoObjects = new List<Object>();
            undoObjects.Add(Camera.main.transform);
            undoObjects.Add(Camera.main);
            MaxCamera camscript = Camera.main.GetComponent<MaxCamera>();
            undoObjects.Add(camscript);
            if (camscript.plane) undoObjects.Add(camscript.plane.transform);
#pragma warning disable 0618
            AdvancedGizmo.Gizmo gizmo = Object.FindObjectOfType<AdvancedGizmo.Gizmo>();
#pragma warning restore 0618
            Vector3 cameraToGizmo = Vector3.zero;
            if (gizmo)
            {
                undoObjects.Add(gizmo.transform);
                cameraToGizmo = gizmo.transform.position - Camera.main.transform.position;
            }

            SectionSetup ss = GetComponent<SectionSetup>();
            Bounds bounds = SectionSetup.GetBounds(ss.model);

            //Change ss.model layerMask
#pragma warning disable 0618
            RectGizmo rg = FindObjectOfType<RectGizmo>();
#pragma warning restore 0618
            string renderPipelineAssetName = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.GetType().Name;
            if (rg && (renderPipelineAssetName != ""))
            {
                var children = ss.model.GetComponentsInChildren<Transform>(includeInactive: true);
                foreach (var child in children)
                {
                    undoObjects.Add(child.gameObject);
                }
            }
            //
            Undo.RegisterCompleteObjectUndo(undoObjects.ToArray(), "Fit scale");
            if (rg && (renderPipelineAssetName != ""))
            {
                var children = ss.model.GetComponentsInChildren<Transform>(includeInactive: true);
                foreach (var child in children)
                {
                    child.gameObject.layer = 13;
                }
            }

            //Assume we want to fit the object inside 45 deg range (0.28*PI)
            float sizeMagnitude = bounds.size.magnitude;
            float dist = 0.5f * sizeMagnitude * (1 + 1 / Mathf.Tan(0.28f * Mathf.PI));
            if(camscript.target) camscript.target.position = bounds.center;
            //camscript.Init();
            float olddist = Vector3.Distance(bounds.center, Camera.main.gameObject.transform.position);
            float scale = dist / olddist;
            camscript.desiredDistance = dist;
            Debug.Log(scale.ToString());
            if (Application.isPlaying) 
            {
                //camscript.desiredDistance = dist;
            }
            else
            {
                //Camera.main.transform.LookAt(bounds.center, Vector3.up);
                Camera.main.transform.position = bounds.center - dist * Camera.main.transform.forward;
            }
            if (gizmo)
            {
                gizmo.transform.localScale *= scale;
                gizmo.transform.position = Camera.main.transform.position + scale * cameraToGizmo;
            }
            Camera.main.farClipPlane *= scale;
            Camera.main.farClipPlane *= scale;
            camscript.maxDistance *= scale;
            camscript.minDistance *= scale;

            if (camscript.plane) camscript.plane.transform.position = new Vector3(0, bounds.min.y - 0.01f,0);
        }
    }
}
#endif