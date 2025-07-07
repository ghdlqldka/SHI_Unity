using Dummiesman;
using PaintIn3D;
using System;
using System.Collections;
using System.IO;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
public class SimpleBlockShow : MonoBehaviour
{
    public GameObject Block;
    public GameObject Block_02;
    public GameObject Block_03;


    public void ShowBlock()
    {
        if (Block != null)
        {
            Block.SetActive(true);
            Block_02.SetActive(true);
            Block_03.SetActive(true);
        }
        

    }
    public void HideBlock()
    {
        if (Block != null)
        {
            Block.SetActive(false);
            Block_02.SetActive(false);
            Block_03.SetActive(false);
        }


    }
}