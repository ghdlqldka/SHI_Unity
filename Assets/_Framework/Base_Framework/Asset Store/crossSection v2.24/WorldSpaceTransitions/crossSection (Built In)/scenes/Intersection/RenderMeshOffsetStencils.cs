using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WorldSpaceTransitions
{
    [ExecuteInEditMode]
    public class RenderMeshOffsetStencils : MonoBehaviour
    {
        public int offsetMaterialQueue = 1997;
        public int offsetBackQueue = 1999;
        public int stencilRef = 127;
        public Shader offsetShader;
        Material moveMaterial = null;
        Material moveBackMaterial = null;
        public GameObject[] rendererObjects = new GameObject[0];
        List<Mesh> ml;
        List<MeshFilter> mf_all;
        void Awake()
        {

        }
        // Start is called before the first frame update
        void OnEnable()
        {
            if(offsetShader==null) offsetShader = Shader.Find("CrossSection/Others/CapPrepare");
            moveMaterial = new Material(offsetShader);
            moveMaterial.SetInt("_Cull", 1);
            moveMaterial.SetInt("_StencilMask", stencilRef);
            moveMaterial.SetInt("_StencilOpBack", 2);
            moveMaterial.SetInt("_ZTest", 8);
            moveMaterial.renderQueue = offsetMaterialQueue;
            moveBackMaterial = new Material(offsetShader);
            moveBackMaterial.SetInt("_Cull", 2);
            moveBackMaterial.SetInt("_StencilMask", stencilRef);
            moveBackMaterial.SetInt("_StencilOpFront", 1);
            moveBackMaterial.SetInt("_StencilCompFront", 3);
            moveBackMaterial.SetInt("_ZTest", 8);
            moveBackMaterial.renderQueue = offsetBackQueue;
            ml = new List<Mesh>();
            mf_all = new List<MeshFilter>();
            foreach (GameObject ro in rendererObjects)
            {
                List<MeshFilter> mf = ro.GetComponentsInChildren<MeshFilter>().ToList();
                mf_all.AddRange(mf);
            }
            //Debug.Log(mf_all.Count);
            foreach (MeshFilter m in mf_all) ml.Add(m.sharedMesh);
        }

        // Update is called once per frame
        void Update()
        {
            if (moveMaterial)
            {
                RenderParams rp1 = new RenderParams(moveMaterial);
                for (int i = 0; i < mf_all.Count; ++i)
                {
                    Graphics.RenderMesh(rp1, ml[i], 0, mf_all[i].transform.localToWorldMatrix);
                }
            }
            if (moveBackMaterial)
            {
                RenderParams rp2 = new RenderParams(moveBackMaterial);
                for (int i = 0; i < mf_all.Count; ++i)
                {
                    Graphics.RenderMesh(rp2, ml[i], 0, mf_all[i].transform.localToWorldMatrix);
                }
            }
        }
        private void OnDisable()
        {
            if (moveMaterial) Destroy(moveMaterial);
            if (moveBackMaterial) Destroy(moveBackMaterial);
        }
    }
}