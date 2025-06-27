using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapCleanProcess : MonoBehaviour
{
    public Slider CleanSlider;
    // Start is called before the first frame update
    void Start()
    {
        CleanSlider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        CleanSlider.value = Map.MapInstance.CurrSpatterAreaSize / (Map.MapInstance.MapSize * 0.95f);
    }
}
