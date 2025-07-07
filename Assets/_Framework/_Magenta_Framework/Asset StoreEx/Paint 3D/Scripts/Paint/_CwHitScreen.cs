using UnityEngine;
using System.Collections.Generic;
using CW.Common;
using PaintCore;
using System.Collections;

namespace PaintIn3D
{
	/// <summary>This component will perform a raycast under the mouse or finger as it moves across the screen. It will then send hit events to components like <b>CwPaintDecal</b>, allowing you to paint the scene.</summary>
	// [HelpURL(CwCommon.HelpUrlPrefix + "CwHitScreen")]
	// [AddComponentMenu(PaintCore.CwCommon.ComponentHitMenuPrefix + "Hit Screen")]
	public class _CwHitScreen : CwHitScreen
    {
        private static string LOG_FORMAT = "<color=#FFF4D6><b>[_CwHitScreen]</b></color> {0}";

        public new _CwPointConnector Connector
        {
            get
            {
                // Debug.LogFormat(LOG_FORMAT, "0. connector : " + connector);
                if (connector == null)
                {
                    connector = new _CwPointConnector();
                    // Debug.LogFormat(LOG_FORMAT, "1. connector : " + connector);
                }
                // if (connector is CwPointConnector)
                if (connector.GetType() == typeof(CwPointConnector))
                {
                    connector = new _CwPointConnector(connector);
                    Debug.LogFormat(LOG_FORMAT, "2. connector : " + connector);
                }

                // Debug.LogFormat(LOG_FORMAT, "3. connector : " + connector);
                return connector as _CwPointConnector;
            }
        }

        // public Camera Camera { set { _camera = value; } get { return _camera; } }
        protected Camera _Camera 
        { 
            set 
            { 
                _camera = value;
            } 
            get 
            { 
                return _camera;
            }
        }

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + 
                "</b>, Emit : <b><color=yellow>" + Emit + "</color></b>");

            Debug.Assert(_Camera != null);

            StartCoroutine(PostAwake());
        }

        protected IEnumerator PostAwake()
        {
            while (_CwPaintableManager.Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "");
                yield return null;
            }

            _CwPaintableManager.Instance._Camera = _Camera;
        }

        protected override void OnEnable()
        {
            Debug.LogFormat(LOG_FORMAT, "OnEnable()");

            foreach (Link link in links)
            {
                link.Clear();
            }

            Connector.ResetConnections();

            if (ShouldUpgradePointers() == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Upgrading CwHitScreen Controls - To remove this warning you can manually click the \"Upgrade\" button in the inspector of this component while outside of play mode.", gameObject);

                TryUpgradePointers();
            }
        }

        protected override void Update()
        {
            Connector.Update();
        }

        public override void HandleFingerUpdate(CwInputManager.Finger finger, bool down, bool up)
        {
            Link link = CwInputManager.Link.Find(links, finger);
            var set = true;

            if (finger.Index < 0) // Preview?
            {
                if (CwInputManager.PointOverGui(finger.ScreenPosition, GuiLayers) == true)
                {
                    Connector.BreakHits(link);

                    return;
                }
            }
            else
            {
                if (down == true)
                {
                    if (CwInputManager.PointOverGui(finger.ScreenPosition, GuiLayers) == true)
                    {
                        Connector.BreakHits(link);

                        return;
                    }
                }
                else
                {
                    if (link == null)
                    {
                        return;
                    }
                }
            }

            if (link == null)
            {
                link = CwInputManager.Link.Create(ref links, finger);
            }

            link.Move(finger.ScreenPosition);

            if (finger.Index < 0) // Preview?
            {
                RecordAndPaintAt(link, finger.ScreenPosition, link.ScreenOld, true, 0.0f, link);

                return;
            }

            if (NeedsDrawAngle == true)
            {
                down = link.State == 2;
                set = link.State >= 2;
            }

            if (set == true)
            {
                switch (Frequency)
                {
                    case FrequencyType.PixelInterval:
                        PaintSmooth(link, down, Interval);
                        break;
                    case FrequencyType.ScaledPixelInterval: 
                        PaintSmooth(link, down, Interval / CwInputManager.ScaleFactor);
                        break;
                    case FrequencyType.TimeInterval:
                        PaintInterval(link, down);
                        break;
                    case FrequencyType.OnceOnRelease: 
                        PaintRelease(link, up); 
                        break;
                    case FrequencyType.OnceOnPress:
                        PaintPress(link, down);
                        break;
                    case FrequencyType.OnceEveryFrame: 
                        PaintEvery(link, down); 
                        break;
                }
            }

            // base.HandleFingerUpdate(finger, down, up);
            if (up == true)
            {
                HandleFingerUp(finger);
            }

            _CwPaintableManager.MarkActivelyPainting();
        }

        protected override void PaintAt(CwPointConnector _c, CwHitCache hitCache, Vector2 screenPosition, 
            Vector2 screenPositionOld, bool preview, float pressure, object owner)
        {
            // base.PaintAt(_c, hitCache, screenPosition, screenPositionOld, preview, pressure, owner);

            _CwPointConnector _connector = _c as _CwPointConnector;

            var camera = default(Camera);
            var ray = default(Ray);
            var hit2D = default(RaycastHit2D);
            var hit3D = default(CwHit);
            var finalPosition = default(Vector3);
            var finalRotation = default(Quaternion);

            DoQuery(screenPosition, ref camera, ref ray, ref hit3D, ref hit2D);

            bool valid2D = hit2D.distance > 0.0f;
            bool valid3D = hit3D.Distance > 0.0f;

            // Hit 3D?
            if (valid3D == true && (valid2D == false || hit3D.Distance < hit2D.distance))
            {
                CalcHitData(hit3D.Position, hit3D.Normal, ray, camera, screenPositionOld, ref finalPosition, ref finalRotation);

                if (Emit == EmitType.PointsIn3D)
                {
                    if (_connector != null)
                    {
                        _connector.SubmitPoint(this.gameObject, preview, Priority, pressure, finalPosition, finalRotation, owner);
                    }
                    else
                    {
                        hitCache.InvokePoint(this.gameObject, preview, Priority, pressure, finalPosition, finalRotation);
                    }

                    return;
                }
                else if (Emit == EmitType.PointsOnUV)
                {
                    hitCache.InvokeCoord(this.gameObject, preview, Priority, pressure, hit3D, finalRotation);

                    return;
                }
                else if (Emit == EmitType.TrianglesIn3D)
                {
                    hitCache.InvokeTriangle(this.gameObject, preview, Priority, pressure, hit3D, finalRotation);

                    return;
                }
            }
            // Hit 2D?
            else if (valid2D == true)
            {
                CalcHitData(hit2D.point, new Vector3(0.0f, 0.0f, -1.0f), ray, camera, screenPositionOld, ref finalPosition, ref finalRotation);

                if (Emit == EmitType.PointsIn3D)
                {
                    if (_connector != null)
                    {
                        _connector.SubmitPoint(gameObject, preview, Priority, pressure, finalPosition, finalRotation, owner);
                    }
                    else
                    {
                        hitCache.InvokePoint(gameObject, preview, Priority, pressure, finalPosition, finalRotation);
                    }

                    return;
                }
            }

            if (_connector != null)
            {
                _connector.BreakHits(owner);
            }
        }

        protected override void DoQuery(Vector2 screenPosition, ref Camera camera, ref Ray ray, ref CwHit hit3D, ref RaycastHit2D hit2D)
        {
            var hit = default(RaycastHit);

            camera = CwHelper.GetCamera(_Camera);
            ray = camera.ScreenPointToRay(screenPosition);
            hit2D = Physics2D.GetRayIntersection(ray, float.PositiveInfinity, Layers);

            Physics.Raycast(ray, out hit, float.PositiveInfinity, Layers);

            hit3D = new CwHit(hit);
        }
    }
}

#if false //
#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = CwHitScreen;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwHitScreen_Editor : CwHitScreenBase_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			base.OnInspector();

			Separator();

			DrawBasic();

			Separator();

			DrawAdvancedFoldout();

			Separator();

			var point    = tgt.Emit == CwHitScreenBase.EmitType.PointsIn3D;
			var line     = tgt.Emit == CwHitScreenBase.EmitType.PointsIn3D && tgt.Connector.ConnectHits == true;
			var triangle = tgt.Emit == CwHitScreenBase.EmitType.TrianglesIn3D;
			var coord    = tgt.Emit == CwHitScreenBase.EmitType.PointsOnUV;

			tgt.Connector.HitCache.Inspector(tgt.gameObject, point: point, line: line, triangle: triangle, coord: coord);
		}

		protected override void DrawBasic()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			base.DrawBasic();

			Draw("frequency", "This allows you to control how often the screen is painted.\n\nPixelInterval = Once every Interval pixels.\n\nScaledPixelInterval = Once every Interval scaled pixels.\n\nTimeInterval = Once every Interval seconds.\n\nOnceOnRelease = When the finger/mouse goes down a preview will be shown, and when it goes up the paint will apply.\n\nOnceOnPress = When the finger/mouse goes down the paint will apply.\n\nOnceEveryFrame = Every frame the paint will apply.");
			if (Any(tgts, t => t.Frequency == CwHitScreen.FrequencyType.PixelInterval || t.Frequency == CwHitScreen.FrequencyType.ScaledPixelInterval || t.Frequency == CwHitScreen.FrequencyType.TimeInterval))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.Interval <= 0.0f));
						Draw("interval", "This allows you to set the pixels/seconds between each hit point based on the current Frequency setting.");
					EndError();
				EndIndent();
			}
		}

		protected override void DrawAdvanced()
		{
			base.DrawAdvanced();

			Separator();

			CwPointConnector_Editor.Draw();
		}
	}
}
#endif
#endif