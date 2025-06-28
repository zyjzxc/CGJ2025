using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergy : MonoBehaviour
{
    private float MaxEnergy;

	private float curEnergy;
    
    private Slider energySlider;
	
	public static PlayerEnergy PlayerEnergyInstance;
	private void Awake()
	{
		PlayerEnergyInstance = this;
	}

	private void Start()
    {
        MaxEnergy = RoleController.Instance.maxPowerAmount;
        energySlider = GetComponent<Slider>();
    }

    void Update()
    {
    }

	public void UpdateBar(float tempEnergy, float ratio)
	{
		curEnergy = tempEnergy;
		energySlider.value = ratio;
	}
}
