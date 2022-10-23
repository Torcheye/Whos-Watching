using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", 
    menuName = "CameraDisplayScriptableObject", order = 1)]

public class CameraDisplayScriptableObject : ScriptableObject
{
    public List<Vector2> uiPos1;
    public List<Vector2> uiPos2;
    public List<Vector2> uiPos3;
    public List<Vector2> uiPos4;
    public List<Vector2> uiPos5;
    public List<Vector2> uiPos6;
    public List<Vector2> uiPos7;
    public List<Vector2> uiPos8;
    public List<float> uiSizes;
    
    public Vector2 GetUIPos(int number, int index)
    {
        return number switch
        {
            1 => uiPos1[index],
            2 => uiPos2[index],
            3 => uiPos3[index],
            4 => uiPos4[index],
            5 => uiPos5[index],
            6 => uiPos6[index],
            7 => uiPos7[index],
            8 => uiPos8[index],
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public Vector3 GetUISize(int number)
    {
        return uiSizes[number-1] * Vector3.one;
    }
}