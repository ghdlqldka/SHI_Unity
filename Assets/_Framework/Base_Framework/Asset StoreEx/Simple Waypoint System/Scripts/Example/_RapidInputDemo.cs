/*  This file is part of the "Simple Waypoint System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System.Collections;
using UnityEngine;

namespace SWS
{
    using DG.Tweening;

    /// <summary>
    /// Example: object controlled by user input, speed decreases when not pressing a key.
    /// <summary>
    public class _RapidInputDemo : RapidInputDemo
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
            move = GetComponent<_SplineMove>();
            /*
            if (!move)
            {
                Debug.LogWarning(gameObject.name + " missing movement script!");
                return;
            }
            */
            Debug.Assert(_myMove != null);

            //set speed to an arbitrary small value
            //otherwise the tween can't be initialized
            _myMove.speed = 0.01f;
            //initialize movement but don't start it yet
            _myMove.StartMove();
            _myMove.Pause();
            _myMove.speed = 0f;
        }


        protected override void Update()
        {
            //do not continue if the tween reached its end
            if (_myMove._Tween == null || !_myMove._Tween.IsActive() || _myMove._Tween.IsComplete())
                return;

            //check for user input
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                //resume tween the first time the game starts
                if (!_myMove._Tween.IsPlaying())
                    _myMove.Resume();

                //get desired speed after pressing the button
                //we add the desired value to the current speed for acceleration
                float speed = currentSpeed + addSpeed;
                //limit the speed value by the maximum value
                if (speed >= topSpeed)
                    speed = topSpeed;

                //change the speed of the tween by the calculated value
                _myMove.ChangeSpeed(speed);

                //restart slow down
                StopAllCoroutines();
                StartCoroutine("SlowDown");
            }

            //display values and increase timer
            speedDisplay.text = "YOUR SPEED: " + Mathf.Round(_myMove.speed * 100f) / 100f;
            timeCounter += Time.deltaTime;
            timeDisplay.text = "YOUR TIME: " + Mathf.Round(timeCounter * 100f) / 100f;
        }


        //coroutine for slowing down the object
        protected override IEnumerator SlowDown()
        {
            //wait desired delay before affecting speed
            yield return new WaitForSeconds(delay);

            //temp time value (0-1)
            float t = 0f;
            //time rate based on slowTime
            float rate = 1f / slowTime;
            //cache actual current speed
            float speed = _myMove.speed;

            //slow down until slowTime is elapsed
            while (t < 1)
            {
                //increase time value over time
                t += Time.deltaTime * rate;
                //smoothly slow down speed value to zero over time
                //cache smoothed current speed value
                currentSpeed = Mathf.Lerp(speed, 0, t);
                //apply current speed to the tween
                _myMove.ChangeSpeed(currentSpeed);

                //get pitch factor as difference between min and max pitch
                float pitchFactor = maxPitch - minPitch;
                //calculate pitch based on the current speed multiplied by the pitch factor
                float pitch = minPitch + (_myMove.speed / topSpeed) * pitchFactor;
                //smooth pitch value over 0.2 seconds and assign it to the audio clip 
                if (GetComponent<AudioSource>()) 
                    GetComponent<AudioSource>().pitch = Mathf.SmoothStep(GetComponent<AudioSource>().pitch, pitch, 0.2f);

                //yield loop
                yield return null;
            }
        }
    }
}