using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBullet : Bullet
{
    public float IdleTimer = 4.0f;
    
    protected float CurrIdleTimer = 0.0f;

    private Vector3 BounceBackPosition;

    public float SpatterRadius = 1.0f;

    private Vector3 TargetRunningPos;

    protected Material mat;
    
    public override bool BounceBack()
    {
        if(BulletState != BulletState.Running)//(BulletState == BulletState.BeBounceBack)
            return false;
        
        BulletState = BulletState.BeBounceBack;
        
        if (mat == null)
        {
            mat = new Material(GetComponent<Renderer>().sharedMaterial);
            GetComponent<Renderer>().SetSharedMaterials(new List<Material>(){mat});
        }
        mat.SetColor("_BaseColor", Color.green);
        
        var randomTheta = Mathf.Deg2Rad * UnityEngine.Random.Range(0, 360);
        BounceBackPosition = transform.position + 3.0f * new Vector3(Mathf.Cos(randomTheta), 0, Mathf.Sin(randomTheta));
		return true;
    }

    public override void Tick()
    {
        if (BulletState == BulletState.BeBounceBack)
        {
            float speed = (BounceBackPosition - transform.position).magnitude;
            Vector3 direction = BounceBackPosition - transform.position;
            if (speed < 0.2f || speed > 3.5f)
            {
                Spatter(SpatterRadius);
            }
            else
            {
                transform.Translate(direction * (speed * Speed / 3 * Time.deltaTime), Space.World);
            }
        }
        
        else if (BulletState == BulletState.Idle)
        {
            CurrIdleTimer += Time.deltaTime;
            if (mat == null)
            {
                mat = new Material(GetComponent<Renderer>().sharedMaterial);
                GetComponent<Renderer>().SetSharedMaterials(new List<Material>(){mat});
            }
            mat.SetColor("_BaseColor", Color.Lerp(Color.white, Color.red, CurrIdleTimer / IdleTimer));
            
            if (CurrIdleTimer >= IdleTimer)
            {
                BulletState = BulletState.Running;
                CurrIdleTimer = Random.Range(0.0f, 0.5f);
                // TODO: Some ready to run vfx
                Direction = RoleController.Instance.transform.position - transform.position;
                Direction = new Vector3(Direction.x, 0, Direction.z).normalized;
                TargetRunningPos = RoleController.Instance.transform.position + Direction * 2.0f;
            }
        }
        
        else if (BulletState == BulletState.Running)
        {
            transform.Translate(Direction * (Speed * Time.deltaTime), Space.World);
            if (Vector3.Dot(TargetRunningPos - transform.position, Direction) < 0.0f)
            {
                transform.position = TargetRunningPos;
                BulletState = BulletState.Idle;
            }

            if (Map.MapInstance.IsOutSide(transform.position))
            {
                transform.position = TargetRunningPos;
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

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }
}
