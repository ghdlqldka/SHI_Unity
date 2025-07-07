// Point Cloud Binary Viewer DX11 with Tiles (v3)
// http://unitycoder.com

namespace pointcloudviewer.binaryviewer
{
    public interface ITileAction
    {
        bool ValidateTile(double avgGPSTime, float overlapRatio);
    }
}