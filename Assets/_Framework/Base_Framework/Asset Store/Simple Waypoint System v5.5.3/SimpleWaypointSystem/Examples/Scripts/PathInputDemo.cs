﻿/*  This file is part of the "Simple Waypoint System" project by FLOBUK.
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
    public class PathInputDemo : MonoBehaviour
    {
        /// <summary>
        /// Speed value to multiply the input speed with. 
        /// <summary>
        public float speedMultiplier = 10f;

        /// <summary>
        /// Object progress on the path, should be read only.
        /// <summary>
        public float progress = 0f;

        //references
        protected splineMove move;
        protected Animator animator;
        protected Tweener tween;


        //get references at start
        //initialize movement but don't start it yet
        protected virtual void Start()
        {
            animator = GetComponent<Animator>();
            move = GetComponent<splineMove>();
            move.StartMove();
            move.Pause();

            tween = move.tween;
            progress = 0f;
        }


        //listens to user input
        protected virtual void Update()
        {
            //we could have reached the end of the path, where the tween reference gets null
            //ensure that we always have a tween reference available by caching it locally
            if (move.tween == null) move.tween = tween;

            float speed = speedMultiplier / 100f;
            float duration = move.tween.Duration();

            //right arrow key
            if (Input.GetKey("right"))
            {
                //add a value based on time and speed to the progress to start moving right
                progress += Time.deltaTime * 10 * speed;
                progress = Mathf.Clamp(progress, 0, duration);
                move.tween.fullPosition = progress;
            }

            //left arrow key
            //same as above, but here we invert the progress direction
            if (Input.GetKey("left"))
            {
                progress -= Time.deltaTime * 10 * speed;
                progress = Mathf.Clamp(progress, 0, duration);
                move.tween.fullPosition = progress;
            }

            //let Mecanim animate our object when moving,
            //otherwise set speed to zero
            if ((Input.GetKey("right") || Input.GetKey("left"))
                && progress != 0 && progress != duration)
                animator.SetFloat("Speed", move.speed);
            else
                animator.SetFloat("Speed", 0f);
        }


        void LateUpdate()
        {
            //if we are moving backwards, rotate our walker by 180 degrees
            //this happens after the tween has updated the transform
            if (Input.GetKey("left"))
            {
                if (progress <= 0) return;
                Vector3 rot = transform.localEulerAngles;
                rot.y += 180;
                transform.localEulerAngles = rot;
            }
        }
    }
}