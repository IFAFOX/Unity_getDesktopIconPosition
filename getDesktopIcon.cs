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
        Debug.Log($"ϵͳ�汾��{DesktopUtil.GetSysMajorVer()}");
        Debug.Log($"����ͼ������{iconNames.Length}");
        Debug.Log($"λ����:{iconPoints.Length}");
        for(int i = 0; i < length; i++)
        {
            Debug.Log($"λ��: {iconPoints[i]}������: {iconNames[i]}");
        }
    }

}
