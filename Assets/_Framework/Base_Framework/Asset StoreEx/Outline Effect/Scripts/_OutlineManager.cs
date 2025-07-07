/*
//  Copyright (c) 2015 José Guerreiro. All rights reserved.
//
//  MIT license, see http://www.opensource.org/licenses/mit-license.php
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
*/

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Unity.VisualScripting;
// using UnityEngine.VR;

namespace cakeslice
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	/* [ExecuteInEditMode] */
	public class _OutlineManager : OutlineEffect
    {
        private static string LOG_FORMAT = "<color=#FFC300><b>[_OutlineManager]</b></color> {0}";

        // public static OutlineEffect Instance { get; private set; }
        protected static _OutlineManager _instance;
        public static new _OutlineManager Instance 
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

        // private readonly LinkedSet<Outline> outlines = new LinkedSet<Outline>();

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            if (Instance == null)
            {
                Instance = this;

                Debug.Assert(sourceCamera != null);
                Debug.Assert(sourceCamera.GetComponent<_OutlineCamera>() == null);
                sourceCamera.AddComponent<_OutlineCamera>();

                /*
                Transform _transform = this.transform.Find("Outline Camera");
                GameObject outlineCameraObj;
                if (_transform == null)
                {
                    outlineCameraObj = new GameObject("Outline Camera");
                    _transform = outlineCameraObj.transform;
                }
                else
                {
                    outlineCameraObj = _transform.gameObject;
                }
                outlineCamera = _transform.GetComponent<Camera>();
                if (outlineCamera == null)
                {
                    outlineCameraObj.transform.parent = this.transform;
                    outlineCamera = outlineCameraObj.AddComponent<Camera>();
                    outlineCamera.enabled = false;
                }
                else
                {
                    Debug.Assert(outlineCamera.enabled == false);
                }
                */
                outlineCamera = this.GetComponent<Camera>();
            }
            else
            {
                Destroy(this);
                throw new System.Exception("you can only have one outline camera in the scene");
            }
        }

        protected override void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            if (renderTexture != null)
                renderTexture.Release();
            if (extraRenderTexture != null)
                extraRenderTexture.Release();
            DestroyMaterials();

            Instance = null;
        }

        protected override void OnEnable()
        {
#pragma warning disable 0618
            _Outline[] o = FindObjectsOfType<_Outline>();
#pragma warning restore 0618
            if (autoEnableOutlines)
            {
                foreach (_Outline oL in o)
                {
                    oL.enabled = false;
                    oL.enabled = true;
                }
            }
            else
            {
                foreach (_Outline oL in o)
                {
                    if (!outlines.Contains(oL))
                        outlines.Add(oL);
                }
            }
        }

        protected override void Start()
        {
            CreateMaterialsIfNeeded();
            UpdateMaterialsPublicProperties();

            /*
            if (sourceCamera == null)
            {
                sourceCamera = GetComponent<Camera>();

                if (sourceCamera == null)
                    sourceCamera = Camera.main;
            }
            */

            /*
            if (outlineCamera == null)
            {
                foreach (Camera c in GetComponentsInChildren<Camera>())
                {
                    if (c.name == "Outline Camera")
                    {
                        outlineCamera = c;
                        c.enabled = false;

                        break;
                    }
                }

                if (outlineCamera == null)
                {
                    GameObject cameraGameObject = new GameObject("Outline Camera");
                    cameraGameObject.transform.parent = sourceCamera.transform;
                    outlineCamera = cameraGameObject.AddComponent<Camera>();
                    outlineCamera.enabled = false;
                }
            }
            */

            if (renderTexture != null)
                renderTexture.Release();
            if (extraRenderTexture != null)
                renderTexture.Release();
            renderTexture = new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 16, RenderTextureFormat.Default);
            extraRenderTexture = new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 16, RenderTextureFormat.Default);
            UpdateOutlineCameraFromSource();

            commandBuffer = new CommandBuffer();
            outlineCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, commandBuffer);
        }

        public virtual void _OnPreRender()
        {
            OnPreRender();
        }

        public override void OnPreRender()
        {
            if (commandBuffer == null)
                return;

            // The first frame during which there are no outlines, we still need to render 
            // to clear out any outlines that were being rendered on the previous frame
            if (outlines.Count == 0)
            {
                if (!RenderTheNextFrame)
                    return;

                RenderTheNextFrame = false;
            }
            else
            {
                RenderTheNextFrame = true;
            }

            CreateMaterialsIfNeeded();

            if (renderTexture == null || renderTexture.width != sourceCamera.pixelWidth || renderTexture.height != sourceCamera.pixelHeight)
            {
                if (renderTexture != null)
                    renderTexture.Release();
                if (extraRenderTexture != null)
                    renderTexture.Release();
                renderTexture = new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 16, RenderTextureFormat.Default);
                extraRenderTexture = new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 16, RenderTextureFormat.Default);
                outlineCamera.targetTexture = renderTexture;
            }
            UpdateMaterialsPublicProperties();
            UpdateOutlineCameraFromSource();
            outlineCamera.targetTexture = renderTexture;
            commandBuffer.SetRenderTarget(renderTexture);

            commandBuffer.Clear();

            DrawOutlineRenderer();
            /*
            foreach (Outline _outline in outlines)
            {
                _Outline outline = _outline as _Outline;
                LayerMask l = sourceCamera.cullingMask;

                if (outline != null && l == (l | (1 << outline.gameObject.layer)))
                {
                    for (int v = 0; v < outline.SharedMaterials.Length; v++)
                    {
                        Material _material = null;

                        if (outline.SharedMaterials[v].HasProperty("_MainTex") && outline.SharedMaterials[v].mainTexture != null && outline.SharedMaterials[v])
                        {
                            foreach (Material g in materialBuffer)
                            {
                                if (g.mainTexture == outline.SharedMaterials[v].mainTexture)
                                {
                                    if (outline.eraseRenderer && g.color == outlineEraseMaterial.color)
                                        _material = g;
                                    else if (!outline.eraseRenderer && g.color == GetMaterialFromID(outline.color).color)
                                        _material = g;
                                }
                            }

                            if (_material == null)
                            {
                                if (outline.eraseRenderer)
                                    _material = new Material(outlineEraseMaterial);
                                else
                                    _material = new Material(GetMaterialFromID(outline.color));

                                _material.mainTexture = outline.SharedMaterials[v].mainTexture;
                                materialBuffer.Add(_material);
                            }
                        }
                        else
                        {
                            if (outline.eraseRenderer)
                                _material = outlineEraseMaterial;
                            else
                                _material = GetMaterialFromID(outline.color);
                        }

                        if (backfaceCulling)
                            _material.SetInt("_Culling", (int)UnityEngine.Rendering.CullMode.Back);
                        else
                            _material.SetInt("_Culling", (int)UnityEngine.Rendering.CullMode.Off);

                        MeshFilter mL = outline.MeshFilter;
                        SkinnedMeshRenderer sMR = outline.SkinnedMeshRenderer;
                        SpriteRenderer sR = outline.SpriteRenderer;
                        if (mL)
                        {
                            if (mL.sharedMesh != null)
                            {
                                if (v < mL.sharedMesh.subMeshCount)
                                    commandBuffer.DrawRenderer(outline.Renderer, _material, v, 0);
                            }
                        }
                        else if (sMR)
                        {
                            if (sMR.sharedMesh != null)
                            {
                                if (v < sMR.sharedMesh.subMeshCount)
                                    commandBuffer.DrawRenderer(outline.Renderer, _material, v, 0);
                            }
                        }
                        else if (sR)
                        {
                            commandBuffer.DrawRenderer(outline.Renderer, _material, v, 0);
                        }
                    }
                }
            }
            */

            outlineCamera.Render();
        }

        protected virtual void DrawOutlineRenderer()
        {
            foreach (Outline _outline in outlines)
            {
                _Outline outline = _outline as _Outline;
                LayerMask l = sourceCamera.cullingMask;

                if (outline != null && l == (l | (1 << outline.gameObject.layer)))
                {
                    for (int v = 0; v < outline.SharedMaterials.Length; v++)
                    {
                        Material _material = null;

                        if (outline.SharedMaterials[v].HasProperty("_MainTex") && 
                            outline.SharedMaterials[v].mainTexture != null && outline.SharedMaterials[v])
                        {
                            foreach (Material g in materialBuffer)
                            {
                                if (g.mainTexture == outline.SharedMaterials[v].mainTexture)
                                {
                                    if (outline.eraseRenderer && g.color == outlineEraseMaterial.color)
                                        _material = g;
                                    else if (!outline.eraseRenderer && g.color == GetMaterialFromID(outline.color).color)
                                    // else if (!outline.eraseRenderer && g.color == outline._Material.color)
                                        _material = g;
                                }
                            }

                            if (_material == null)
                            {
                                if (outline.eraseRenderer)
                                    _material = new Material(outlineEraseMaterial);
                                else
                                    _material = new Material(GetMaterialFromID(outline.color));
                                    // _material = new Material(outline._Material);

                                _material.mainTexture = outline.SharedMaterials[v].mainTexture;
                                materialBuffer.Add(_material);
                            }
                        }
                        else
                        {
                            if (outline.eraseRenderer)
                                _material = outlineEraseMaterial;
                            else
                                _material = GetMaterialFromID(outline.color);
                                // _material = outline._Material;
                        }

                        if (backfaceCulling)
                            _material.SetInt("_Culling", (int)UnityEngine.Rendering.CullMode.Back);
                        else
                            _material.SetInt("_Culling", (int)UnityEngine.Rendering.CullMode.Off);

                        MeshFilter mL = outline.MeshFilter;
                        SkinnedMeshRenderer sMR = outline.SkinnedMeshRenderer;
                        SpriteRenderer sR = outline.SpriteRenderer;
                        if (mL)
                        {
                            if (mL.sharedMesh != null)
                            {
                                if (v < mL.sharedMesh.subMeshCount)
                                    commandBuffer.DrawRenderer(outline.Renderer, _material, v, 0);
                            }
                        }
                        else if (sMR)
                        {
                            if (sMR.sharedMesh != null)
                            {
                                if (v < sMR.sharedMesh.subMeshCount)
                                    commandBuffer.DrawRenderer(outline.Renderer, _material, v, 0);
                            }
                        }
                        else if (sR)
                        {
                            commandBuffer.DrawRenderer(outline.Renderer, _material, v, 0);
                        }
                    }
                }
            }
        }

        protected override Material GetMaterialFromID(int ID)
        {
            if (ID == 0)
                return outline1Material;
            else if (ID == 1)
                return outline2Material;
            else if (ID == 2)
                return outline3Material;
            else
                return outline1Material;
        }

        protected override void CreateMaterialsIfNeeded()
        {
            if (outlineShader == null)
                outlineShader = Resources.Load<Shader>("_OutlineShader");
            if (outlineBufferShader == null)
            {
                outlineBufferShader = Resources.Load<Shader>("_OutlineBufferShader");
            }
            if (outlineShaderMaterial == null)
            {
                outlineShaderMaterial = new Material(outlineShader);
                outlineShaderMaterial.hideFlags = HideFlags.HideAndDontSave;
                UpdateMaterialsPublicProperties();
            }
            if (outlineEraseMaterial == null)
                outlineEraseMaterial = CreateMaterial(new Color(0, 0, 0, 0));
            if (outline1Material == null)
                outline1Material = CreateMaterial(new Color(1, 0, 0, 0));
            if (outline2Material == null)
                outline2Material = CreateMaterial(new Color(0, 1, 0, 0));
            if (outline3Material == null)
                outline3Material = CreateMaterial(new Color(0, 0, 1, 0));
        }

        public virtual void _OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            base.OnRenderImage(source, destination);
        }

        public override void AddOutline(Outline outline)
        {
            outlines.Add(outline);
        }

        public override void RemoveOutline(Outline outline)
        {
            outlines.Remove(outline);
        }
    }
}
