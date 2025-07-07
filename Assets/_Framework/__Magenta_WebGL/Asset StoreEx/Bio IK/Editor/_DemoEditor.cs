using UnityEngine;
using UnityEditor;
using __BioIK;

namespace BioIK 
{
    [CustomEditor(typeof(_Demo))]
    public class _DemoEditor : DemoEditor
    {

        public _Demo _Target
        {
            get 
            { 
                return Target as _Demo;
            }
        }

        protected override void Awake()
        {
            Target = (_Demo)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        protected override void SetThreading(bool value)
        {
            // In Unity WebGL, multiple threads cannot be used by default.
            /*
            for (int i = 0; i < _Target.Models.Length; i++)
            {
                _Target.Models[i].SetThreading(value);
            }
            */
        }
    }
}
