/*  This file is part of the "Simple Waypoint System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;

namespace SWS
{
    using DG.Tweening;

    /// <summary>
    /// Example: user input script which moves through waypoints one by one.
    /// <summary>
    public class _CameraInputDemo : CameraInputDemo
    {
        // protected splineMove myMove;
        protected _SplineMove _myMove
        {
            get
            {
                return myMove as _SplineMove;
            }
        }

        protected override void Start()
        {
            myMove = this.gameObject.GetComponent<_SplineMove>();
            _myMove.StartMove();
            _myMove.Pause();
        }

        protected override void Update()
        {
            //do nothing in moving state
            if (!_myMove._Tween.IsActive() || _myMove._Tween.IsPlaying())
                return;

            //on up arrow, move forwards
            if (Input.GetKeyDown(KeyCode.UpArrow))
                _myMove.Resume();
        }


        //display GUI stuff on screen
        protected override void OnGUI()
        {
            //do nothing in moving state
            if (_myMove._Tween.IsActive() && _myMove._Tween.IsPlaying())
                return;

            //draw top right box with info text received from messages
            GUI.Box(new Rect(Screen.width - 150, Screen.height / 2, 150, 100), "");
            Rect infoPos = new Rect(Screen.width - 130, Screen.height / 2 + 10, 110, 90);
            GUI.Label(infoPos, infoText);
        }
    }
}