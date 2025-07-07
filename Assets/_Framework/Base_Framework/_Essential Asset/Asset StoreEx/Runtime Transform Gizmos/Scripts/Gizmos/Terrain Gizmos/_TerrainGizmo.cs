using UnityEngine;
using System;
using System.Collections.Generic;

namespace RTG
{
    public class _TerrainGizmo : TerrainGizmo
    {
        // protected bool _isVisible = false;
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
        }

        public override void OnGizmoRender(Camera camera)
        {
            if (IsTargetReady() == false) 
                return;

            if (_RTGizmosEngine.Instance.NumRenderCameras > 1)
            {
                UpdateTicks();
            }

            if (_isVisible)
            {
                var wireMaterial = GizmoLineMaterial.Get;
                wireMaterial.ResetValuesToSensibleDefaults();
                wireMaterial.SetColor(LookAndFeel.RadiusCircleColor);
                wireMaterial.SetPass(0);

                float terrainY = _targetTerrain.transform.position.y;
                Vector3 circleCenter = Gizmo.Transform.Position3D;
                int numCirclePoints = _modelRadiusCirclePoints.Count;
                for (int ptIndex = 0; ptIndex < numCirclePoints; ++ptIndex)
                {
                    Vector3 ptPos = _modelRadiusCirclePoints[ptIndex] * _radius + circleCenter;
                    ptPos.y = terrainY + _targetTerrain.SampleHeight(_radiusCirclePoints[ptIndex]);
                    _radiusCirclePoints[ptIndex] = ptPos;
                }
                GLRenderer.DrawLines3D(_radiusCirclePoints);
            }

            _axisSlider.Render(camera);
            _midCap.Render(camera);

            _leftRadiusTick.Tick.Render(camera);
            _rightRadiusTick.Tick.Render(camera);
            _backRadiusTick.Tick.Render(camera);
            _forwardRadiusTick.Tick.Render(camera);
        }

        protected override void SetVisible(bool visible)
        {
            // Debug.Log("SetVisible(), visible : " + visible);

            _axisSlider.SetVisible(visible);
            _axisSlider.Set3DCapVisible(visible);
            _midCap.SetVisible(visible);

            _leftRadiusTick.Tick.SetVisible(visible);
            _rightRadiusTick.Tick.SetVisible(visible);
            _backRadiusTick.Tick.SetVisible(visible);
            _forwardRadiusTick.Tick.SetVisible(visible);

            _isVisible = visible;
        }

#if DEBUG
        public virtual void DEBUG_SetVisible(bool visible)
        {
            SetVisible(visible);
        }
#endif
    }
}