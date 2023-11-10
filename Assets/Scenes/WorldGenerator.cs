using System;
using System.Xml;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TilePlacement : MonoBehaviour
{
    public Tilemap tilemap,objectmap;
    public Formulas formula;
    public Toggle ds;
    public Camera camara;
    public Text Loading;
    public TMP_InputField[] wm;
    public Tile[] tile,objetos;
    public Slider[] pbt,pbb,pbo,pbd;

    private int seedElevation,seedBiome,iterationsx,iterationsy,ix,iy;
    private float[] randomValues;
    private float chunksize = 30f;
    private bool canceled;
    private void Start() {
        Reset();
        canceled = false;
        seedElevation = Random.Range(0, 100000);
        seedBiome = Random.Range(100000, 200000);

        iterationsx = Mathf.CeilToInt(float.Parse(wm[0].text)/chunksize);
        iterationsy = Mathf.CeilToInt(float.Parse(wm[1].text)/chunksize);
        ix=0;
        iy=0;
        
        randomValues = new float[6];
        for (int i = 0; i < 6; i++) {
            randomValues[i] = Random.Range(0, 1);
        }

        camara.transform.position = new Vector3(float.Parse(wm[0].text)/2,float.Parse(wm[1].text)/2,camara.transform.position.z);
    }

    private void Update() {
        fixvalues();
        
        if(Input.GetKeyDown(KeyCode.R) && ((ix == iterationsx && iy == iterationsy) || canceled)){
            Reset();
            iterationsx = Mathf.CeilToInt(float.Parse(wm[0].text)/chunksize);
            iterationsy = Mathf.CeilToInt(float.Parse(wm[1].text)/chunksize);
            ix=0;
            iy=0;
        }

        if(Input.GetKeyDown(KeyCode.C)){
            canceled = true;
        }

        if(Input.GetKeyDown(KeyCode.G)){
            seedElevation = Random.Range(0, 100000);
            seedBiome = Random.Range(100000, 200000);
            iterationsx = Mathf.CeilToInt(float.Parse(wm[0].text)/chunksize);
            iterationsy = Mathf.CeilToInt(float.Parse(wm[1].text)/chunksize);
            ix=0;
            iy=0;
            for (int i = 0; i < 6; i++) {
                randomValues[i] = Random.Range(0, 1);
            }
            canceled = false;
        }

        float progress = (float)(iy * iterationsx + ix) / (iterationsx * iterationsy) * 100f;
        Loading.text = "Generating: " + Mathf.RoundToInt(progress) + "%";

        if((iy < iterationsy) && !canceled){
            if(ix == 0 && iy == 0){
                objectmap.ClearAllTiles();
                tilemap.ClearAllTiles();
            }
            InteractuarConModificadores(false);
            GeneratePerlinTexture(ix*chunksize+1f,Mathf.Min((ix+1)*chunksize,float.Parse(wm[0].text)),iy*chunksize+1f,Mathf.Min((iy+1)*chunksize,float.Parse(wm[1].text)),int.Parse(wm[0].text),int.Parse(wm[1].text),1f,int.Parse(wm[2].text),int.Parse(wm[3].text),seedElevation,seedBiome,randomValues);
            
            if (ix < iterationsx - 1) {
            ix++;
        } else {
            ix = 0;
            iy++;
        }
        } else {
            InteractuarConModificadores(true);
        }
    }

    private void GeneratePerlinTexture(float xi, float xf, float yi,  float yf, float ancho, float alto, float tilesize, float worldsize, float biomesize, int seedElevation, int seedBiome, float[] randomValues)
    {
        for (float x = xi; x <= xf; x += tilesize)
        {
            for (float y = yi; y <= yf; y += tilesize)
            {   
                float xCoordE = x / worldsize + seedElevation;
                float yCoordE = y / worldsize + seedElevation;
                float xCoordB = x / biomesize + seedBiome;
                float yCoordB = y / biomesize + seedBiome;
                float xCoordO = x / 1.5f + seedElevation;
                float yCoordo = y / 1.5f + seedElevation;

                float tnoise1 = Mathf.PerlinNoise(xCoordE, yCoordE);
                float tnoise2 = 0.5f * Mathf.PerlinNoise(2 * xCoordE - ancho, 2 * yCoordE + alto);
                float tnoise3 = 0.25f * Mathf.PerlinNoise(4 * xCoordE - 2 * ancho, 4 * yCoordE + 2 * alto);
                float perlinElevationValue = tnoise1 + tnoise2 + tnoise3;

                float bnoise1 = Mathf.PerlinNoise(xCoordB, yCoordB);
                float bnoise2 = 0.5f * Mathf.PerlinNoise(2 * xCoordB - ancho, 2 * yCoordB + alto);
                float bnoise3 = 0.25f * Mathf.PerlinNoise(4 * xCoordB - 2 * ancho, 4 * yCoordB + 2 * alto);
                float perlinBiomeValue = bnoise1 + bnoise2 + bnoise3;

                float perlinObjectValue = Mathf.PerlinNoise(xCoordO,yCoordo);

                float dcx=2*x/ancho -1;
                float dcy=2*y/alto -1;

                Vector3Int cellPosition = tilemap.WorldToCell(new Vector3(x,y,0));
                float LinearShaping = pbd[0].value/100f;
                
                perlinElevationValue = (1-LinearShaping)*perlinElevationValue + LinearShaping * (1-formula.calcular(dcx,dcy,pbd[1].value,pbd[1].maxValue,perlinElevationValue)) * 1.75f;
                
                Tile selected = selectTile(perlinElevationValue,perlinBiomeValue);
                
                if(ds.isOn)
                    selected.color = depth(perlinElevationValue);
                else
                    selected.color = Color.white;

                tilemap.SetTile(cellPosition,selected);
                objectmap.SetTile(cellPosition,selectObject(perlinElevationValue,perlinObjectValue,perlinBiomeValue));
            }
        }
    }


    private Tile selectTile(float e, float b){
        if(e < 1.75f * ((float)pbt[0].value/100f)){ //agua
            return tile[0];
        }
        
        if(e < 1.75f * ((float)(pbt[0].value+pbt[1].value)/100)){ //playa
            return tile[1];
        }

        if(e < 1.75f * ((float)(pbt[0].value+pbt[1].value + pbt[2].value)/100f)){ //tierra

            if(b < 1.75f * ((float)pbb[0].value/100f)) {
                return tile[3];
            }

            if(b < 1.75f * ((float)(pbb[0].value + pbb[1].value)/100f)) {
                return tile[4];
            }

            return tile[5];
        }

        return tile[2]; //montaña
    }

    private TileBase selectObject(float e, float o, float b){
        TileBase aux = null;

        if((1.75f * ((float)(pbt[0].value+pbt[1].value)/100)) < e && e < 1.75f * ((float)(pbt[0].value+pbt[1].value + pbt[2].value)/100f)){ //tierra

            if(b < 1.75f * ((float)pbb[0].value/100f) && o < (float)pbo[0].value/100f)
                aux = objetos[0];

            if(1.75f * ((float)pbb[0].value/100f) < b  && b < 1.75f * ((float)(pbb[0].value + pbb[1].value)/100f) && o < (float)pbo[1].value/100f)
                aux = objetos[1];

            if(1.75f * ((float)(pbb[0].value + pbb[1].value)/100f) < b  && b < 1.75f * ((float)(pbb[0].value + pbb[1].value + pbb[2].value)/100f) && o < (float)pbo[2].value/100f)
                aux = objetos[2];

            if(o > 0.85f) 
                aux = objetos[3];

        }

        return aux;
    }

    private Color depth(float e){
        float color = 255f;
        
        if(e < 1.75f * ((float)pbt[0].value/100f)){ //agua
            color = (100f + 155f*(e/(1.75f * ((float)pbt[0].value/100f))))/255f;
        }

        Color aux = new Color(color,color,color);
        return aux;
    }

    public Toggle[] toggles;
    public Button[] buttons;
    public TMP_Dropdown[] dD;
    private void InteractuarConModificadores(bool interactuable)
    {
        SetInteractables(wm, interactuable);
        SetInteractables(pbt, interactuable);
        SetInteractables(pbb, interactuable);
        SetInteractables(pbo, interactuable);
        SetInteractables(pbd, interactuable);
        SetInteractables(toggles, interactuable);
        SetInteractables(buttons, interactuable);
        SetInteractables(dD, interactuable);
    }

    private void SetInteractables(UnityEngine.UI.Selectable[] objects, bool interactuable)
    {
        foreach (var obj in objects)
        {
            obj.interactable = interactuable;
        }
    }

    private void Reset(){
        wm[0].text = "100";
        wm[1].text = "100";
        wm[2].text = "15";
        wm[3].text = "15";

        pbt[0].maxValue = 30;
        pbt[0].value = 30;
        pbt[1].maxValue = 8;
        pbt[1].value = 8;
        pbt[2].maxValue = 32;
        pbt[2].value = 32;
        pbt[3].maxValue = 30;
        pbt[3].value = 30;
        pbb[0].maxValue = 55;
        pbb[0].value = 55;
        pbb[1].maxValue = 30;
        pbb[1].value = 30;
        pbb[2].maxValue = 20;
        pbb[2].value = 20;
        pbo[0].value = 25;
        pbo[1].value = 25;
        pbo[2].value = 25;
        pbd[0].value = 50;
        pbd[1].value = 5;
    }

    private void fixvalues(){
        pbt[0].maxValue = 100-(pbt[1].value + pbt[2].value + pbt[3].value);
        pbt[1].maxValue = 100-(pbt[0].value + pbt[2].value + pbt[3].value);
        pbt[2].maxValue = 100-(pbt[0].value + pbt[1].value + pbt[3].value);
        pbt[3].maxValue = 100-(pbt[0].value + pbt[1].value + pbt[2].value);
        pbb[0].maxValue = 100-(pbb[1].value + pbb[2].value);
        pbb[1].maxValue = 100-(pbb[0].value + pbb[2].value);
        pbb[2].maxValue = 100-(pbb[0].value + pbb[1].value);
    }
}
