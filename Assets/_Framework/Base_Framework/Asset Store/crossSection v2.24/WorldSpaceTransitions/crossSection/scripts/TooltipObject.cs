using UnityEngine;
using System.Collections;

namespace WorldSpaceTransitions.Examples
{
    public class TooltipObject : MonoBehaviour
    {

        void OnMouseEnter()
        {
            ToolTipManager.SetCurrent(gameObject);
            Debug.Log(name);
        }
        void OnMouseExit()
        {
            ToolTipManager.SetCurrent(null);
        }
    }
}