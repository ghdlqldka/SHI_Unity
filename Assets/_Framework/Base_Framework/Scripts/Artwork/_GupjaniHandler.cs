// using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace _Base_Framework
{
    [RequireComponent(typeof(AudioSource))]
    public class _GupjaniHandler : _ArtworkCharacterHandler
    {
        // private static string LOG_FORMAT = "<color=magenta><b>[_GupjaniHandler]</b></color> {0}";

        public enum ClipType
        {
            _1_appear = 0, // 
            _1_diorama_talk01,
            _1_talk01,
            _1_talk02,
            _1_talk03,
            _1_talk04,
            _1_talk05,
            _2_come_and_see,
            _2_appear,
            _2_move,
            _2_talk01,
            _2_talk02,
            _2_talk03,
            _2_talk04,
            _3_talk01,
            _3_Where_are_you_going,
            _4_Questions,

            idle,
        }

        protected virtual void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                int minInclusive = (int)ClipType._1_appear;
                int maxExclusive = (int)ClipType.idle;
                int index = Random.Range(minInclusive, maxExclusive);

                PlayAnimation((ClipType)index);
            }

            if (Input.GetKeyDown(KeyCode.Space) == true)
            {
                _Reset();
            }
#endif
        }

        public override void _Reset()
        {
            PlayAnimation(ClipType._1_appear);
        }

        public void PlayAnimation(ClipType animTrigger)
        {
            // int i = Enum.Parse()
            _animator.SetTrigger(animTrigger.ToString());
        }
    }
}