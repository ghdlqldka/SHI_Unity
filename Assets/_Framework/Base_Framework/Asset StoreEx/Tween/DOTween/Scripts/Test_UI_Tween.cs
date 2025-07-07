using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework
{
    public class Test_UI_Tween : MonoBehaviour
    {
        public Camera _camera;
        public Image image;

        // Start is called before the first frame update
        void Start()
        {
            //

        }

        // Update is called once per frame
        protected virtual void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                image.TweenAlpha(0.5f, 5);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                this.transform.TweenRotation(new Vector3(180, 0, 0), 5.0f);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _camera.TweenFieldOfView(10, 5f);
            }
#endif
        }
    }
}