using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TerrainLevelsDisplay : MonoBehaviour
{
    public Image originalImage;
    public Transform father;
    public Canvas canvas;
    public Slider[] pbt;
    private Image waterImage,sandImage,grassImage,rockImage;
    private GameObject water,sand,grass,rock;
    void Start()
    {

        water = new GameObject("water");
        sand = new GameObject("sand");
        grass = new GameObject("grass");
        rock = new GameObject("rock");


        water.transform.SetParent(father, false);
        sand.transform.SetParent(father, false);
        grass.transform.SetParent(father, false);
        rock.transform.SetParent(father, false);
        originalImage.transform.SetAsLastSibling();
        
        waterImage = water.AddComponent<Image>();
        sandImage = sand.AddComponent<Image>();
        grassImage = grass.AddComponent<Image>();
        rockImage = rock.AddComponent<Image>();

        waterImage.color = Color.cyan;
        sandImage.color = Color.yellow;
        grassImage.color = Color.green;
        rockImage.color = Color.gray;

    }

    private void Update() {
        waterImage.rectTransform.sizeDelta = new Vector2(originalImage.rectTransform.sizeDelta.x, originalImage.rectTransform.sizeDelta.y * pbt[0].value / 100f);
        sandImage.rectTransform.sizeDelta = new Vector2(originalImage.rectTransform.sizeDelta.x, originalImage.rectTransform.sizeDelta.y * pbt[1].value / 100f);
        grassImage.rectTransform.sizeDelta = new Vector2(originalImage.rectTransform.sizeDelta.x, originalImage.rectTransform.sizeDelta.y * pbt[2].value / 100f);
        rockImage.rectTransform.sizeDelta = new Vector2(originalImage.rectTransform.sizeDelta.x, originalImage.rectTransform.sizeDelta.y * (100 - (pbt[0].value + pbt[1].value + pbt[2].value))/100f);

        waterImage.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        waterImage.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        waterImage.rectTransform.pivot = new Vector2(0.5f, 0f);
        sandImage.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        sandImage.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        sandImage.rectTransform.pivot = new Vector2(0.5f, 0f);
        grassImage.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        grassImage.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        grassImage.rectTransform.pivot = new Vector2(0.5f, 0f);
        rockImage.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rockImage.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rockImage.rectTransform.pivot = new Vector2(0.5f, 0f);

        waterImage.rectTransform.localPosition = new Vector3(0f, -originalImage.rectTransform.sizeDelta.y * 0.27f, 0f);
        sandImage.rectTransform.localPosition = new Vector3(0f, -originalImage.rectTransform.sizeDelta.y * 0.27f + waterImage.rectTransform.sizeDelta.y , 0);
        grassImage.rectTransform.localPosition = new Vector3(0f, -originalImage.rectTransform.sizeDelta.y * 0.27f + waterImage.rectTransform.sizeDelta.y + sandImage.rectTransform.sizeDelta.y, 0);
        rockImage.rectTransform.localPosition = new Vector3(0f, -originalImage.rectTransform.sizeDelta.y * 0.27f + waterImage.rectTransform.sizeDelta.y + sandImage.rectTransform.sizeDelta.y + grassImage.rectTransform.sizeDelta.y , 0);
    }
}
