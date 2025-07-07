using UnityEngine;
using UnityEngine.EventSystems;

// namespace UnityEngine.EventSystems
namespace _Magenta_Framework
{
    [AddComponentMenu("Event/Event System(Magenta_Framework)")]
    [DisallowMultipleComponent]
    /// <summary>
    /// Handles input, raycasting, and sending events.
    /// </summary>
    /// <remarks>
    /// The EventSystem is responsible for processing and handling events in a Unity scene. A scene should only contain one EventSystem. The EventSystem works in conjunction with a number of modules and mostly just holds state and delegates functionality to specific, overrideable components.
    /// When the EventSystem is started it searches for any BaseInputModules attached to the same GameObject and adds them to an internal list. On update each attached module receives an UpdateModules call, where the module can modify internal state. After each module has been Updated the active module has the Process call executed.This is where custom module processing can take place.
    /// </remarks>
    public class EventSystemEx : _Base_Framework._EventSystem
    {
        private static string LOG_FORMAT = "<color=#FF7F00><b>[EventSystemEx]</b></color> {0}";

        public static new EventSystemEx _current
        {
            get
            {
                return current as EventSystemEx;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            // base.Awake();
        }

        public static new bool IsPointerOverGameObject()
        {
            // base.IsPointerOverGameObject();

            if (_current != null)
            {
                // Check mouse
                if (((EventSystem)_current).IsPointerOverGameObject() == true)
                {
#if DEBUG
                    _current.DEBUG_isPointerOverGameObject = true;
#endif
                    return true;
                }

                // Check touches
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began)
                    {
                        if (_current.IsPointerOverGameObject(touch.fingerId) == true)
                        {
#if DEBUG
                            _current.DEBUG_isPointerOverGameObject = true;
#endif
                            return true;
                        }
                    }
                }

#if DEBUG
                _current.DEBUG_isPointerOverGameObject = false;
#endif
            }

            return false;
        }
    }
}
