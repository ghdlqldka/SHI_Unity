using UnityEngine;
using PaintCore;
using System.Collections.Generic;


#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(_CwPaintableTexture))]
	public class _CwPaintableTextureEditor : CwPaintableMeshTexture_Editor
    {
        protected SerializedProperty m_Script;

#if DEBUG
        // 
        protected SerializedProperty DEBUG_texture;
        protected SerializedProperty DEBUG_current;
        protected SerializedProperty DEBUG_preview;
#endif

        protected virtual void OnEnable()
		{
            m_Script = serializedObject.FindProperty("m_Script");

#if DEBUG
            DEBUG_texture = serializedObject.FindProperty("DEBUG_texture");
            DEBUG_current = serializedObject.FindProperty("DEBUG_current");
            DEBUG_preview = serializedObject.FindProperty("DEBUG_preview");
#endif
        }

        protected override void OnInspector()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            _CwPaintableTexture tgt; 
            _CwPaintableTexture[] tgts; 
            GetTargets(out tgt, out tgts);

            var overlapSlot = false;
            var overlapGroup = false;

            TestOverlap(tgts, ref overlapSlot, ref overlapGroup);

            if (Any(tgts, t => t.Activated == true))
            {
                Info("This component has been activated.");
            }

            if (Any(tgts, t => t.Activated == true && Application.isPlaying == false))
            {
                Error("This component shouldn't be activated during edit mode. Deactivate it from the component context menu.");
            }

            if (Any(tgts, t => t.Activated == false && t.Model != null && t.Model.IsActivated == true))
            {
                Warning("This component isn't activated, but the CwPaintable has been, so you must manually activate this.");
            }

            var slotTitle = Any(tgts, t => t.IsDummy) == true ? "Slot (DUMMY)" : "Slot";
            var showTexture = DrawExpand("slot", "The material index and shader texture slot name that this component will paint.", slotTitle);
            if (SlotHasTilingOrOffset(tgts) == true)
            {
                Warning("This slot uses Tiling and/or Offset values, which are not supported.");
            }
            if (overlapSlot == true)
            {
                Error("This slot is used by multiple CwPaintableTexture components.");
            }
            if (showTexture == true)
            {
                BeginIndent();
                BeginDisabled();
                EditorGUI.ObjectField(Reserve(18), new GUIContent("Texture", "This is the current texture in the specified texture slot."), tgt.Slot.FindTexture(tgt.gameObject), typeof(Texture), false);
                EndDisabled();
                EndIndent();
            }
            Draw("coord", "The UV channel this texture is mapped to.");

            Separator();

            Draw("group", "The group you want to associate this texture with. Only painting components with a matching group can paint this texture. This allows you to paint multiple textures at the same time with different settings (e.g. Albedo + Normal).");
            if (overlapGroup == true)
            {
                Error("This group is used by multiple CwPaintableTexture components.");
            }
            Draw("undoRedo", "This allows you to set how this texture's state is stored, allowing you to perform undo and redo operations.\n\nFullTextureCopy = A full copy of your texture will be copied for each state. This allows you to quickly undo and redo, and works with animated skinned meshes, but it uses up a lot of texture memory.\n\nLocalCommandCopy = Each paint command will be stored in local space for each state. This allows you to perform unlimited undo and redo states with minimal memory usage, because the object will be repainted from scratch. However, performance will depend on how many states must be redrawn, and it may not work well with skinned meshes.");
            if (Any(tgts, t => t.UndoRedo == CwPaintableTexture.UndoRedoType.FullTextureCopy))
            {
                BeginIndent();
                Draw("stateLimit", "The amount of times this texture can have its paint operations undone.");
                EndIndent();
            }
            DrawSaveName(tgts);

            Separator();

            DrawSize(tgts);
            _DrawTexture(tgts);
            Draw("color", "When activated or cleared, this paintable texture will be given this color.\n\nNOTE: If Texture is set, then each pixel RGBA value will be multiplied/tinted by this color.");

            Separator();

            if (DrawFoldout("Advanced", "Show advanced settings?") == true)
            {
                BeginIndent();
                DrawAdvanced();
                EndIndent();
            }

            if (Any(tgts, t => t.Conversion != CwPaintableTexture.ConversionType.Normal && (t.Slot.Name.Contains("Bump") == true || t.Slot.Name.Contains("Normal") == true)))
            {
                Warning("If you're painting a normal map, then you should enable the Normal setting in the advanced menu.");
            }

#if DEBUG
            EditorGUILayout.PropertyField(DEBUG_texture);
            EditorGUILayout.PropertyField(DEBUG_current);
            EditorGUILayout.PropertyField(DEBUG_preview);
#endif
        }

        protected void _DrawTexture(_CwPaintableTexture[] tgts)
        {
            Rect rect = Reserve();
            Rect rectL = rect; 
            rectL.xMax -= 50;
            Rect rectR = rect; 
            rectR.xMin = rectR.xMax - 48;

            EditorGUI.PropertyField(rectL, serializedObject.FindProperty("texture"), new GUIContent("Texture", "When activated or cleared, this paintable texture will be given this texture, and then multiplied/tinted by the Color.\n\nNone = White."));

            BeginDisabled(All(tgts, CannotCopyTexture));
            if (GUI.Button(rectR, new GUIContent("Copy", "Copy the texture from the current slot?"), EditorStyles.miniButton) == true)
            {
                Undo.RecordObjects(targets, "Copy Textures");

                Each(tgts, t => t.CopyTexture(), true);
            }
            EndDisabled();
        }
    }
}
#endif