using System;
using UnityEngine;
using UnityStandardAssets.Utility;

namespace UnityStandardAssets.Characters.FirstPerson
{
    public class _HeadBob : HeadBob
    {
        private static string LOG_FORMAT = "<color=green><b>[_HeadBob]</b></color> {0}";

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");
        }
    }
}
