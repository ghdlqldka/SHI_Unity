// Copyright 2015 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework
{
    // [RequireComponent(typeof(Text))]
    public class _FPS : FPS_
    {
        private static string LOG_FORMAT = "<color=#997AC1><b>[_FPS]</b></color> {0}";

        [SerializeField]
        protected TMP_Text fpsText;

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            Debug.Assert(fpsText != null);
#if !DEBUG
            this.gameObject.SetActive(false);
#endif
        }

        protected override void LateUpdate()
        {
            float interp = Time.deltaTime / (0.5f + Time.deltaTime);
            float currentFPS = 1.0f / Time.deltaTime;
            fps = Mathf.Lerp(fps, currentFPS, interp);
            fpsText.text = Mathf.RoundToInt(fps) + "fps";
        }
    }
}