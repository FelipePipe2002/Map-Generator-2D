using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    [SerializeField] public Slider _slider;
    [SerializeField] public Text _sliderText;

    [SerializeField] private bool selected = false;

    private float oldValue=0;
    // Start is called before the first frame update
    private void Update()
    {   
        if(oldValue == _slider.value){
            selected=false;
        }
        if(Input.GetKey(KeyCode.LeftShift) && selected){
            _slider.wholeNumbers=true;
        } else {
            _slider.wholeNumbers=false;
        }
        _sliderText.text = _slider.value.ToString("0.00");
        oldValue = _slider.value;
    }

    public void selectedtrue(){
        selected = true;
    }
}
