using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBullet : Bullet
{
    public float IdleTimer = 2.0f;
    
    public float CurrIdleTimer = 0.0f;

    public Vector3 BounceBackPosition;

    public float SpatterRadius = 1.0f;

    public Vector3 TargetRunningPos;
    
    public override void BounceBack()
    {
        if(BulletState != BulletState.Running)
            return;
        
        BulletState = BulletState.BeBounceBack;
        
        var randomTheta = Mathf.Deg2Rad * UnityEngine.Random.Range(0, 360);
        BounceBackPosition = transform.position + Random.Range(0.2f, 0.8f) * new Vector3(Mathf.Cos(randomTheta), 0, Mathf.Sin(randomTheta));
    }

    public override void Tick()
    {
        if (BulletState == BulletState.BeBounceBack)
        {
            float speed = (BounceBackPosition - transform.position).magnitude;
            if (speed < 0.1f)
            {
                Spatter(SpatterRadius);
            }
            else
            {
                Vector3 direction = BounceBackPosition - transform.position;
                transform.Translate(direction * (speed * Time.deltaTime), Space.World);
            }
        }
        
        else if (BulletState == BulletState.Idle)
        {
            CurrIdleTimer += Time.deltaTime;
            if (CurrIdleTimer >= IdleTimer)
            {
                BulletState = BulletState.Running;
                CurrIdleTimer = Random.Range(0.0f, 0.5f);
                // TODO: Some ready to run vfx
                Direction = (RoleController.Instance.transform.position - transform.position).normalized;
                TargetRunningPos = RoleController.Instance.transform.position + Direction * 2.0f;
            }
        }
        
        else if (BulletState == BulletState.Running)
        {
            transform.Translate(Direction * (Speed * Time.deltaTime), Space.World);
            if (Vector3.Dot(TargetRunningPos - transform.position, Direction) < 0.0f)
            {
                BulletState = BulletState.Idle;
            }
        }
    }

    public override BulletType GetBulletType()
    {
        return BulletType.Big;
    }

    public override void Init()
    {
        transform.position = ParentBulletEmitter.RandomBulletStartPos();
        BulletState = BulletState.Idle;
    }
}
