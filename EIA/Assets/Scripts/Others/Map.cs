using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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

    public float MapHeight = 1.0f;

    public float MapSize
    {
        get
        {
            return MapRadius * MapRadius * Mathf.PI;
        }
    }

    public float CurrSpatterAreaSize;

    public float CurrCleanAreaRation
    {
        get
        {
            return CurrSpatterAreaSize / (MapSize * 0.95f);
        }
    }

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
        CurrSpatterAreaSize = CalculateArea(SpatterAreas);

        List<Bullet> needClear = new();

        foreach (var bulletList in BulletEmitter.Bullets)
        {
            foreach (var bullet in bulletList)
            {
                if ((bullet.transform.position - pos).magnitude < radius && bullet.BulletState != BulletState.BeBounceBack && bullet.GetBulletType() == BulletType.Small)
                {
                    needClear.Add(bullet);
                }
            }
        }
        
        foreach (var bullet in needClear)
            bullet.Disappear();
        
        GreenAreaMgr.Instance.AddCircle(new Vector2(pos.x, pos.z), radius);

        // render?
    }

    private static float CalculateArea(IEnumerable<SpatterArea> areas)
    {
        if (areas == null) return 0;
        
        // 获取包围盒和有效圆列表
        var validAreas = new List<SpatterArea>();
        Bounds bounds = GetBoundingBox(areas, validAreas);
        
        if (validAreas.Count == 0) return 0f;
        if (validAreas.Count == 1) return Mathf.PI * validAreas[0].radius * validAreas[0].radius;
        
        // 蒙特卡洛采样
        int samples = CalculateSampleCount(validAreas);
        int hits = 0;
        
        for (int i = 0; i < samples; i++)
        {
            Vector2 randomPoint = GetRandomPoint(bounds);
            if (IsPointInAnyCircle(randomPoint, validAreas))
            {
                hits++;
            }
        }
        
        // 计算总面积
        float boundsArea = bounds.size.x * bounds.size.y;
        return boundsArea * (hits / (float)samples);
    }

    // 动态计算采样次数（基于圆的大小和数量）
    private static int CalculateSampleCount(List<SpatterArea> areas)
    {
        float totalPotentialArea = 0f;
        foreach (var area in areas)
        {
            totalPotentialArea += Mathf.PI * area.radius * area.radius;
        }
        
        // 基础采样数 + 根据总面积动态增加
        return Mathf.Clamp(10000 + (int)(totalPotentialArea * 0.1f), 5000, 50000);
    }

    // 创建包含所有圆的包围盒
    private static Bounds GetBoundingBox(IEnumerable<SpatterArea> areas, List<SpatterArea> validAreas)
    {
        var bounds = new Bounds();
        bool first = true;
        
        foreach (var area in areas)
        {
            if (area == null || area.radius <= 0) continue;
            
            validAreas.Add(area);
            var center = area.Center;
            float r = area.radius;
            
            if (first)
            {
                bounds = new Bounds(center, Vector2.zero);
                bounds.Expand(2 * r);
                first = false;
            }
            else
            {
                bounds.Encapsulate(new Vector2(center.x - r, center.y - r));
                bounds.Encapsulate(new Vector2(center.x + r, center.y + r));
            }
        }
        return bounds;
    }

    // 生成包围盒内的随机点
    private static Vector2 GetRandomPoint(Bounds bounds)
    {
        return new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y)
        );
    }

    // 检查点是否在任一圆内
    private static bool IsPointInAnyCircle(Vector2 point, List<SpatterArea> areas)
    {
        foreach (var area in areas)
        {
            if (Vector2.Distance(point, area.Center) <= area.radius)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsOutSide(Vector3 pos)
    {
        return (pos - transform.position).magnitude > MapRadius + 2;
    }
}
