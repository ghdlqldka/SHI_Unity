using UnityEngine;
using System.Collections;

public class ReporterGUI : MonoBehaviour
{
	protected Reporter reporter;

	protected virtual void Awake()
	{
		reporter = gameObject.GetComponent<Reporter>();
	}

	protected virtual void OnGUI()
	{
		reporter.OnGUIDraw();
	}
}
