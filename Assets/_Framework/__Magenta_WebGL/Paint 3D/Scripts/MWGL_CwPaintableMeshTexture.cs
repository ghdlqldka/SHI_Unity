using UnityEngine;
using PaintCore;
using System.Collections.Generic;

namespace _Magenta_WebGL
{
	/// <summary>This component allows you to make one texture on the attached Renderer paintable.
	/// NOTE: If the texture or texture slot you want to paint is part of a shared material (e.g. prefab material), then I recommend you add the CwMaterialCloner component to make it unique.</summary>
	// [HelpURL(CwCommon.HelpUrlPrefix + "CwPaintableMeshTexture")]
	// [AddComponentMenu(CwCommon.ComponentMenuPrefix + "Paintable Mesh Texture")]
	public class MWGL_CwPaintableMeshTexture : PaintIn3D._CwPaintableTexture
    {
        // private static string LOG_FORMAT = "<color=#94B530><b>[MWGL_CwPaintableMeshTexture]</b></color> {0}";

    }
}
