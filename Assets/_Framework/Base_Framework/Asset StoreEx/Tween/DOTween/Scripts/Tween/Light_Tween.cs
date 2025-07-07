// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2018/07/13

#if true && (UNITY_4_6 || UNITY_5 || UNITY_2017_1_OR_NEWER) // MODULE_MARKER

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening;
using DG.Tweening.Plugins;
using System.Globalization;
using DG.Tweening.Core.Enums;

#pragma warning disable 1591
namespace _Base_Framework
{
    public static class Light_Tween
    {
        //
        // Summary:
        //     Tweens a Light's color to the given value, in a way that allows other DOBlendableColor
        //     tweens to work together on the same target, instead than fight each other as
        //     multiple DOColor would do. Also stores the Light as the tween's target so it
        //     can be used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The value to tween to
        //
        //   duration:
        //     The duration of the tween
        // public static Tweener DOBlendableColor(this Light target, Color endValue, float duration);
        public static Tweener TweenBlendableColor(this Light target, Color endValue, float duration)
        {
            return target.DOBlendableColor(endValue, duration);
        }

        //
        // Summary:
        //     Tweens a Light's color to the given value. Also stores the light as the tween's
        //     target so it can be used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        // public static TweenerCore<Color, Color, ColorOptions> DOColor(this Light target, Color endValue, float duration);
        public static TweenerCore<Color, Color, ColorOptions> TweenColor(this Light target, Color endValue, float duration)
        {
            return target.DOColor(endValue, duration);
        }

        //
        // Summary:
        //     Tweens a Light's intensity to the given value. Also stores the light as the tween's
        //     target so it can be used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        // public static TweenerCore<float, float, FloatOptions> DOIntensity(this Light target, float endValue, float duration);
        public static TweenerCore<float, float, FloatOptions> TweenIntensity(this Light target, float endValue, float duration)
        {
            return target.DOIntensity(endValue, duration);
        }

        //
        // Summary:
        //     Tweens a Light's shadowStrength to the given value. Also stores the light as
        //     the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        // public static TweenerCore<float, float, FloatOptions> DOShadowStrength(this Light target, float endValue, float duration);
        public static TweenerCore<float, float, FloatOptions> TweenShadowStrength(this Light target, float endValue, float duration)
        {
            return target.DOShadowStrength(endValue, duration);
        }
    }
}
#endif
