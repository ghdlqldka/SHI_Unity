using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CW.Common;
using PaintCore;
using System;

#if UNITY_EDITOR
namespace _Magenta_WebGL
{
	using UnityEditor;
	using TARGET = MWGL_CwPaintableMesh;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(MWGL_CwPaintableMesh))]
	public class MWGL_CwPaintableMeshEditor : PaintIn3D._CwPaintableMeshEditor
    {
        //
    }
}
#endif