using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    [SerializeField] public Slider _slider;
    [SerializeField] public Text _sliderText;
    // Start is called before the first frame update
    private void Update()
    {
        _sliderText.text = _slider.value.ToString("0.00");
    }
}
