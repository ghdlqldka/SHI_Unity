/*  This file is part of the "Simple Waypoint System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace SWS
{
    using DG.Tweening;
    using DG.Tweening.Core;
    using DG.Tweening.Plugins.Options;

    /// <summary>
    /// Movement script: linear or curved splines.
    /// <summary>
    public class _SplineMove : splineMove
    {
        private static string LOG_FORMAT = "<color=#0EFFDE><b>[_SplineMove]</b></color> {0}";

        // public Tweener tween;
        public Tweener _Tween
        {
            get
            {
                return tween;
            }
            set
            {
                tween = value;
            }
        }

        // public PathManager pathContainer;
        protected PathManager _PathContainer
        {
            get
            {
                return pathContainer;
            }
            set
            {
                pathContainer = value;
            }
        }

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), onStart : " + onStart + ", localType : " + localType +
                ", loopType : <b>" + loopType + "</b>");
        }

        protected override void Start()
        {
            if (onStart == true)
            {
                StartMove();
            }
        }

        protected override void OnDestroy()
        {
            DOTween.Kill(this.transform);
        }

        /// <summary>
        /// Starts movement. Can be called from other scripts to allow start delay.
        /// <summary>
        public override void StartMove()
        {
            //don't continue without path container
            if (_PathContainer == null)
            {
                Debug.LogWarningFormat(LOG_FORMAT, this.gameObject.name + " has no path! Please set Path Container.");
                return;
            }

            //get array with waypoint positions
            waypoints = _PathContainer.GetPathPoints(localType != LocalType.none);
            //cache original speed for future speed changes
            originSpeed = speed;
            //cache original rotation if waypoint rotation is enabled
            originRot = transform.rotation;
            //cache bool whether to move to the path
            moveToPathBool = moveToPath;

            //initialize waypoint positions
            startPoint = Mathf.Clamp(startPoint, 0, waypoints.Length - 1);
            int index = startPoint;
            if (reverse == true)
            {
                Array.Reverse(waypoints);
                index = waypoints.Length - 1 - index;
            }
            Initialize(index);

            Stop();
            CreateTween();
        }

        protected override void Initialize(int startAt = 0)
        {
            if (moveToPathBool == false)
            {
                startAt = 0;
            }

            wpPos = new Vector3[waypoints.Length - startAt];
            for (int i = 0; i < wpPos.Length; i++)
            {
                wpPos[i] = waypoints[i + startAt] + new Vector3(0, sizeToAdd, 0);
            }

            //position waypoints relative to object
            if (localType == LocalType.toObject)
            {
                for (int i = 0; i < wpPos.Length; i++)
                {
                    wpPos[i] = transform.position + wpPos[i];
                }
            }
        }

        protected override void CreateTween()
        {
            //prepare DOTween's parameters, you can look them up here
            //http://dotween.demigiant.com/documentation.php

            TweenParams parms = new TweenParams();
            //differ between speed or time based tweening
            if (timeValue == TimeValue.speed)
            {
                parms.SetSpeedBased();
            }
            if (loopType == LoopType.yoyo)
            {
                parms.SetLoops(-1, DG.Tweening.LoopType.Yoyo);
            }

            //apply ease type or animation curve
            if (easeType == Ease.Unset)
            {
                parms.SetEase(animEaseType);
            }
            else
            {
                parms.SetEase(easeType);
            }

            if (moveToPathBool == true)
            {
                parms.OnWaypointChange(OnWaypointReached);
            }
            else
            {
                //on looptype random initialize random order of waypoints
                if (loopType == LoopType.random)
                    RandomizeWaypoints();
                else if (loopType == LoopType.yoyo)
                    parms.OnStepComplete(ReachedEnd);
                else
                {
                    //
                }

                Vector3 startPos = wpPos[0];
                if (localType == LocalType.toPath)
                {
                    startPos = _PathContainer.transform.TransformPoint(startPos);
                }
                this.transform.position = startPos;

                parms.OnWaypointChange(OnWaypointChange);
                parms.OnComplete(ReachedEnd);
            }

            if (pathMode == PathMode.Ignore && waypointRotation != RotationType.none)
            {
                if (rotationTarget == null)
                {
                    rotationTarget = this.transform;
                }
                parms.OnUpdate(OnWaypointRotation);
            }

            if (localType == LocalType.toPath)
            {
                _Tween = this.transform.DOLocalPath(wpPos, originSpeed, pathType, pathMode)
                                 .SetAs(parms)
                                 .SetOptions(closeLoop, lockPosition, lockRotation)
                                 .SetLookAt(lookAhead);
            }
            else if (localType == LocalType.toObject)
            {
                _Tween = this.transform.DOPath(wpPos, originSpeed, pathType, pathMode)
                                 .SetAs(parms)
                                 .SetOptions(closeLoop, lockPosition, lockRotation)
                                 .SetLookAt(lookAhead);
            }
            else
            {
                _Tween = this.transform.DOPath(wpPos, originSpeed, pathType, pathMode)
                                 .SetAs(parms)
                                 .SetOptions(closeLoop, lockPosition, lockRotation)
                                 .SetLookAt(lookAhead);
            }

            _Tween.Play();
            if (moveToPathBool == false && startPoint > 0)
            {
                GoToWaypoint(startPoint);
                startPoint = 0;
            }

            //continue new tween with adjusted speed if it was changed before
            if (originSpeed != speed)
            {
                ChangeSpeed(speed);
            }

            movementStart.Invoke();

            /*
            if (movementStartEvent != null)
                movementStartEvent();
            */
            Invoke_MovementStartEvent();
        }

        /*
        protected void Invoke_MovementStartEvent()
        {
            if (movementStartEvent != null)
                movementStartEvent();
        }
        */

        public override void Stop()
        {
            StopAllCoroutines();
            waitRoutine = null;

            if (_Tween != null)
            {
                _Tween.Kill();
            }
            _Tween = null;
        }

        public override void Pause(float seconds = 0f)
        {
            if (waitRoutine != null)
            {
                StopCoroutine(waitRoutine);
            }
            if (_Tween != null)
            {
                _Tween.Pause();
            }

            if (seconds > 0)
            {
                waitRoutine = StartCoroutine(Wait(seconds));
            }
        }

        public override void Resume()
        {
            if (waitRoutine != null)
            {
                StopCoroutine(waitRoutine);
                waitRoutine = null;
            }

            if (_Tween != null)
            {
                _Tween.Play();
            }
        }

        public override void SetPath(PathManager newPath)
        {
            throw new System.NotSupportedException("");
        }

        public virtual void SetPath(_PathHandler newPath)
        {
            Debug.LogFormat(LOG_FORMAT, "SetPath()");

            //disable any running movement methods
            Stop();
            //set new path container
            _PathContainer = newPath;
            //restart movement
            StartMove();
        }
    }
}