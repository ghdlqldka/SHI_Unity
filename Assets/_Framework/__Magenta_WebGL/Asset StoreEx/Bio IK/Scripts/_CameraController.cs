using UnityEngine;

namespace __BioIK
{
	public class _CameraController : CameraController
    {
        protected override void Awake()
        {
            ZeroRotation = this.transform.rotation;
        }

        protected override Vector2 GetNormalizedMousePosition()
        {
            // Vector2 ViewPortPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            Vector2 ViewPortPosition = _BioIKCamera._Camera.ScreenToViewportPoint(Input.mousePosition);
            return new Vector2(ViewPortPosition.x, ViewPortPosition.y);
        }
    }
}