using UnityEngine;
using System.Collections;
using static iTween;

public class _MoveSample : MoveSample
{	
	protected override void Start(){
		// _iTween.MoveBy(gameObject, iTween.Hash("x", 2, "easeType", "easeInOutExpo", "loopType", "pingPong", "delay", .1));
		Vector3 amount = new Vector3(2, 0, 0);
		EaseType easeType = EaseType.easeInOutExpo;
		LoopType loopType = LoopType.pingPong;
		// _iTween.MoveBy(gameObject, iTween.Hash("x", 2, "easeType", "easeInOutExpo", "loopType", "pingPong", "delay", .1));
		_iTween.MoveBy(gameObject, amount, easeType, loopType, 0.1f, _OnStart, _OnUpdate, _OnComplete);
	}

	protected virtual void _OnStart(GameObject target)
	{
		Debug.Log("0");
	}

	protected virtual void _OnUpdate(GameObject target)
	{
		Debug.Log("1");
	}

	protected virtual void _OnComplete(GameObject target)
	{
		Debug.Log("2");
	}
}