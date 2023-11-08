using System;
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
    public GameObject camera;
    public Toggle ds,gm;
    public TMP_InputField[] wm;
    public Tile[] tile,objetos;
    public Slider[] pbt,pbb,pbo,pbd;

    public Tile whitetile;

    //to save data    
    private string[] wmold;
    private float[] pbtold,pbbold,pboold,pbdold;
    private string fold;
    private bool dsold,gmold;

    private int seedElevation,seedBiome;

    private void Start() {
        wmold = new string[wm.Length];
        pbtold = new float[pbt.Length];
        pbbold = new float[pbb.Length];
        pboold = new float[pbo.Length];
        pbdold = new float[pbd.Length];
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

    private void GeneratePerlinTexture(float ancho, float alto, float tilesize, float worldsize, float biomesize, int seedElevation, int seedBiome)
    {
        float[] randomValues = new float[6];
        for (int i = 0; i < 4; i++) {
            randomValues[i] = Random.Range(0, 1);
        }

        for (float x = 0; x <= ancho; x += tilesize)
        {
            for (float y = 0; y <= alto; y += tilesize)
            {   
                float xCoordE = randomValues[0] == 1 ? x / worldsize + seedElevation : x / worldsize - seedElevation;
                float yCoordE = randomValues[1] == 1 ? y / worldsize + seedElevation : y / worldsize - seedElevation;
                float xCoordB = randomValues[2] == 1 ? x / biomesize + seedBiome : x / biomesize - seedBiome;
                float yCoordB = randomValues[3] == 1 ? y / biomesize + seedBiome : y / biomesize - seedBiome;
                float xCoordO = randomValues[4] == 1 ? x / 1.5f + seedElevation : x / 1.5f - seedElevation;
                float yCoordo = randomValues[5] == 1 ? y / 1.5f + seedElevation : y / 1.5f - seedElevation;

                float perlinElevationValue = Mathf.PerlinNoise(xCoordE,yCoordE) +  0.5f * Mathf.PerlinNoise(2*xCoordE - ancho,2*yCoordE + alto) + 0.25f * Mathf.PerlinNoise(4*xCoordE - 2*ancho,4*yCoordE + 2*alto);
                float perlinBiomeValue = Mathf.PerlinNoise(xCoordB,yCoordB) + 0.5f * Mathf.PerlinNoise(2*xCoordB + ancho,2*yCoordB - alto) + 0.25f * Mathf.PerlinNoise(4*xCoordB + 2*ancho,4*yCoordB - 2*alto);
                float perlinObjectValue = Mathf.PerlinNoise(xCoordO,yCoordo);


                float dcx=2*x/ancho -1;
                float dcy=2*y/alto -1;

                Vector3Int cellPosition = tilemap.WorldToCell(new Vector3(x,y,0));
                float LinearShaping = pbd[0].value/100f;
                
                perlinElevationValue = (1-LinearShaping)*perlinElevationValue + LinearShaping * (1-formula.calcular(dcx,dcy,pbd[1].value,pbd[1].maxValue,perlinElevationValue)) * 1.75f;
                
                if (!gm.isOn){
                    Tile selected = selectTile(perlinElevationValue,perlinBiomeValue);
                    
                    if(ds.isOn)
                        selected.color = depth(perlinElevationValue);
                    else
                        selected.color = Color.white;

                    tilemap.SetTile(cellPosition,selected);
                    objectmap.SetTile(cellPosition,selectObject(perlinElevationValue,perlinObjectValue,perlinBiomeValue));
                } else {
                    float color = perlinElevationValue/1.75f;
                    whitetile.color = new Color(color,color,color);
                    tilemap.SetTile(cellPosition,whitetile);
                }
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

    private bool settingschanged(){
        bool changed = false;

        for (int i = 0; i < pbt.Length && !changed; i++)
        {
            changed = pbtold[i] != pbt[i].value;
        }

        for (int i = 0; i < pbb.Length && !changed; i++)
        {
            changed = pbbold[i] != pbb[i].value;
        }

        for (int i = 0; i < pbo.Length && !changed; i++)
        {
            changed = pboold[i] != pbo[i].value;
        }

        if(!changed)
            changed = fold != formula.getFormula();

        if(!changed && formula.getFormula() != "No Effect"){  
            for (int i = 0; i < pbd.Length && !changed; i++)
            {
                changed = pbdold[i] != pbd[i].value;
            }
        }

        if(!changed)
            changed = dsold != ds.isOn;

        if(!changed)
            changed = gmold != gm.isOn;

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
        for (int i = 0; i < pbt.Length; i++)
        {
            pbtold[i] = pbt[i].value;
        }

        for (int i = 0; i < pbb.Length; i++)
        {
            pbbold[i] = pbb[i].value;
        }

        for (int i = 0; i < pbo.Length; i++)
        {
            pboold[i] = pbo[i].value;
        }

        for (int i = 0; i < pbd.Length; i++)
        {
            pbdold[i] = pbd[i].value;
        }

        for (int i = 0; i < wm.Length; i++)
        {
            wmold[i] = wm[i].text;
        }

        fold = formula.getFormula();
        dsold = ds.isOn;
        gmold = gm.isOn;
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
