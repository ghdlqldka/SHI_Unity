using UnityEngine;
using UnityEngine.EventSystems;

namespace __BioIK
{
	public class _MouseDrag : MouseDrag
    {
        private static string LOG_FORMAT = "<color=#8DA278><b>[_MouseDrag]</b></color> {0}";

        [ReadOnly]
        [SerializeField]
        protected Camera _camera;

        protected override void Awake()
		{
			Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
		}

        protected override void Start()
        {
            _camera = _BioIKCamera._Camera;
            LastMousePosition = GetNormalizedMousePosition();
        }

        protected override void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                Translate = true;
                Rotate = false;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                Translate = false;
                Rotate = true;
            }
            LastMousePosition = GetNormalizedMousePosition();
        }

        protected override void OnMouseDrag()
        {
            if (InformEventSystem == true)
            {
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(this.gameObject);
                }
            }

            if (Translate == true)
            {
                // float distance_to_screen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
                // transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
                float distance_to_screen = _camera.WorldToScreenPoint(this.gameObject.transform.position).z;
                this.transform.position = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
            }

            if (Rotate == true)
            {
                Vector2 deltaMousePosition = GetNormalizedDeltaMousePosition();
                // transform.Rotate(Camera.main.transform.right, 1000f * Sensitivity * Time.deltaTime * deltaMousePosition.y, Space.World);
                // transform.Rotate(Camera.main.transform.up, -1000f * Sensitivity * Time.deltaTime * deltaMousePosition.x, Space.World);
                this.transform.Rotate(_camera.transform.right, 1000f * Sensitivity * Time.deltaTime * deltaMousePosition.y, Space.World);
                this.transform.Rotate(_camera.transform.up, -1000f * Sensitivity * Time.deltaTime * deltaMousePosition.x, Space.World);
            }
        }

        protected override Vector2 GetNormalizedMousePosition()
        {
            // Vector2 ViewPortPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            Vector2 ViewPortPosition = _camera.ScreenToViewportPoint(Input.mousePosition);
            return new Vector2(ViewPortPosition.x, ViewPortPosition.y);
        }
    }
}