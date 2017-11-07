using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SGameGraphicRaycaster : GraphicRaycaster
{
    [CompilerGenerated]
    private static Comparison<Graphic> <>f__am$cacheC;
    private Canvas canvas_;
    private Vector3[] corners = new Vector3[4];
    private List<Item> m_allItems = new List<Item>();
    [NonSerialized]
    private List<Graphic> m_RaycastResults = new List<Graphic>();
    private int m_screenHeight;
    private int m_screenWidth;
    private Tile[] m_tiles;
    private int m_tileSizeX;
    private int m_tileSizeY;
    private int raycast_mask = 4;
    public RaycastMode raycastMode = RaycastMode.Sgame;
    private const int TileCount = 4;
    [NonSerialized, HideInInspector]
    public bool tilesDirty;

    private void AddToTileList(Item item)
    {
        int num = item.coord.x + (item.coord.y * 4);
        for (int i = 0; i < item.coord.numX; i++)
        {
            for (int j = 0; j < item.coord.numY; j++)
            {
                int index = ((j * 4) + i) + num;
                this.m_tiles[index].items.Add(item);
            }
        }
    }

    private void AppendResultList(ref Ray ray, float hitDistance, List<RaycastResult> resultAppendList, List<Graphic> raycastResults)
    {
        for (int i = 0; i < raycastResults.Count; i++)
        {
            GameObject gameObject = raycastResults[i].gameObject;
            bool flag = true;
            if (base.ignoreReversedGraphics)
            {
                if (this.eventCamera == null)
                {
                    Vector3 rhs = (Vector3) (gameObject.transform.rotation * Vector3.forward);
                    flag = Vector3.Dot(Vector3.forward, rhs) > 0f;
                }
                else
                {
                    Vector3 lhs = (Vector3) (this.eventCamera.transform.rotation * Vector3.forward);
                    Vector3 vector3 = (Vector3) (gameObject.transform.rotation * Vector3.forward);
                    flag = Vector3.Dot(lhs, vector3) > 0f;
                }
            }
            if (flag)
            {
                float num2 = 0f;
                if ((this.eventCamera == null) || (this.canvas.renderMode == RenderMode.ScreenSpaceOverlay))
                {
                    num2 = 0f;
                }
                else
                {
                    num2 = Vector3.Dot(gameObject.transform.forward, gameObject.transform.position - ray.origin) / Vector3.Dot(gameObject.transform.forward, ray.direction);
                    if (num2 < 0f)
                    {
                        continue;
                    }
                }
                if (num2 < hitDistance)
                {
                    RaycastResult item = new RaycastResult {
                        gameObject = gameObject,
                        module = this,
                        distance = num2,
                        index = resultAppendList.Count,
                        depth = raycastResults[i].depth,
                        sortingLayer = this.canvas.sortingLayerID,
                        sortingOrder = this.canvas.sortingOrder
                    };
                    resultAppendList.Add(item);
                }
            }
        }
    }

    private void CalcItemCoord(ref Coord coord, Item item)
    {
        item.rectTransform.GetWorldCorners(this.corners);
        int b = 0x7fffffff;
        int num2 = -2147483648;
        int num3 = 0x7fffffff;
        int num4 = -2147483648;
        Camera worldCamera = this.canvas.worldCamera;
        for (int i = 0; i < this.corners.Length; i++)
        {
            Vector3 vector = (Vector3) CUIUtility.WorldToScreenPoint(worldCamera, this.corners[i]);
            b = Mathf.Min((int) vector.x, b);
            num2 = Mathf.Max((int) vector.x, num2);
            num3 = Mathf.Min((int) vector.y, num3);
            num4 = Mathf.Max((int) vector.y, num4);
        }
        coord.x = Mathf.Clamp(b / this.m_tileSizeX, 0, 3);
        coord.numX = (Mathf.Clamp(num2 / this.m_tileSizeX, 0, 3) - coord.x) + 1;
        coord.y = Mathf.Clamp(num3 / this.m_tileSizeY, 0, 3);
        coord.numY = (Mathf.Clamp(num4 / this.m_tileSizeY, 0, 3) - coord.y) + 1;
    }

    public void InitTiles()
    {
        if (this.m_tiles == null)
        {
            this.m_tiles = new Tile[0x10];
            for (int i = 0; i < this.m_tiles.Length; i++)
            {
                this.m_tiles[i] = new Tile();
            }
            this.m_screenWidth = Screen.width;
            this.m_screenHeight = Screen.height;
            this.m_tileSizeX = this.m_screenWidth / 4;
            this.m_tileSizeY = this.m_screenHeight / 4;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        this.m_allItems.Clear();
        this.m_RaycastResults.Clear();
        if (this.m_tiles != null)
        {
            for (int i = 0; i < this.m_tiles.Length; i++)
            {
                this.m_tiles[i].items.Clear();
            }
            this.m_tiles = null;
        }
    }

    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        switch (this.raycastMode)
        {
            case RaycastMode.Unity:
                base.Raycast(eventData, resultAppendList);
                break;

            case RaycastMode.Sgame:
                this.Raycast2(eventData, resultAppendList, false);
                break;

            case RaycastMode.Sgame_tile:
                this.Raycast2(eventData, resultAppendList, true);
                break;
        }
    }

    private void Raycast2(PointerEventData eventData, List<RaycastResult> resultAppendList, bool useTiles)
    {
        if (this.canvas != null)
        {
            Vector2 vector;
            if (this.eventCamera == null)
            {
                vector = new Vector2(eventData.position.x / ((float) Screen.width), eventData.position.y / ((float) Screen.height));
            }
            else
            {
                vector = this.eventCamera.ScreenToViewportPoint((Vector3) eventData.position);
            }
            if (((vector.x >= 0f) && (vector.x <= 1f)) && ((vector.y >= 0f) && (vector.y <= 1f)))
            {
                float maxValue = float.MaxValue;
                Ray ray = new Ray();
                if (this.eventCamera != null)
                {
                    ray = this.eventCamera.ScreenPointToRay((Vector3) eventData.position);
                }
                this.m_RaycastResults.Clear();
                Vector2 position = eventData.position;
                List<Item> items = null;
                if (useTiles && (this.m_tiles != null))
                {
                    int num2 = Mathf.Clamp(((int) position.x) / this.m_tileSizeX, 0, 3);
                    int index = (Mathf.Clamp(((int) position.y) / this.m_tileSizeY, 0, 3) * 4) + num2;
                    items = this.m_tiles[index].items;
                }
                else
                {
                    items = this.m_allItems;
                }
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].Raycast(this.m_RaycastResults, position, this.eventCamera);
                }
                if (<>f__am$cacheC == null)
                {
                    <>f__am$cacheC = (g1, g2) => g2.depth.CompareTo(g1.depth);
                }
                this.m_RaycastResults.Sort(<>f__am$cacheC);
                this.AppendResultList(ref ray, maxValue, resultAppendList, this.m_RaycastResults);
            }
        }
    }

    public void RefreshEventScript(CUIComponent eventScript)
    {
        if ((eventScript != null) && (this.m_allItems != null))
        {
            Item item = null;
            for (int i = 0; i < this.m_allItems.Count; i++)
            {
                if (this.m_allItems[i].owner == eventScript)
                {
                    item = this.m_allItems[i];
                    break;
                }
            }
            if (item != null)
            {
                if (this.raycastMode == RaycastMode.Sgame_tile)
                {
                    this.RemoveFromTileList(item);
                }
            }
            else
            {
                item = Item.Create(eventScript);
                if (item == null)
                {
                    return;
                }
                this.m_allItems.Add(item);
            }
            if (this.raycastMode == RaycastMode.Sgame_tile)
            {
                this.CalcItemCoord(ref item.coord, item);
                this.AddToTileList(item);
            }
        }
    }

    public void RefreshGameObject(GameObject go)
    {
        if ((go != null) && (this.m_allItems != null))
        {
            CUIEventScript[] componentsInChildren = go.GetComponentsInChildren<CUIEventScript>(true);
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                this.RefreshEventScript(componentsInChildren[i]);
            }
            CUIMiniEventScript[] scriptArray2 = go.GetComponentsInChildren<CUIMiniEventScript>(true);
            for (int j = 0; j < scriptArray2.Length; j++)
            {
                this.RefreshEventScript(scriptArray2[j]);
            }
        }
    }

    public void RemoveEventScript(CUIComponent eventScript)
    {
        if ((eventScript != null) && (this.m_allItems != null))
        {
            Item item = null;
            for (int i = 0; i < this.m_allItems.Count; i++)
            {
                if (this.m_allItems[i].owner == eventScript)
                {
                    item = this.m_allItems[i];
                    if (this.raycastMode == RaycastMode.Sgame_tile)
                    {
                        this.RemoveFromTileList(item);
                    }
                    this.m_allItems.RemoveAt(i);
                    break;
                }
            }
        }
    }

    private void RemoveFromTileList(Item item)
    {
        if (item.coord.IsValid())
        {
            int num = item.coord.x + (item.coord.y * 4);
            for (int i = 0; i < item.coord.numX; i++)
            {
                for (int j = 0; j < item.coord.numY; j++)
                {
                    int index = ((j * 4) + i) + num;
                    this.m_tiles[index].items.Remove(item);
                }
            }
            item.coord = Coord.Invalid;
        }
    }

    public void RemoveGameObject(GameObject go)
    {
        if ((go != null) && (this.m_allItems != null))
        {
            CUIEventScript[] componentsInChildren = go.GetComponentsInChildren<CUIEventScript>(true);
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                this.RemoveEventScript(componentsInChildren[i]);
            }
            CUIMiniEventScript[] scriptArray2 = go.GetComponentsInChildren<CUIMiniEventScript>(true);
            for (int j = 0; j < scriptArray2.Length; j++)
            {
                this.RemoveEventScript(scriptArray2[j]);
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        foreach (CUIEventScript script in base.gameObject.GetComponentsInChildren<CUIEventScript>(true))
        {
            Item item = Item.Create(script);
            if (item != null)
            {
                this.m_allItems.Add(item);
            }
        }
        foreach (CUIMiniEventScript script2 in base.gameObject.GetComponentsInChildren<CUIMiniEventScript>(true))
        {
            Item item2 = Item.Create(script2);
            if (item2 != null)
            {
                this.m_allItems.Add(item2);
            }
        }
        if (this.raycastMode == RaycastMode.Sgame_tile)
        {
            this.InitTiles();
            for (int i = 0; i < this.m_allItems.Count; i++)
            {
                Item item3 = this.m_allItems[i];
                this.CalcItemCoord(ref item3.coord, item3);
                this.AddToTileList(item3);
            }
        }
    }

    public void UpdateTiles()
    {
        if (this.raycastMode == RaycastMode.Sgame_tile)
        {
            Coord invalid = Coord.Invalid;
            for (int i = 0; i < this.m_allItems.Count; i++)
            {
                Item item = this.m_allItems[i];
                this.CalcItemCoord(ref invalid, item);
                if (!invalid.Equals(ref item.coord))
                {
                    this.RemoveFromTileList(item);
                    item.coord = invalid;
                    this.AddToTileList(item);
                }
            }
        }
    }

    private void UpdateTiles_Editor()
    {
        if (((this.m_screenWidth != Screen.width) || (this.m_screenHeight != Screen.height)) && ((this.m_tiles != null) && (this.raycastMode == RaycastMode.Sgame_tile)))
        {
            this.m_screenWidth = Screen.width;
            this.m_screenHeight = Screen.height;
            this.m_tileSizeX = this.m_screenWidth / 4;
            this.m_tileSizeY = this.m_screenHeight / 4;
            for (int i = 0; i < this.m_tiles.Length; i++)
            {
                this.m_tiles[i].items.Clear();
            }
            for (int j = 0; j < this.m_allItems.Count; j++)
            {
                Item item = this.m_allItems[j];
                this.CalcItemCoord(ref item.coord, item);
                this.AddToTileList(item);
            }
        }
    }

    private Canvas canvas
    {
        get
        {
            if (this.canvas_ == null)
            {
                this.canvas_ = base.GetComponent<Canvas>();
            }
            return this.canvas_;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Coord
    {
        public int x;
        public int y;
        public int numX;
        public int numY;
        public static SGameGraphicRaycaster.Coord Invalid;
        static Coord()
        {
            SGameGraphicRaycaster.Coord coord = new SGameGraphicRaycaster.Coord {
                x = -1,
                y = -1
            };
            Invalid = coord;
        }

        public bool IsValid()
        {
            return ((this.x >= 0) && (this.y >= 0));
        }

        public bool Equals(ref SGameGraphicRaycaster.Coord r)
        {
            return ((((r.x == this.x) && (r.y == this.y)) && (r.numX == this.numX)) && (r.numY == this.numY));
        }
    }

    private class Item
    {
        public SGameGraphicRaycaster.Coord coord = SGameGraphicRaycaster.Coord.Invalid;
        public Image image;
        public List<Image> imagesInChildren;
        public CUIComponent owner;
        public RectTransform rectTransform;

        private static void AppendImagesInChildren(SGameGraphicRaycaster.Item item, GameObject child)
        {
            RectTransform transform = child.transform as RectTransform;
            if ((transform != null) && ((child.GetComponent<CUIEventScript>() == null) && (child.GetComponent<CUIMiniEventScript>() == null)))
            {
                Image component = child.GetComponent<Image>();
                if (component != null)
                {
                    if (item.imagesInChildren == null)
                    {
                        item.imagesInChildren = new List<Image>();
                    }
                    item.imagesInChildren.Add(component);
                }
                int childCount = transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    AppendImagesInChildren(item, transform.GetChild(i).gameObject);
                }
            }
        }

        public static SGameGraphicRaycaster.Item Create(CUIComponent owner)
        {
            if (owner == null)
            {
                return null;
            }
            RectTransform transform = owner.transform as RectTransform;
            if (transform == null)
            {
                return null;
            }
            SGameGraphicRaycaster.Item item = new SGameGraphicRaycaster.Item {
                owner = owner,
                rectTransform = transform,
                image = owner.GetComponent<Image>()
            };
            if (item.image == null)
            {
                int childCount = transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    AppendImagesInChildren(item, transform.GetChild(i).gameObject);
                }
            }
            if ((item.image == null) && (item.imagesInChildren == null))
            {
                return null;
            }
            return item;
        }

        public void Raycast(List<Graphic> raycastResults, Vector2 pointerPosition, Camera eventCamera)
        {
            if ((this.owner.enabled && this.rectTransform.gameObject.activeInHierarchy) && RectTransformUtility.RectangleContainsScreenPoint(this.rectTransform, pointerPosition, eventCamera))
            {
                if (this.image != null)
                {
                    raycastResults.Add(this.image);
                }
                else
                {
                    for (int i = 0; i < this.imagesInChildren.Count; i++)
                    {
                        Image item = this.imagesInChildren[i];
                        if (item.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(item.rectTransform, pointerPosition, eventCamera))
                        {
                            raycastResults.Add(item);
                            break;
                        }
                    }
                }
            }
        }

        public CUIMiniEventScript miniScript
        {
            get
            {
                return (this.owner as CUIMiniEventScript);
            }
        }

        public CUIEventScript script
        {
            get
            {
                return (this.owner as CUIEventScript);
            }
        }
    }

    public enum RaycastMode
    {
        Unity,
        Sgame,
        Sgame_tile
    }

    private class Tile
    {
        public List<SGameGraphicRaycaster.Item> items = new List<SGameGraphicRaycaster.Item>();
    }
}

