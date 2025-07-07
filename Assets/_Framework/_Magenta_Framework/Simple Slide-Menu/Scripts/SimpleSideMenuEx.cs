// Simple Side-Menu - https://assetstore.unity.com/packages/tools/gui/simple-side-menu-143623
// Copyright (c) Daniel Lochner

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Magenta_Framework
{
    [AddComponentMenu("UI/Simple Side-Menu Ex")]
    public class SimpleSideMenuEx : DanielLochner.Assets.SimpleSideMenu._SimpleSideMenu
    {
        private static string LOG_FORMAT = "<color=#B1C5EF><b>[SimpleSideMenuEx]</b></color> {0}";

        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            // Initialize();
        }
#endif

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            // base.Awake();
            Initialize();
        }

        protected override void Update()
        {
            HandleState();
            HandleOverlay();
        }

    }
}