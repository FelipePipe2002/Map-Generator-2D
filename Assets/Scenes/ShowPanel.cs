using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPanel : MonoBehaviour
{
    public GameObject[] options;
    public ShowPanel[] otherButtons;
    public GameObject panel;
    public bool Selected;

    private void Start() {
        Selected = false;
    }
    // Start is called before the first frame update
    public void clicked() {
        foreach (ShowPanel button in otherButtons)
        {
            button.hideoptions();
        }
        Selected = !Selected;
        if(Selected){
            panel.SetActive(true);
            foreach (GameObject option in options)
            {
                option.SetActive(true);
            }
            
        } else {
            panel.SetActive(false);
            foreach (GameObject option in options)
            {
                option.SetActive(false);
            }
        }
    }

    public void hideoptions(){
        foreach (GameObject option in options)
        {
            option.SetActive(false);
            Selected = false;
        }
    }
}
