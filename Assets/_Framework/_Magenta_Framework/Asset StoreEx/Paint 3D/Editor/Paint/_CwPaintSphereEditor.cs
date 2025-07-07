using UnityEngine;
using System.Collections.Generic;
using CW.Common;
using PaintCore;

#if UNITY_EDITOR
namespace PaintIn3D
{
    using UnityEditor;
    // using TARGET = _CwPaintSphere;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(_CwPaintSphere))]
    public class _CwPaintSphereEditor : CwClickToPaintSphere_Editor
    {
        protected SerializedProperty m_Script;

        // 
        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
        }

        protected override void OnInspector()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            _CwPaintSphere tgt;
            _CwPaintSphere[] tgts; 
            GetTargets(out tgt, out tgts);

            BeginError(Any(tgts, t => t.Layers == 0 && t.TargetModel == null));
            Draw("layers", "Only the CwPaintable___ GameObjects whose layers are within this mask will be eligible for painting.");
            EndError();
            Draw("group", "Only the CwPaintableTexture components with a matching group will be painted by this component.");

            Separator();

            Draw("blendMode", "This allows you to choose how the paint from this component will combine with the existing pixels of the textures you paint.\n\nNOTE: See the Blend Mode section of the documentation for more information.");
            Draw("color", "The color of the paint.");
            Draw("opacity", "The opacity of the brush.");

            Separator();

            Draw("angle", "The angle of the paint in degrees.\n\nNOTE: This is only useful if you change the Scale.x/y values.");
            Draw("scale", "By default this component paints using a sphere shape, but you can override this here to paint an ellipsoid.\n\nNOTE: When painting an ellipsoid, the orientation of the sphere matters. This can be controlled from the CwHit__ component settings.");
            Draw("radius", "The radius of the paint brush.");
            Draw("hardness", "This allows you to control the sharpness of the near+far depth cut-off point.");

            Separator();

            if (DrawFoldout("Advanced", "Show advanced settings?") == true)
            {
                BeginIndent();
                Draw("targetModel", "If this is set, then only the specified model will be painted, regardless of the layer setting.");
                Draw("targetTexture", "If this is set, then only the specified CwPaintableTexture will be painted, regardless of the layer or group setting.");
                Draw("findMask", "If your scene contains a <b>CwMask</b>, should this paint component use it?");
                Draw("findDepthMask", "If your scene contains a <b>CwRenderDepth</b>, should this paint component use it?");

                Separator();

                Draw("tileTexture", "This allows you to apply a tiled detail texture to your decals. This tiling will be applied in world space using triplanar mapping.");
                Draw("tileTransform", "This allows you to adjust the tiling position + rotation + scale using a Transform.");
                Draw("tileOpacity", "This allows you to control the triplanar influence.\n\n0 = No influence.\n\n1 = Full influence.");
                Draw("tileTransition", "This allows you to control how quickly the triplanar mapping transitions between the X/Y/Z planes.");
                EndIndent();
            }

            Separator();

            tgt.Modifiers.DrawEditorLayout(serializedObject, target, "Color", "Opacity", "Radius", "Scale", "Hardness", "Position");
        }
    }
}
#endif