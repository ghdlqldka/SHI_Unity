using System;
using _SHI_BA;
using BioIK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CubeCustom : MonoBehaviour
{
    private static string LOG_FORMAT = "<color=white><b>[UI_CubeCustom]</b></color> {0}";

    [SerializeField]
    protected CubeStruct _cubeStruct;

    [Space(5)]
    // Point 0 (X, Y, Z)
    public TMP_InputField point0_X;
    public TMP_InputField point0_Y;
    public TMP_InputField point0_Z;

    // Point 1 (X, Y, Z)
    public TMP_InputField point1_X;
    public TMP_InputField point1_Y;
    public TMP_InputField point1_Z;

    // Point 2 (X, Y, Z)
    public TMP_InputField point2_X;
    public TMP_InputField point2_Y;
    public TMP_InputField point2_Z;

    // Point 3 (X, Y, Z)
    public TMP_InputField point3_X;
    public TMP_InputField point3_Y;
    public TMP_InputField point3_Z;

    // Point 4 (X, Y, Z)
    public TMP_InputField point4_X;
    public TMP_InputField point4_Y;
    public TMP_InputField point4_Z;

    // Point 5 (X, Y, Z)
    public TMP_InputField point5_X;
    public TMP_InputField point5_Y;
    public TMP_InputField point5_Z;

    // Point 6 (X, Y, Z)
    public TMP_InputField point6_X;
    public TMP_InputField point6_Y;
    public TMP_InputField point6_Z;

    // Point 7 (X, Y, Z)
    public TMP_InputField point7_X;
    public TMP_InputField point7_Y;
    public TMP_InputField point7_Z;

    protected Vector3[] customPoints = new Vector3[8];

    protected void Awake()
    {
        Debug.LogFormat(LOG_FORMAT, "Awake()");

        Debug.Assert(_cubeStruct != null);

        Debug.Assert(point0_X != null);
        Debug.Assert(point0_Y != null);
        Debug.Assert(point0_Z != null);

        Debug.Assert(point1_X != null);
        Debug.Assert(point1_Y != null);
        Debug.Assert(point1_Z != null);

        Debug.Assert(point2_X != null);
        Debug.Assert(point2_Y != null);
        Debug.Assert(point2_Z != null);

        Debug.Assert(point3_X != null);
        Debug.Assert(point3_Y != null);
        Debug.Assert(point3_Z != null);

        Debug.Assert(point4_X != null);
        Debug.Assert(point4_Y != null);
        Debug.Assert(point4_Z != null);

        Debug.Assert(point5_X != null);
        Debug.Assert(point5_Y != null);
        Debug.Assert(point5_Z != null);

        Debug.Assert(point6_X != null);
        Debug.Assert(point6_Y != null);
        Debug.Assert(point6_Z != null);

        Debug.Assert(point7_X != null);
        Debug.Assert(point7_Y != null);
        Debug.Assert(point7_Z != null);
    }

    public void OnClick_CreateRegion()
    {
        ReadInputFieldValues(ref customPoints);
        _cubeStruct.CreateCustomCubeMesh(customPoints);
        this.gameObject.SetActive(false);
    }

    public void OnClick_CancelRegion()
    {
        this.gameObject.SetActive(false);
    }

    protected void ReadInputFieldValues(ref Vector3[] customPoints)
    {
        // Point 0 ÁÂÇ¥ ÀÐ±â
        float x = float.Parse(point0_X.text);
        float y = float.Parse(point0_Y.text);
        float z = float.Parse(point0_Z.text);
        customPoints[0] = new Vector3(x, y, z);

        // Point 1 ÁÂÇ¥ ÀÐ±â
        x = float.Parse(point1_X.text);
        y = float.Parse(point1_Y.text);
        z = float.Parse(point1_Z.text);
        customPoints[1] = new Vector3(x, y, z);

        // Point 2 ÁÂÇ¥ ÀÐ±â
        x = float.Parse(point2_X.text);
        y = float.Parse(point2_Y.text);
        z = float.Parse(point2_Z.text);
        customPoints[2] = new Vector3(x, y, z);

        // Point 3 ÁÂÇ¥ ÀÐ±â
        x = float.Parse(point3_X.text);
        y = float.Parse(point3_Y.text);
        z = float.Parse(point3_Z.text);
        customPoints[3] = new Vector3(x, y, z);

        // Point 4 ÁÂÇ¥ ÀÐ±â
        x = float.Parse(point4_X.text);
        y = float.Parse(point4_Y.text);
        z = float.Parse(point4_Z.text);
        customPoints[4] = new Vector3(x, y, z);

        // Point 5 ÁÂÇ¥ ÀÐ±â
        x = float.Parse(point5_X.text);
        y = float.Parse(point5_Y.text);
        z = float.Parse(point5_Z.text);
        customPoints[5] = new Vector3(x, y, z);

        // Point 6 ÁÂÇ¥ ÀÐ±â
        x = float.Parse(point6_X.text);
        y = float.Parse(point6_Y.text);
        z = float.Parse(point6_Z.text);
        customPoints[6] = new Vector3(x, y, z);

        // Point 7 ÁÂÇ¥ ÀÐ±â
        x = float.Parse(point7_X.text);
        y = float.Parse(point7_Y.text);
        z = float.Parse(point7_Z.text);
        customPoints[7] = new Vector3(x, y, z);
    }


    public void LoadCurrentValuesToInputFields()
    {
        point0_X.text = _cubeStruct.point_0.x.ToString("F3");
        point0_Y.text = _cubeStruct.point_0.y.ToString("F3");
        point0_Z.text = _cubeStruct.point_0.z.ToString("F3");

        point1_X.text = _cubeStruct.point_1.x.ToString("F3");
        point1_Y.text = _cubeStruct.point_1.y.ToString("F3");
        point1_Z.text = _cubeStruct.point_1.z.ToString("F3");

        point2_X.text = _cubeStruct.point_2.x.ToString("F3");
        point2_Y.text = _cubeStruct.point_2.y.ToString("F3");
        point2_Z.text = _cubeStruct.point_2.z.ToString("F3");

        point3_X.text = _cubeStruct.point_3.x.ToString("F3");
        point3_Y.text = _cubeStruct.point_3.y.ToString("F3");
        point3_Z.text = _cubeStruct.point_3.z.ToString("F3");

        point4_X.text = _cubeStruct.point_4.x.ToString("F3");
        point4_Y.text = _cubeStruct.point_4.y.ToString("F3");
        point4_Z.text = _cubeStruct.point_4.z.ToString("F3");

        point5_X.text = _cubeStruct.point_5.x.ToString("F3");
        point5_Y.text = _cubeStruct.point_5.y.ToString("F3");
        point5_Z.text = _cubeStruct.point_5.z.ToString("F3");

        point6_X.text = _cubeStruct.point_6.x.ToString("F3");
        point6_Y.text = _cubeStruct.point_6.y.ToString("F3");
        point6_Z.text = _cubeStruct.point_6.z.ToString("F3");

        point7_X.text = _cubeStruct.point_7.x.ToString("F3");
        point7_Y.text = _cubeStruct.point_7.y.ToString("F3");
        point7_Z.text = _cubeStruct.point_7.z.ToString("F3");
    }
}
