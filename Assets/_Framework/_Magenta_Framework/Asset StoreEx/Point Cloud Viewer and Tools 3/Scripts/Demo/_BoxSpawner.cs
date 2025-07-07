// sample code for spawning cubes

using PointCloudViewer;
using UnityEngine;
namespace pointcloudviewer.examples
{
    public class _BoxSpawner : BoxSpawner
    {
        protected override void Start()
        {
            // cam = Camera.main;
            cam = null; // Not used!!!!
        }

        protected override void LateUpdate()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // var rb = Instantiate(prefab, cam.transform.position, Quaternion.identity) as Rigidbody;
                var rb = Instantiate(prefab, _PointCloudManager.Instance._Camera.transform.position, Quaternion.identity) as Rigidbody;
                var ray = _PointCloudManager.Instance._Camera.ScreenPointToRay(Input.mousePosition);
                rb.AddForce(ray.direction * 20, ForceMode.Impulse);
                Destroy(rb.gameObject, 3);
            }
        }
    }
}