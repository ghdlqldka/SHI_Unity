using UnityEngine;

namespace PointCloudViewer.Experimental
{
    public class FeaturesV3 : MonoBehaviour
    {
        private bool useIntensity = false;
        private bool useClassification = false;
        private bool useGradient = false;

        public void ToggleIntensity(bool state)
        {
            useIntensity = state;
            useClassification = false;
            ApplyShaderKeywords();
        }

        public void ToggleClassification(bool state)
        {
            useClassification = state;
            useIntensity = false;
            ApplyShaderKeywords();
        }

        public void ToggleGradient(bool state)
        {
            useGradient = state;
            ApplyShaderKeywords();
        }

        private void ApplyShaderKeywords()
        {
            // Disable all keywords
            Shader.DisableKeyword("_USEINTENSITY_ON");
            Shader.DisableKeyword("_USEINTENSITY_OFF");
            Shader.DisableKeyword("_USECLASSIFICATION_ON");
            Shader.DisableKeyword("_USECLASSIFICATION_OFF");
            Shader.DisableKeyword("_GRADIENT_ON");
            Shader.DisableKeyword("_GRADIENT_OFF");

            // Enable current states
            Shader.EnableKeyword("_USEINTENSITY_" + (useIntensity ? "ON" : "OFF"));
            Shader.EnableKeyword("_USECLASSIFICATION_" + (useClassification ? "ON" : "OFF"));
            Shader.EnableKeyword("_GRADIENT_" + (useGradient ? "ON" : "OFF"));
        }
    }
}
