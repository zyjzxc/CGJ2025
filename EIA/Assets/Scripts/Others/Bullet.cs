using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum BulletState
{
    Idle = 0,
    Running,
    BeBounceBack,
}

public enum BulletType
{
    Small,
    Big,
    BulletTypeCount,
}

public interface IBullet
{
    public abstract BulletType GetBulletType();
}

public class Bullet : MonoBehaviour, IBullet
{
    [Range(0,100)]
    public float Speed;
    
    public Vector3 Direction;

    public int Damage;

    [FormerlySerializedAs("CaughtState")] public BulletState BulletState = BulletState.Idle;
    
    private Transform m_Transfrom;

    public float SpeedDecraseSpeed;

    public GameObject ColliderSettings;

    public BulletEmitter ParentBulletEmitter;
    
    // Start is called before the first frame update
    void Start()
    {
        m_Transfrom = transform;
        gameObject.layer = GameContext.BulletLayer;
        BulletState = BulletState.Idle;

        do
        {
            if (ColliderSettings != null)
            {
                var cols = ColliderSettings.GetComponents<Collider>();
                if(cols.Length == 0)
                    break;
                foreach (var c in cols)
                {
                    CopyCollider(c);
                }

                return;
            }
        } while (false);
        
        if(GetComponent<Collider>() == null)
            Debug.LogWarning($"{gameObject.name} no collider!");
    }

    // Update is called once per frame
    void Update()
    {
        Tick();
    }

    public virtual void Tick()
    {
        if (BulletState == BulletState.Idle)
        {
            BulletState = BulletState.Running;
        }
        
        else if (BulletState == BulletState.Running)
        {
            transform.Translate(Direction * (Speed * Time.deltaTime), Space.World);
        }
        Direction = RoleController.Instance.transform.position - transform.position;
        Direction = new Vector3(Direction.x, 0, Direction.z).normalized;
        if (Map.MapInstance.IsOutSide(transform.position))
        {
            //TODO: rebounce
            //Direction = ParentBulletEmitter.RandomBulletStartDir(transform.position);
        }
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{other.gameObject.name} collide {gameObject.name}");
        
        if (other.gameObject.layer == GameContext.PlayerLayer && BulletState == BulletState.Running)
        {
            DoDamage(other.gameObject);
        }
    }

    public virtual void BounceBack()
    {
        
    }

    public virtual void Init()
    {
        transform.position = ParentBulletEmitter.RandomBulletStartPos();
        Direction = ParentBulletEmitter.RandomBulletStartDir(transform.position);
    }

    private void CopyCollider(Collider source)
    {
        Collider newCollider = null;

        // 根据不同类型的碰撞盒创建对应的新碰撞盒
        if (source is BoxCollider)
        {
            BoxCollider box = source as BoxCollider;
            BoxCollider newBox = gameObject.AddComponent<BoxCollider>();
            newBox.center = box.center;
            newBox.size = box.size;
            newCollider = newBox;
        }
        else if (source is SphereCollider)
        {
            SphereCollider sphere = source as SphereCollider;
            SphereCollider newSphere = gameObject.AddComponent<SphereCollider>();
            newSphere.center = sphere.center;
            newSphere.radius = sphere.radius;
            newCollider = newSphere;
        }
        else if (source is CapsuleCollider)
        {
            CapsuleCollider capsule = source as CapsuleCollider;
            CapsuleCollider newCapsule = gameObject.AddComponent<CapsuleCollider>();
            newCapsule.center = capsule.center;
            newCapsule.radius = capsule.radius;
            newCapsule.height = capsule.height;
            newCapsule.direction = capsule.direction;
            newCollider = newCapsule;
        }
        else if (source is MeshCollider)
        {
            MeshCollider mesh = source as MeshCollider;
            MeshCollider newMesh = gameObject.AddComponent<MeshCollider>();
            newMesh.sharedMesh = mesh.sharedMesh;
            newMesh.convex = mesh.convex;
            newMesh.cookingOptions = mesh.cookingOptions;
            newCollider = newMesh;
        }
        else if (source is TerrainCollider)
        {
            Debug.LogWarning("地形碰撞盒不支持复制");
            return;
        }
        else
        {
            Debug.LogWarning($"不支持的碰撞盒类型: {source.GetType()}");
            return;
        }

        // 复制通用属性
        newCollider.isTrigger = source.isTrigger;
        newCollider.material = source.material;
    }

    // 
    public void Spatter(float radius)
    {
        Debug.Log($"{gameObject.name} spawn");
        Map.MapInstance.SpatterOnMap(transform.position, radius);
        Disappear();
    }

    public void DoDamage(GameObject player)
    {
        Debug.Log($"{player.name} {gameObject.name} damage");
        PlayerHealth.PlayerHealthInstance.TakeDamage(Damage);
    }

    public virtual void Disappear()
    {
        //TODO: some vfx
        ParentBulletEmitter.DestroyBullet(this);
    }

    public virtual BulletType GetBulletType()
    {
        return BulletType.Small;
    }
}
