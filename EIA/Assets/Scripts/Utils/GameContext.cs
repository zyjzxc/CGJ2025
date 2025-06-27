using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameContext
{
    public static readonly int PlayerLayer= LayerMask.NameToLayer("Player");
    
    public static readonly int BulletLayer = LayerMask.NameToLayer("Bullet");
    
    public static readonly int FloorLayer = LayerMask.NameToLayer("Floor");

    public static bool GameOver
    {
        get
        {
            return PlayerHealth.PlayerHealthInstance.Health <= 0;
        }
    }

    public static bool GameWin
    {
        get
        {
            return Map.MapInstance.CurrSpatterAreaSize >= Map.MapInstance.MapSize * 0.95f;
        }
    }
}
