using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BulletSetting
{
    public Bullet BulletPrefab;

    public float EmitRate;
}

public class BulletEmitter : MonoBehaviour
{
    public List<BulletSetting>  BulletSettings;

    private Vector3 m_MapCenter;
    private float m_MapRadius;

    public float EmitRate;

    private int m_Frame;
    
    void Start()
    {
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

            var bulletGO = Instantiate(bullet);
            bulletGO.transform.position = RandomBulletStartPos();
            bulletGO.Direction = RandomBulletStartDir(bulletGO.transform.position);
        }
    }

    private Bullet RandomBullet()
    {
        float totalRate = 0;
        foreach (BulletSetting b in BulletSettings)
        {
            totalRate += b.EmitRate;
        }
        
        float random =  UnityEngine.Random.Range(0, totalRate);

        totalRate = 0;
        foreach (BulletSetting b in BulletSettings)
        {
            totalRate += b.EmitRate;
            if (totalRate > random)
            {
                return b.BulletPrefab;
            }
        }
        
        return BulletSettings.Last().BulletPrefab;
    }

    private Vector3 RandomBulletStartPos()
    {
        float theta = Mathf.Deg2Rad * UnityEngine.Random.Range(0, 360);
        
        return new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * m_MapRadius + m_MapCenter;
    }

    private Vector3 RandomBulletStartDir(Vector3 startPos)
    {
        // TODO
        return (-startPos + m_MapCenter).normalized;
    }
}
