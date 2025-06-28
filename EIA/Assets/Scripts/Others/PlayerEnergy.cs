using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergy : MonoBehaviour
{
    private float MaxEnergy;
    
    private Slider energySlider;

    private void Start()
    {
        MaxEnergy = RoleController.Instance.maxPowerAmount;
        energySlider = GetComponent<Slider>();
    }

    void Update()
    {
        float ratio = RoleController.Instance.curPower / MaxEnergy;
        energySlider.value = ratio;
    }
}
