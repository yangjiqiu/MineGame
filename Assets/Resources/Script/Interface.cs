/* =========================
 * 描 述：
 * 作 者：
 * 创建时间：2018/09/14 10:44:02
 * ========================= */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Epitome.Utility;

public class Interface : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(Window.GetSingleton().SetWinDPI_New(800, 600, true));
    }

    /// <summary>
    /// 经典扫雷
    /// </summary>
    public void ClassicMine(int varInt)
    {
        int tempX, tempY, tempCount;
        if (varInt == 1)
        {
            tempX = tempY = 16;
            tempCount = 40;
        }
        else if (varInt == 2)
        {
            tempX = 30;
            tempY = 16;
            tempCount = 99;
        }
        else
        {
            tempX = tempY = 9;
            tempCount = 10;
        }

        WorldInit._MapMatrixX = tempX;
        WorldInit._MapMatrixY = tempY;
        WorldInit._MapMineCount = tempCount;
        Project.GetSingleton().JumpScene(1);
    }

    public void Exit()
    {
        Application.Quit();
    }

}
