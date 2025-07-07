// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2018/07/13

#if true && (UNITY_4_6 || UNITY_5 || UNITY_2017_1_OR_NEWER) // MODULE_MARKER

using UnityEngine;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening;
using DG.Tweening.Plugins.Core.PathCore;

#pragma warning disable 1591
namespace _Base_Framework
{
    public static class Transform_Tween
    {
        //
        // Summary:
        //     Tweens a Transform's localPosition BY the given value (as if you chained a
        //     SetRelative
        //     ), in a way that allows other DOBlendableMove tweens to work together on the
        //     same target, instead than fight each other as multiple DOMove would do. Also
        //     stores the transform as the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   byValue:
        //     The value to tween by
        //
        //   duration:
        //     The duration of the tween
        //
        //   snapping:
        //     If TRUE the tween will smoothly snap all values to integers
        // public static Tweener DOBlendableLocalMoveBy(this Transform target, Vector3 byValue, float duration, bool snapping = false);
        public static Tweener TweenBlendableLocalMoveBy(this Transform target, Vector3 byValue, float duration, bool snapping = false)
        {
            return target.DOBlendableLocalMoveBy(byValue, duration, snapping);
        }

        //
        // Summary:
        //     EXPERIMENTAL METHOD - Tweens a Transform's lcoalRotation BY the given value (as
        //     if you chained a
        //     SetRelative
        //     ), in a way that allows other DOBlendableRotate tweens to work together on the
        //     same target, instead than fight each other as multiple DORotate would do. Also
        //     stores the transform as the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   byValue:
        //     The value to tween by
        //
        //   duration:
        //     The duration of the tween
        //
        //   mode:
        //     Rotation mode
        // public static Tweener DOBlendableLocalRotateBy(this Transform target, Vector3 byValue, float duration, RotateMode mode = RotateMode.Fast);
        public static Tweener TweenBlendableLocalRotateBy(this Transform target, Vector3 byValue, float duration, RotateMode mode = RotateMode.Fast)
        {
            return target.DOBlendableLocalRotateBy(byValue, duration, mode);
        }

        //
        // Summary:
        //     Tweens a Transform's position BY the given value (as if you chained a
        //     SetRelative
        //     ), in a way that allows other DOBlendableMove tweens to work together on the
        //     same target, instead than fight each other as multiple DOMove would do. Also
        //     stores the transform as the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   byValue:
        //     The value to tween by
        //
        //   duration:
        //     The duration of the tween
        //
        //   snapping:
        //     If TRUE the tween will smoothly snap all values to integers
        // public static Tweener DOBlendableMoveBy(this Transform target, Vector3 byValue, float duration, bool snapping = false);
        public static Tweener TweenBlendableMoveBy(this Transform target, Vector3 byValue, float duration, bool snapping = false)
        {
            return target.DOBlendableMoveBy(byValue, duration, snapping);
        }

        //
        // Summary:
        //     Punches a Transform's localRotation BY the given value and then back to the starting
        //     one as if it was connected to the starting rotation via an elastic. Does it in
        //     a way that allows other DOBlendableRotate tweens to work together on the same
        //     target
        //
        // Parameters:
        //   punch:
        //     The punch strength (added to the Transform's current rotation)
        //
        //   duration:
        //     The duration of the tween
        //
        //   vibrato:
        //     Indicates how much will the punch vibrate
        //
        //   elasticity:
        //     Represents how much (0 to 1) the vector will go beyond the starting rotation
        //     when bouncing backwards. 1 creates a full oscillation between the punch rotation
        //     and the opposite rotation, while 0 oscillates only between the punch and the
        //     start rotation
        // public static Tweener DOBlendablePunchRotation(this Transform target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1);
        public static Tweener TweenBlendablePunchRotation(this Transform target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1)
        {
            return target.DOBlendablePunchRotation(punch, duration, vibrato, elasticity);
        }

        //
        // Summary:
        //     EXPERIMENTAL METHOD - Tweens a Transform's rotation BY the given value (as if
        //     you chained a
        //     SetRelative
        //     ), in a way that allows other DOBlendableRotate tweens to work together on the
        //     same target, instead than fight each other as multiple DORotate would do. Also
        //     stores the transform as the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   byValue:
        //     The value to tween by
        //
        //   duration:
        //     The duration of the tween
        //
        //   mode:
        //     Rotation mode
        // public static Tweener DOBlendableRotateBy(this Transform target, Vector3 byValue, float duration, RotateMode mode = RotateMode.Fast);
        public static Tweener TweenBlendableRotateBy(this Transform target, Vector3 byValue, float duration, RotateMode mode = RotateMode.Fast)
        {
            return target.DOBlendableRotateBy(byValue, duration, mode);
        }

        //
        // Summary:
        //     Tweens a Transform's localScale BY the given value (as if you chained a
        //     SetRelative
        //     ), in a way that allows other DOBlendableScale tweens to work together on the
        //     same target, instead than fight each other as multiple DOScale would do. Also
        //     stores the transform as the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   byValue:
        //     The value to tween by
        //
        //   duration:
        //     The duration of the tween
        // public static Tweener DOBlendableScaleBy(this Transform target, Vector3 byValue, float duration);
        public static Tweener TweenBlendableScaleBy(this Transform target, Vector3 byValue, float duration)
        {
            return target.DOBlendableScaleBy(byValue, duration);
        }

        //
        // Summary:
        //     EXPERIMENTAL
        //     Tweens a Transform's rotation so that it will look towards the given world position,
        //     while also updating the lookAt position every frame (contrary to DG.Tweening.ShortcutExtensions.DOLookAt(UnityEngine.Transform,UnityEngine.Vector3,System.Single,DG.Tweening.AxisConstraint,System.Nullable{UnityEngine.Vector3})
        //     which calculates the lookAt rotation only once, when the tween starts). Also
        //     stores the transform as the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   towards:
        //     The position to look at
        //
        //   duration:
        //     The duration of the tween
        //
        //   axisConstraint:
        //     Eventual axis constraint for the rotation
        //
        //   up:
        //     The vector that defines in which direction up is (default: Vector3.up)
        // public static Tweener DODynamicLookAt(this Transform target, Vector3 towards, float duration, AxisConstraint axisConstraint = AxisConstraint.None, Vector3? up = null);
        public static Tweener TweenDynamicLookAt(this Transform target, Vector3 towards, float duration, AxisConstraint axisConstraint = AxisConstraint.None, Vector3? up = null)
        {
            return target.TweenDynamicLookAt(towards, duration, axisConstraint, up);
        }

        //
        // Summary:
        //     Tweens a Transform's position to the given value, while also applying a jump
        //     effect along the Y axis. Returns a Sequence instead of a Tweener. Also stores
        //     the transform as the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   jumpPower:
        //     Power of the jump (the max height of the jump is represented by this plus the
        //     final Y offset)
        //
        //   numJumps:
        //     Total number of jumps
        //
        //   duration:
        //     The duration of the tween
        //
        //   snapping:
        //     If TRUE the tween will smoothly snap all values to integers
        // public static Sequence DOJump(this Transform target, Vector3 endValue, float jumpPower, int numJumps, float duration, bool snapping = false);
        public static Sequence TweenJump(this Transform target, Vector3 endValue, float jumpPower, int numJumps, float duration, bool snapping = false)
        {
            return target.DOJump(endValue, jumpPower, numJumps, duration, snapping);
        }

        //
        // Summary:
        //     Tweens a Transform's localPosition to the given value, while also applying a
        //     jump effect along the Y axis. Returns a Sequence instead of a Tweener. Also stores
        //     the transform as the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   jumpPower:
        //     Power of the jump (the max height of the jump is represented by this plus the
        //     final Y offset)
        //
        //   numJumps:
        //     Total number of jumps
        //
        //   duration:
        //     The duration of the tween
        //
        //   snapping:
        //     If TRUE the tween will smoothly snap all values to integers
        // public static Sequence DOLocalJump(this Transform target, Vector3 endValue, float jumpPower, int numJumps, float duration, bool snapping = false);
        public static Sequence TweenLocalJump(this Transform target, Vector3 endValue, float jumpPower, int numJumps, float duration, bool snapping = false)
        {
            return target.DOLocalJump(endValue, jumpPower, numJumps, duration, snapping);
        }

        //
        // Summary:
        //     Tweens a Transform's localPosition to the given value. Also stores the transform
        //     as the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        //
        //   snapping:
        //     If TRUE the tween will smoothly snap all values to integers
        // public static TweenerCore<Vector3, Vector3, VectorOptions> DOLocalMove(this Transform target, Vector3 endValue, float duration, bool snapping = false);
        public static TweenerCore<Vector3, Vector3, VectorOptions> TweenLocalPosition(this Transform target, Vector3 endValue, float duration, bool snapping = false)
        {
            return target.DOLocalMove(endValue, duration, snapping);
        }

        //
        // Summary:
        //     Tweens a Transform's localPosition through the given path waypoints, using the
        //     chosen path algorithm. Also stores the transform as the tween's target so it
        //     can be used for filtered operations
        //
        // Parameters:
        //   path:
        //     The waypoint to go through
        //
        //   duration:
        //     The duration of the tween
        //
        //   pathType:
        //     The type of path: Linear (straight path), CatmullRom (curved CatmullRom path)
        //     or CubicBezier (curved with control points)
        //
        //   pathMode:
        //     The path mode: 3D, side-scroller 2D, top-down 2D
        //
        //   resolution:
        //     The resolution of the path: higher resolutions make for more detailed curved
        //     paths but are more expensive. Defaults to 10, but a value of 5 is usually enough
        //     if you don't have dramatic long curves between waypoints
        //
        //   gizmoColor:
        //     The color of the path (shown when gizmos are active in the Play panel and the
        //     tween is running)
        // public static TweenerCore<Vector3, Path, PathOptions> DOLocalPath(this Transform target, Vector3[] path, float duration, PathType pathType = PathType.Linear, PathMode pathMode = PathMode.Full3D, int resolution = 10, Color? gizmoColor = null);
        public static TweenerCore<Vector3, Path, PathOptions> TweenLocalPath(this Transform target, Vector3[] path, float duration, PathType pathType = PathType.Linear, PathMode pathMode = PathMode.Full3D, int resolution = 10, Color? gizmoColor = null)
        {
            return target.DOLocalPath(path, duration, pathType, pathMode, resolution, gizmoColor);
        }

        //
        // Summary:
        //     IMPORTANT: Unless you really know what you're doing, you should use the overload
        //     that accepts a Vector3 array instead.
        //     Tweens a Transform's localPosition via the given path. Also stores the transform
        //     as the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   path:
        //     The path to use
        //
        //   duration:
        //     The duration of the tween
        //
        //   pathMode:
        //     The path mode: 3D, side-scroller 2D, top-down 2D
        // public static TweenerCore<Vector3, Path, PathOptions> DOLocalPath(this Transform target, Path path, float duration, PathMode pathMode = PathMode.Full3D);
        public static TweenerCore<Vector3, Path, PathOptions> TweenLocalPath(this Transform target, Path path, float duration, PathMode pathMode = PathMode.Full3D)
        {
            return target.DOLocalPath(path, duration, pathMode);
        }

        //
        // Summary:
        //     Tweens a Transform's localRotation to the given value. Also stores the transform
        //     as the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        //
        //   mode:
        //     Rotation mode
        // public static TweenerCore<Quaternion, Vector3, QuaternionOptions> DOLocalRotate(this Transform target, Vector3 endValue, float duration, RotateMode mode = RotateMode.Fast);
        public static TweenerCore<Quaternion, Vector3, QuaternionOptions> TweenLocalRotation(this Transform target, Vector3 endValue, float duration, RotateMode mode = RotateMode.Fast)
        {
            return target.DOLocalRotate(endValue, duration, mode);
        }

        //
        // Summary:
        //     Tweens a Transform's rotation to the given value using pure quaternion values.
        //     Also stores the transform as the tween's target so it can be used for filtered
        //     operations.
        //     PLEASE NOTE: DOLocalRotate, which takes Vector3 values, is the preferred rotation
        //     method. This method was implemented for very special cases, and doesn't support
        //     LoopType.Incremental loops (neither for itself nor if placed inside a LoopType.Incremental
        //     Sequence)
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        // public static TweenerCore<Quaternion, Quaternion, NoOptions> DOLocalRotateQuaternion(this Transform target, Quaternion endValue, float duration);
        public static TweenerCore<Quaternion, Quaternion, NoOptions> TweenLocalRotateQuaternion(this Transform target, Quaternion endValue, float duration)
        {
            return target.DOLocalRotateQuaternion(endValue, duration);
        }

        //
        // Summary:
        //     Tweens a Transform's rotation so that it will look towards the given world position.
        //     Also stores the transform as the tween's target so it can be used for filtered
        //     operations
        //
        // Parameters:
        //   towards:
        //     The position to look at
        //
        //   duration:
        //     The duration of the tween
        //
        //   axisConstraint:
        //     Eventual axis constraint for the rotation
        //
        //   up:
        //     The vector that defines in which direction up is (default: Vector3.up)
        // public static Tweener DOLookAt(this Transform target, Vector3 towards, float duration, AxisConstraint axisConstraint = AxisConstraint.None, Vector3? up = null);
        public static Tweener TweenLookAt(this Transform target, Vector3 towards, float duration, AxisConstraint axisConstraint = AxisConstraint.None, Vector3? up = null)
        {
            return target.DOLookAt(towards, duration, axisConstraint, up);
        }

        //
        // Summary:
        //     Tweens a Transform's position to the given value. Also stores the transform as
        //     the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        //
        //   snapping:
        //     If TRUE the tween will smoothly snap all values to integers
        // public static TweenerCore<Vector3, Vector3, VectorOptions> DOMove(this Transform target, Vector3 endValue, float duration, bool snapping = false);
        public static TweenerCore<Vector3, Vector3, VectorOptions> TweenPosition(this Transform target, Vector3 endValue, float duration, bool snapping = false)
        {
            return target.DOMove(endValue, duration, snapping);
        }

        //
        // Summary:
        //     Tweens a Transform's position through the given path waypoints, using the chosen
        //     path algorithm. Also stores the transform as the tween's target so it can be
        //     used for filtered operations
        //
        // Parameters:
        //   path:
        //     The waypoints to go through
        //
        //   duration:
        //     The duration of the tween
        //
        //   pathType:
        //     The type of path: Linear (straight path), CatmullRom (curved CatmullRom path)
        //     or CubicBezier (curved with control points)
        //
        //   pathMode:
        //     The path mode: 3D, side-scroller 2D, top-down 2D
        //
        //   resolution:
        //     The resolution of the path (useless in case of Linear paths): higher resolutions
        //     make for more detailed curved paths but are more expensive. Defaults to 10, but
        //     a value of 5 is usually enough if you don't have dramatic long curves between
        //     waypoints
        //
        //   gizmoColor:
        //     The color of the path (shown when gizmos are active in the Play panel and the
        //     tween is running)
        // public static TweenerCore<Vector3, Path, PathOptions> DOPath(this Transform target, Vector3[] path, float duration, PathType pathType = PathType.Linear, PathMode pathMode = PathMode.Full3D, int resolution = 10, Color? gizmoColor = null);
        public static TweenerCore<Vector3, Path, PathOptions> TweenPath(this Transform target, Vector3[] path, float duration, PathType pathType = PathType.Linear, PathMode pathMode = PathMode.Full3D, int resolution = 10, Color? gizmoColor = null)
        {
            return target.DOPath(path, duration, pathType, pathMode, resolution, gizmoColor);
        }

        //
        // Summary:
        //     IMPORTANT: Unless you really know what you're doing, you should use the overload
        //     that accepts a Vector3 array instead.
        //     Tweens a Transform's position via the given path. Also stores the transform as
        //     the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   path:
        //     The path to use
        //
        //   duration:
        //     The duration of the tween
        //
        //   pathMode:
        //     The path mode: 3D, side-scroller 2D, top-down 2D
        // public static TweenerCore<Vector3, Path, PathOptions> DOPath(this Transform target, Path path, float duration, PathMode pathMode = PathMode.Full3D);
        public static TweenerCore<Vector3, Path, PathOptions> TweenPath(this Transform target, Path path, float duration, PathMode pathMode = PathMode.Full3D)
        {
            return target.DOPath(path, duration, pathMode);
        }

        //
        // Summary:
        //     Punches a Transform's localPosition towards the given direction and then back
        //     to the starting one as if it was connected to the starting position via an elastic.
        //
        // Parameters:
        //   punch:
        //     The direction and strength of the punch (added to the Transform's current position)
        //
        //   duration:
        //     The duration of the tween
        //
        //   vibrato:
        //     Indicates how much will the punch vibrate
        //
        //   elasticity:
        //     Represents how much (0 to 1) the vector will go beyond the starting position
        //     when bouncing backwards. 1 creates a full oscillation between the punch direction
        //     and the opposite direction, while 0 oscillates only between the punch and the
        //     start position
        //
        //   snapping:
        //     If TRUE the tween will smoothly snap all values to integers
        // public static Tweener DOPunchPosition(this Transform target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1, bool snapping = false);
        public static Tweener TweenPunchPosition(this Transform target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1, bool snapping = false)
        {
            return target.DOPunchPosition(punch, duration, vibrato, elasticity, snapping);
        }

        //
        // Summary:
        //     Punches a Transform's localRotation towards the given size and then back to the
        //     starting one as if it was connected to the starting rotation via an elastic.
        //
        // Parameters:
        //   punch:
        //     The punch strength (added to the Transform's current rotation)
        //
        //   duration:
        //     The duration of the tween
        //
        //   vibrato:
        //     Indicates how much will the punch vibrate
        //
        //   elasticity:
        //     Represents how much (0 to 1) the vector will go beyond the starting rotation
        //     when bouncing backwards. 1 creates a full oscillation between the punch rotation
        //     and the opposite rotation, while 0 oscillates only between the punch and the
        //     start rotation
        // public static Tweener DOPunchRotation(this Transform target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1);
        public static Tweener TweenPunchRotation(this Transform target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1)
        {
            return target.DOPunchRotation(punch, duration, vibrato, elasticity);
        }

        //
        // Summary:
        //     Punches a Transform's localScale towards the given size and then back to the
        //     starting one as if it was connected to the starting scale via an elastic.
        //
        // Parameters:
        //   punch:
        //     The punch strength (added to the Transform's current scale)
        //
        //   duration:
        //     The duration of the tween
        //
        //   vibrato:
        //     Indicates how much will the punch vibrate
        //
        //   elasticity:
        //     Represents how much (0 to 1) the vector will go beyond the starting size when
        //     bouncing backwards. 1 creates a full oscillation between the punch scale and
        //     the opposite scale, while 0 oscillates only between the punch scale and the start
        //     scale
        // public static Tweener DOPunchScale(this Transform target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1);
        public static Tweener TweenPunchScale(this Transform target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1)
        {
            return target.DOPunchScale(punch, duration, vibrato, elasticity);
        }





        //
        // Summary:
        //     Tweens a Transform's rotation to the given value. Also stores the transform as
        //     the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        //
        //   mode:
        //     Rotation mode
        // public static TweenerCore<Quaternion, Vector3, QuaternionOptions> DORotate(this Transform target, Vector3 endValue, float duration, RotateMode mode = RotateMode.Fast);
        public static TweenerCore<Quaternion, Vector3, QuaternionOptions> TweenRotation(this Transform target, Vector3 endValue, float duration, RotateMode mode = RotateMode.Fast)
        {
            return target.DORotate(endValue, duration, mode);
        }

        //
        // Summary:
        //     Tweens a Transform's localScale to the given value. Also stores the transform
        //     as the tween's target so it can be used for filtered operations
        //
        // Parameters:
        //   endValue:
        //     The end value to reach
        //
        //   duration:
        //     The duration of the tween
        // public static TweenerCore<Vector3, Vector3, VectorOptions> DOScale(this Transform target, Vector3 endValue, float duration);
        public static TweenerCore<Vector3, Vector3, VectorOptions> TweenScale(this Transform target, float endValue, float duration)
        {
            return target.DOScale(endValue, duration);
        }

        //
        // Summary:
        //     Shakes a Transform's localPosition with the given values.
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
        //   snapping:
        //     If TRUE the tween will smoothly snap all values to integers
        //
        //   fadeOut:
        //     If TRUE the shake will automatically fadeOut smoothly within the tween's duration,
        //     otherwise it will not
        //
        //   randomnessMode:
        //     Randomness mode
        // public static Tweener DOShakePosition(this Transform target, float duration, float strength = 1, int vibrato = 10, float randomness = 90, bool snapping = false, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full);
        public static Tweener TweenShakePosition(this Transform target, float duration, float strength = 1, int vibrato = 10, float randomness = 90, bool snapping = false, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full)
        {
            return target.DOShakePosition(duration, strength, vibrato, randomness, snapping, fadeOut, randomnessMode);
        }

        //
        // Summary:
        //     Shakes a Transform's localPosition with the given values.
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
        //   snapping:
        //     If TRUE the tween will smoothly snap all values to integers
        //
        //   fadeOut:
        //     If TRUE the shake will automatically fadeOut smoothly within the tween's duration,
        //     otherwise it will not
        //
        //   randomnessMode:
        //     Randomness mode
        // public static Tweener DOShakePosition(this Transform target, float duration, Vector3 strength, int vibrato = 10, float randomness = 90, bool snapping = false, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full);
        public static Tweener TweenShakePosition(this Transform target, float duration, Vector3 strength, int vibrato = 10, float randomness = 90, bool snapping = false, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full)
        {
            return target.DOShakePosition(duration, strength, vibrato, randomness, snapping, fadeOut, randomnessMode);
        }

    }
}
#endif
