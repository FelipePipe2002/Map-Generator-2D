using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class awake : MonoBehaviour
{
    public List<GameObject> paneles;
    // Start is called before the first frame update
    private void Awake() {
        foreach (GameObject panel in paneles)
        {
            panel.SetActive(true);
        }
        foreach (GameObject panel in paneles)
        {
            panel.SetActive(false);
        }
    }
}
