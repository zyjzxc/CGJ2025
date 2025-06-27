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
        if (Health <= 0)
        {
            Health = 0;
            Debug.Log("Game Over!!");
        }
        HealthSlider.value = Health / MaxHealth;
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
    }
}
