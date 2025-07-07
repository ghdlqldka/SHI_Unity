using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace UnityStandardAssets.CrossPlatformInput
{
    [ExecuteInEditMode]
    public class _MobileControlRig : MobileControlRig
    {
#if UNITY_EDITOR

        protected override void OnEnable()
        {
            EditorApplication.update += OnUpdate;
        }


        protected override void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
        }


        protected virtual void OnUpdate()
        {
            CheckEnableControlRig();
        }
#endif
    }
}