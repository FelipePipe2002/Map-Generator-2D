using System;
using TMPro;
using Unity.Collections;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TilePlacement : MonoBehaviour
{
    public Tilemap tilemap;
    public Formulas formula;
    public GameObject camera;
    public TMP_InputField[] wm;
    public TileBase[] tile;
    public Slider[] probabilities;
    
    private string[] wmold;
    private float[] pbold;
    private string fold;

    private int seedElevation,seedBiome;

    private void Start() {
        wmold = new string[wm.Length];
        pbold = new float[probabilities.Length];
        Reset();
        GeneratePerlinTexture(int.Parse(wm[0].text),int.Parse(wm[1].text),1f,int.Parse(wm[2].text),int.Parse(wm[3].text),Random.Range(0, 1000000),Random.Range(1000000, 2000000));
        camera.transform.position = new Vector3(int.Parse(wm[0].text)/2,int.Parse(wm[1].text)/2,camera.transform.position.z);
    }
    private void Update() {
        fixvalues();

        if(Input.GetKeyDown(KeyCode.R)){
            Reset();
        }

        if(Input.GetKeyDown(KeyCode.G)){
            seedElevation = Random.Range(0, 1000000);
            seedBiome = Random.Range(1000000, 2000000);
        }

        if(settingschanged() || changedmap()|| Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.R)){

            if(changedmap())
                tilemap.ClearAllTiles();

            GeneratePerlinTexture(int.Parse(wm[0].text),int.Parse(wm[1].text),1f,int.Parse(wm[2].text),int.Parse(wm[3].text),seedElevation,seedBiome);

        }
        saveOldData();
    }

    private void Reset(){
        wm[0].text = "100";
        wm[1].text = "100";
        wm[2].text = "15";
        wm[3].text = "15";

        probabilities[0].value = 30;
        probabilities[1].value = 8;
        probabilities[2].value = 32;
        probabilities[3].value = 30;
        probabilities[4].value = 55;
        probabilities[5].value = 30;
        probabilities[6].value = 20;
        probabilities[7].value = 50;
        probabilities[8].value = 5;
    }

    private bool settingschanged(){
        bool changed = false;

        for (int i = 0; i < probabilities.Length && !changed; i++)
        {
            changed = pbold[i] != probabilities[i].value;
            
        }

        if(!changed)
            changed = fold != formula.getFormula();

        return changed;
    }

    private bool changedmap(){
        bool changed = false;

        for (int i = 0; i < wm.Length && !changed; i++)
        {
            changed = wmold[i] != wm[i].text;
        }

        return changed;
    }

    private void saveOldData(){
        for (int i = 0; i < probabilities.Length; i++)
        {
            pbold[i] = probabilities[i].value;
        }

        for (int i = 0; i < wm.Length; i++)
        {
            wmold[i] = wm[i].text;
        }

        fold = formula.getFormula();
    }

    private void GeneratePerlinTexture(float ancho, float alto, float tilesize, float worldsize, float biomesize, int seedElevation, int seedBiome)
    {
        for (float x = 0; x <= ancho; x += tilesize)
        {
            for (float y = 0; y <= alto; y += tilesize)
            {   
                float xCoordE = Random.Range(0, 1) == 1 ? x / worldsize + seedElevation : x / worldsize - seedElevation;
                float yCoordE = Random.Range(0, 1) == 1 ? y / worldsize + seedElevation : y / worldsize - seedElevation;
                float xCoordB = Random.Range(0, 1) == 1 ? x / biomesize + seedBiome : x / biomesize - seedBiome;
                float yCoordB = Random.Range(0, 1) == 1 ? y / biomesize + seedBiome : y / biomesize - seedBiome;

                float perlinElevationValue = Mathf.PerlinNoise(xCoordE,yCoordE) +  0.5f * Mathf.PerlinNoise(2*xCoordE - ancho,2*yCoordE + alto) + 0.25f * Mathf.PerlinNoise(4*xCoordE - 2*ancho,4*yCoordE + 2*alto);
                float perlinBiomeValue = Mathf.PerlinNoise(xCoordB,yCoordB) + 0.5f * Mathf.PerlinNoise(2*xCoordB + ancho,2*yCoordB - alto) + 0.25f * Mathf.PerlinNoise(4*xCoordB + 2*ancho,4*yCoordB - 2*alto);

                float dcx=2*x/ancho -1;
                float dcy=2*y/alto -1;

                Vector3Int cellPosition = tilemap.WorldToCell(new Vector3(x,y,0));
                float LinearShaping = probabilities[7].value/100f;
                tilemap.SetTile(cellPosition,selectTile((1-LinearShaping)*perlinElevationValue + LinearShaping * (1-formula.calcular(dcx,dcy,probabilities[8].value,probabilities[8].maxValue)) * 1.75f,perlinBiomeValue));
            }
        }
    }


    private TileBase selectTile(float e, float b){

        if(e < 1.75f * ((float)probabilities[0].value/100f)){ //agua
            return tile[0];
        }
        
        if(e < 1.75f * ((float)(probabilities[0].value+probabilities[1].value)/100)){ //playa
            return tile[1];
        }

        if(e < 1.75f * ((float)(probabilities[0].value+probabilities[1].value + probabilities[2].value)/100f)){ //tierra
            if(b < 1.75f * ((float)probabilities[4].value/100f)) return tile[3];
            if(b < 1.75f * ((float)(probabilities[4].value + probabilities[5].value)/100f)) return tile[4];
            return tile[5];
        }

        return tile[2]; //montaña
    }

    private void fixvalues(){
        probabilities[0].maxValue = 100-(probabilities[1].value + probabilities[2].value + probabilities[3].value);
        probabilities[1].maxValue = 100-(probabilities[0].value + probabilities[2].value + probabilities[3].value);
        probabilities[2].maxValue = 100-(probabilities[0].value + probabilities[1].value + probabilities[3].value);
        probabilities[3].maxValue = 100-(probabilities[0].value + probabilities[1].value + probabilities[2].value);
        probabilities[4].maxValue = 100-(probabilities[5].value + probabilities[6].value);
        probabilities[5].maxValue = 100-(probabilities[4].value + probabilities[6].value);
        probabilities[6].maxValue = 100-(probabilities[4].value + probabilities[5].value);

        for (int i = 0; i < wm.Length; i++)
        {
            try{
                int aux = int.Parse(wm[i].text);
            } catch (FormatException)
            {
                wm[i].text = "1";
            }
        }
    }
}
