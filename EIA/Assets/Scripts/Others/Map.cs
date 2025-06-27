using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatterArea
{
    public Vector3 Position;
    public float radius;
}

public class Map : MonoBehaviour
{
    public float MapSize = 100;

    public float CurrSpatterAreaSize;

    public static Map MapInstance;
    
    public List<SpatterArea>  SpatterAreas = new List<SpatterArea>();

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
        if (CurrSpatterAreaSize >= MapSize * 0.95)
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
        // TODO
        return 0;
    }
}
