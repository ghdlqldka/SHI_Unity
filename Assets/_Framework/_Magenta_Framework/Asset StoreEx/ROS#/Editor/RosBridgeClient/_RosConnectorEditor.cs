using UnityEditor;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    [CustomEditor(typeof(_RosConnector))]
    public class _RosConnectorEditor : RosConnectorEditor
    {
        // RosConnector rosConnector;
        protected _RosConnector _rosConnector
        {
            get 
            { 
                return rosConnector as _RosConnector;
            }
        }

        /*
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // Dropdown to select ROS version
            RosVersion newSelectedRosVersion = (RosVersion)EditorGUILayout.EnumPopup("ROS Version", _rosConnector.selectedRosVersion);

            if (newSelectedRosVersion != _rosConnector.selectedRosVersion)
            {
                _rosConnector.selectedRosVersion = newSelectedRosVersion;
                ToggleROSVersion(_rosConnector.selectedRosVersion);
            }
        }
        */
    }
}
