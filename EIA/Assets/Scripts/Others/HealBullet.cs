using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealBullet : BigBullet
{
    public override bool BounceBack()
    {
        if(BulletState != BulletState.Idle)//(BulletState == BulletState.BeBounceBack)
            return false;
        
        BulletState = BulletState.BeBounceBack;
        
        PlayerHealth.PlayerHealthInstance.Heal(1);
        Disappear();
        return true;
    }

    public override void Tick()
    {
        if (BulletState == BulletState.BeBounceBack)
        {
            
        }
        
        else if (BulletState == BulletState.Idle)
        {
            CurrIdleTimer += Time.deltaTime;

            if (CurrIdleTimer >= IdleTimer)
            {
                CurrIdleTimer = 0;
                Init();
                BulletState = BulletState.Running;
            }
        }
        
        else if (BulletState == BulletState.Running)
        {
            BulletState = BulletState.Idle;
        }
    }

    public override BulletType GetBulletType()
    {
        return BulletType.Heal;
    }
}
