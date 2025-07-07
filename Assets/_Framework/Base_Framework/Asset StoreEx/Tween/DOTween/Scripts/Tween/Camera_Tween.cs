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
    public static class Camera_Tween
    {
        //
        // Summary:
        //     Tweens a Camera's
        //     aspect
        //     to the given value. Also stores the camera as the tween's target so it can be
        //     used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        // public static TweenerCore<float, float, FloatOptions> DOAspect(this Camera target, float endValue, float duration);
        public static TweenerCore<float, float, FloatOptions> TweenAspect(this Camera target, float endValue, float duration)
        {
            return target.DOAspect(endValue, duration);
        }

        //
        // Summary:
        //     Tweens a Camera's backgroundColor to the given value. Also stores the camera
        //     as the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        // public static TweenerCore<Color, Color, ColorOptions> DOColor(this Camera target, Color endValue, float duration);
        public static TweenerCore<Color, Color, ColorOptions> TweenColor(this Camera target, Color endValue, float duration)
        {
            return target.DOColor(endValue, duration);
        }

        //
        // Summary:
        //     Tweens a Camera's
        //     farClipPlane
        //     to the given value. Also stores the camera as the tween's target so it can be
        //     used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        // public static TweenerCore<float, float, FloatOptions> DOFarClipPlane(this Camera target, float endValue, float duration);
        public static TweenerCore<float, float, FloatOptions> TweenFarClipPlane(this Camera target, float endValue, float duration)
        {
            return target.DOFarClipPlane(endValue, duration);
        }


        //
        // Summary:
        //     Tweens a Camera's
        //     fieldOfView
        //     to the given value. Also stores the camera as the tween's target so it can be
        //     used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        // public static TweenerCore<float, float, FloatOptions> DOFieldOfView(this Camera target, float endValue, float duration);
        public static TweenerCore<float, float, FloatOptions> TweenFieldOfView(this Camera target, float endValue, float duration)
        {
            return target.DOFieldOfView(endValue, duration);
        }

        //
        // Summary:
        //     Tweens a Camera's
        //     nearClipPlane
        //     to the given value. Also stores the camera as the tween's target so it can be
        //     used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        // public static TweenerCore<float, float, FloatOptions> DONearClipPlane(this Camera target, float endValue, float duration);
        public static TweenerCore<float, float, FloatOptions> TweenNearClipPlane(this Camera target, float endValue, float duration)
        {
            return target.DONearClipPlane(endValue, duration);
        }

        //
        // Summary:
        //     Tweens a Camera's
        //     orthographicSize
        //     to the given value. Also stores the camera as the tween's target so it can be
        //     used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        // public static TweenerCore<float, float, FloatOptions> DOOrthoSize(this Camera target, float endValue, float duration);
        public static TweenerCore<float, float, FloatOptions> TweenOrthoSize(this Camera target, float endValue, float duration)
        {
            return target.DOOrthoSize(endValue, duration);
        }

        //
        // Summary:
        //     Tweens a Camera's
        //     pixelRect
        //     to the given value. Also stores the camera as the tween's target so it can be
        //     used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        // public static TweenerCore<Rect, Rect, RectOptions> DOPixelRect(this Camera target, Rect endValue, float duration);
        public static TweenerCore<Rect, Rect, RectOptions> TweenPixelRect(this Camera target, Rect endValue, float duration)
        {
            return target.DOPixelRect(endValue, duration);
        }

        //
        // Summary:
        //     Tweens a Camera's
        //     rect
        //     to the given value. Also stores the camera as the tween's target so it can be
        //     used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        // public static TweenerCore<Rect, Rect, RectOptions> DORect(this Camera target, Rect endValue, float duration);
        public static TweenerCore<Rect, Rect, RectOptions> TweenRect(this Camera target, Rect endValue, float duration)
        {
            return target.DORect(endValue, duration);
        }

        //
        // Summary:
        //     Shakes a Camera's localPosition along its relative X Y axes with the given values.
        //     Also stores the camera as the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   duration:
        //     The duration of the tween
        //
        //   strength:
        //     The shake strength on each axis
        //
        //   vibrato:
        //     Indicates how much will the shake vibrate
        //
        //   randomness:
        //     Indicates how much the shake will be random (0 to 180 - values higher than 90
        //     kind of suck, so beware). Setting it to 0 will shake along a single direction.
        //
        //   fadeOut:
        //     If TRUE the shake will automatically fadeOut smoothly within the tween's duration,
        //     otherwise it will not
        //
        //   randomnessMode:
        //     Randomness mode
        // public static Tweener DOShakePosition(this Camera target, float duration, Vector3 strength, int vibrato = 10, float randomness = 90, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full);
        public static Tweener TweenShakePosition(this Camera target, float duration, Vector3 strength, int vibrato = 10, float randomness = 90, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full)
        {
            return target.DOShakePosition(duration, strength, vibrato, randomness, fadeOut, randomnessMode);
        }

        //
        // Summary:
        //     Shakes a Camera's localPosition along its relative X Y axes with the given values.
        //     Also stores the camera as the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   duration:
        //     The duration of the tween
        //
        //   strength:
        //     The shake strength
        //
        //   vibrato:
        //     Indicates how much will the shake vibrate
        //
        //   randomness:
        //     Indicates how much the shake will be random (0 to 180 - values higher than 90
        //     kind of suck, so beware). Setting it to 0 will shake along a single direction.
        //
        //   fadeOut:
        //     If TRUE the shake will automatically fadeOut smoothly within the tween's duration,
        //     otherwise it will not
        //
        //   randomnessMode:
        //     Randomness mode
        // public static Tweener DOShakePosition(this Camera target, float duration, float strength = 3, int vibrato = 10, float randomness = 90, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full);
        public static Tweener TweenShakePosition(this Camera target, float duration, float strength = 3, int vibrato = 10, float randomness = 90, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full)
        {
            return target.DOShakePosition(duration, strength, vibrato, randomness, fadeOut, randomnessMode);
        }

        //
        // Summary:
        //     Shakes a Camera's localRotation. Also stores the camera as the tween's target
        //     so it can be used for filtered operations
        //
        // Parameters:
        //   duration:
        //     The duration of the tween
        //
        //   strength:
        //     The shake strength
        //
        //   vibrato:
        //     Indicates how much will the shake vibrate
        //
        //   randomness:
        //     Indicates how much the shake will be random (0 to 180 - values higher than 90
        //     kind of suck, so beware). Setting it to 0 will shake along a single direction.
        //
        //   fadeOut:
        //     If TRUE the shake will automatically fadeOut smoothly within the tween's duration,
        //     otherwise it will not
        //
        //   randomnessMode:
        //     Randomness mode
        // public static Tweener DOShakeRotation(this Camera target, float duration, float strength = 90, int vibrato = 10, float randomness = 90, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full);
        public static Tweener TweenShakeRotation(this Camera target, float duration, float strength = 90, int vibrato = 10, float randomness = 90, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full)
        {
            return target.DOShakeRotation(duration, strength, vibrato, randomness, fadeOut, randomnessMode);
        }

        //
        // Summary:
        //     Shakes a Camera's localRotation. Also stores the camera as the tween's target
        //     so it can be used for filtered operations
        //
        // Parameters:
        //   duration:
        //     The duration of the tween
        //
        //   strength:
        //     The shake strength on each axis
        //
        //   vibrato:
        //     Indicates how much will the shake vibrate
        //
        //   randomness:
        //     Indicates how much the shake will be random (0 to 180 - values higher than 90
        //     kind of suck, so beware). Setting it to 0 will shake along a single direction.
        //
        //   fadeOut:
        //     If TRUE the shake will automatically fadeOut smoothly within the tween's duration,
        //     otherwise it will not
        //
        //   randomnessMode:
        //     Randomness mode
        // public static Tweener DOShakeRotation(this Camera target, float duration, Vector3 strength, int vibrato = 10, float randomness = 90, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full);
        public static Tweener TweenShakeRotation(this Camera target, float duration, Vector3 strength, int vibrato = 10, float randomness = 90, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full)
        {
            return target.DOShakeRotation(duration, strength, vibrato, randomness, fadeOut, randomnessMode);
        }
    }
}
#endif
