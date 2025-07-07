/*  This file is part of the "Simple Waypoint System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;

namespace SWS
{
    using DG.Tweening;

    /// <summary>
    /// Example: object controlled by user input along the path
    /// <summary>
    public class _PathInputDemo : PathInputDemo
    {
        // protected splineMove move;
        protected _SplineMove _myMove
        {
            get
            {
                return move as _SplineMove;
            }
        }

        protected override void Start()
        {
            animator = GetComponent<Animator>();
            move = GetComponent<splineMove>();
            _myMove.StartMove();
            _myMove.Pause();

            tween = _myMove._Tween;
            progress = 0f;
        }

        protected override void Update()
        {
            //we could have reached the end of the path, where the tween reference gets null
            //ensure that we always have a tween reference available by caching it locally
            if (_myMove._Tween == null)
                _myMove._Tween = tween;

            float speed = speedMultiplier / 100f;
            float duration = _myMove._Tween.Duration();

            //right arrow key
            if (Input.GetKey("right"))
            {
                //add a value based on time and speed to the progress to start moving right
                progress += Time.deltaTime * 10 * speed;
                progress = Mathf.Clamp(progress, 0, duration);
                _myMove._Tween.fullPosition = progress;
            }

            //left arrow key
            //same as above, but here we invert the progress direction
            if (Input.GetKey("left"))
            {
                progress -= Time.deltaTime * 10 * speed;
                progress = Mathf.Clamp(progress, 0, duration);
                _myMove._Tween.fullPosition = progress;
            }

            //let Mecanim animate our object when moving,
            //otherwise set speed to zero
            if ((Input.GetKey("right") || Input.GetKey("left"))
                && progress != 0 && progress != duration)
            {
                animator.SetFloat("Speed", _myMove.speed);
            }
            else
                animator.SetFloat("Speed", 0f);
        }
    }
}