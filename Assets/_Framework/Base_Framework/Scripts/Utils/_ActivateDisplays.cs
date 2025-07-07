using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://docs.unity3d.com/ScriptReference/Display-displays.html
// ===========> public static Display[] displays; <===========
// @@@@@ Description @@@@@
// The list of connected displays.

// In a built application, Unity populates this list when the application starts. It always contains at least one (main) display.
// In the Unity Editor, displays is not supported; displays.Length always has a value of 1, regardless of how many displays you have connected.

// https://docs.unity3d.com/Manual/MultiDisplay.html
// =====> Multi - display <=====
// You can use multi-display to display up to eight different Camera views of your application on up to eight different monitors at the same time.
// You can use this for setups such as PC games, arcade game machines, or public display installations.

// Unity supports multi-display on:
// - Desktop platforms (Windows, macOS X, and Linux)
// - Android(OpenGL ES and Vulkan)
// - iOS
// Some features work only on some platforms. See the Display, Screen and FullScreenMode APIs for more information about compatibility.

// @@@@@ Activating multi-display support @@@@@
// Unity¡¯s default display mode is one monitor only. When you run your application, you need use Display.Activate() to explicitly activate additional displays.
// Once you activate a display, you can¡¯t deactivate it.
//
// The best time to activate additional displays is when your application creates a new Scene.
// A good way to do this is to attach a script component to the default Camera.
// Make sure you call Display.Activate() only once during startup.
// As a best practice, you might find it helpful to create a small initial Scene to test your script.

namespace _Base_Framework
{
    public class _ActivateDisplays : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=#D1FF86><b>[_ActivateDisplays]</b></color> {0}";

        [TextArea]
        [ReadOnly]
        [SerializeField]
        public string description = "Display.displays[0] is the primary, default display and is always ON, so \"activateDisplayList\"start at index 1";

        [ReadOnly]
        [SerializeField]
        protected bool supportDisplay0 = true;
        [SerializeField]
        protected bool supportDisplay1;

        // Start is called before the first frame update
        protected virtual void Start()
        {
            // In a built application, Unity populates this list when the application starts. It always contains at least one (main) display.
            // In the Unity Editor, displays is not supported; displays.Length always has a value of 1, regardless of how many displays you have connected.
            Debug.LogWarningFormat(LOG_FORMAT, "displays connected : <b><color=red>" + Display.displays.Length + "</color></b>");

            // Display.displays[0] is the primary, default display and is always ON, so start at index 1.
            // Check if additional displays are available and activate each.



#if UNITY_EDITOR
            // You can use multi-display to display up to eight different Camera views of your application on up to eight different monitors at the same time.
            // You can use this for setups such as PC games, arcade game machines, or public display installations.
#else
            //

            /*
            if (supportDisplay0 == true)
            {
                Display.displays[0].Activate();
            }
            */

            if (supportDisplay1 == true)
            {
                Display.displays[1].Activate();
            }
#endif
        }
    }
}