﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[System.Serializable]
public class ScenesSelection
{
    public List<SceneAsset> scenes;
}


[CreateAssetMenu(fileName = "BuildScenesSelections", menuName = "ScriptableObjects/BuildScenesSelections", order = 1)]
public class BuildSceneSelections : ScriptableObject
{
    public List<ScenesSelection> sceneSelections;
}
#endif