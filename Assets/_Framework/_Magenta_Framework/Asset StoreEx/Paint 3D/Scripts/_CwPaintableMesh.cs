using UnityEngine;
using System.Collections.Generic;
using CW.Common;
using PaintCore;
using System.Collections;

namespace PaintIn3D
{
	/// <summary>This component marks the current GameObject as being paintable.
	/// NOTE: This GameObject must have the <b>MeshFilter + MeshRenderer</b>, or <b>SkinnedMeshRenderer</b> component.
	/// NOTE: If your mesh is part of a texture atlas, then you can use the <b>CwPaintableMeshAtlas</b> component on all the other atlas mesh GameObjects.</summary>
	// [DisallowMultipleComponent]
	// [RequireComponent(typeof(Renderer))]
	// [HelpURL(CwCommon.HelpUrlPrefix + "CwPaintableMesh")]
	// [AddComponentMenu(CwCommon.ComponentMenuPrefix + "Paintable Mesh")]
	public class _CwPaintableMesh : CwPaintableMesh
    {
        private static string LOG_FORMAT = "<color=#00F8D3><b>[_CwPaintableMesh]</b></color> {0}";

        public new GameObject CachedGameObject
        {
            get
            {
#if false //
                if (cachedRendererSet == false)
                {
                    CacheRenderer();
                }

                return cachedGameObject;
#else
                throw new System.NotSupportedException("");
#endif
            }
        }

        /*
        public List<Renderer> OtherRenderers
        {
            set { otherRenderers = value; }
            get
            {
                if (otherRenderers == null)
                    otherRenderers = new List<Renderer>();
                return otherRenderers;
            }
        }
        */
        public List<Renderer> OtherRendererList
        {
            get
            {
                if (otherRenderers == null)
                {
                    otherRenderers = new List<Renderer>();
                }
                return otherRenderers;
            }
            set
            {
                otherRenderers = value;
            }
        }

#if DEBUG
        [Header("=====> DEBUG <=====")]
        [ReadOnly]
        [SerializeField]
        protected Mesh DEBUG_preparedMesh;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected bool DEBUG_cachedRendererSet;
        [ReadOnly]
        [SerializeField]
        protected Renderer DEBUG_cachedRenderer;
#endif

        protected override void Awake()
		{
			Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + 
                "</b>, Activation : <b>" + Activation + "</b>, MaterialApplication : <b>" + MaterialApplication + "</b>");
            Debug.LogFormat(LOG_FORMAT, "RendererList.Count : <b>" + OtherRendererList.Count + "</b>");

            instances = null; // Do not use!!!!!
            instancesNode = null; // Do not use!!!!!

            tempMaterialCloners = null; // Do not use!!!!
            tempPaintableTextures = null; // Do not use!!!!!
            tempPaintableMeshTextures = null; // Do not use!!!!!

            Debug.LogFormat(LOG_FORMAT, "PaintableTextures.Count : <b><color=red>" + PaintableTextures.Count + "</color></b>");
            if (Activation == ActivationType.Awake && activated == false)
            {
                Activate();
            }
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

            // cachedGameObject = this.gameObject;
            cachedGameObject = null; // Not used!!!!!!!
            cachedTransform = this.transform;
            cachedRenderer = this.GetComponent<Renderer>();

            CwSerialization.TryRegister(this, hash);

            if (Activation == ActivationType.OnEnable && activated == false)
            {
                Activate();
            }

            // _CwPaintableManager.GetOrCreateInstance();
        }

        protected override void OnDisable()
        {
            // base.OnDisable();
            // instances.Remove(instancesNode);
            // instancesNode = null;
            _CwPaintableManager.Instance.PaintableModelList.Remove(this);
        }

        protected override void Start()
        {
            if (Activation == ActivationType.Start && activated == false)
            {
                Activate();
            }
        }

        /*
        protected void Invoke_onActivating()
        {
            if (onActivating != null)
            {
                onActivating.Invoke();
            }
        }

        protected void Invoke_onActivated()
        {
            if (onActivated != null)
            {
                onActivated.Invoke();
            }
        }
        */

        protected override void DoActivate()
        {
            /*
            if (onActivating != null)
            {
                onActivating.Invoke();
            }
            */
            Invoke_onActivating();

            // Activate material cloners
            if (MaterialApplication == MaterialApplicationType.ClonerAndTextures)
            {
                List<CwMaterialCloner> tempMaterialCloners = new List<CwMaterialCloner>();
                GetComponents(tempMaterialCloners);

                for (var i = tempMaterialCloners.Count - 1; i >= 0; i--)
                {
                    tempMaterialCloners[i].Activate();
                }
            }

            // Activate textures
            AddPaintableTextures(this.transform);

            foreach (CwPaintableMeshTexture paintableTexture in PaintableTextures)
            {
                _CwPaintableTexture _paintableTexture = paintableTexture as _CwPaintableTexture;
                _paintableTexture.Activate();
            }

            activated = true;

            /*
            if (onActivated != null)
            {
                onActivated.Invoke();
            }
            */
            Invoke_onActivated();
        }

        public override void GetPrepared(ref Mesh mesh, ref Matrix4x4 matrix, CwCoord coord)
        {
            if (prepared == false)
            {
                prepared = true;

                if (cachedRendererSet == false)
                {
                    CacheRenderer();
                }

                TryGetPrepared(coord);
            }

            Debug.Assert(preparedMesh != null);
            mesh = preparedMesh;
            matrix = preparedMatrix;
        }

        protected override bool TryCacheRenderer()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "TryCacheRenderer()");
            // base.TryCacheRenderer();

            if (cachedRenderer is SkinnedMeshRenderer)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "<b><color=yellow>SkinnedMeshRenderer</color></b>");
                cachedSkinned = (SkinnedMeshRenderer)cachedRenderer;
                cachedSkinnedSet = true;

                return true;
            }
            else if (cachedRenderer is MeshRenderer)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "<b><color=yellow>MeshRenderer</color></b>");
                cachedFilter = GetComponent<MeshFilter>();
                cachedFilterSet = true;

                return true;
            }
            else
            {
                Debug.Assert(false);
            }

            return false;
        }

        protected override void TryGetPrepared(CwCoord coord)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "TryGetPrepared(), cachedSkinnedSet : <b>" + cachedSkinnedSet + 
                "</b>, bakedMeshSet : <b>" + bakedMeshSet + "</b>, cachedFilterSet : <b>" + cachedFilterSet + "</b>");
            // base.TryGetPrepared(coord);

            if (cachedSkinnedSet == true)
            {
                if (bakedMeshSet == false)
                {
                    bakedMesh = new Mesh();
                    bakedMeshSet = true;
                }

                if (useMesh == UseMeshType.AutoSeamFix)
                {
                    Mesh skinnedMesh = cachedSkinned.sharedMesh;

                    if (skinnedMesh != null && skinnedMesh.name.EndsWith("(Fixed Seams)") == false && skinnedMesh.name.EndsWith("(Fixed)") == false)
                    {
                        cachedSkinned.sharedMesh = _CwMeshFixer.GetCachedMesh(skinnedMesh, coord);
                    }
                }

                Vector3 lossyScale = CachedTransform.lossyScale;
                Vector3 scaling = new Vector3(CwHelper.Reciprocal(lossyScale.x), CwHelper.Reciprocal(lossyScale.y), CwHelper.Reciprocal(lossyScale.z));
                Vector3 oldLocalScale = CachedTransform.localScale;

                CachedTransform.localScale = Vector3.one;

                cachedSkinned.BakeMesh(bakedMesh);

                CachedTransform.localScale = oldLocalScale;

                preparedMesh = bakedMesh;
#if DEBUG
                DEBUG_preparedMesh = preparedMesh;
#endif
                preparedMatrix = cachedRenderer.localToWorldMatrix;

                if (includeScale == true)
                {
                    preparedMatrix *= Matrix4x4.Scale(scaling);
                }
            }
            else if (cachedFilterSet == true)
            {
                preparedMesh = cachedFilter.sharedMesh;
                preparedMatrix = cachedRenderer.localToWorldMatrix;

                if (useMesh == UseMeshType.AutoSeamFix)
                {
                    preparedMesh = _CwMeshFixer.GetCachedMesh(preparedMesh, coord);
                }
#if DEBUG
                DEBUG_preparedMesh = preparedMesh;
#endif
            }
        }

        protected override void CacheRenderer()
        {
            Debug.LogFormat(LOG_FORMAT, "CacheRenderer()");
            // base.CacheRenderer();

            cachedRenderer = this.GetComponent<Renderer>();
            cachedRendererSet = true;

            if (TryCacheRenderer() == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "This CwModel/CwPaintable (" + this.name + ") doesn't have a suitable Renderer, so it cannot be painted.", this);
            }
#if DEBUG
            DEBUG_cachedRendererSet = cachedRendererSet;
            DEBUG_cachedRenderer = cachedRenderer;
#endif
        }

        public override Texture GetExistingTexture(CwSlot slot)
        {
            Debug.LogFormat(LOG_FORMAT, "GetExistingTexture(), CachedRenderer : <b>" + CachedRenderer + "</b>, slot : <b>" + slot + "</b>");
            // base.GetExistingTexture(slot);

            CachedRenderer.GetSharedMaterials(tempMaterials); // NOTE: Property

            if (slot.Index >= 0 && slot.Index < tempMaterials.Count)
            {
                Material tempMaterial = tempMaterials[slot.Index];

                if (tempMaterial != null)
                {
                    Texture _texture = tempMaterial.GetTexture(slot.Name);
#if DEBUG
                    Debug.LogFormat(LOG_FORMAT, "_texture : " + _texture + ", slot.Name : <color=yellow><b>" + slot.Name + "</b></color>");
#endif
                    return _texture;
                }
                else
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "There is no MATERIAL");
                }
            }

            return null;
        }

        protected override void AddPaintableTextures(Transform root)
        {
            Debug.LogFormat(LOG_FORMAT, "AddPaintableTextures()");

            List<_CwPaintableTexture> tempPaintableTextures = new List<_CwPaintableTexture>();
            root.GetComponents(tempPaintableTextures);

            foreach (_CwPaintableTexture paintableTexture in tempPaintableTextures)
            {
                PaintableTextures.Add(paintableTexture);
            }

            // tempPaintableTextures.Clear();

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);

                if (child.GetComponent<_CwPaintableMesh>() == null)
                {
                    AddPaintableTextures(child); // <========= Recursive
                }
            }
        }

        public override void ClearAll(Texture texture, Color color)
        {
            Debug.LogFormat(LOG_FORMAT, "ClearAll()");

            if (activated == true)
            {
                foreach (var paintableTexture in paintableTextures)
                {
                    paintableTexture.Clear(texture, color);
                }
            }
        }

        /// <summary>This allows you to manually register a CwPaintableTexture.</summary>
        public override void Register(CwPaintableMeshTexture paintableTexture)
        {
            Debug.LogFormat(LOG_FORMAT, "Register()");

            PaintableTextures.Add(paintableTexture);
        }

        /// <summary>This allows you to manually unregister a CwPaintableTexture.</summary>
        public override void Unregister(CwPaintableMeshTexture paintableTexture)
        {
            Debug.LogFormat(LOG_FORMAT, "Unregister()");

            PaintableTextures.Remove(paintableTexture);
        }

        public override List<CwPaintableTexture> FindPaintableTextures(CwGroup group)
        {
            // return base.FindPaintableTextures(group);
            throw new System.NotSupportedException("");
        }

        public virtual List<_CwPaintableTexture> _FindPaintableTextures(CwGroup group)
        {
            List<_CwPaintableTexture> paintableTextureList = new List<_CwPaintableTexture>();
            paintableTextureList.Clear();

            foreach (CwPaintableMeshTexture paintableTexture in PaintableTextures)
            {
                _CwPaintableTexture _paintableTexture = paintableTexture as _CwPaintableTexture;
                if (_paintableTexture.Group == group)
                {
                    paintableTextureList.Add(_paintableTexture);
                }
            }

            return paintableTextureList;
        }

        public override void RemoveComponents()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "RemoveComponents()");

            List<_CwPaintableTexture> tempPaintableTextures = new List<_CwPaintableTexture>();

            // Remove paintable mesh textures
            this.GetComponents(tempPaintableTextures);

            for (int i = paintableTextures.Count - 1; i >= 0; i--)
            {
                _CwPaintableTexture paintableTexture = tempPaintableTextures[i];

                paintableTexture.Deactivate();

                CwHelper.Destroy(paintableTexture);
            }

            List<CwMaterialCloner> tempMaterialCloners = new List<CwMaterialCloner>();
            // Remove material cloners
            this.GetComponents(tempMaterialCloners);

            for (int i = tempMaterialCloners.Count - 1; i >= 0; i--)
            {
                Debug.LogFormat(LOG_FORMAT, "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                var materialCloner = tempMaterialCloners[i];

                materialCloner.Deactivate();

                CwHelper.Destroy(materialCloner);
            }

            CwHelper.Destroy(this);
        }
    }
}
