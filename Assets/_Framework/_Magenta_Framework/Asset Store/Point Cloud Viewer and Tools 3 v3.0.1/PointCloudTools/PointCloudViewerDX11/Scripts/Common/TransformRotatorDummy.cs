using UnityEngine;

namespace pointcloudviewer.examples
{
    public class TransformRotatorDummy : MonoBehaviour
    {
        void Update()
        {
            transform.Rotate(0, 0, -1000 * Time.deltaTime);
        }
    }
}