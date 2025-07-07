using UnityEngine;
using PaintCore;
using System.Collections.Generic;


#if UNITY_EDITOR
namespace _Magenta_WebGL
{
	using CW.Common;
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(MWGL_CwPaintableMeshTexture))]
	public class MWGL_CwPaintableMeshTextureEditor : PaintIn3D._CwPaintableTextureEditor
    {
        //
    }
}
#endif