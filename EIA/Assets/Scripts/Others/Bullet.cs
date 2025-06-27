using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BulletState
{
    BeforeCaught = 0,
    BeCaught,
    AfterCaught,
}

public class Bullet : MonoBehaviour
{
    [Range(0,100)]
    public float Speed;
    
    public Vector3 Direction;

    public float Damage;

    public BulletState CaughtState = BulletState.BeforeCaught;
    
    private Transform m_Transfrom;

    public float SpeedDecraseSpeed;

    public GameObject ColliderSettings;
    
    // Start is called before the first frame update
    void Start()
    {
        m_Transfrom = transform;
        gameObject.layer = GameContext.BulletLayer;

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
        if (CaughtState ==  BulletState.BeforeCaught)
        {
            transform.Translate(Direction * (Speed * Time.deltaTime), Space.World);
        }

        else if (CaughtState == BulletState.BeCaught)
        {
            
        }
        
        else if (CaughtState == BulletState.AfterCaught)
        {
            Speed -=  SpeedDecraseSpeed * Time.deltaTime;
            transform.Translate(Direction * (Speed * Time.deltaTime), Space.World);

            if (Speed <= 0)
            {
                Spatter();
            }
        }

        if (Map.MapInstance.IsOutSide(transform.position))
        {
            Disappear();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log($"{other.gameObject.name} collide {gameObject.name}");
        
        if (other.gameObject.layer == GameContext.PlayerLayer && CaughtState != BulletState.BeCaught)
        {
            DoDamage(other.gameObject);
        }
    }

    public void Caught()
    {
        CaughtState = BulletState.BeCaught;
    }

    // 抓住后重新发射
    public void ReEmit(Vector3 direction, float startSpeed)
    {
        Direction = direction.normalized;
        Speed = startSpeed;

        CaughtState = BulletState.AfterCaught;
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
    public void Spatter()
    {
        Debug.Log($"{gameObject.name} spawn");
        Map.MapInstance.SpatterOnMap(transform.position, 1);
        Disappear();
    }

    public void DoDamage(GameObject player)
    {
        Debug.Log($"{player.name} {gameObject.name} damage");
        Disappear();
    }

    public void Disappear()
    {
        GameObject.DestroyImmediate(this.gameObject);
    }
}
