using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _InputTouches
{
    public class _MultiDisplayInputManager : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=#D1FF86><b>[_MultiDisplayInputManager]</b></color> {0}";

        protected static _MultiDisplayInputManager _instance = null;
        public static _MultiDisplayInputManager Instance
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

        public enum DisplayIndex
        {
            Display_None = -1,

            Display1 = 0,
            Display2 = 1,
            Display3 = 2,
            Display4 = 3,
        }

        [Header("Display Cameras")]
        [SerializeField]
        protected Camera disp1Camera;
        [SerializeField]
        protected Camera disp2Camera;
        [SerializeField]
        protected Camera disp3Camera;
        [SerializeField]
        protected Camera disp4Camera;

        // https://docs.unity3d.com/Manual/MultiDisplay.html
        protected virtual void Awake()
        {
            // In a built application, Unity populates this list when the application starts. It always contains at least one (main) display.
            // In the Unity Editor, displays is not supported; displays.Length always has a value of 1, regardless of how many displays you have connected.
            Debug.LogWarningFormat(LOG_FORMAT, "Awake(), Display.displays.Length : " + Display.displays.Length);

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        public delegate void MouseButtonHandler(int mouseButton, Vector3 pos, DisplayIndex display);
        public static event MouseButtonHandler OnMouseButtonDown;
        protected void Invoke_OnMouseButtonDown(int mouseButton, Vector3 pos, DisplayIndex display)
        {
            // Debug.LogFormat(LOG_FORMAT, "Invoke_OnMouseButtonDown(), mouseButton : " + ", display : " + display);

            if (OnMouseButtonDown != null)
            {
                OnMouseButtonDown(mouseButton, pos, display);
            }
        }

        public static event MouseButtonHandler OnMouseButtonUp;
        protected void Invoke_OnMouseButtonUp(int mouseButton, Vector3 pos, DisplayIndex display)
        {
            // Debug.LogFormat(LOG_FORMAT, "Invoke_OnMouseButtonUp(), mouseButton : <b>" + mouseButton + "</b>, display : <b><color=yellow>" + display + "</color></b>");

            if (OnMouseButtonUp != null)
            {
                OnMouseButtonUp(mouseButton, pos, display);
            }
        }

        public delegate void MultiTapHandler(Tap tap, DisplayIndex display);
        public static event MultiTapHandler OnMultiTap;
        protected void Invoke_OnMultiTap(Tap tap, DisplayIndex display)
        {
            if (OnMultiTap != null)
            {
                OnMultiTap(tap, display);
            }
        }

        public static event MultiTapHandler OnLongTap;
        public void Invoke_OnLongTap(Tap tap, DisplayIndex display)
        {
            if (OnLongTap != null)
            {
                OnLongTap(tap, display);
            }
        }

        public delegate void DraggingHandler(DragInfo dragInfo, DisplayIndex display);
        public static event DraggingHandler OnDraggingStart;
        protected void Invoke_OnDraggingStart(DragInfo dragInfo, DisplayIndex display)
        {
            if (OnDraggingStart != null)
            {
                OnDraggingStart(dragInfo, display);
            }
        }

        public static event DraggingHandler OnDragging;
        protected void Invoke_OnDragging(DragInfo dragInfo, DisplayIndex display)
        {
            if (OnDragging != null)
            {
                OnDragging(dragInfo, display);
            }
        }

        public static event DraggingHandler OnDraggingEnd;
        protected void Invoke_OnDraggingEnd(DragInfo dragInfo, DisplayIndex display)
        {
            if (OnDraggingEnd != null)
            {
                OnDraggingEnd(dragInfo, display);
            }
        }

        protected virtual void OnEnable()
        {
            _IT_Gesture.onMouseLeftButtonDownE += OnMouseLeftButtonDownE;
            _IT_Gesture.onMouseLeftButtonUpE += OnMouseLeftButtonUpE;
            _IT_Gesture.onMouseRightButtonDownE += OnMouseRightButtonDownE;
            _IT_Gesture.onMouseRightButtonUpE += OnMouseRightButtonUpE;
            _IT_Gesture.onMouseMiddleButtonDownE += OnMouseMiddleButtonDownE;
            _IT_Gesture.onMouseMiddleButtonUpE += OnMouseMiddleButtonUpE;

            _IT_Gesture.onMultiTapE += OnMultiTapE;
            _IT_Gesture.onLongTapE += OnLongTapE;

            // IT_GestureEx.onChargingE += OnCharging;
            // IT_GestureEx.onChargeEndE += OnChargeEnd;

            _IT_Gesture.onDraggingStartE += OnDraggingStartE;
            _IT_Gesture.onDraggingE += OnDraggingE;
            _IT_Gesture.onDraggingEndE += OnDraggingEndE;
        }

        protected virtual void OnDisable()
        {
            _IT_Gesture.onMouseLeftButtonDownE -= OnMouseLeftButtonDownE;
            _IT_Gesture.onMouseLeftButtonUpE -= OnMouseLeftButtonUpE;
            _IT_Gesture.onMouseRightButtonDownE -= OnMouseRightButtonDownE;
            _IT_Gesture.onMouseRightButtonUpE -= OnMouseRightButtonUpE;
            _IT_Gesture.onMouseMiddleButtonDownE -= OnMouseMiddleButtonDownE;
            _IT_Gesture.onMouseMiddleButtonUpE -= OnMouseMiddleButtonUpE;

            _IT_Gesture.onMultiTapE -= OnMultiTapE;
            _IT_Gesture.onLongTapE -= OnLongTapE;

            // IT_GestureEx.onChargingE -= OnCharging;
            // IT_GestureEx.onChargeEndE -= OnChargeEnd;

            _IT_Gesture.onDraggingStartE -= OnDraggingStartE;
            _IT_Gesture.onDraggingE -= OnDraggingE;
            _IT_Gesture.onDraggingEndE -= OnDraggingEndE;
        }

        public virtual Camera GetDiplayCamera(DisplayIndex display)
        {
            if (display == DisplayIndex.Display_None)
            {
                return null;
            }
            else if (display == DisplayIndex.Display1)
            {
                return disp1Camera;
            }
            else if (display == DisplayIndex.Display2)
            {
                return disp2Camera;
            }
            else if (display == DisplayIndex.Display3)
            {
                return disp3Camera;
            }
            else if (display == DisplayIndex.Display4)
            {
                return disp4Camera;
            }
            else
            {
                Debug.Assert(false);
            }

            return null;
        }

        protected virtual void OnMouseLeftButtonDownE(Vector3 pos)
        {
            // Debug.LogFormat(LOG_FORMAT, "On<b><color=yellow>MouseLeftButtonDownE</color></b>(), pos : " + pos);

            if (disp1Camera != null && _OnMouseButtonDownE(_IT_Gesture.Mouse_Left_Button, pos, DisplayIndex.Display1) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>1</color></b> HANDLE MouseLeftButtonDown!!!!!");
            }
            else if (disp2Camera != null && _OnMouseButtonDownE(_IT_Gesture.Mouse_Left_Button, pos, DisplayIndex.Display2) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>2</color></b> HANDLE MouseLeftButtonDown!!!!!");
            }
            else if (disp3Camera != null && _OnMouseButtonDownE(_IT_Gesture.Mouse_Left_Button, pos, DisplayIndex.Display3) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>3</color></b> HANDLE MouseLeftButtonDown!!!!!");
            }
            else if (disp4Camera != null && _OnMouseButtonDownE(_IT_Gesture.Mouse_Left_Button, pos, DisplayIndex.Display4) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>4</color></b> HANDLE MouseLeftButtonDown!!!!!");
            }
            else
            {
                // Debug.Assert(false);
            }
        }

        protected virtual void OnMouseLeftButtonUpE(Vector3 pos)
        {
            // Debug.LogFormat(LOG_FORMAT, "On<b><color=yellow>MouseLeftButtonUpE</color></b>(), pos : " + pos);

            if (disp1Camera != null && _OnMouseButtonUpE(_IT_Gesture.Mouse_Left_Button, pos, DisplayIndex.Display1) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>1</color></b> HANDLE MouseLeftButtonUp!!!!!");
            }
            else if (disp2Camera != null && _OnMouseButtonUpE(_IT_Gesture.Mouse_Left_Button, pos, DisplayIndex.Display2) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>2</color></b> HANDLE MouseLeftButtonUp!!!!!");
            }
            else if (disp3Camera != null && _OnMouseButtonUpE(_IT_Gesture.Mouse_Left_Button, pos, DisplayIndex.Display3) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>3</color></b> HANDLE MouseLeftButtonUp!!!!!");
            }
            else if (disp4Camera != null && _OnMouseButtonUpE(_IT_Gesture.Mouse_Left_Button, pos, DisplayIndex.Display4) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>4</color></b> HANDLE MouseLeftButtonUp!!!!!");
            }
            else
            {
                // Debug.Assert(false);
            }
        }

        protected virtual void OnMouseRightButtonDownE(Vector3 pos)
        {
            // Debug.LogFormat(LOG_FORMAT, "On<b><color=yellow>MouseRightButtonDownE</color></b>(), pos : " + pos);

            if (disp1Camera != null && _OnMouseButtonDownE(_IT_Gesture.Mouse_Right_Button, pos, DisplayIndex.Display1) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>1</color></b> HANDLE Mouse_Right_Button!!!!!");
            }
            else if (disp2Camera != null && _OnMouseButtonDownE(_IT_Gesture.Mouse_Right_Button, pos, DisplayIndex.Display2) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>2</color></b> HANDLE Mouse_Right_Button!!!!!");
            }
            else if (disp3Camera != null && _OnMouseButtonDownE(_IT_Gesture.Mouse_Right_Button, pos, DisplayIndex.Display3) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>3</color></b> HANDLE Mouse_Right_Button!!!!!");
            }
            else if (disp4Camera != null && _OnMouseButtonDownE(_IT_Gesture.Mouse_Right_Button, pos, DisplayIndex.Display4) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>4</color></b> HANDLE Mouse_Right_Button!!!!!");
            }
            else
            {
                // Debug.Assert(false);
            }
        }

        protected virtual void OnMouseMiddleButtonUpE(Vector3 pos)
        {
            // Debug.LogFormat(LOG_FORMAT, "On<b><color=yellow>MouseMiddleButtonUpE</color></b>(), pos : " + pos);

            if (disp1Camera != null && _OnMouseButtonUpE(_IT_Gesture.Mouse_Right_Button, pos, DisplayIndex.Display1) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>1</color></b> HANDLE Mouse_Right_Button!!!!!");
            }
            else if (disp2Camera != null && _OnMouseButtonUpE(_IT_Gesture.Mouse_Right_Button, pos, DisplayIndex.Display2) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>2</color></b> HANDLE Mouse_Right_Button!!!!!");
            }
            else if (disp3Camera != null && _OnMouseButtonUpE(_IT_Gesture.Mouse_Right_Button, pos, DisplayIndex.Display3) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>3</color></b> HANDLE Mouse_Right_Button!!!!!");
            }
            else if (disp4Camera != null && _OnMouseButtonUpE(_IT_Gesture.Mouse_Right_Button, pos, DisplayIndex.Display4) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>4</color></b> HANDLE Mouse_Right_Button!!!!!");
            }
            else
            {
                // Debug.Assert(false);
            }
        }

        protected virtual void OnMouseMiddleButtonDownE(Vector3 pos)
        {
            // Debug.LogFormat(LOG_FORMAT, "On<b><color=yellow>MouseMiddleButtonDown</color></b>(), pos : " + pos);

            if (disp1Camera != null && _OnMouseButtonDownE(_IT_Gesture.Mouse_Middle_Button, pos, DisplayIndex.Display1) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>1</color></b> HANDLE Mouse_Middle_Button!!!!!");
            }
            else if (disp2Camera != null && _OnMouseButtonDownE(_IT_Gesture.Mouse_Middle_Button, pos, DisplayIndex.Display2) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>2</color></b> HANDLE Mouse_Middle_Button!!!!!");
            }
            else if (disp3Camera != null && _OnMouseButtonDownE(_IT_Gesture.Mouse_Middle_Button, pos, DisplayIndex.Display3) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>3</color></b> HANDLE Mouse_Middle_Button!!!!!");
            }
            else if (disp4Camera != null && _OnMouseButtonDownE(_IT_Gesture.Mouse_Middle_Button, pos, DisplayIndex.Display4) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>4</color></b> HANDLE Mouse_Middle_Button!!!!!");
            }
            else
            {
                // Debug.Assert(false);
            }
        }

        protected virtual void OnMouseRightButtonUpE(Vector3 pos)
        {
            // Debug.LogFormat(LOG_FORMAT, "On<b><color=yellow>MouseRightButtonUpE</color></b>(), pos : " + pos);

            if (disp1Camera != null && _OnMouseButtonUpE(_IT_Gesture.Mouse_Middle_Button, pos, DisplayIndex.Display1) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>1</color></b> HANDLE Mouse_Middle_Button!!!!!");
            }
            else if (disp2Camera != null && _OnMouseButtonUpE(_IT_Gesture.Mouse_Middle_Button, pos, DisplayIndex.Display2) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>2</color></b> HANDLE Mouse_Middle_Button!!!!!");
            }
            else if (disp3Camera != null && _OnMouseButtonUpE(_IT_Gesture.Mouse_Middle_Button, pos, DisplayIndex.Display3) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>3</color></b> HANDLE Mouse_Middle_Button!!!!!");
            }
            else if (disp4Camera != null && _OnMouseButtonUpE(_IT_Gesture.Mouse_Middle_Button, pos, DisplayIndex.Display4) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>4</color></b> HANDLE Mouse_Middle_Button!!!!!");
            }
            else
            {
                // Debug.Assert(false);
            }
        }

        protected virtual void OnMultiTapE(Tap tap)
		{
            // Debug.LogFormat(LOG_FORMAT, "On<b><color=yellow>MultiTapE</color></b>(), tap.count : " + tap.count);

            if (disp1Camera != null && _OnMultiTapE(tap, DisplayIndex.Display1) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>1</color></b> HANDLE MultiTap!!!!!");
            }
            else if (disp2Camera != null && _OnMultiTapE(tap, DisplayIndex.Display2) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>2</color></b> HANDLE MultiTap!!!!!");
            }
            else if (disp3Camera != null && _OnMultiTapE(tap, DisplayIndex.Display3) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>3</color></b> HANDLE MultiTap!!!!!");
            }
            else if (disp4Camera != null && _OnMultiTapE(tap, DisplayIndex.Display4) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>4</color></b> HANDLE MultiTap!!!!!");
            }
            else
            {
                // Debug.Assert(false);
            }
        }

		protected virtual void OnLongTapE(Tap tap)
		{
            // Debug.LogFormat(LOG_FORMAT, "On<b><color=yellow>LongTap</color></b>()");

            if (disp1Camera != null && _OnLongTapE(tap, DisplayIndex.Display1) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>1</color></b> HANDLE LongTap!!!!!");
            }
            else if (disp2Camera != null && _OnLongTapE(tap, DisplayIndex.Display2) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>2</color></b> HANDLE LongTap!!!!!");
            }
            else if (disp3Camera != null && _OnLongTapE(tap, DisplayIndex.Display3) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>3</color></b> HANDLE LongTap!!!!!");
            }
            else if (disp4Camera != null && _OnLongTapE(tap, DisplayIndex.Display4) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>4</color></b> HANDLE LongTap!!!!!");
            }
            else
            {
                // Debug.Assert(false);
            }
        }

        /*
		protected virtual void OnCharging(ChargedInfo cInfo)
		{
            Debug.LogFormat(LOG_FORMAT, "OnCharging(), " + cInfo.percent);
        }

		protected virtual void OnChargeEnd(ChargedInfo cInfo)
		{
            Debug.LogFormat(LOG_FORMAT, "OnChargeEnd()");
        }
        */

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected DisplayIndex draggingStartDisplay = DisplayIndex.Display_None;
        protected virtual void OnDraggingStartE(DragInfo dragInfo)
		{
            // Debug.LogFormat(LOG_FORMAT, "On<b><color=yellow>DraggingStart</color></b>()");

            if (disp1Camera != null && _OnDraggingStartE(dragInfo, DisplayIndex.Display1) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>1</color></b> HANDLE DraggingStart!!!!!");
                draggingStartDisplay = DisplayIndex.Display1;
            }
            else if (disp2Camera != null && _OnDraggingStartE(dragInfo, DisplayIndex.Display2) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>2</color></b> HANDLE DraggingStart!!!!!");
                draggingStartDisplay = DisplayIndex.Display2;
            }
            else if (disp3Camera != null && _OnDraggingStartE(dragInfo, DisplayIndex.Display3) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>3</color></b> HANDLE DraggingStart!!!!!");
                draggingStartDisplay = DisplayIndex.Display3;
            }
            else if (disp4Camera != null && _OnDraggingStartE(dragInfo, DisplayIndex.Display4) == true)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Display<b><color=megenta>4</color></b> HANDLE DraggingStart!!!!!");
                draggingStartDisplay = DisplayIndex.Display4;
            }
            else
            {
                // Debug.Assert(false);
            }
        }

        protected virtual void OnDraggingE(DragInfo dragInfo)
		{
            // Debug.LogFormat(LOG_FORMAT, "OnDragging()");
            // Debug.AssertFormat(draggingStartDisplay != Display.Display_None, "draggingStartDisplay : " + draggingStartDisplay);

            Invoke_OnDragging(dragInfo, draggingStartDisplay);
        }

		protected virtual void OnDraggingEndE(DragInfo dragInfo)
		{
            // Debug.LogFormat(LOG_FORMAT, "On<b><color=yellow>DraggingEnd</color></b>()");
            // Debug.Assert(draggingStartDisplay != Display.Display_None, "draggingStartDisplay : " + draggingStartDisplay);

            Invoke_OnDraggingEnd(dragInfo, draggingStartDisplay);
            draggingStartDisplay = DisplayIndex.Display_None;
        }

        protected virtual bool _OnMouseButtonDownE(int mouseButton, Vector3 pos, DisplayIndex display)
        {
            Ray ray = disp1Camera.ScreenPointToRay(pos);
            if (display == DisplayIndex.Display1)
            {
                // ray = disp1Camera.ScreenPointToRay(pos);
            }
            else if (display == DisplayIndex.Display2)
            {
                ray = disp2Camera.ScreenPointToRay(pos);
            }
            else if (display == DisplayIndex.Display3)
            {
                ray = disp3Camera.ScreenPointToRay(pos);
            }
            else if (display == DisplayIndex.Display4)
            {
                ray = disp4Camera.ScreenPointToRay(pos);
            }
            else
            {
                Debug.Assert(false);
            }

            Debug.DrawRay(ray.origin, ray.direction, Color.blue, 3);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity) == true)
            {
                Invoke_OnMouseButtonDown(mouseButton, pos, display);
                return true;
            }

            return false;
        }

        protected virtual bool _OnMouseButtonUpE(int mouseButton, Vector3 pos, DisplayIndex display)
        {
            Ray ray = disp1Camera.ScreenPointToRay(pos);
            if (display == DisplayIndex.Display1)
            {
                ray = disp1Camera.ScreenPointToRay(pos);
            }
            else if (display == DisplayIndex.Display2)
            {
                ray = disp2Camera.ScreenPointToRay(pos);
            }
            else if (display == DisplayIndex.Display3)
            {
                ray = disp3Camera.ScreenPointToRay(pos);
            }
            else if (display == DisplayIndex.Display4)
            {
                ray = disp4Camera.ScreenPointToRay(pos);
            }
            else
            {
                Debug.Assert(false);
            }

            Debug.DrawRay(ray.origin, ray.direction, Color.blue, 3);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity) == true)
            {
                Invoke_OnMouseButtonUp(mouseButton, pos, display);
                return true;
            }

            return false;
        }

        protected virtual bool _OnMultiTapE(Tap tap, DisplayIndex display)
        {
            // Debug.LogFormat(LOG_FORMAT, "_On<b><color=yellow>MultiTapE</color></b>(), tap.count : " + tap.count);

            Ray ray = disp1Camera.ScreenPointToRay(tap.pos);
            if (display == DisplayIndex.Display1)
            {
                ray = disp1Camera.ScreenPointToRay(tap.pos);
            }
            else if (display == DisplayIndex.Display2)
            {
                ray = disp2Camera.ScreenPointToRay(tap.pos);
            }
            else if (display == DisplayIndex.Display3)
            {
                ray = disp3Camera.ScreenPointToRay(tap.pos);
            }
            else if (display == DisplayIndex.Display4)
            {
                ray = disp4Camera.ScreenPointToRay(tap.pos);
            }
            else
            {
                Debug.Assert(false);
            }

            Debug.DrawRay(ray.origin, ray.direction, Color.blue, 3);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Invoke_OnMultiTap(tap, display);
                return true;
            }

            return false;
        }

        protected virtual bool _OnLongTapE(Tap tap, DisplayIndex display)
        {
            // Debug.LogFormat(LOG_FORMAT, "_On<b><color=yellow>LongTapE</color></b>(), tap.count : " + tap.count + ", display : " + display);

            Ray ray = disp1Camera.ScreenPointToRay(tap.pos);
            if (display == DisplayIndex.Display1)
            {
                ray = disp1Camera.ScreenPointToRay(tap.pos);
            }
            else if (display == DisplayIndex.Display2)
            {
                ray = disp2Camera.ScreenPointToRay(tap.pos);
            }
            else if (display == DisplayIndex.Display3)
            {
                ray = disp3Camera.ScreenPointToRay(tap.pos);
            }
            else if (display == DisplayIndex.Display4)
            {
                ray = disp4Camera.ScreenPointToRay(tap.pos);
            }
            else
            {
                Debug.Assert(false);
            }

            Debug.DrawRay(ray.origin, ray.direction, Color.blue, 3);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Invoke_OnLongTap(tap, display);
                return true;
            }

            return false;
        }

        protected virtual bool _OnDraggingStartE(DragInfo dragInfo, DisplayIndex display)
        {
            // Debug.LogFormat(LOG_FORMAT, "On<b><color=yellow>MultiTap</color></b>(), tap.count : " + tap.count);

            Ray ray = disp1Camera.ScreenPointToRay(dragInfo.pos);
            if (display == DisplayIndex.Display1)
            {
                // ray = disp1Camera.ScreenPointToRay(dragInfo.pos);
            }
            else if (display == DisplayIndex.Display2)
            {
                ray = disp2Camera.ScreenPointToRay(dragInfo.pos);
            }
            else if (display == DisplayIndex.Display3)
            {
                ray = disp3Camera.ScreenPointToRay(dragInfo.pos);
            }
            else if (display == DisplayIndex.Display4)
            {
                ray = disp4Camera.ScreenPointToRay(dragInfo.pos);
            }
            else
            {
                Debug.Assert(false);
            }

            RaycastHit hit;
            //use raycast at the cursor position to detect the object
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Invoke_OnDraggingStart(dragInfo, display);
                return true;
            }

            return false;
        }

    }
}