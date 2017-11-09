/* 物体移动类*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GoodsMove : MonoBehaviour
{

    private int nId = 0;                    //id
    private int raw = 0;                    //宽几个格子
    private int col = 0;                    //长几个格子
    private int nSort = 1;                  //保存原始的层级

    private float width = 0;                //建筑宽度
    private float height = 0;               //建筑高度

    private GoodsType gt;                   //物品类型
    private Vector2 oldMesh;                //低格的原始位置
    private Vector2 oldPos;                 //原始地址
    private Vector2 srcPos;                 //物体的原先位置
    private Vector2[] mesh_list;            //低格的所有位置
    private GameObject[] obj_list;          //低格对象

    private bool isSelf = false;            //判断是否是自己被点击
    private bool isEnterEdit = false;           //判断是否进入编辑模式
    private bool isClick = false;

    private GameObject MainCamera;          //摄像机
    private GameObject m_floorObj;          //floor 
    private int m_ft;                      //floor type
    private BuildManager m_buildManager;

    // Use this for initialization
    void Start()
    {
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //如果鼠标在宽度和高度之内的话则表示该建筑被点击
            if (Mathf.Abs(transform.position.x - temp.x) <= width / 2 &&
                Mathf.Abs(transform.position.y - temp.y) <= height / 2)
            {
                //被点击的id 为了防止上述判断导致多个建筑重叠
                if (MapBasicData.SelectID == 0)
                    MapBasicData.SelectID = nId;
                else
                    return;

                MapBasicData.snCurrId = MapBasicData.SelectID;
                MapBasicData.IsSelect = true;
                isSelf = true; isClick = true;

                srcPos = new Vector2(temp.x - transform.position.x, temp.y - transform.position.y);
                oldPos = transform.position;

                Invoke("Timer", 1.0f);
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (!isSelf || !isEnterEdit) return;

            Vector2 temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector2(temp.x - srcPos.x, temp.y - srcPos.y);
            showMeshColor();
            followCamera();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!isSelf) return;

            isClick = false;
            MapBasicData.IsSelect = false;
            MapBasicData.SelectID = 0;
            isSelf = false;

            if (!isEnterEdit)
            {
                return;
            }

            destroyBottomImg();
            OnMouseUp();
            //cs.SceneManager.Get().GetMainScene.LineActiveStatus(false);

            isEnterEdit = false;
        }
    }

    /*
     * 检测点击 编辑模式使用的func
     * @param EventType 事件类型
     */
    public void OnCheck(EventType et)
    {

        if (et == EventType.mouseDown)
        {
            OnMouseDown();
        }
        else if (et == EventType.mouseDrag)
        {
            OnMouseMove();
        }
        else if (et == EventType.mouseUp)
        {
            if (gt != GoodsType.FLOOR)
                OnMouseUp();
            else
            {
                DestroyImmediate(m_floorObj);
                //MapBasicData.LoadFloorFile(m_buildManager.FloorTypeList);
            }
        }
    }

    //鼠标按下
    public void OnMouseDown()
    {
        if (gt != GoodsType.FLOOR)
        {
            oldPos = transform.position;
            nSort = transform.GetComponent<Renderer>().sortingOrder;
        }
        else
        {
            m_floorObj.transform.localScale = new Vector2(1, 1);
        }

        print(" down ----");
    }

    //鼠标移动
    public void OnMouseMove()
    {
        if (gt != GoodsType.FLOOR)
            transform.GetComponent<Renderer>().sortingOrder = 1000;
        else
        {
            m_buildManager.CheckDifferentFloor(m_ft, transform.position);
        }
    }

    //鼠标放开
    private void OnMouseUp()
    {
        transform.GetComponent<Renderer>().sortingOrder = nSort;
        InitMeshList();
        if (IsOverOtherHouse())
        {
            transform.position = oldPos;
            transform.GetComponent<Renderer>().sortingOrder = nSort;
        }
        else
        {
            putOnIt();
            m_buildManager.ChangeObjSort(mesh_list, transform.position, nId);
        }
    }

    //设置位置
    private void putOnIt()
    {
        for (int i = 0; i < 6400; i++)
        {
            if (Mathf.Abs(transform.position.x - MapBasicData.map_list[i].x) <= 0.35f && Mathf.Abs(transform.position.y - MapBasicData.map_list[i].y) <= 0.2f)
            {
                transform.position = MapBasicData.map_list[i];
                InitMeshList();
                break;
            }
        }
    }

    //初始化低格的位置
    void InitMeshList()
    {
        Vector2 tempVec = new Vector2(transform.position.x - width / 2 + oldMesh.x, transform.position.y - height / 2 + oldMesh.y);

        int nNum = 0;
        for (int i = 0; i < raw; i++)
        {
            for (int j = 0; j < col; j++)
            {
                mesh_list[nNum++] = new Vector2(tempVec.x + j * 0.7f / 2 + i * 0.7f / 2, tempVec.y + j * 0.4f / 2 - i * 0.4f / 2);
            }
        }
    }

    /*
     * 判断是否跟其他的尖子重叠
     * @param GoodsInfo 建筑的基本信息
     */
    bool isOverHouse(GoodsInfo gi)
    {
        for (int i = 0; i < mesh_list.Length; i++)
        {
            for (int j = 0; j < gi.mesh_list.Length; j++)
            {
                if (Mathf.Abs(mesh_list[i].x - gi.mesh_list[j].x) <= 0.35f && Mathf.Abs(mesh_list[i].y - gi.mesh_list[j].y) <= 0.2f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool IsOverOtherHouse()
    {
        if (m_buildManager.GoodsList.Count == 0) return false;

        for (int i = 0; i < m_buildManager.GoodsList.Count; i++)
        {
            GoodsInfo gi = (GoodsInfo)m_buildManager.GoodsList[i];
            if (gi.nID != nId)
            {
                if (isOverHouse(gi))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// 判断是否跟其他的建筑触碰到
    /// @param vector2 本身的位置
    /// @param GoodsInfo 其他的建筑信息
    bool isCollider(Vector2 vc, GoodsInfo gi)
    {
        for (int i = 0; i < gi.mesh_list.Length; i++)
        {
            if (Mathf.Abs(vc.x - gi.mesh_list[i].x) <= 0.35f && Mathf.Abs(vc.y - gi.mesh_list[i].y) <= 0.2)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    ///  显示底部格子的状态并且根据不同的状态显示对应的颜色
    /// </summary>
    void showMeshColor()
    {
        InitMeshList();
        bool isCollid = false;
        for (int i = 0; i < m_buildManager.GoodsList.Count; i++)
        {
            GoodsInfo gi = (GoodsInfo)m_buildManager.GoodsList[i];
            if (nId != gi.nID && isOverHouse(gi))
            {
                isCollid = true;
                for (int j = 0; j < mesh_list.Length; j++)
                {
                    Renderer render = obj_list[j].GetComponent<Renderer>();
                    render.material.color = isCollider(mesh_list[j], gi) ? Color.red : Color.green;
                }
            }
        }

        //判断是否碰撞 如果没有碰撞 则回复原来的颜色
        if (!isCollid)
        {
            for (int j = 0; j < mesh_list.Length; j++)
            {
                Renderer render = obj_list[j].GetComponent<Renderer>();
                render.material.color = Color.green;
            }
        }
    }

    void followCamera()
    {
        float left = MapBasicData.BuildLeft - MapBasicData.CameraLeft + width / 2;
        float right = MapBasicData.BuildRight - MapBasicData.CameraRight - width / 2;
        float up = MapBasicData.BuildUp - MapBasicData.CameraUp - height / 2;
        float down = MapBasicData.BuildDown - MapBasicData.CameraDown + height / 2;

        //left
        if (transform.position.x - MainCamera.transform.position.x <= left)
        {
            transform.position = new Vector2(MainCamera.transform.position.x + left, transform.position.y);
            if (MainCamera.transform.position.x >= MapBasicData.CameraLeft + 0.15f)
            {
                MainCamera.transform.position -= new Vector3(0.15f, 0, 0);
                transform.position = new Vector3(MainCamera.transform.position.x + left, transform.position.y, 0);
            }
        }

        //right
        if (transform.position.x - MainCamera.transform.position.x >= right)
        {
            transform.position = new Vector2(MainCamera.transform.position.x + right, transform.position.y);
            if (MainCamera.transform.position.x <= MapBasicData.CameraRight - 0.15f)
            {
                MainCamera.transform.position += new Vector3(0.15f, 0, 0);
                transform.position = new Vector3(MainCamera.transform.position.x + right, transform.position.y, 0);
            }
        }

        //up
        if (transform.position.y - MainCamera.transform.position.y >= up)
        {
            transform.position = new Vector2(transform.position.x, MainCamera.transform.position.y + up);
            if (MainCamera.transform.position.y <= MapBasicData.CameraUp - 0.1f)
            {
                MainCamera.transform.position += new Vector3(0, 0.1f, 0);
                transform.position = new Vector2(transform.position.x, MainCamera.transform.position.y + up);
            }
        }

        //down
        if (transform.position.y - MainCamera.transform.position.y <= down)
        {
            transform.position = new Vector2(transform.position.x, MainCamera.transform.position.y + down);
            if (MainCamera.transform.position.y >= MapBasicData.CameraDown + 0.1f)
            {
                MainCamera.transform.position -= new Vector3(0, 0.1f, 0);
                transform.position = new Vector2(transform.position.x, MainCamera.transform.position.y + down);
            }
        }
    }


    /// <summary>
    /// 初始化数据
    /// @param HouseData 房子的基本数据
    /// @param GoodsTYpe 建筑的基本类型
    /// @param name      建筑名字
    /// @param nId       建筑的id
    /// </summary>
    public void InitData(string name, int id, BuildManager bm = null)
    {
        gt = MapBasicData.GetHouseType(name);
        HouseData hd = MapBasicData.GetHouseData(gt);

        raw = hd.raw;
        col = hd.col;
        width = hd.width;
        height = hd.height;
        oldMesh = hd.srcMesh;
        mesh_list = new Vector2[raw * col];
        obj_list = new GameObject[raw * col];
        nId = id;

        InitMeshList();
        m_buildManager = bm;
        transform.GetComponent<Renderer>().sortingOrder = m_buildManager.GoodsList.Count;
        m_buildManager.AddGoods(nId, gt, gameObject, mesh_list, oldMesh, name);
    }

    //set goods info
    public void SetGoodsInfo(GameObject tempObj, GoodsType tempGt, int ft, BuildManager buildManager)
    {
        m_floorObj = tempObj;
        gt = tempGt;
        m_ft = ft;
        m_buildManager = buildManager;
    }

    //创建底部的格子
    void createBottomImg()
    {
        Vector2 tempVec = new Vector2(transform.position.x - width / 2 + oldMesh.x, transform.position.y - height / 2 + oldMesh.y);
        int nNum = 0;
        for (int i = 0; i < raw; i++)
        {
            for (int j = 0; j < col; j++)
            {
                obj_list[nNum] = Instantiate(Resources.Load("Main/Prefab/floor/bottom_floor")) as GameObject;
                obj_list[nNum].transform.SetParent(transform, true);
                Renderer render = obj_list[nNum].GetComponent<Renderer>();
                render.material.color = Color.green;
                obj_list[nNum].transform.position = new Vector2(tempVec.x + j * 0.7f / 2 + i * 0.7f / 2, tempVec.y + j * 0.4f / 2 - i * 0.4f / 2);
                nNum++;
            }
        }
    }

    //销毁底部的格子
    void destroyBottomImg()
    {
        for (int i = 0; i < obj_list.Length; i++)
        {
            Destroy(obj_list[i]);
            obj_list[i] = null;
        }
    }

    void Timer()
    {
        if (!isClick) return;

        isEnterEdit = true;
        nSort = transform.GetComponent<Renderer>().sortingOrder;
        transform.GetComponent<Renderer>().sortingOrder = 1000;
        createBottomImg();
        //cs.SceneManager.Get().GetMainScene.LineActiveStatus(true);
    }
}
