using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEngine.UIElements;
using System.IO;
using System.Text.RegularExpressions;
public class getDesktopIcon : MonoBehaviour
{
    void Start()
    {
        DesktopUtil.GetIconNamesAndPoints(out string[] iconNames, out System.Drawing.Point[] iconPoints);
        int length = iconNames.Length;
        Debug.Log($"系统版本：{DesktopUtil.GetSysMajorVer()}");
        Debug.Log($"桌面图标数：{iconNames.Length}");
        Debug.Log($"位置数:{iconPoints.Length}");
        for(int i = 0; i < length; i++)
        {
            Debug.Log($"位置: {iconPoints[i]}，名字: {iconNames[i]}");
        }
    }

}
