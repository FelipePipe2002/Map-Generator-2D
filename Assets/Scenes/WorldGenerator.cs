using System;
using TMPro;
using Unity.Collections;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TilePlacement : MonoBehaviour
{
    public Tilemap tilemap,objectmap;
    public Formulas formula;
    public GameObject camera;
    public TMP_InputField[] wm;
    public Tile[] tile,objetos;
    public Slider[] pb;
    
    private string[] wmold;
    private float[] pbold;
    private string fold;

    private int seedElevation,seedBiome;

    private void Start() {
        wmold = new string[wm.Length];
        pbold = new float[pb.Length];
        Reset();
        GeneratePerlinTexture(int.Parse(wm[0].text),int.Parse(wm[1].text),1f,int.Parse(wm[2].text),int.Parse(wm[3].text),Random.Range(0, 100000),Random.Range(100000, 200000));
        camera.transform.position = new Vector3(int.Parse(wm[0].text)/2,int.Parse(wm[1].text)/2,camera.transform.position.z);
    }
    private void Update() {
        fixvalues();

       

        if(Input.GetKeyDown(KeyCode.R)){
            Reset();
        }

        if(Input.GetKeyDown(KeyCode.G)){
            seedElevation = Random.Range(0, 100000);
            seedBiome = Random.Range(100000, 200000);
        }

        if(settingschanged() || changedmap()|| Input.GetKeyDown(KeyCode.G)){
            objectmap.ClearAllTiles();
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

        pb[0].value = 30;
        pb[1].value = 8;
        pb[2].value = 32;
        pb[3].value = 30;
        pb[4].value = 55;
        pb[5].value = 30;
        pb[6].value = 20;
        pb[7].value = 25;
        pb[8].value = 25;
        pb[9].value = 25;
        pb[10].value = 50;
        pb[11].value = 5;
    }

    private bool settingschanged(){
        bool changed = false;

        for (int i = 0; i < pb.Length && !changed; i++)
        {
            changed = pbold[i] != pb[i].value;
            
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
        for (int i = 0; i < pb.Length; i++)
        {
            pbold[i] = pb[i].value;
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
                float xCoordO = Random.Range(0, 1) == 1 ? x / 1.5f + seedElevation : x / 1.5f - seedElevation;
                float yCoordo = Random.Range(0, 1) == 1 ? y / 1.5f + seedElevation : y / 1.5f - seedElevation;

                float perlinElevationValue = Mathf.PerlinNoise(xCoordE,yCoordE) +  0.5f * Mathf.PerlinNoise(2*xCoordE - ancho,2*yCoordE + alto) + 0.25f * Mathf.PerlinNoise(4*xCoordE - 2*ancho,4*yCoordE + 2*alto);
                float perlinBiomeValue = Mathf.PerlinNoise(xCoordB,yCoordB) + 0.5f * Mathf.PerlinNoise(2*xCoordB + ancho,2*yCoordB - alto) + 0.25f * Mathf.PerlinNoise(4*xCoordB + 2*ancho,4*yCoordB - 2*alto);
                float perlinObjectValue = Mathf.PerlinNoise(xCoordO,yCoordo);


                float dcx=2*x/ancho -1;
                float dcy=2*y/alto -1;

                Vector3Int cellPosition = tilemap.WorldToCell(new Vector3(x,y,0));
                float LinearShaping = pb[10].value/100f;
                
                perlinElevationValue = (1-LinearShaping)*perlinElevationValue + LinearShaping * (1-formula.calcular(dcx,dcy,pb[11].value,pb[11].maxValue)) * 1.75f;
                
                Tile selected = selectTile(perlinElevationValue,perlinBiomeValue);
                selected.color = depth(perlinElevationValue);
                tilemap.SetTile(cellPosition,selected);
                objectmap.SetTile(cellPosition,selectObject(perlinElevationValue,perlinObjectValue,perlinBiomeValue));
            }
        }
    }


    private Tile selectTile(float e, float b){
        if(e < 1.75f * ((float)pb[0].value/100f)){ //agua
            return tile[0];
        }
        
        if(e < 1.75f * ((float)(pb[0].value+pb[1].value)/100)){ //playa
            return tile[1];
        }

        if(e < 1.75f * ((float)(pb[0].value+pb[1].value + pb[2].value)/100f)){ //tierra

            if(b < 1.75f * ((float)pb[4].value/100f)) {
                return tile[3];
            }

            if(b < 1.75f * ((float)(pb[4].value + pb[5].value)/100f)) {
                return tile[4];
            }

            return tile[5];
        }

        return tile[2]; //montaña
    }

    private TileBase selectObject(float e, float o, float b){
        TileBase aux = null;

        if((1.75f * ((float)(pb[0].value+pb[1].value)/100)) < e && e < 1.75f * ((float)(pb[0].value+pb[1].value + pb[2].value)/100f)){ //tierra

            if(b < 1.75f * ((float)pb[4].value/100f) && o < (float)pb[7].value/100f)
                aux = objetos[0];

            if(1.75f * ((float)pb[4].value/100f) < b  && b < 1.75f * ((float)(pb[4].value + pb[5].value)/100f) && o < (float)pb[8].value/100f)
                aux = objetos[1];

            if(1.75f * ((float)(pb[4].value + pb[5].value)/100f) < b  && b < 1.75f * ((float)(pb[4].value + pb[5].value + pb[6].value)/100f) && o < (float)pb[9].value/100f)
                aux = objetos[2];

            if(o > 0.85f) 
                aux = objetos[3];

        }

        return aux;
    }

    private Color depth(float e){
        float color = 255f;
        
        if(e < 1.75f * ((float)pb[0].value/100f)){ //agua

            color = (100f + 155f*(e/(1.75f * ((float)pb[0].value/100f))))/255f;
        }
        
        if(e > 1.75f * ((float)pb[0].value/100f)){ //playa
            color = (255f - 25f*(e/(1.75f * ((float)(pb[0].value+pb[1].value + pb[2].value)/100f))))/255f;
        }


        Color aux = new Color(color,color,color);
        return aux;
    }

    private void fixvalues(){
        pb[0].maxValue = 100-(pb[1].value + pb[2].value + pb[3].value);
        pb[1].maxValue = 100-(pb[0].value + pb[2].value + pb[3].value);
        pb[2].maxValue = 100-(pb[0].value + pb[1].value + pb[3].value);
        pb[3].maxValue = 100-(pb[0].value + pb[1].value + pb[2].value);
        pb[4].maxValue = 100-(pb[5].value + pb[6].value);
        pb[5].maxValue = 100-(pb[4].value + pb[6].value);
        pb[6].maxValue = 100-(pb[4].value + pb[5].value);

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
