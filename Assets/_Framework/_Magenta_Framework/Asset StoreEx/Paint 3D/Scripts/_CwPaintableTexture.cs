using UnityEngine;
using PaintCore;
using System.Collections.Generic;
using System.Collections;

namespace PaintIn3D
{
	/// <summary>This component allows you to make one texture on the attached Renderer paintable.
	/// NOTE: If the texture or texture slot you want to paint is part of a shared material (e.g. prefab material), then I recommend you add the CwMaterialCloner component to make it unique.</summary>
	// [HelpURL(CwCommon.HelpUrlPrefix + "CwPaintableMeshTexture")]
	// [AddComponentMenu(CwCommon.ComponentMenuPrefix + "Paintable Mesh Texture")]
	public class _CwPaintableTexture : CwPaintableMeshTexture
    {
        private static string LOG_FORMAT = "<color=#94B530><b>[_CwPaintableMeshTexture]</b></color> {0}";

        // protected CwModel model;
        public CwMeshModel _Model
        {
            get
            {
                return model as CwMeshModel;
            }
            set
            {
                model = value;
            }
        }

        // protected CwPaintableMesh parent;
        protected _CwPaintableMesh PaintableMesh
        {
            get
            {
                return parent as _CwPaintableMesh;
            }
            set
            {
                parent = value;
            }
        }

        // protected RenderTexture current;
        protected RenderTexture CurrentTexture
        {
            get
            {
                return current;
            }
            set
            {
                Debug.LogWarningFormat(LOG_FORMAT, "CurrentTexture has SET!!!!!, <b>" + value + "</b>");
                current = value;
            }
        }

        // protected RenderTexture preview;
        protected RenderTexture PreviewTexture
        {
            get
            {
                return preview;
            }
            set
            {
                preview = value;
            }
        }

        // public Texture Texture { set { texture = value; } get { return texture; } }
        public Texture _Texture 
        { 
            set 
            { 
                texture = value;
            } 
            get 
            {
                return texture;
            }
        }

        // public Color Color { set { color = value; } get { return color; } }
        protected Color _Color 
        { 
            set 
            { 
                color = value;
            } 
            get 
            { 
                return color;
            }
        }
#if DEBUG
        [Header("=====> DEBUG <=====")]
        // [SerializeField] protected Texture texture;
        [ReadOnly]
        [SerializeField]
        protected Texture DEBUG_texture;

        [ReadOnly]
        [SerializeField]
        protected RenderTexture DEBUG_current;

        [ReadOnly]
        [SerializeField]
        protected RenderTexture DEBUG_preview;
#endif

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>, Existing : <b>" + Existing +
                "</b>, SaveLoad : <b>" + SaveLoad + "</b>, SaveName : <b>" + SaveName + "</b>, IsDummy : <b>" + IsDummy + "</b>");

            _CwPaintableTexture.OnInstanceAdded += _OnInstanceAdded;
            _CwPaintableTexture.OnInstanceRemoved += _OnInstanceRemoved;
        }

        protected override void OnDestroy()
        {
            _CwPaintableTexture.OnInstanceRemoved -= _OnInstanceRemoved;
            _CwPaintableTexture.OnInstanceAdded -= _OnInstanceAdded;

            // base.OnDestroy();

            if (activated == true)
            {
                if (SaveLoad == SaveLoadType.Automatic && string.IsNullOrEmpty(SaveName) == false)
                {
                    Save();
                }

                CwCommon.ReleaseRenderTexture(CurrentTexture);
                CwCommon.ReleaseRenderTexture(PreviewTexture);

                ClearStates();
            }

            CwSerialization.TryRegister(this, default(CwHash));
        }

        protected override void OnEnable()
        {
            Debug.LogFormat(LOG_FORMAT, "OnEnable()");

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
            _CwPaintableManager.Instance.PaintableTextureList.Add(this);

            if (OnInstanceAdded != null)
            {
                OnInstanceAdded.Invoke(this);
            }

            CwSerialization.TryRegister(this, hash);
        }

        protected override void OnDisable()
        {
            // base.OnDisable();
            // instances.Remove(instancesNode);
            // instancesNode = null;
            _CwPaintableManager.Instance.PaintableTextureList.Remove(this);

            if (OnInstanceRemoved != null)
            {
                OnInstanceRemoved.Invoke(this);
            }
        }

        protected override void OnApplicationPause(bool paused)
        {
            if (paused == true)
            {
                if (activated == true)
                {
                    if (SaveLoad == SaveLoadType.Automatic && string.IsNullOrEmpty(SaveName) == false)
                    {
                        Save();
                    }
                }
            }
        }

        protected virtual void _OnInstanceAdded(CwPaintableTexture instance)
        {
            Debug.LogFormat(LOG_FORMAT, "_OnInstanceAdded(), instance : <b>" + instance.name + "</b>");
        }

        protected virtual void _OnInstanceRemoved(CwPaintableTexture instance)
        {
            Debug.LogFormat(LOG_FORMAT, "_OnInstanceRemoved(), instance : <b>" + instance.name + "</b>");
        }

        public override void Activate()
        {
            Debug.LogFormat(LOG_FORMAT, "Activate(), activated : <b>" + activated + "</b>");
            // base.Activate();

            if (activated == false)
            {
                _Model = GetComponentInParent<CwMeshModel>();
                Debug.Assert(_Model != null);
#if DEBUG
                if (_Model is _CwPaintableMesh)
                {
                    Debug.LogWarningFormat(LOG_FORMAT, "_Model(<color=red>_CwPaintableMesh</color>) : <color=yellow>" + _Model + "</color>");
                }
                else if (_Model is _CwPaintableAtlas)
                {
                    Debug.LogWarningFormat(LOG_FORMAT, "_Model(<color=red>_CwPaintableAtlas</color>) : <color=yellow>" + _Model + "</color>");
                }
                else
                {
                    Debug.Assert(false);
                }
#endif

                // if (model != null)
                {
                    if (_Model is _CwPaintableMesh)
                    {
                        // [SerializeField] protected CwSlot slot = new CwSlot(0, "_MainTex");
                        oldTexture = ((_CwPaintableMesh)_Model).GetExistingTexture(Slot);
                    }
                    else if (_Model is _CwPaintableAtlas)
                    {
                        oldTexture = ((_CwPaintableAtlas)_Model).GetExistingTexture(Slot);
                    }
                    else
                    {
                        Debug.Assert(false);
                        oldTexture = _Model.GetExistingTexture(Slot);
                    }

                    int finalWidth = Width;
                    int finalHeight = Height;
                    Texture finalTexture = _Texture;

                    _Model.ScaleSize(ref finalWidth, ref finalHeight);

                    if (finalTexture == null && Existing != ExistingType.Ignore)
                    {
                        finalTexture = oldTexture;

                        if (Existing == ExistingType.UseAndKeep)
                        {
                            _Texture = oldTexture;
#if DEBUG
                            DEBUG_texture = oldTexture;
#endif
                        }
                    }

                    if (string.IsNullOrEmpty(ShaderKeyword) == false)
                    {
                        //material.EnableKeyword(ShaderKeyword);
                    }

                    RenderTextureDescriptor desc = new RenderTextureDescriptor(Width, Height, Format, 0);

                    desc.autoGenerateMips = false;

                    if (MipMaps == MipType.Auto)
                    {
                        if (finalTexture != null)
                        {
                            desc.useMipMap = CwCommon.HasMipMaps(finalTexture);
                        }
                    }
                    else
                    {
                        desc.useMipMap = MipMaps == MipType.On;
                    }

                    CurrentTexture = CwCommon.GetRenderTexture(desc);
#if DEBUG
                    DEBUG_current = CurrentTexture;
#endif

                    if (Filter == FilterType.Auto)
                    {
                        if (finalTexture != null)
                        {
                            CurrentTexture.filterMode = finalTexture.filterMode;
                        }
                    }
                    else
                    {
                        CurrentTexture.filterMode = (FilterMode)Filter;
                    }

                    if (Aniso == AnisoType.Auto)
                    {
                        if (finalTexture != null)
                        {
                            CurrentTexture.anisoLevel = finalTexture.anisoLevel;
                        }
                    }
                    else
                    {
                        CurrentTexture.anisoLevel = (int)Aniso;
                    }

                    if (WrapU == WrapType.Auto)
                    {
                        if (finalTexture != null)
                        {
                            CurrentTexture.wrapModeU = finalTexture.wrapModeU;
                        }
                    }
                    else
                    {
                        CurrentTexture.wrapModeU = (TextureWrapMode)WrapU;
                    }

                    if (WrapV == WrapType.Auto)
                    {
                        if (finalTexture != null)
                        {
                            CurrentTexture.wrapModeV = finalTexture.wrapModeV;
                        }
                    }
                    else
                    {
                        CurrentTexture.wrapModeV = (TextureWrapMode)WrapV;
                    }

                    activated = true;

                    Clear(finalTexture, _Color);

                    if (IsDummy == false)
                    {
                        ApplyTexture(CurrentTexture);
                    }

                    if (SaveLoad == SaveLoadType.Automatic && string.IsNullOrEmpty(SaveName) == false)
                    {
                        Load();
                    }

                    NotifyOnModified(false);

                    // If an undo state has already been created, create a state for this
                    if (AutoCreateState == true && CwStateManager.AllStatesStored == true)
                    {
                        StoreState();
                    }
                }
            }
        }

        public override void ExecuteCommands(bool sendNotifications, bool doSort)
        {
            // base.ExecuteCommands(sendNotifications, doSort);

            if (activated == false)
            {
                Debug.LogFormat(LOG_FORMAT, "skippppppppppppppppppppppppppppppppppppppp");
                return;
            }

            // if (activated == true)
            // {
            bool hidePreview = true;

            if (CommandsPending == true)
            {
                var oldActive = RenderTexture.active;

                // Paint
                if (paintCommands.Count > 0)
                {
                    if (doSort == true)
                    {
                        paintCommands.Sort(CwCommand.Compare);
                    }

                    Debug.LogFormat(LOG_FORMAT, "ExecuteCommands(), CurrentTexture");
                    ExecuteCommands(paintCommands, sendNotifications, CurrentTexture, ref preview);
                }

                var swap = PreviewTexture;

                PreviewTexture = null;
#if DEBUG
                DEBUG_preview = PreviewTexture;
#endif

                // Preview
                if (previewCommands.Count > 0)
                {
                    PreviewTexture = swap;
#if DEBUG
                    DEBUG_preview = PreviewTexture;
#endif
                    swap = null;

                    if (PreviewTexture == null)
                    {
                        PreviewTexture = CwCommon.GetRenderTexture(CurrentTexture);
#if DEBUG
                        DEBUG_preview = PreviewTexture;
#endif
                    }

                    hidePreview = false;

                    PreviewTexture.DiscardContents();

                    Graphics.Blit(CurrentTexture, PreviewTexture);

                    if (doSort == true)
                    {
                        previewCommands.Sort(CwCommand.Compare);
                    }

                    Debug.LogFormat(LOG_FORMAT, "ExecuteCommands(), <b>PreviewTexture</b>");
                    ExecuteCommands(previewCommands, sendNotifications, PreviewTexture, ref swap);
                }

                CwCommon.ReleaseRenderTexture(swap);

                RenderTexture.active = oldActive;
            }

            if (hidePreview == true)
            {
                PreviewTexture = CwCommon.ReleaseRenderTexture(PreviewTexture);
#if DEBUG
                DEBUG_preview = PreviewTexture;
#endif
            }

            if (IsDummy == false)
            {
                // ApplyTexture(PreviewTexture != null ? PreviewTexture : CurrentTexture);
                if (PreviewTexture != null)
                {
                    ApplyTexture(PreviewTexture);
                }
                else
                {
                    ApplyTexture(CurrentTexture);
                }
            }
            // }
        }

        protected override void ApplyTexture(Texture texture)
        {
            // Debug.LogWarningFormat(LOG_FORMAT, "ApplyTexture()");

            if (PaintableMesh == null)
            {
                PaintableMesh = GetComponentInParent<_CwPaintableMesh>();
            }

            Debug.Assert(PaintableMesh != null);
            if (PaintableMesh != null)
            {
                if (PaintableMesh.MaterialApplication == CwPaintableMesh.MaterialApplicationType.PropertyBlock)
                {
                    PaintableMesh.ApplyTexture(Slot, texture);

                    // foreach (Renderer otherRenderer in PaintableMesh.OtherRenderers)
                    foreach (Renderer otherRenderer in PaintableMesh.OtherRendererList)
                    {
                        if (otherRenderer != null)
                        {
                            PaintableMesh.ApplyTexture(otherRenderer, Slot, texture);
                        }
                    }
                }
                else if (PaintableMesh.MaterialApplication == CwPaintableMesh.MaterialApplicationType.ClonerAndTextures)
                {
                    if (Slot.Index >= 0)
                    {
                        var materials = PaintableMesh.Materials;

                        if (Slot.Index < materials.Length)
                        {
                            var material = materials[Slot.Index];

                            if (material != null)
                            {
                                material.SetTexture(Slot.Name, texture);
                            }
                        }
                    }
                }
                else
                {
                    Debug.Assert(false);
                }
            }
        }

#if false //
        public static new void HideAllPreviews()
        {
            Debug.LogFormat(LOG_FORMAT, "HideAllPreviews()");
            foreach (CwPaintableTexture instance in TextureLinkedList)
            {
                ((_CwPaintableMeshTexture)instance).HidePreview();
            }
        }
#endif

        protected override void ExecuteCommands(List<CwCommand> commands, 
            bool sendNotifications, RenderTexture mainTexture, ref RenderTexture swapTexture)
        {
            Debug.LogFormat(LOG_FORMAT, "ExecuteCommands(), commands.Count : " + commands.Count);
            // base.ExecuteCommands(commands, sendNotifications, mainTexture, ref swapTexture);

            RenderTexture.active = mainTexture;

            // Loop through all commands
            foreach (CwCommand command in commands)
            {
                if (command is _CwCommandSphere)
                {
                    ProcessCommandSphere(command as _CwCommandSphere, mainTexture, ref swapTexture);
                }
                else if (command is _CwCommandDecal)
                {
                    ProcessCommandDecal(command as _CwCommandDecal, mainTexture, ref swapTexture);
                }
                else
                {
                    ProcessCommand(command, mainTexture, ref swapTexture);
                }
            } // end-foreach

            commands.Clear();

            PostExecuteCommands(mainTexture);

            if (mainTexture.useMipMap == true)
            {
                mainTexture.GenerateMips();
            }

            if (sendNotifications == true)
            {
                NotifyOnModified(commands == previewCommands);
            }
        }

        protected virtual void ProcessCommand(CwCommand command, RenderTexture mainTexture, ref RenderTexture swapTexture)
        {
            Debug.LogFormat(LOG_FORMAT, "commandddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd");
            CwModel commandMesh;
            Material commandMaterial;

            if (command.Model.TryGetInstance(out commandMesh) == true && command.Material.TryGetInstance(out commandMaterial) == true)
            {
                Mesh preparedMesh = default(Mesh);
                Matrix4x4 preparedMatrix = Matrix4x4.identity;
                int preparedSubmesh = 0;
                CwCoord preparedCoord = CwCoord.First;

                if (command.RequireMesh == true)
                {
                    Debug.LogFormat(LOG_FORMAT, "1 - commandMesh : " + commandMesh);
                    if (commandMesh is _CwPaintableMesh)
                    {
                        ((_CwPaintableMesh)commandMesh).GetPrepared(ref preparedMesh, ref preparedMatrix, Coord);
                    }
                    else
                    {
                        commandMesh.GetPrepared(ref preparedMesh, ref preparedMatrix, Coord);
                    }

                    preparedSubmesh = command.Submesh;
                    preparedCoord = Coord;
                }
                else
                {
                    Debug.LogFormat(LOG_FORMAT, "2");
                    preparedMesh = CwCommon.GetQuadMesh();
                }

                //if (doubleBuffer == true)
                {
                    if (swapTexture == null)
                    {
                        swapTexture = CwCommon.GetRenderTexture(mainTexture);
                    }

                    Debug.Assert(preparedMesh != null);
                    CwBlit.Blit(swapTexture, preparedMesh, preparedSubmesh, mainTexture, preparedCoord);

                    commandMaterial.SetTexture(_Buffer, swapTexture);
                    commandMaterial.SetVector(_BufferSize, new Vector2(swapTexture.width, swapTexture.height));
                }

                command.Apply(commandMaterial);

                RenderTexture.active = mainTexture;

                CwCommon.Draw(commandMaterial, command.Pass, preparedMesh, preparedMatrix, preparedSubmesh, preparedCoord);
            }

            command.Pool();
        }

        protected virtual void ProcessCommandDecal(_CwCommandDecal command, RenderTexture mainTexture, ref RenderTexture swapTexture)
        {
            // Debug.LogFormat(LOG_FORMAT, "commandddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd");
            CwModel commandMesh;
            Material commandMaterial;

            if (command.Model.TryGetInstance(out commandMesh) == true && command.Material.TryGetInstance(out commandMaterial) == true)
            {
                Mesh preparedMesh = default(Mesh);
                Matrix4x4 preparedMatrix = Matrix4x4.identity;
                int preparedSubmesh = 0;
                CwCoord preparedCoord = CwCoord.First;

                if (command.RequireMesh == true)
                {
                    Debug.LogFormat(LOG_FORMAT, "1 - commandMesh : " + commandMesh);
                    if (commandMesh is _CwPaintableMesh)
                    {
                        ((_CwPaintableMesh)commandMesh).GetPrepared(ref preparedMesh, ref preparedMatrix, Coord);
                    }
                    else
                    {
                        commandMesh.GetPrepared(ref preparedMesh, ref preparedMatrix, Coord);
                    }

                    preparedSubmesh = command.Submesh;
                    preparedCoord = Coord;
                }
                else
                {
                    Debug.LogFormat(LOG_FORMAT, "2");
                    preparedMesh = CwCommon.GetQuadMesh();
                }

                //if (doubleBuffer == true)
                {
                    if (swapTexture == null)
                    {
                        swapTexture = CwCommon.GetRenderTexture(mainTexture);
                    }

                    Debug.Assert(preparedMesh != null);
                    CwBlit.Blit(swapTexture, preparedMesh, preparedSubmesh, mainTexture, preparedCoord);

                    commandMaterial.SetTexture(_Buffer, swapTexture);
                    commandMaterial.SetVector(_BufferSize, new Vector2(swapTexture.width, swapTexture.height));
                }

                command.Apply(commandMaterial);

                RenderTexture.active = mainTexture;

                CwCommon.Draw(commandMaterial, command.Pass, preparedMesh, preparedMatrix, preparedSubmesh, preparedCoord);
            }

            command.Pool();
        }

        protected virtual void ProcessCommandSphere(_CwCommandSphere command, RenderTexture mainTexture, ref RenderTexture swapTexture)
        {
            CwModel commandMesh;
            Material commandMaterial;

            if (command.Model.TryGetInstance(out commandMesh) == true && command.Material.TryGetInstance(out commandMaterial) == true)
            {
                Mesh preparedMesh = default(Mesh);
                Matrix4x4 preparedMatrix = Matrix4x4.identity;
                int preparedSubmesh = 0;
                CwCoord preparedCoord = CwCoord.First;

                if (command.RequireMesh == true)
                {
                    Debug.LogFormat(LOG_FORMAT, "3 - commandMesh : " + commandMesh);
                    if (commandMesh is _CwPaintableMesh)
                    {
                        ((_CwPaintableMesh)commandMesh).GetPrepared(ref preparedMesh, ref preparedMatrix, Coord);
                    }
                    else
                    {
                        commandMesh.GetPrepared(ref preparedMesh, ref preparedMatrix, Coord);
                    }

                    preparedSubmesh = command.Submesh;
                    preparedCoord = Coord;
                }
                else
                {
                    Debug.LogFormat(LOG_FORMAT, "4");
                    preparedMesh = CwCommon.GetQuadMesh();
                }

                //if (doubleBuffer == true)
                {
                    if (swapTexture == null)
                    {
                        swapTexture = CwCommon.GetRenderTexture(mainTexture);
                    }

                    Debug.Assert(preparedMesh != null);
                    CwBlit.Blit(swapTexture, preparedMesh, preparedSubmesh, mainTexture, preparedCoord);

                    commandMaterial.SetTexture(_Buffer, swapTexture);
                    commandMaterial.SetVector(_BufferSize, new Vector2(swapTexture.width, swapTexture.height));
                }

                command.Apply(commandMaterial);

                RenderTexture.active = mainTexture;

                CwCommon.Draw(commandMaterial, command.Pass, preparedMesh, preparedMatrix, preparedSubmesh, preparedCoord);
            }

            command.Pool();
        }

        /*
        protected void Invoke_OnAddCommand(CwCommand command)
        {
            if (OnAddCommand != null)
            {
                OnAddCommand.Invoke(command);
            }
        }

        protected void Invoke_OnAddCommandGlobal(CwPaintableTexture paintableTexture, CwCommand command)
        {
            if (OnAddCommandGlobal != null)
            {
                OnAddCommandGlobal.Invoke(paintableTexture, command);
            }
        }
        */

        public override void AddCommand(CwCommand command)
        {
            // base.AddCommand(command);

            if (command.Preview == true)
            {
                command.Index = previewCommands.Count;

                previewCommands.Add(command);
            }
            else
            {
                command.Index = paintCommands.Count;

                paintCommands.Add(command);

                if (UndoRedo == UndoRedoType.LocalCommandCopy && command.Preview == false)
                {
                    CwCommand localCommand = command.SpawnCopyLocal(transform);

                    localCommand.Index = localCommands.Count;

                    localCommands.Add(localCommand);
                }
            }

            Invoke_OnAddCommand(command);
            /*
            if (OnAddCommand != null)
            {
                OnAddCommand.Invoke(command);
            }
            */

            Invoke_OnAddCommandGlobal(this, command);
            /*
            if (OnAddCommandGlobal != null)
            {
                OnAddCommandGlobal.Invoke(this, command);
            }
            */
        }

        // [ContextMenu("Copy Texture")]
        public override void CopyTexture()
        {
            // base.CopyTexture();

            _Texture = Slot.FindTexture(gameObject);
#if DEBUG
            DEBUG_texture = _Texture;
#endif
        }

        // [ContextMenu("Clear")]
        public override void Clear()
        {
            Debug.LogFormat(LOG_FORMAT, "Clear(), _Texture : " + _Texture + ", _Color : " + _Color);
            // base.Clear();

            Clear(_Texture, _Color);
        }
    }
}
