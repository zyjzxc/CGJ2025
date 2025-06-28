using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float MaxHealth = 100;
    
    public float Health;
    
    public static PlayerHealth PlayerHealthInstance;
    
    public Slider HealthSlider;

    public float InvincibleTime;

    private void Awake()
    {
        PlayerHealthInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Health = MaxHealth;
        HealthSlider = GetComponent<Slider>();

        if (HealthSlider == null)
        {
            Debug.LogError($"{gameObject.name} HealthSlider is null");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameContext.GameOver)
        {
            Health = 0;
            Debug.Log("Game Over!!");
        }

        if (InvincibleTime >= 0)
        {
            InvincibleTime -= Time.deltaTime;
            
            //TODO: some vfx
        }
            
        HealthSlider.value = Health / MaxHealth;
    }

    public bool TakeDamage(float damage, GameObject src = null)
    {
        if (InvincibleTime > 0)
        {
            Debug.Log($"{gameObject.name} invincible {InvincibleTime}");
            return false;
        }
            
        Health -= damage;
        if (src == null)
        {
            //TODO: do dome Invincible vfx
            InvincibleTime = 1.0f;
        }
        return true;
    }

    public void AddInvincibleTime(float time)
    {
        InvincibleTime += time;
    }
}
