/*
//  Copyright (c) 2015 José Guerreiro. All rights reserved.
//
//  MIT license, see http://www.opensource.org/licenses/mit-license.php
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
*/

using System.Collections;
using UnityEngine;

namespace cakeslice
{
	[RequireComponent(typeof(Renderer))]
	/* [ExecuteInEditMode] */
	public class _Outline : Outline
    {
        private static string LOG_FORMAT = "<color=#FFC300><b>[_Outline]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : " + this.gameObject.name);

            Renderer = GetComponent<Renderer>();
            SkinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            MeshFilter = GetComponent<MeshFilter>();
        }

        protected override void OnEnable()
        {
            // _OutlineManager.Instance.AddOutline(this);
            StartCoroutine(PostOnEnable());
        }

        protected virtual IEnumerator PostOnEnable()
        {
            while (_OutlineManager.Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "");
                yield return null;
            }
            _OutlineManager.Instance.AddOutline(this);
        }

        protected override void OnDisable()
        {
            _OutlineManager.Instance.RemoveOutline(this);
        }

    }
}
