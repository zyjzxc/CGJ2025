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
        
        PlayerHealth.PlayerHealthInstance.Heal(100);
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
            if (mat == null)
            {
                mat = new Material(GetComponent<Renderer>().sharedMaterial);
                GetComponent<Renderer>().SetSharedMaterials(new List<Material>(){mat});
            }
            mat.SetColor("_BaseColor", Color.Lerp(Color.green, Color.white, CurrIdleTimer / IdleTimer));
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
