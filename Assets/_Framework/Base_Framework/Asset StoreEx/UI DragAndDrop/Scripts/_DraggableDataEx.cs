using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Base_Framework
{
    [CreateAssetMenu(fileName = "_DraggableDataEx", menuName = "_DraggableDataEx")]
    public class _DraggableDataEx : ScriptableObject
    {
        public int id;

        [Space(10)]
        public Sprite icon;
        public Sprite silhouetteIcon;
    }
}