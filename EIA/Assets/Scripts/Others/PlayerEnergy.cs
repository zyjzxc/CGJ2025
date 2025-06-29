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

	private Material roleMat;

	public Color noEnergyColor;
	
	public Color energyColor;

	private float shiningTime = 0;
	
	private void Awake()
	{
		PlayerEnergyInstance = this;
	}

	private void Start()
    {
        MaxEnergy = RoleController.Instance.maxPowerAmount;
        energySlider = GetComponent<Slider>();
        
        roleMat = RoleController.Instance.GetComponentInChildren<Renderer>().sharedMaterial;
    }

    void Update()
    {
	    if (shiningTime > 0)
	    {
		    shiningTime -= Time.deltaTime;
		    
		    if (roleMat == null)
		    {
			    roleMat = RoleController.Instance.GetComponentInChildren<Renderer>()?.sharedMaterial;
		    }
		    roleMat?.SetColor("_BaseColor", Color.Lerp(energyColor, 
			    noEnergyColor, shiningTime * 6 - (int)(shiningTime * 6)));
	    }
    }

	public void UpdateBar(float tempEnergy, float ratio)
	{
		curEnergy = tempEnergy;
		energySlider.value = ratio;
		
		var c = UnityEngine.Color.Lerp(noEnergyColor, energyColor, ratio);
		var alpha = roleMat.color.a;
		roleMat.SetColor("_BaseColor", new Color(c.r, c.g, c.b, alpha));
	}

	public void Shining()
	{
		shiningTime = 0.8f;
	}
}
