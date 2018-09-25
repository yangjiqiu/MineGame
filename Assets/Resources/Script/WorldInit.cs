/* =========================
 * 描 述：游戏初始化
 * 作 者：杨继求
 * 创建时间：2018/09/11 14:52:36
 * ========================= */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Epitome.Utility;
using UnityEngine.EventSystems;

public class WorldInit : MonoBehaviour
{
    public GameObject mMineField; //雷区：所有雷块父物体

    int[,] mMinefieldsData; //雷区数据 -2：墙 -1：地雷 0：周边无雷 1-8：周边雷数量
    int[,] mMinefieldsStart; //雷区状态  -1：排除 0：无操作 1：标记

    //状态图标
    public Sprite[] mMineCount; // 0：周边无雷 1-8：周边雷数量
    public Sprite mSign; // 标记
    public Sprite mMine; // 雷
    public Sprite mChunk; // 原块

    public static int _MapMatrixX, _MapMatrixY, _MapMineCount;

    int mMapMatrixX, mMapMatrixY; //地图矩阵长度
    int mMapMineCount; //地图地雷数量
    int mSurplusMineCount; // 剩余地雷数

    public Text mShowMineCoun; //显示地雷数
    public Text mShowGameTime; //显示游戏时间

    bool IfSignMine; // 是否标记地雷
    bool IfGmaeEnd = true;

    public GameObject mEndInterface; // 结束界面

    private void Awake()
    {
        IfSignMine = false;

        //初始化
        mMapMatrixX = _MapMatrixX;
        mMapMatrixY = _MapMatrixY;
        mMapMineCount = mSurplusMineCount = _MapMineCount;

        mShowMineCoun.text = mSurplusMineCount.ToString();
        mShowGameTime.text = "0";
    }

    private void Start()
    {
        StartCoroutine(Window.GetSingleton().SetWinDPI_New(mMapMatrixX * 30, mMapMatrixY * 30 + 40, true));
        MineFieldInit(mMapMatrixX * 30, mMapMatrixY * 30 + 40);
    }

    float mGameTime;
    float mCompareTime = 1;

    private void Update()
    {
        mGameTime += Time.deltaTime;
        if (mGameTime > mCompareTime && IfGmaeEnd && mCompareTime < 10000)
        {
            mShowGameTime.text = mCompareTime.ToString();
            mCompareTime += 1;
        }
    }

    /// <summary>
    /// 雷区初始化 varInterval:块之间的间隔
    /// </summary>
    void MineFieldInit(float varWidth, float varHeight,int varInterval = 4)
    {
        //int tempYPixel = (int)(varHeight / mMapMatrixY); //计算X轴所需最大块像素
        //int tempXPixel = (int)(varWidth / mMapMatrixX); //计算Y轴所需最大块像素

        //int tempPixel = tempXPixel < tempYPixel ? tempXPixel : tempYPixel; //取最小像素块
        int tempPixel = 30;

        Transform tempMineLump = mMineField.transform.Find("MineChunk"); //获取块模板
        tempMineLump.GetComponent<RectTransform>().sizeDelta = new Vector2(tempPixel - varInterval, tempPixel - varInterval); //设置块大小像素

        TypeSetting(mMapMatrixX, mMapMatrixY, tempPixel, tempMineLump);
    }

    /// <summary>
    /// 排版
    /// </summary>
    void TypeSetting(int varX, int varY, int varPixel, Transform varMineLump)
    {
        for (int i = 0; i < varX; i++)
        {
            for (int j = 0; j < varY; j++)
            {
                // 克隆
                GameObject tempGame = Instantiate(varMineLump, mMineField.transform).gameObject;
                tempGame.name = i + "_" + j;
                // 给雷块添加点击事件 + 回调
                UIEventListener tempUIEventListener = tempGame.gameObject.AddComponent<UIEventListener>();
                tempUIEventListener.OnMouseClick += Ret;
                // 设置位置像素大小等
                RectTransform tempRect = tempGame.transform.GetComponent<RectTransform>();
                tempRect.anchoredPosition3D = new Vector3(i * varPixel + 2, j * varPixel + 2, 0); //设置块像素
                tempRect.localScale = Vector3.one;

                tempGame = null;
            }
        }

        BuriedMine(mMapMatrixX, mMapMatrixY, mMapMineCount);
    }

    /// <summary>
    /// 填充数据
    /// </summary>
    void BuriedMine(int varX, int varY, int varMineCount)
    {
        mMinefieldsData = new int[varX, varY];
        mMinefieldsStart = new int[varX, varY];

        int tempMineCount = 0;
        int tempX_Index;
        int tempY_Index;
        // 填充地雷
        do
        {
            // 随机某区域
            tempX_Index = Random.Range(0, varX);
            tempY_Index = Random.Range(0, varY);

            if (mMinefieldsData[tempX_Index, tempY_Index] != -1) //当区块没有地雷时可填充地雷
            {
                mMinefieldsData[tempX_Index, tempY_Index] = -1;

                if (IfSignMine)
                {
                    // 标记地雷 测试使用
                    Image tempImage = GetChunkImage(tempX_Index, tempY_Index);
                    if (tempImage != null) tempImage.color = Color.red;
                    tempImage = null;
                }

                for (int i = (tempX_Index == 0 ? tempX_Index : tempX_Index - 1); i <= (tempX_Index == mMapMatrixX ? tempX_Index : tempX_Index + 1); i++)
                {
                    for (int j = (tempY_Index == 0 ? tempY_Index : tempY_Index - 1); j <= (tempY_Index == mMapMatrixY ? tempY_Index : tempY_Index + 1); j++)
                    {
                        if (i == mMapMatrixX || j == mMapMatrixY) continue;
                        if (mMinefieldsData[i, j] != -1)
                        {
                            mMinefieldsData[i, j] += 1;
                        }
                    }
                }
                tempMineCount += 1;
            }
        } while (tempMineCount < varMineCount);

        tempMineCount = tempX_Index = tempY_Index = 0;
    }

    void Ret(GameObject varGO, PointerEventData varData)
    {
        if (!varGO.tag.Contains("GameBlock")) //不是雷块时
            return;

        string[] tempStr = varGO.name.Split('_');

        int tempX = int.Parse(tempStr[0]);
        int tempY = int.Parse(tempStr[1]);

        if (varData.pointerId == -1) // 鼠标左键
        {
            if (mMinefieldsStart[tempX, tempY] != 1 && mMinefieldsData[tempX, tempY] == -1)
            {
                Debug.Log("游戏结束");
                //显示所有
                for (int i = 0; i < mMinefieldsData.GetLength(0); i++)
                {
                    for (int j = 0; j < mMinefieldsData.GetLength(1); j++)
                    {
                        if (mMinefieldsData[i, j] == -1)
                            SweepShow(i, j, true);
                    }
                }

                EndInterface(false);
            }
            else 
            {
                SweepShow(tempX, tempY, true);
            }
        }
        else if (varData.pointerId == -2)// 鼠标左键
        {
            MarkIcon(tempX, tempY);

            if (mSurplusMineCount == 0)  //剩余雷 == 0 时
            {
                if (DetectionMine())
                {
                    Debug.Log("游戏成功");
                    EndInterface(true);
                }
            }
        }
    }

    /// <summary>
    /// 检测地雷是否全部完成
    /// </summary>
    bool DetectionMine()
    {
        for (int i = 0; i < mMinefieldsStart.GetLength(0); i++)
        {
            for (int j = 0; j < mMinefieldsStart.GetLength(1); j++)
            {
                // 当（i, j）没有排除时 显示
                if (mMinefieldsStart[i, j] == 1)
                {
                    if (mMinefieldsData[i, j] != -1) //标记地方不是雷
                        return false;
                }
            }
        }
        return true;
    }

    int DetectionMineCount()
    {
        int tempCount = 0;

        for (int i = 0; i < mMinefieldsStart.GetLength(0); i++)
        {
            for (int j = 0; j < mMinefieldsStart.GetLength(1); j++)
            {
                // 当（i, j）没有排除时 显示
                if (mMinefieldsStart[i, j] == 1)
                {
                    if (mMinefieldsData[i, j] == -1) //标记地方不是雷
                        tempCount+=1;
                }
            }
        }
        return tempCount;
    }

    /// <summary>
    /// 扫雷显示
    /// </summary>
    void SweepShow(int varX,int varY,bool varBool = false)
    {
        int tempSafetyZone = mMinefieldsStart[varX, varY]; //雷块状态
        int tempMinefieldsMatrix = mMinefieldsData[varX, varY]; //雷块数据

        // 没有排除
        if (tempSafetyZone != -1 && (tempSafetyZone != 1 || !varBool))
        {
            Image tempImage = GetChunkImage(varX, varY);
            if (tempImage == null) return;

            if (tempMinefieldsMatrix == -1)
            {
                tempImage.sprite = mMine;
                mMinefieldsStart[varX, varY] = -1;
            }
            else if (tempMinefieldsMatrix >= 0 && tempMinefieldsMatrix < 9)
            {
                tempImage.sprite = mMineCount[tempMinefieldsMatrix];
                mMinefieldsStart[varX, varY] = -1;

                Demining(varX, varY);

                tempImage.raycastTarget = false; //显示后 不可触发检测
            }
        }
    }

    /// <summary>
    /// 排雷:只有在周边没雷时继续向外围进行排雷
    /// </summary>
    void Demining(int varX, int varY)
    {
        if (mMinefieldsData[varX, varY] != 0) return; 

        // 获取（varX，varY）周边八个点 && 限制数组超出范围
        for (int i = (varX == 0 ? varX : varX - 1); i <= (varX == mMapMatrixX ? varX : varX + 1); i++)
        {
            for (int j = (varY == 0 ? varY : varY - 1); j <= (varY == mMapMatrixY ? varY : varY + 1); j++)
            {
                if (i == mMapMatrixX || j == mMapMatrixY) continue;
                // 当（i, j）没有排除时 显示
                if (mMinefieldsStart[i, j] != -1)
                    SweepShow(i, j);
            }
        }
    }

    /// <summary>
    /// 标记图标
    /// </summary>
    void MarkIcon(int varX, int varY)
    {
        // 当（varX, varY）没有排除显示时
        if (mMinefieldsStart[varX, varY] != -1)
        {
            Image tempImage = GetChunkImage(varX, varY);
            if (tempImage == null) return;

            // 当（varX, varY）无操作 && 剩余雷数>零
            if (mMinefieldsStart[varX, varY] == 0 && mSurplusMineCount > 0)
            {
                tempImage.sprite = mSign;//替换标记图标
                mMinefieldsStart[varX, varY] = 1;//添加标记
                mSurplusMineCount -= 1;
            }
            // 当（varX, varY）有标记 && 剩余雷数<mMapMineCount(初始雷数)
            else if (mMinefieldsStart[varX, varY] == 1 && mSurplusMineCount < mMapMineCount)
            {
                tempImage.sprite = mChunk;//替换块原图
                mMinefieldsStart[varX, varY] = 0;//设置无操作
                mSurplusMineCount += 1;
            }

            mShowMineCoun.text = mSurplusMineCount.ToString();
        }
    }

    /// <summary>
    /// 获取块图标
    /// </summary>
    /// <returns></returns>
    Image GetChunkImage(int varX, int varY)
    {
        Transform tempTrans = mMineField.transform.Find(string.Format("{0}{1}{2}", varX, "_", varY));
        if (tempTrans != null)
        {
            Image tempImage = tempTrans.GetComponent<Image>();
            return tempImage;
        }
        return null;
    }

    /// <summary>
    /// 结束界面
    /// </summary>
    void EndInterface(bool varResult)
    {
        IfGmaeEnd = false;

        string tempStr;
        if (varResult)
            tempStr = string.Format("挑战成功\n成功排雷{0}个,真不错", mMapMineCount);
        else
            tempStr = string.Format("挑战失败\n成功排雷{0}个,再接再厉", DetectionMineCount());

        mEndInterface.transform.Find("Text").GetComponent<Text>().text = tempStr;
        mEndInterface.SetActive(true);
    }

    /// <summary>
    /// 再来一次
    /// </summary>
    public void TryAgain()
    {
        Project.GetSingleton().JumpScene(1);
    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    public void Return()
    {
        Project.GetSingleton().JumpScene(0);
    }

    private void OnApplicationQuit()
    {
        StartCoroutine(Window.GetSingleton().SetWinDPI(800, 600,WinPosType.NO));
    }
}