using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class _Utilities : MonoBehaviour
{
    private const string LOG_FORMAT = "<color=#D1FF86><b>[_Utilities]</b></color> {0}";

    private static float lastCastTime = 0;
    private static List<RaycastResult> raycastResults = new List<RaycastResult>();

    public delegate void EndCallback();

    //return true if the on screen position passed is hovering on top of a UI element, false if otherwise
    public static bool IsCursorOnUI(Vector2 cursorPos)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        if (lastCastTime != Time.time)
        {
            lastCastTime = Time.time;

            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = cursorPos;
            EventSystem.current.RaycastAll(pointer, raycastResults);
        }

        return raycastResults.Count > 0 ? true : false;
    }

    //return the UI element current being hovered by the on screen position passed
    public static GameObject GetHoveredUIElement(Vector2 cursorPos)
    {
        if (EventSystem.current == null)
        {
            return null;
        }

        if (lastCastTime != Time.time)
        {
            lastCastTime = Time.time;

            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = cursorPos;
            EventSystem.current.RaycastAll(pointer, raycastResults);
        }

        return raycastResults.Count > 0 ? raycastResults[0].gameObject : null;
    }

    public static GameObject GetHovered2DObject(Vector2 cursorPos, Camera camera)
    {
        Debug.Assert(camera != null);

        if (camera == null)
        {
            return null;
        }

        Ray ray = camera.ScreenPointToRay(cursorPos);
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);

        return hit2D.collider != null ? hit2D.collider.gameObject : null;
    }

    public static GameObject GetHovered3DObject(Vector2 cursorPos, Camera camera)
    {
        Debug.LogFormat(LOG_FORMAT, "GetHovered3DObject()");
        Debug.Assert(camera != null);

        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(cursorPos);
        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.gameObject;
        }

        return null;
    }

    public static int ToLayer(LayerMask layerMask)
    {
        int result = layerMask > 0 ? 0 : 31;
        while (layerMask > 1)
        {
            layerMask = layerMask >> 1;
            result++;
        }

        return result;
    }

    public enum RenderPipelineType
    {
        Standard,
        URP,
        HDRP,
    }
    public static RenderPipelineType GetRenderPipelineType()
    {
        if (GraphicsSettings.defaultRenderPipeline != null)
        {
            string renderPipeline = GraphicsSettings.defaultRenderPipeline.GetType().ToString();
            if (renderPipeline.Contains("HighDefinition"))
            {
                return RenderPipelineType.HDRP;
            }
            else if (renderPipeline.Contains("Universal"))
            {
                return RenderPipelineType.URP;
            }
            else
            {
                Debug.LogWarningFormat(LOG_FORMAT, "<color=red>GraphicsSettings.defaultRenderPipeline : <b>" + renderPipeline + "</b></color>");
            }
        }
        else
        {
            // Debug.Assert(false);
            Debug.LogWarningFormat(LOG_FORMAT, "<color=red>GraphicsSettings.defaultRenderPipeline is <b>NULL</b></color>");
        }

        return RenderPipelineType.Standard;
    }
}