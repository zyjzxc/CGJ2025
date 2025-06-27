using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System;

public class GreenAreaMgr : MonoBehaviour
{
    
    public struct CircleData
    {
        public Vector2 position; // 圆心位置(x,y)，范围通常是0-1（UV坐标）
        public float radius;     // 圆半径
    }
    
    private List<CircleData> circles = new List<CircleData>();
    
    // 小圆参数范围
    public float minSmallRadius = 0.1f; // 小圆最小半径
    public float maxSmallRadius = 0.4f; // 小圆最大半径
    
    
    public void ClearCircles()
    {
        circles.Clear();
    }

    public void AddCircle(Vector2 position, float radius)
    {
        var showRadius = radius * 0.7f;
        circles.Add(new CircleData() { position = new Vector2(position.x, position.y), radius = showRadius });
        System.Random random = new System.Random();
        for (int i = 0; i < 8; i++)
        {
            // 1. 在大圆边缘随机生成角度(0到2π)
            float angle = (float)random.NextDouble() * Mathf.PI * 2;
            
            // 2. 计算小圆圆心坐标(在大圆边缘上)
            float x = position.x + showRadius * Mathf.Cos(angle);
            float y = position.y + showRadius * Mathf.Sin(angle);
            
            circles.Add(new CircleData() { position = new Vector2(x, y), radius = minSmallRadius * radius + 
                (float)random.NextDouble() * (maxSmallRadius * radius - minSmallRadius * radius) });
        }
        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        ClearCircles();
        AddCircle(new Vector2(0, 0), 1);
        
        AddCircle(new Vector2(1, 0), 1);
        
        AddCircle(new Vector2(2, 2), 1);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateFloorMaterial();
    }
    
    public void UpdateFloorMaterial()
    {
        SetCircleParameters();
    }
    
    private void SetCircleParameters()
    {
        const int MAX_CIRCLES = 1000;
        int circleCount = Mathf.Min(circles.Count, MAX_CIRCLES);
        var _CircleCountID = Shader.PropertyToID("_CircleCount");
        var _CircleParamsID = Shader.PropertyToID("_CircleParams");
        Shader.SetGlobalFloat(_CircleCountID, circleCount);
        
        Vector4[] circleParams = new Vector4[MAX_CIRCLES];
        for (int i = 0; i < circleCount; i++)
        {
            circleParams[i] = new Vector4(
                circles[i].position.x,
                circles[i].position.y,
                circles[i].radius,
                0
            );
        }
        
        Shader.SetGlobalVectorArray(_CircleParamsID, circleParams);
    }
}
