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

	private Material playerMat;

	public Color noEnergyColor;
	
	public Color energyColor;
	
	private void Awake()
	{
		PlayerEnergyInstance = this;
	}

	private void Start()
    {
        MaxEnergy = RoleController.Instance.maxPowerAmount;
        energySlider = GetComponent<Slider>();
        
        playerMat = RoleController.Instance.GetComponentInChildren<Renderer>().sharedMaterial;
    }

    void Update()
    {
    }

	public void UpdateBar(float tempEnergy, float ratio)
	{
		curEnergy = tempEnergy;
		energySlider.value = ratio;
		
		var c = UnityEngine.Color.Lerp(noEnergyColor, energyColor, ratio);
		var alpha = playerMat.color.a;
		playerMat.SetColor("_BaseColor", new Color(c.r, c.g, c.b, alpha));
	}
}
