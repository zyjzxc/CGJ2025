using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatterArea
{
    public Vector3 Position;
    public float radius;

    public Vector2 Center
    {
        get
        {
            return new Vector2(Position.x, Position.z);
        }
    }
}

public class Map : MonoBehaviour
{
    public float MapRadius = 100;

    public float MapSize
    {
        get
        {
            return MapRadius * MapRadius * Mathf.PI;
        }
    }

    public float CurrSpatterAreaSize;

    public static Map MapInstance;
    
    public List<SpatterArea> SpatterAreas = new List<SpatterArea>();

    private void Awake()
    {
        MapInstance = this;
        SpatterAreas.Clear();
    }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.layer = GameContext.FloorLayer;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameContext.GameWin)
        {
            Debug.Log("Win!!");
        }
        
    }

    public void SpatterOnMap(Vector3 pos, float radius)
    {
        SpatterAreas.Add(new SpatterArea { Position = pos, radius = radius });
        CurrSpatterAreaSize = GetTotalSpatterAreaSize();

        // render?
    }

    private float GetTotalSpatterAreaSize()
    {
        var areas = SpatterAreas;
        if (areas.Count == 0)
            return 0f;

        float totalArea = 0f;
        int n = areas.Count;
        bool[] processed = new bool[n];

        // 按半径降序排序
        List<int> indices = new List<int>();
        for (int i = 0; i < n; i++)
            indices.Add(i);
        indices.Sort((a, b) => areas[b].radius.CompareTo(areas[a].radius));

        // 处理每个圆
        for (int i = 0; i < n; i++)
        {
            int idx = indices[i];
            if (processed[idx]) continue;

            float area = Mathf.PI * areas[idx].radius * areas[idx].radius;
            processed[idx] = true;

            // 减去与已处理圆的重叠部分
            for (int j = 0; j < i; j++)
            {
                int prevIdx = indices[j];
                if (!processed[prevIdx]) continue;

                float overlap = CalculateOverlapArea(areas[idx].Center, areas[idx].radius, areas[prevIdx].Center,
                    areas[prevIdx].radius);
                area -= overlap;
            }

            totalArea += area;
        }
        
        return totalArea;
    }

    // 计算两个圆的重叠面积
    private float CalculateOverlapArea(Vector2 c1, float r1, Vector2 c2, float r2)
    {
        float d = Vector2.Distance(c1, c2);

        // 圆不相交
        if (d >= r1 + r2)
            return 0f;

        // 一个圆完全在另一个圆内
        if (d <= Mathf.Abs(r1 - r2))
            return Mathf.PI * Mathf.Min(r1, r2) * Mathf.Min(r1, r2);

        // 部分重叠
        float a1 = 2 * Mathf.Acos(Mathf.Max(-1f, Mathf.Min(1f, (r1 * r1 + d * d - r2 * r2) / (2 * r1 * d))));
        float a2 = 2 * Mathf.Acos(Mathf.Max(-1f, Mathf.Min(1f, (r2 * r2 + d * d - r1 * r1) / (2 * r2 * d))));

        float segment1 = 0.5f * a1 * r1 * r1 - 0.5f * r1 * r1 * Mathf.Sin(a1);
        float segment2 = 0.5f * a2 * r2 * r2 - 0.5f * r2 * r2 * Mathf.Sin(a2);

        return segment1 + segment2;
    }

    public bool IsOutSide(Vector3 pos)
    {
        return (pos - transform.position).magnitude > MapRadius + 2;
    }
}
