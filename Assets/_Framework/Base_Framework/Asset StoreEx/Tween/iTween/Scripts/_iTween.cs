using System.Collections;
using UnityEngine;

public class _iTween : iTween
{

	/// <summary>
	/// Adds the supplied coordinates to a GameObject's position with FULL customization options.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount of change in position to move the GameObject.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="orienttopath">
	/// A <see cref="System.Boolean"/> for whether or not the GameObject will orient to its direction of travel.  False by default.
	/// </param>
	/// <param name="looktarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="looktime">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the object will take to look at either the "looktarget" or "orienttopath".
	/// </param>
	/// <param name="axis">
	/// A <see cref="System.String"/>. Restricts rotation to the supplied axis only.
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> or <see cref="System.String"/> for applying the transformation in either the world coordinate or local cordinate system. Defaults to local space.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="speed">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> can be used instead of time to allow animation based on speed
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void _MoveBy(GameObject target, Vector3 amount, float time, bool localspace, Vector3 looktarget, bool orienttopath, float looktime, float delay/*, onstart, oncomplete*/)
	{
#if true
		MoveBy(target, Hash("amount", amount, "time", time));
#else
		//clean args:
		args = iTween.CleanArgs(args);

		//establish iTween:
		args["type"] = "move";
		args["method"] = "by";
		Launch(target, args);
#endif
	}

	public delegate void _iTweenCallback(GameObject target);

	public static void MoveBy(GameObject target, Vector3 amount, EaseType easeType, LoopType loopType, float delay, _iTweenCallback onstart = null, _iTweenCallback onupdate = null, _iTweenCallback oncomplete = null)
	{
		Hashtable args = Hash("amount", amount, "easetype", easeType.ToString(), "looptype", loopType.ToString(), "delay", delay);
		if (oncomplete != null)
		{
			args["oncomplete"] = oncomplete.Method.Name;
			args["oncompleteparams"] = target;
		}

		if (onupdate != null)
		{
			args["onupdate"] = onupdate.Method.Name;
			args["onupdateparams"] = target;
		}

		if (onstart != null)
		{
			args["onstart"] = onstart.Method.Name;
			args["onstartparams"] = target;
		}

		MoveBy(target, args);
	}

	public _iTween(Hashtable h) : base(h)
    {
		//
	}
} 
