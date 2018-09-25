using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏初始化
/// </summary>
public class GameInit : MonoBehaviour
{
    private void Awake()
    {
        CanvasInit();
    }

    /// <summary>
    /// 画布初始化
    /// </summary>
    private void CanvasInit()
    {
        GameObject tempCanvas = new GameObject();
        tempCanvas.AddComponent<Canvas>();
    }
}
