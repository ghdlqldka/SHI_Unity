using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CW.Common;
using PaintCore;
using System.Collections;

namespace _Magenta_WebGL
{
	/// <summary>This component marks the current GameObject as being paintable.
	/// NOTE: This GameObject must have the <b>MeshFilter + MeshRenderer</b>, or <b>SkinnedMeshRenderer</b> component.
	/// NOTE: If your mesh is part of a texture atlas, then you can use the <b>CwPaintableMeshAtlas</b> component on all the other atlas mesh GameObjects.</summary>
	// [DisallowMultipleComponent]
	// [RequireComponent(typeof(Renderer))]
	// [HelpURL(CwCommon.HelpUrlPrefix + "CwPaintableMesh")]
	// [AddComponentMenu(CwCommon.ComponentMenuPrefix + "Paintable Mesh")]
	public class MWGL_CwPaintableMesh : PaintIn3D._CwPaintableMesh
    {
        private static string LOG_FORMAT = "<color=#00F8D3><b>[MWGL_CwPaintableMesh]</b></color> {0}";

        protected override IEnumerator PostOnEnable()
        {
            while (MWGL_CwPaintableManager.Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "");
                yield return null;
            }

            MWGL_CwPaintableManager.Instance.PaintableModelList.Add(this);

            // cachedGameObject = this.gameObject;
            cachedGameObject = null; // Not used!!!!!!!
            cachedTransform = this.transform;
            cachedRenderer = this.GetComponent<Renderer>();

            CwSerialization.TryRegister(this, hash);

            if (Activation == ActivationType.OnEnable && activated == false)
            {
                Activate();
            }
        }
    }
}
