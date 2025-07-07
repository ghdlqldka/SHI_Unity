using UnityEngine;
using System.Collections;
using EndlessJumper;

namespace _Base_Framework._EndlessJumper
{
	public class _Enemy : EndlessJumper.Enemy
    {

        protected override void Start()
        {
            // base.Start();
            Game = _GameManager.Instance;
        }
    }
}