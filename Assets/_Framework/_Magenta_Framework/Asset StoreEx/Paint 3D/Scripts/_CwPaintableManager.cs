using UnityEngine;
using System.Collections.Generic;
using PaintIn3D;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component automatically updates all CwModel and CwPaintableTexture instances at the end of the frame, batching all paint operations together.</summary>
	// [DefaultExecutionOrder(100)]
	// [DisallowMultipleComponent]
	// [HelpURL(CwCommon.HelpUrlPrefix + "CwPaintableManager")]
	// [AddComponentMenu(CwCommon.ComponentMenuPrefix + "Paintable Manager")]
	public class _CwPaintableManager : CwPaintableManager
    {
        private static string LOG_FORMAT = "<color=#F3940F><b>[_CwPaintableManager]</b></color> {0}";

        protected static _CwPaintableManager _instance;
        public static _CwPaintableManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        protected Camera _camera;
        public Camera _Camera
        {
            get
            {
                Debug.Assert(_camera != null);
                return _camera;
            }
            set
            {
                _camera = value;
            }
        }

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected List<CwMeshModel> _paintableModelList = new List<CwMeshModel>();
        public List<CwMeshModel> PaintableModelList
        {
            get
            {
                return _paintableModelList;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected List<_CwPaintableTexture> _paintableTextureList = new List<_CwPaintableTexture>();
        public List<_CwPaintableTexture> PaintableTextureList
        {
            get
            {
                return _paintableTextureList;
            }
        }

        public static new _CwPaintableManager GetOrCreateInstance()
        {
            throw new System.NotSupportedException("");
        }

        protected virtual void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");

            if (Instance == null)
            {
                Instance = this;

                instances = null; // Not use!!!!!!!
                instancesNode = null; // Not use!!!!!
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        protected override void OnEnable()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "OnEnable()");

            // base.OnEnable();
            // instancesNode = instances.AddLast(this);
        }

        protected override void OnDisable()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "OnDisable()");
            
            // base.OnDisable();
            // instances.Remove(instancesNode);
            // instancesNode = null;
        }

        /*
        protected IEnumerator _LateUpdate()
        {
            var wait = new WaitForEndOfFrame();

            while (true)
            {
                yield return wait; // Wait until the end of the frame (similar timing to LateUpdate)
            }
        }
        */

        protected override void ClearAll()
        {
            // foreach (var model in CwModel.Instances)
            foreach (CwMeshModel model in PaintableModelList)
            {
                model.Prepared = false;
            }
        }

        protected override void LateUpdate()
        {
            // base.LateUpdate();

            // _CwPaintableManager manager = ManagerLinkedList.First.Value as _CwPaintableManager;
            // if (this == manager && CwModel.Instances.Count > 0)
            if (/*this == manager && */PaintableModelList.Count > 0) // 
            {
                ClearAll();
                UpdateAll();
            }
            /*
            else
            {
                CwHelper.Destroy(this.gameObject);
            }
            */

            if (activePaintCount > 1)
            {
                activePaintCount = 1;
            }
            else if (activePaintCount == 1)
            {
                activePaintCount = 0;
            }

            // Update all readers
            // Uses temp list in case any of the reader complete events cause the Instances to change
            int remainingBudget = ReadPixelsBudget;

            tempReaders.Clear();
            tempReaders.AddRange(CwReader.Instances);

            foreach (CwReader reader in tempReaders)
            {
                reader.UpdateRequest(ref remainingBudget);
            }
        }

        protected override void UpdateAll()
        {
            // base.UpdateAll();

            // foreach (var paintableTexture in CwPaintableMeshTexture.Instances)
            foreach (_CwPaintableTexture paintableTexture in PaintableTextureList)
            {
                paintableTexture.ExecuteCommands(true, true);
            }
        }

        public virtual List<CwMeshModel> FindOverlap(Vector3 position, float radius, int layerMask)
        {
            List<CwMeshModel> paintableMeshList = new List<CwMeshModel>();
            // paintableMeshList.Clear();

            foreach (CwMeshModel model in PaintableModelList)
            {
                if (model is _CwPaintableMesh)
                {
                    _CwPaintableMesh paintableMesh = model as _CwPaintableMesh;
                    if (CwHelper.IndexInMask(paintableMesh.gameObject.layer, layerMask) == true)
                    {
                        var bounds = paintableMesh.CachedRenderer.bounds;
                        var sqrRadius = radius + bounds.extents.magnitude; sqrRadius *= sqrRadius;

                        if (Vector3.SqrMagnitude(position - bounds.center) < sqrRadius)
                        {
                            paintableMeshList.Add(paintableMesh);

                            if (paintableMesh.IsActivated == false)
                            {
                                paintableMesh.Activate();
                            }
                        }
                    }
                }
                else if (model is _CwPaintableAtlas)
                {
                    _CwPaintableAtlas paintableMesh = model as _CwPaintableAtlas;
                    if (CwHelper.IndexInMask(paintableMesh.gameObject.layer, layerMask) == true)
                    {
                        var bounds = paintableMesh.CachedRenderer.bounds;
                        var sqrRadius = radius + bounds.extents.magnitude; sqrRadius *= sqrRadius;

                        if (Vector3.SqrMagnitude(position - bounds.center) < sqrRadius)
                        {
                            paintableMeshList.Add(paintableMesh);

                            if (paintableMesh.IsActivated == false)
                            {
                                paintableMesh.Activate();
                            }
                        }
                    }
                }
                else
                {
                    Debug.Assert(false);
                }
            }

            return paintableMeshList;
        }

        public virtual void SubmitAll(CwCommand command, Vector3 position, float radius, int layerMask, CwGroup group, CwMeshModel targetModel, _CwPaintableTexture targetTexture)
        {
            _DoSubmitAll(command, position, radius, layerMask, group, targetModel, targetTexture);

            // Repeat paint?
            CwClone.BuildCloners();

            for (var c = 0; c < CwClone.ClonerCount; c++)
            {
                for (var m = 0; m < CwClone.MatrixCount; m++)
                {
                    var copy = command.SpawnCopy();

                    CwClone.Clone(copy, c, m);

                    _DoSubmitAll(copy, position, radius, layerMask, group, targetModel, targetTexture);

                    copy.Pool();
                }
            }
        }

        protected static void _SubmitAll(CwCommand command, Vector3 position, float radius, int layerMask, CwGroup group)
        {
            var models = CwMeshModel.FindOverlap(position, radius, layerMask);

            for (var i = models.Count - 1; i >= 0; i--)
            {
                SubmitAll(command, models[i] as CwMeshModel, group);
            }
        }

        public virtual void SubmitAll(_CwCommandSphere commandSphere, Vector3 position, float radius, int layerMask, CwGroup group, 
            CwMeshModel targetMesh, _CwPaintableTexture targetTexture)
        {
            Debug.LogFormat(LOG_FORMAT, "SubmitAll(), commandSphere : <b>" + commandSphere + "</b>, targetMesh : " + targetMesh + ", targetTexture : " + targetTexture);
            // CwPaintableManager.SubmitAll(commandSphere, position, radius, layerMask, group, targetMesh, targetTexture);

            _DoSubmitAll(commandSphere, position, radius, layerMask, group, targetMesh, targetTexture);

            // Repeat paint?
            CwClone.BuildCloners();

            for (int c = 0; c < CwClone.ClonerCount; c++)
            {
                for (int m = 0; m < CwClone.MatrixCount; m++)
                {
                    CwCommand copy = commandSphere.SpawnCopy();

                    CwClone.Clone(copy, c, m);

                    _DoSubmitAll(copy, position, radius, layerMask, group, targetMesh, targetTexture);

                    copy.Pool();
                }
            }
        }

        protected void _DoSubmitAll(CwCommand command, Vector3 position, float radius, int layerMask, CwGroup group,
            CwMeshModel targetMesh, _CwPaintableTexture targetTexture)
        {
            if (targetMesh != null)
            {
                if (targetTexture != null)
                {
                    Submit(command, targetMesh, targetTexture);
                }
                else
                {
                    // CwPaintableManager.SubmitAll(command, targetMesh, group);
                    SubmitAll(command, targetMesh, group);
                }
            }
            else
            {
                if (targetTexture != null)
                {
                    Submit(command, targetTexture._Model, targetTexture);
                }
                else
                {
                    if (command is _CwCommandSphere)
                    {
                        _SubmitAll(command as _CwCommandSphere, position, radius, layerMask, group);
                    }
                    else
                    {
                        // CwPaintableManager.SubmitAll(command, position, radius, layerMask, group);
                        _SubmitAll(command, position, radius, layerMask, group);
                    }
                }
            }
        }

        protected virtual void _SubmitAll(_CwCommandSphere commandSphere, Vector3 position, float radius, int layerMask, CwGroup group)
        {
            Debug.LogFormat(LOG_FORMAT, "_SubmitAll()");

            // var models = CwModel.FindOverlap(position, radius, layerMask);
            // List<CwMeshModel> paintableMeshList = _CwPaintableMesh._FindOverlap(position, radius, layerMask);
            List<CwMeshModel> paintableMeshList = Instance.FindOverlap(position, radius, layerMask);

            for (int i = paintableMeshList.Count - 1; i >= 0; i--)
            {
                if (paintableMeshList[i] is _CwPaintableMesh)
                {
                    _SubmitAll(commandSphere, paintableMeshList[i] as _CwPaintableMesh, group);
                }
                else if (paintableMeshList[i] is _CwPaintableAtlas)
                {
                    _SubmitAll(commandSphere, paintableMeshList[i] as _CwPaintableAtlas, group);
                }
            }

#if false //
            paintableMeshList = _CwPaintableMeshAtlas._FindOverlap(position, radius, layerMask);
            for (int i = paintableMeshList.Count - 1; i >= 0; i--)
            {
                if (paintableMeshList[i] is _CwPaintableMesh)
                {
                    _SubmitAll(commandSphere, paintableMeshList[i] as _CwPaintableMesh, group);
                }
                else if (paintableMeshList[i] is _CwPaintableMeshAtlas)
                {
                    _SubmitAll(commandSphere, paintableMeshList[i] as _CwPaintableMeshAtlas, group);
                }
            }
#endif
        }

        protected virtual void _SubmitAll(_CwCommandSphere commandSphere, _CwPaintableMesh paintableMesh, CwGroup group)
        {
            List<_CwPaintableTexture> paintableTextures = paintableMesh._FindPaintableTextures(group);

            for (int i = paintableTextures.Count - 1; i >= 0; i--)
            {
                Submit(commandSphere, paintableMesh, paintableTextures[i]);
            }
        }

        protected virtual void _SubmitAll(_CwCommandSphere commandSphere, _CwPaintableAtlas model, CwGroup group)
        {
            List<_CwPaintableTexture> paintableTextures = model._FindPaintableTextures(group);

            for (var i = paintableTextures.Count - 1; i >= 0; i--)
            {
                Submit(commandSphere, model, paintableTextures[i]);
            }
        }

        public virtual CwCommand Submit(CwCommand command, CwMeshModel model, _CwPaintableTexture paintableTexture)
        {
            if (command.Preview == false && CwStateManager.PotentiallyStoreStates == true)
            {
                CwStateManager.StoreAllStates();
            }

            CwCommand copy = null;
            if (command is _CwCommandSphere)
            {
                copy = ((_CwCommandSphere)command).SpawnCopy();
                ((_CwCommandSphere)copy).Apply(paintableTexture);
            }
            else if (command is _CwCommandDecal)
            {
                copy = ((_CwCommandDecal)command).SpawnCopy();
                ((_CwCommandDecal)copy).Apply(paintableTexture);
            }
            else
            {
                Debug.Assert(false);
                copy = command.SpawnCopy();
                copy.Apply(paintableTexture);
            }

            

            copy.Model = model;
            copy.Submesh = paintableTexture.Slot.Index;

            paintableTexture.AddCommand(copy);

            return copy;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ///CwStateManager WRAPPER
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool CanUndo
        {
            get
            {
                return CwStateManager.CanUndo;
            }
        }

        public bool CanRedo
        {
            get
            {
                return CwStateManager.CanRedo;
            }
        }

        public bool PotentiallyStoreStates
        {
            set
            {
                CwStateManager.PotentiallyStoreStates = value;
            }

            get
            {
                return CwStateManager.PotentiallyStoreStates;
            }
        }

        public bool AllStatesStored
        {
            set
            {
                CwStateManager.AllStatesStored = value;
            }

            get
            {
                return CwStateManager.AllStatesStored;
            }
        }

        public void StoreAllStates()
        {
            CwStateManager.StoreAllStates();
        }

        /// <summary>This method should be called if you're about to send paint hits that might apply paint to objects. If so, <b>StoreState</b> will be called on all active and enabled CwPaintableTextures</summary>
        public void PotentiallyStoreAllStates()
        {
            CwStateManager.PotentiallyStoreAllStates();
        }

        /// <summary>This method will call <b>ClearStates</b> on all active and enabled CwPaintableTextures.</summary>
        public void ClearAllStates()
        {
            CwStateManager.ClearAllStates();
        }

        /// <summary>This method will call <b>Undo</b> on all active and enabled CwPaintableTextures.</summary>
        public void UndoAll()
        {
            Debug.LogFormat(LOG_FORMAT, "UndoAll()");

            CwStateManager.UndoAll();
        }

        /// <summary>This method will call <b>Redo</b> on all active and enabled CwPaintableTextures.</summary>
        public void RedoAll()
        {
            CwStateManager.RedoAll();
        }
    }
}