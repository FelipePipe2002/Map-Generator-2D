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
    public CityTile ct;
    public TMP_InputField[] wm;
    public Tile[] tile,objetos;
    public Slider[] pbt,pbo,pbd;

    private int seedElevation,seedTemperature,seedPrecipitation,iterationsx,iterationsy,it,itx,ity;
    private float chunksize = 30f;
    private bool canceled,hide;
 
    private void Start() {
        Reset();
        canceled = false;
        hide = false;
        seedElevation = Random.Range(0, 100000);
        seedTemperature = Random.Range(100000, 200000);
        seedPrecipitation = Random.Range(200000, 300000);

        iterationsx = Mathf.CeilToInt(float.Parse(wm[0].text)/chunksize);
        iterationsy = Mathf.CeilToInt(float.Parse(wm[1].text)/chunksize);
        it=0;
        itx=0;
        ity=0;

        for (int i = 0; i < tile.Length; i++)
        {
            tile[i].color = new Color(255f,255f,255f);
        }

        camara.transform.position = new Vector3(float.Parse(wm[0].text)/2,float.Parse(wm[1].text)/2,camara.transform.position.z);
    }
    //create generate points
    private void Update() {
        if(!ct.MostrandoCiudad()){
            fixvalues();
            
            if(camara.GetComponent<Camera>().orthographicSize >= 60f){
                objectmap.GetComponent<TilemapRenderer>().enabled = false;
            } else {
                if(!hide) objectmap.GetComponent<TilemapRenderer>().enabled = true;
            }

            if(Input.GetKeyDown(KeyCode.R) && (it==4 || canceled)){
                Reset();
            }

            if(Input.GetKeyDown(KeyCode.C)){
                canceled = true;
            }

            if(Input.GetKeyDown(KeyCode.H) && !ct.MostrandoCiudad()){
                hide = !hide;
                if(!hide){
                    MostrarMapas(true);
                } else {
                    MostrarMapas(false);
                }
            }

            if(Input.GetKeyDown(KeyCode.G)){
                seedElevation = Random.Range(0, 100000);
                seedTemperature = Random.Range(100000, 200000);
                seedPrecipitation = Random.Range(200000, 300000);
                iterationsx = Mathf.CeilToInt(float.Parse(wm[0].text)/chunksize);
                iterationsy = Mathf.CeilToInt(float.Parse(wm[1].text)/chunksize);
                it=0;
                itx=0;
                ity=0;
                canceled = false;
                camara.transform.position = new Vector3(float.Parse(wm[0].text)/2,float.Parse(wm[1].text)/2,camara.transform.position.z);
                for (int i = 0; i < tile.Length; i++)
                {
                    tile[i].color = new Color(255f,255f,255f);
                }
            }

            float progress = (float)(ity * iterationsx + itx) / (iterationsx * iterationsy) * 100f;
            if(it == 0){
                Loading.enabled = true;
                Loading.text = "Generating World: " + Mathf.RoundToInt(progress) + "%";
            }
            else if(it == 1)
                Loading.text = "Generating World: 100%\nGenerating Biomes: " + Mathf.RoundToInt(progress) + "%";
            else if(it == 2)
                Loading.text = "Generating World: 100%\nGenerating Biomes: 100%\nGenerating Rivers: " + Mathf.RoundToInt(progress) + "%";
            else if(it == 3)
                Loading.text = "Generating World: 100%\nGenerating Biomes: 100%\nGenerating Rivers: 100%\nGenerating Structures: " + Mathf.RoundToInt(progress) + "%";
            else {
                Loading.enabled = false;
            }


            if((ity < iterationsy) && !canceled && it<4){
                if(itx == 0 && ity == 0 && it==0){
                    objectmap.ClearAllTiles();
                    tilemap.ClearAllTiles();
                }
                
                InteractuarConModificadores(false);
                switch (it)
                {
                    case 0:
                        GenerateTerrain(itx*chunksize,Mathf.Min((itx+1)*chunksize-1f,float.Parse(wm[0].text)),ity*chunksize,Mathf.Min((ity+1)*chunksize-1f,float.Parse(wm[1].text)),int.Parse(wm[0].text),int.Parse(wm[1].text),1f,int.Parse(wm[2].text),seedElevation);
                        break;
                    case 1:
                        GenerateBiomes(itx*chunksize,Mathf.Min((itx+1)*chunksize-1f,float.Parse(wm[0].text)),ity*chunksize,Mathf.Min((ity+1)*chunksize-1f,float.Parse(wm[1].text)),int.Parse(wm[0].text),int.Parse(wm[1].text),1f,int.Parse(wm[3].text),seedTemperature,seedPrecipitation);
                        break;
                    case 2:
                        GenerateRivers(itx*chunksize,Mathf.Min((itx+1)*chunksize-1f,float.Parse(wm[0].text)),ity*chunksize,Mathf.Min((ity+1)*chunksize-1f,float.Parse(wm[1].text)),int.Parse(wm[0].text),int.Parse(wm[1].text),1f,int.Parse(wm[3].text),seedTemperature);                    
                        break;
                    case 3:
                        GenerateObjects(itx*chunksize,Mathf.Min((itx+1)*chunksize-1f,float.Parse(wm[0].text)),ity*chunksize,Mathf.Min((ity+1)*chunksize-1f,float.Parse(wm[1].text)),1f);
                        break;
                }

                if (itx < iterationsx - 1) {
                    itx++;
                } else {
                    itx = 0;
                    ity++;
                }
            } else {
                InteractuarConModificadores(true);
            }

            if(ity == iterationsx && itx == 0 && it<4){
                it++;
                itx = 0;
                ity = 0;
            }
        }
    }

    private void GenerateTerrain(float xi, float xf, float yi,  float yf, float ancho, float alto, float tilesize, float worldsize, int seedElevation)
    {
        for (float x = xi; x <= xf; x += tilesize)
        {
            for (float y = yi; y <= yf; y += tilesize)
            {   
                float xCoordE = x / worldsize + seedElevation;
                float yCoordE = y / worldsize + seedElevation;

                float perlinElevationValue = CalcNoise(xCoordE,yCoordE,4);

                float dcx=2*x/ancho -1;
                float dcy=2*y/alto -1;

                Vector3Int cellPosition = tilemap.WorldToCell(new Vector3(x,y,0));
                float LinearShaping = pbd[0].value/100f;
                
                perlinElevationValue = (1-LinearShaping)*perlinElevationValue + LinearShaping * (1-formula.calcular(dcx,dcy,pbd[1].value,pbd[1].maxValue,perlinElevationValue)) * 1.75f;
                
                Tile selected = selectTileTerrain(perlinElevationValue);
                
                if(ds.isOn && perlinElevationValue < 1.75f * ((float)pbt[0].value/100f))
                    selected.color = depth(perlinElevationValue);
                else
                    selected.color = Color.white;

                tilemap.SetTile(cellPosition,selected);
            }
        }
    }

    private void GenerateBiomes(float xi, float xf, float yi,  float yf, float ancho, float alto, float tilesize, float biomesize, int seedTemperatura, int seedPrecipitation)
    {
        for (float x = xi; x <= xf; x += tilesize)
        {
            for (float y = yi; y <= yf; y += tilesize)
            {   
                Vector3Int cellPosition = tilemap.WorldToCell(new Vector3(x,y,0));
                TileBase aux = tilemap.GetTile(cellPosition);
                if(aux == tile[3]){
                    float xCoordT = x / biomesize + seedTemperatura;
                    float yCoordT = y / biomesize - seedTemperatura;

                    float xCoordP = x / biomesize - seedPrecipitation;
                    float yCoordP = y / biomesize + seedPrecipitation;

                    float perlinTemperatureValue = CalcNoise(xCoordT,yCoordT,2);
                    float perlinPrecipitationValue = CalcNoise(xCoordP,yCoordP,2);

                    Tile selected = selectTileBiome(perlinTemperatureValue,perlinPrecipitationValue);
                    tilemap.SetTile(cellPosition,selected);
                }
            }
        }
    }

    private void GenerateRivers(float xi, float xf, float yi,  float yf, float ancho, float alto, float tilesize, float biomesize, int seedTemperatura)
    {
        for (float x = xi; x <= xf; x += tilesize)
        {
            for (float y = yi; y <= yf; y += tilesize)
            {   
                Vector3Int cellPosition = tilemap.WorldToCell(new Vector3(x,y,0));
                TileBase aux = tilemap.GetTile(cellPosition);
                
                if(aux != tile[0]){
                    float xCoordT = x / biomesize + seedTemperatura;
                    float yCoordT = y / biomesize - seedTemperatura;

                    float t = CalcNoise(xCoordT,yCoordT,2);
                    float edge1 = 1.75f/3f;

                    if(edge1<t && t<edge1+0.05f){
                        tile[0].color = new Color(255f,255f,255f);
                        tilemap.SetTile(cellPosition,tile[0]);
                    }
                }
            }
        }
    }

    private void GenerateObjects(float xi, float xf, float yi,  float yf, float tilesize)
    {
        for (float x = xi; x <= xf; x += tilesize)
        {
            for (float y = yi; y <= yf; y += tilesize)
            {   
                Vector3Int cellPosition = tilemap.WorldToCell(new Vector3(x,y,0));
                TileBase aux = tilemap.GetTile(cellPosition);

                bool isAuxInSubset = false;
                for (int i = 4; i <= 10; i++) {
                    if (tile[i] == aux) {
                        isAuxInSubset = true;
                        break;
                    }
                }

                if(isAuxInSubset){
                    float xCoordO = x / 1.5f + seedElevation;
                    float yCoordo = y / 1.5f + seedElevation;

                    float perlinObjectValue = Mathf.PerlinNoise(xCoordO,yCoordo);
                    TileBase selected = selectObject(perlinObjectValue, aux);

                    objectmap.SetTile(cellPosition,selected);
                }
            }
        }
    }

    public Tile selectTileTerrain(float e){

        if(e < 1.75f * ((float)pbt[0].value/100f)){ //agua
            return tile[0];
        } else if(e < 1.75f * ((float)(pbt[0].value+pbt[1].value)/100)){ //playa
            return tile[1];
        } else if(e < 1.75f * ((float)(pbt[0].value+pbt[1].value + pbt[2].value)/100f)){ //tierra
            return tile[3];
        }

        return tile[2]; //montaña
    }

    private Tile selectTileBiome(float t,float p){
        if (t< 0.30f){
            return tile[9]; //snow
        } else if (t<0.6f){
            if(p<.65f){
                return tile[4]; //plains
            } else {
                return tile[5]; //taiga
            }
        } else if(t<.75){
            if(p<0.8f){
                return tile[4]; //plains
            } else {
                return tile[6]; //dark forest
            }
        }else if(t<1f){
            if(p<0.65f){
                return tile[7]; //savanna
            } else if(p<.85f){
                return tile[6]; //dark forest
            } else{
                return tile[8]; //jungle
            }
        }else {
            if(p<1.05f){
                return tile[10]; //desert
            } else {
                return tile[8]; //jungle
            }
        }
    }

    private TileBase selectObject(float o, TileBase b){
        TileBase aux = null;

        if(o > 1f - (float)pbo[0].value / 100f ) 
            aux = objetos[0];
        else if(b == tile[4] && o < (float)pbo[1].value / 100f) //plains
            aux = objetos[1];
        else if(b == tile[5] && o < (float)pbo[2].value / 100f) //taiga
            aux = objetos[2];
        else if(b == tile[6] && o < (float)pbo[3].value / 100f) //dark forest
            aux = objetos[3];
        else if(b == tile[7] && o < (float)pbo[4].value / 100f) //savanna
            aux = objetos[4];
        else if(b == tile[8] && o < (float)pbo[5].value / 100f) //jungle
            aux = objetos[5];
        else if(b == tile[9] && o < (float)pbo[6].value / 100f) //snow
            aux = objetos[6];
        else if(b == tile[10] && o < (float)pbo[7].value / 100f) //desert
            aux = objetos[7];

        return aux;
    }

    private Color depth(float e){
        float perlinvalue = Mathf.Round(e*100f)/100f;

        float color = (100f + 155f*(perlinvalue/(1.75f * ((float)pbt[0].value/100f))))/255f;

        Color aux = new Color(color,color,color);
        return aux;
    }

    private float CalcNoise(float xCoord, float yCoord, int it){
        float aux = 0f;

        for (int i = 0; i < it; i++)
        {
            aux += Mathf.Pow(2,-i) * Mathf.Clamp(Mathf.PerlinNoise(Mathf.Pow(2,i)*xCoord,Mathf.Pow(2,i)*yCoord),0,1f);
        }

        return aux;
    }

    public Toggle[] toggles;
    public Button[] buttons;
    public TMP_Dropdown[] dD;
    private void InteractuarConModificadores(bool interactuable)
    {
        SetInteractables(wm, interactuable);
        SetInteractables(pbt, interactuable);
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
        wm[0].text = "500";
        wm[1].text = "500";
        wm[2].text = "55";
        wm[3].text = "45";

        pbt[0].maxValue = 45;
        pbt[0].value = 45;
        pbt[1].maxValue = 3;
        pbt[1].value = 3;
        pbt[2].maxValue = 22;
        pbt[2].value = 22;
        pbt[3].maxValue = 30;
        pbt[3].value = 30;
        pbo[0].value = 5;
        pbo[1].value = 25;
        pbo[2].value = 25;
        pbo[3].value = 25;
        pbo[4].value = 25;
        pbo[5].value = 25;
        pbo[6].value = 25;
        pbo[7].value = 25;
        pbd[0].value = 50;
        pbd[1].value = 5;
    }
    private void fixvalues(){
        pbt[0].maxValue = 100-(pbt[1].value + pbt[2].value + pbt[3].value);
        pbt[1].maxValue = 100-(pbt[0].value + pbt[2].value + pbt[3].value);
        pbt[2].maxValue = 100-(pbt[0].value + pbt[1].value + pbt[3].value);
        pbt[3].maxValue = 100-(pbt[0].value + pbt[1].value + pbt[2].value);
    }

    public void MostrarMapas(bool mostrar){
        tilemap.GetComponent<TilemapRenderer>().enabled = mostrar;
        objectmap.GetComponent<TilemapRenderer>().enabled = mostrar;
    }

    public bool clickOnCiudad(Vector3 pos){
        Vector3Int cellPosition = objectmap.WorldToCell(pos);
        TileBase aux = objectmap.GetTile(cellPosition);
        return aux == objetos[0];
    }
    private Vector3 guardasposcamara;
    private float camerasize;
    public void guardarCamaraPos(){
        guardasposcamara = camara.transform.position;
        camerasize = camara.GetComponent<Camera>().orthographicSize;
        camara.GetComponent<Camera>().orthographicSize = 50f;
    }

    public void restaurarCamaraPos(){
        camara.transform.position = guardasposcamara;
        camara.GetComponent<Camera>().orthographicSize = camerasize;
    }

    public int getSeedElevation(){
        return seedElevation;
    }

    public Vector3 getMapPos(Vector3 pos){
        return tilemap.WorldToCell(pos);
    }

    public TileBase getTilePos(Vector3 pos){
        Vector3Int cellPosition = tilemap.WorldToCell(pos);
        TileBase aux = tilemap.GetTile(cellPosition);
        return aux;
    }
}
