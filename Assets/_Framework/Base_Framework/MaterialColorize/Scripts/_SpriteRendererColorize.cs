using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Base_Framework
{
    public class _SpriteRendererColorize : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#F138A8><b>[_SpriteRendererColorize]</b></color> {0}";

        [SerializeField]
        protected Texture2D _texture;
        public Texture2D _Texture
        {
            get
            {
                return _texture;
            }
            set
            {
                Debug.LogWarningFormat(LOG_FORMAT, "<b><color=red>Texture Changed!!!!!!!!!!!!</color></b>");

                _texture = value;
                for (int i = 0; i < _rendererList.Count; i++)
                {
                    _rendererList[i].material.mainTexture = value; // MainMaps/Albedo
                    _rendererList[i].material.SetTexture("_EmissionMap", value); // Properties/_EmissionMap
                }
            }
        }

        [Header("Colorize SpriteRenderers")]
        [SerializeField]
        protected List<SpriteRenderer> _rendererList;

        protected virtual void Awake()
        {
            //
        }

        // Update is called once per frame
        protected virtual void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _Texture = _texture;
            }
#endif
        }
    }
}