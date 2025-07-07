using UnityEngine;
using System.Collections;

public class RotateSample : MonoBehaviour
{	
	protected virtual void Start(){
		iTween.RotateBy(gameObject, iTween.Hash("x", .25, "easeType", "easeInOutBack", "loopType", "pingPong", "delay", .4));
	}
}

