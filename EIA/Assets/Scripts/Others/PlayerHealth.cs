using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    
    [Header("Settings")]
    [SerializeField] public int MaxHealth = 5;   // 最大血量（心形数量）
    [SerializeField] public int CurrentHealth = 5; // 当前血量

    [SerializeField] public GameObject HeartPrefab;   // 单个心形预制体

    // 心形状态枚举
    private enum HeartState
    {
        Full,    // 满心
        Half,    // 半心（可选）
        Empty    // 空心
    }
    
    public static PlayerHealth PlayerHealthInstance;

    public float InvincibleTime;

	public bool DamageLabel = true;

    private Material roleMat;

    private void Awake()
    {
        PlayerHealthInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        CurrentHealth = MaxHealth;
        
        InitializeHearts();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameContext.GameOver)
        {
            CurrentHealth = 0;
            Debug.Log("Game Over!!");
        }

        if (InvincibleTime >= 0)
        {
            InvincibleTime -= Time.deltaTime;

            //TODO: some vfx
            if (roleMat == null)
            {
                roleMat = RoleController.Instance.GetComponentInChildren<Renderer>()?.sharedMaterial;
            }
            roleMat?.SetColor("_BaseColor", Color.Lerp(Color.white, 
                new Color(1, 1, 1, 0), InvincibleTime * 6 - (int)(InvincibleTime * 6)));
        }
        
        transform.LookAt(Camera.main.transform);
    }

    public bool TakeDamage(int damage, GameObject src = null)
    {
        if (InvincibleTime > 0 || DamageLabel == false)
        {
            Debug.Log($"{gameObject.name} invincible {InvincibleTime}");
            return false;
        }
            
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        if (src == null)
        {
            //TODO: do dome Invincible vfx
            InvincibleTime = 1.0f;
        }
		RoleController.Instance.OnBeingHit();

		UpdateHearts();
        
        for (int i = CurrentHealth; i < Mathf.Min(CurrentHealth + damage, MaxHealth); i++)
        {
            StartCoroutine(PlayHeartBreakAnimation(i));
        }
        return true;
    }

	public void ModifyDamageLabel(bool tempLabel)
	{
		Debug.Log($"DamageLabel change into {tempLabel}");
		DamageLabel = tempLabel;
	}

    public void AddInvincibleTime(float time)
    {
        InvincibleTime += time;
    }
    
    private Image[] hearts;

    // 初始化心形UI
    private void InitializeHearts()
    {
        hearts = new Image[MaxHealth];
        
        // 创建心形UI
        for (int i = 0; i < MaxHealth; i++)
        {
            GameObject heart = Instantiate(HeartPrefab, transform);
            hearts[i] = heart.GetComponent<Image>();
        }

        // 更新UI以匹配当前血量
        UpdateHearts();
    }

    // 更新血量显示
    public void SetHealth(int health)
    {
        CurrentHealth = Mathf.Clamp(health, 0, MaxHealth);
        UpdateHearts();
    }

    // 治疗
    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        UpdateHearts();
    }

    // 更新所有心形的显示状态
    private void UpdateHearts()
    {
        for (int i = 0; i < MaxHealth; i++)
        {
            hearts[i].color = (i < CurrentHealth) ? Color.white : new Color(0.2f, 0.2f, 0.2f, 0.5f);
        }
    }

    // 受伤动画协程
    private IEnumerator PlayHeartBreakAnimation(int index)
    {
        Image heart = hearts[index];
        float duration = 0.5f;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 缩放动画
            heart.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(1.5f, 1.5f, 1), t);
            
            // 颜色闪烁
            heart.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(t * 4, 1));
            
            yield return null;
        }
        
        heart.transform.localScale = Vector3.one;
        UpdateHearts(); // 确保最终状态正确
    }
}
