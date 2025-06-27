using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameContext
{
    public static readonly int PlayerLayer= LayerMask.NameToLayer("Player");
    
    public static readonly int BulletLayer = LayerMask.NameToLayer("Bullet");
    
    public static readonly int FloorLayer = LayerMask.NameToLayer("Floor");
}
