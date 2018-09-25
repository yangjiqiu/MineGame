/*----------------------------------------------------------------
 * 文件名：Window
 * 文件功能描述：窗口
----------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Epitome.Utility
{
    public enum WinPosType
    {
        //显示窗口位置
        NO,//范围外
        Center,//中间
    }

    public class Window : MonoBehaviour
    {
        static Window mInstance;

        public static Window GetSingleton()
        {
            if (mInstance == null)
            {
                //尝试寻找该类的实例。此处不能用GameObject.Find，因为MonoBehaviour继承自Component。
                mInstance = UnityEngine.Object.FindObjectOfType(typeof(Window)) as Window;

                if (mInstance == null)
                {
                    GameObject tempGame = new GameObject("Window");
                    //防止被销毁
                    DontDestroyOnLoad(tempGame);
                    mInstance = tempGame.AddComponent<Window>();
                }
            }
            return mInstance;
        }

        //++++++++++++++++++++     窗口     ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        const uint SWP_SHOWWINDOW = 0x0040;
        const int GWL_STYLE = -16;
        const int WS_BORDER = 1;
        const int WS_POPUP = 0x800000;

        const int SW_SHOWMINIMIZED = 2; //{最小化, 激活}  
        const int SW_SHOWMAXIMIZED = 3;//最大化  
        const int SW_SHOWRESTORE = 1;//还原  

        /// <summary>
        /// 设置窗口分辨率
        /// </summary>
        public IEnumerator SetWinDPI(int varWidte, int varHeight, WinPosType varWinPosType = WinPosType.Center, bool varNoBorder = false)
        {
            bool tempResult = false;

            if (varNoBorder) //无边框后 边框黑线占一个像素
            {
                varWidte += 2;
                varHeight += 2;
            }

            do
            {
                int[] tempWinPos = GetNewWinPos(varWidte, varHeight , varWinPosType);

                if (varNoBorder)
                    SetWindowLong(GetForegroundWindow(), GWL_STYLE, WS_POPUP);//设置无边框框

                //设置 窗口位置居中 窗口分辨率
                tempResult = SetWindowPos(GetForegroundWindow(), 0, tempWinPos[0], tempWinPos[1], varWidte, varHeight, SWP_SHOWWINDOW);
                
                yield return new WaitForSeconds(0.02f);
            } while (!tempResult);
        }

        /// <summary>
        /// 设置窗口分辨率
        /// </summary>
        public IEnumerator SetWinDPI_New(int varWidte, int varHeight, bool varNoBorder = false)
        {
            bool tempResult = false;

            if (varNoBorder) //无边框后 边框黑线占一个像素
            {
                varWidte += 2;
                varHeight += 2;
            }

            int[] tempWinPos;

            do
            {
                if (varNoBorder)
                    SetWindowLong(GetForegroundWindow(), GWL_STYLE, WS_POPUP);//设置无边框框

                do
                {
                    tempWinPos = GetNewWinPos(varWidte, varHeight, WinPosType.NO);
                    tempResult = SetWindowPos(GetForegroundWindow(), 0, tempWinPos[0], tempWinPos[1], varWidte, varHeight, SWP_SHOWWINDOW);
                    yield return new WaitForSeconds(0.02f);
                } while (!tempResult);

                tempWinPos = GetNewWinPos(varWidte, varHeight, WinPosType.Center);

                //设置 窗口位置居中 窗口分辨率
                tempResult = SetWindowPos(GetForegroundWindow(), 0, tempWinPos[0], tempWinPos[1], varWidte, varHeight, SWP_SHOWWINDOW);

                yield return new WaitForSeconds(0.02f);
            } while (!tempResult);
        }

        /// <summary>
        /// 获取新窗口位置
        /// </summary>
        int[] GetNewWinPos(int varWidte,int varHeight, WinPosType varWinPosType)
        {
            //显示器支持的所有分辨率  
            int tempCount = Screen.resolutions.Length;

            //获取屏幕最大分辨率
            int tempResWidth = Screen.resolutions[tempCount - 1].width;
            int tempResHeight = Screen.resolutions[tempCount - 1].height;

            if (varWinPosType == WinPosType.Center)
            {
                //计算窗口坐标
                int tempWinPosX = tempResWidth / 2 - varWidte / 2;
                int tempWinPosY = tempResHeight / 2 - varHeight / 2;
                return new int[] { tempWinPosX, tempWinPosY };
            }
            else
            {
                return new int[] { tempResWidth, tempResHeight };
            }
        }

        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xf010;
        public const int HTCAPTION = 0x0002;

#if NGUI

        /// <summary>
        /// 拖动窗口
        /// </summary>
        public IEnumerator DragTheWindow()
        {
            bool tempWhetherDrag = false;   //是否拖动窗口

            Vector3 tempV3 = Vector3.zero;

            IntPtr tempHandle = GetForegroundWindow();

            while (true)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray tempRay = new Ray(UICamera.mainCamera.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, -10), new Vector3(0, 0, 1));
                    RaycastHit tempRaycastHit;

                    if (Physics.Raycast(tempRay, out tempRaycastHit, 20))
                    {
                        if (tempRaycastHit.collider.tag == "GameController")
                        {
                            tempWhetherDrag = true;
                            tempV3 = Input.mousePosition;
                        }
                    }
                }

                if (tempWhetherDrag)
                {
                    if (Vector3.Distance(tempV3, Input.mousePosition) > 1)
                    {
                        ReleaseCapture(); //释放鼠标捕捉 会使窗口的某些Mouse事件无法响应 
                        SendMessage(tempHandle, 0xA1, 0x02, 0); //发送左键点击的消息至该窗体(标题栏)
                        SendMessage(tempHandle, 0x0202, 0, 0);

                        tempWhetherDrag = false;
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    tempWhetherDrag = false;
                }

                yield return new WaitForSeconds(0.01f);
            }
        }

#endif

        /// <summary>
        /// 窗口最小化
        /// </summary>
        public void WindowMinimize()
        {
            ShowWindow(GetForegroundWindow(), SW_SHOWMINIMIZED);
        }

        /// <summary>
        /// 窗口最大化
        /// </summary>
        public void WindowMaximize()
        {
            ShowWindow(GetForegroundWindow(), SW_SHOWMAXIMIZED);
        }

        /// <summary>
        /// 窗口还原
        /// </summary>
        public void WindowRestore()
        {
            ShowWindow(GetForegroundWindow(), SW_SHOWRESTORE);
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        public void WindowShutDown()
        {
            Application.Quit();
        }
    }
}