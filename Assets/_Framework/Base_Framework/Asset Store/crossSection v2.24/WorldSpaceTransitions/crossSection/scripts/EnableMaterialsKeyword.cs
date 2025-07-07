using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Enable/disable keyword in all materials on this GameObject.
/// Input from Toggle switch.
/// </summary>

namespace WorldSpaceTransitions
{
    public class EnableMaterialsKeyword : MonoBehaviour
    {
        public string kwd = "CLIP_PLANE";
        public Toggle my_Toggle;

        List<Material> mats;

        // Use this for initialization
        void Start()
        {
            Renderer[] rends = GetComponentsInChildren<Renderer>();
            mats = new List<Material>();
            foreach (Renderer r in rends) mats.AddRange(r.materials);
            foreach (Material m in mats) m.EnableKeyword(kwd);
            if (my_Toggle) my_Toggle.onValueChanged.AddListener(delegate { UpdateKeyword(my_Toggle.isOn); });
        }
        public void UpdateKeyword(bool val)
        {
            if (val)
            {
                foreach (Material m in mats) m.EnableKeyword(kwd);
            }
            else
            {
                foreach (Material m in mats) m.DisableKeyword(kwd);
            }
        }

    }
}
