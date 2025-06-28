using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class BulletSetting
{
    public Bullet BulletPrefab;

    public float EmitRate;

    public int MaxNumer;
}

public class BulletEmitter : MonoBehaviour
{
    public List<BulletSetting> BulletSettings;

    private Vector3 m_MapCenter;
    private float m_MapRadius;

    public float EmitRate;
    
    public float EmitDirRandomness = 0.5f;

    private int m_Frame;
    
    public static List<Bullet>[] Bullets = null;
    
    void Start()
    {
        if (Bullets == null)
        {
            Bullets = new List<Bullet>[(int)BulletType.BulletTypeCount];
            for (int i = 0; i < (int)BulletType.BulletTypeCount; i++)
            {
                Bullets[i] = new List<Bullet>();
            }
        }
        
        m_MapCenter = Map.MapInstance.transform.position;
        m_MapRadius = Map.MapInstance.MapRadius;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Frame++ > EmitRate)
        {
            m_Frame = 0;

            var bullet = RandomBullet();

            if (bullet != null)
            {
                EmitBullet(bullet);
            }
        }
    }

    private Bullet RandomBullet()
    {
        float totalRate = 0;
        foreach (BulletSetting b in BulletSettings)
        {
            if(Bullets[(int)b.BulletPrefab.GetBulletType()].Count < b.MaxNumer)
                totalRate += b.EmitRate;
        }
        
        float random = UnityEngine.Random.Range(0, totalRate);

        totalRate = 0;
        foreach (BulletSetting b in BulletSettings)
        {
            if (Bullets[(int)b.BulletPrefab.GetBulletType()].Count < b.MaxNumer)
            {
                totalRate += b.EmitRate;
                if (totalRate > random)
                {
                    return b.BulletPrefab;
                }
            }
        }
        
        return null;
    }

    public Vector3 RandomBulletStartPos()
    {
        float theta = Mathf.Deg2Rad * UnityEngine.Random.Range(0, 360);
        
        return new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * m_MapRadius + m_MapCenter + Vector3.up * Map.MapInstance.MapHeight;
    }

    public Vector3 RandomBulletStartDir(Vector3 startPos)
    {
        // 生成与输入向量夹角≤90°的随机向量（方向符合正态分布）
        Vector2 GetRandomVectorInCone(Vector2 inputDirection)
        {
            // 处理零向量输入
            if (inputDirection == Vector2.zero)
                return Random.insideUnitCircle.normalized;

            // 1. 计算输入向量的基础角度（弧度）
            float baseAngle = Mathf.Atan2(inputDirection.y, inputDirection.x);

            // 2. 生成符合正态分布的角度偏移量
            float angleOffset = GenerateTruncatedGaussian(-Mathf.PI / 2, Mathf.PI / 2, EmitDirRandomness);

            // 3. 计算新角度
            float newAngle = baseAngle + angleOffset;

            // 4. 创建单位向量
            return new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
        }

        // 生成截断正态分布随机数（范围[min, max]）
        float GenerateTruncatedGaussian(float min, float max, float stdDev)
        {
            float result;
            do
            {
                result = BoxMullerTransform(0f, stdDev);
            } 
            while (result < min || result > max); // 确保在有效范围内
        
            return result;
        }

        // Box-Muller变换生成正态分布随机数
        float BoxMullerTransform(float mean, float stdDev)
        {
            float u1 = 1.0f - Random.value; // [0,1) -> (0,1]
            float u2 = 1.0f - Random.value;
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
            return mean + stdDev * randStdNormal;
        }

        Vector3 dir = (-startPos + m_MapCenter).normalized;
        Vector2 randomDirection = GetRandomVectorInCone(new Vector2(dir.x, dir.z));
        
        return new Vector3(randomDirection.x, 0, randomDirection.y).normalized;
    }

    public void DestroyBullet(Bullet bullet)
    {
        Bullets[(int)bullet.GetBulletType()].Remove(bullet);
        GameObject.Destroy(bullet.gameObject);
    }

    public void EmitBullet(Bullet bullet)
    {
        var bulletGO = Instantiate(bullet, transform, true);
        bulletGO.ParentBulletEmitter = this;
        bulletGO.Init();
            
        Bullets[(int)bulletGO.GetBulletType()].Add(bulletGO);
    }
    
}
