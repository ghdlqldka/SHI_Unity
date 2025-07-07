using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class _SerializedDictionarySample : SerializedDictionarySample
{
    private static string LOG_FORMAT = "<color=#EBC6FB><b>[_SerializedDictionarySample]</b></color> {0}";

    [ReadOnly]
    [SerializeField]
    [SerializedDictionary("Element Type", "Description")]
    protected SerializedDictionary<ElementType, string> ElementDescriptions2 = new SerializedDictionary<ElementType, string>();

    protected virtual void Awake()
    {
        Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : " + this.gameObject.name);

        ElementDescriptions2.Add(ElementType.Fire, "Test");
    }
}