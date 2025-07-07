using pointcloudviewer.binaryviewer;
using UnityEngine;

namespace pointcloudviewer.examples
{
    public class _YourOwnDataExample : YourOwnDataExample
    {
        private static string LOG_FORMAT = "<color=#00FF90><b>[_YourOwnDataExample]</b></color> {0}";

        // public PointCloudViewerDX11 binaryViewerDX11;
        protected _PointCloudViewerDX11 _binaryViewerDX11
        {
            get
            {
                return binaryViewerDX11 as _PointCloudViewerDX11;
            }
        }

        protected override void Start()
        {
            // initialize viewer with 1 point and colordata, so that can resize/fill it later
            _binaryViewerDX11.containsRGB = true;
            _binaryViewerDX11.InitDX11Buffers();
        }

        protected override void Update()
        {
            // demo: press space to randomize new data
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.LogFormat(LOG_FORMAT, "Space!!!!!");
                // generate random example dataset, instead of this, you would load/generate your own data
                Vector3[] randomPoints = new Vector3[totalPoints];
                Vector3[] randomColors = new Vector3[totalPoints];
                for (int i = 0; i < totalPoints; i++)
                {
                    randomPoints[i] = Random.insideUnitSphere * 15;
                    var c = Random.ColorHSV(0, 1, 0, 1, 0, 1);
                    randomColors[i] = new Vector3(c.r, c.g, c.b);
                }

                // after you have your own data, send them into viewer
                // _binaryViewerDX11._Points = randomPoints;
                _binaryViewerDX11.UpdatePointData(randomPoints);

                // _binaryViewerDX11._PointColors = randomColors;
                _binaryViewerDX11.UpdateColorData(randomColors);
            }
        }
    }
}