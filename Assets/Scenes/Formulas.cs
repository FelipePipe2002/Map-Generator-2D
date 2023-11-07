using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Formulas : MonoBehaviour
{
    public TMP_Dropdown lista;

    private string formula;
    void Update()
    {
        formula = lista.options[lista.value].text;
    }

    public float calcular(float dcx,float dcy,float tamanio,float maxtamanio){
        switch (formula)
        {
            case "Euclidean Square":
                return Mathf.Min(1f, (Mathf.Pow(dcx,2) + Mathf.Pow(dcy,2)) / Mathf.Sqrt(tamanio/maxtamanio));
            case "Square Bump":
                return 1 - (1-Mathf.Pow(dcx,2)*(maxtamanio - tamanio)/maxtamanio*4f) * (1-Mathf.Pow(dcy,2)*(maxtamanio - tamanio)/maxtamanio*4f);
            case "Column":
                return (maxtamanio - tamanio)/4f * Mathf.Abs(dcx);
            case "Inverted Column":
                return 1 - (maxtamanio - tamanio)/4f * Mathf.Abs(dcx);
            case "Wave":
                return Mathf.Sin(Mathf.Sqrt(Mathf.Pow(dcx*2,2) + Mathf.Pow(dcy*2,2)) / (tamanio/Mathf.Pow(maxtamanio,2f) * Mathf.PI)) + 1f;
            case "Trig":
                return 1 - Mathf.Cos(dcx*tamanio) * Mathf.Cos(dcy*tamanio);
            default:
                return 0;
        }
    }

    public string getFormula(){
        return formula;
    }
}
