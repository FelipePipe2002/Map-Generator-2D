using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputTextModifier : MonoBehaviour
{
    public TMP_InputField text;
    // Update is called once per frame

    private void Update() {
        try{
            int aux = int.Parse(text.text);
        } catch (FormatException)
        {
            text.text = "1";
        }
    }
    public void Sumar(){
        float TextValue = int.Parse(text.text);
        if(Input.GetKey(KeyCode.LeftShift)){
            TextValue += 10;
        } else if(Input.GetKey(KeyCode.LeftAlt)){
            TextValue += 5;
        } else {
            TextValue += 1;
        }
        text.text = TextValue.ToString();
    }

    public void Restar(){
        float TextValue = int.Parse(text.text);
        if(Input.GetKey(KeyCode.LeftShift)){
            TextValue -= 10;
        } else if(Input.GetKey(KeyCode.LeftAlt)){
            TextValue -= 5;
        } else {
            TextValue -= 1;
        }
        if(TextValue < 1){
            TextValue = 1;
        }
        text.text = TextValue.ToString();
    }
}
