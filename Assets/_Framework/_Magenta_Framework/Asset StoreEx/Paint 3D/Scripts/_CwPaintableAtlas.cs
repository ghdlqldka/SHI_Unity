using UnityEngine;
using System.Collections.Generic;
using PaintCore;
using CW.Common;
using System.Collections;

namespace PaintIn3D
{
	/// <summary>If you want to paint a mesh that is part of a texture atlas, except for the main mesh, you can add this component to all other GameObjects that are part of the texture atlas. You can then set the <b>Parent</b> setting to the main mesh.
	/// NOTE: This GameObject must have the <b>MeshFilter + MeshRenderer</b>, or <b>SkinnedMeshRenderer</b> component.
	/// NOTE: This GameObject should NOT have any <b>CwPaintableMeshTexture</b> components. Those should only be on the main (<b>Parent</b>) paintable mesh GameObject.</summary>
	[RequireComponent(typeof(Renderer))]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwPaintableMeshAtlas")]
	// [AddComponentMenu(CwCommon.ComponentMenuPrefix + "Paintable Mesh Atlas")]
	public class _CwPaintableAtlas : CwPaintableMeshAtlas
    {
        private static string LOG_FORMAT = "<color=#00F8D3><b>[_CwPaintableAtlas]</b></color> {0}";

        protected _CwPaintableMesh _Parent 
        { 
            set 
            { 
                parent = value;
            } 
            get 
            { 
                return parent as _CwPaintableMesh;
            } 
        } 

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            instances = null;
            instancesNode = null; // Do not use!!!!!!!
        }

        protected override void OnEnable()
        {
            // base.OnEnable();
            StartCoroutine(PostOnEnable());
        }

        protected virtual IEnumerator PostOnEnable()
        {
            while (_CwPaintableManager.Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "");
                yield return null;
            }

            // instancesNode = instances.AddLast(this);
            _CwPaintableManager.Instance.PaintableModelList.Add(this);

            cachedGameObject = gameObject;
            cachedTransform = transform;
            cachedRenderer = GetComponent<Renderer>();

            CwSerialization.TryRegister(this, hash);
        }

        protected override void OnDisable()
        {
            // base.OnDisable();
            // instances.Remove(instancesNode);
            // instancesNode = null;
            _CwPaintableManager.Instance.PaintableModelList.Remove(this);
        }

        public override List<CwPaintableTexture> FindPaintableTextures(CwGroup group)
        {
            // return base.FindPaintableTextures(group);
            throw new System.NotSupportedException("");
        }

        public virtual List<_CwPaintableTexture> _FindPaintableTextures(CwGroup group)
        {
            if (_Parent != null && _Parent != this)
            {
                return _Parent._FindPaintableTextures(group);
            }

            return null;
        }

        /*
        public virtual List<_CwPaintableMeshTexture> _FindPaintableTextures(CwGroup group)
        {
            List<_CwPaintableMeshTexture> paintableTextureList = new List<_CwPaintableMeshTexture>();
            paintableTextureList.Clear();

            foreach (CwPaintableMeshTexture paintableTexture in PaintableTextures)
            {
                _CwPaintableMeshTexture _paintableTexture = paintableTexture as _CwPaintableMeshTexture;
                if (_paintableTexture.Group == group)
                {
                    paintableTextureList.Add(_paintableTexture);
                }
            }

            return paintableTextureList;
        }
        */
    }
}

#if false //
#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = CwPaintableMeshAtlas;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwPaintableMeshAtlas_Editor : CwMeshModel_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Parent == null));
				Draw("parent", "The paintable this separate paintable is associated with.");
			EndError();
			
			base.OnInspector();
		}
	}
}
#endif
#endif