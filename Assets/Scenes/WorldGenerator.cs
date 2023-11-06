using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TilePlacement : MonoBehaviour
{
    public float ancho = 50f;
    public float alto = 50f;
    public float worldsize = 15f;
    public float biomesize = 15f;
    public Tilemap tilemap;
    public TileBase[] tile;

    public Slider[] probabilities;

    public GameObject camera;

    private int amountofbiomes;
    private Vector2[] points;
    private int seed;
    private void Start() {
        resetProbabilities();

        amountofbiomes= (int)(ancho *alto/70f);
        points = new Vector2[amountofbiomes];
        for (int i = 0; i < amountofbiomes; i++)
        {
            points[i]= new Vector2(Random.Range(0f,ancho),Random.Range(0f,alto));
        }
        GeneratePerlinTexture(ancho,alto,1f,worldsize,biomesize,Random.Range(0, 1000000),Random.Range(1000000, 2000000));
        camera.transform.position = new Vector3(ancho/2,alto/2,camera.transform.position.z);
    }

    private void resetProbabilities(){
        probabilities[0].maxValue = 30;
        probabilities[0].value = 30;
        probabilities[1].maxValue = 9;
        probabilities[1].value = 9;
        probabilities[2].maxValue = 45;
        probabilities[2].value = 45;
        probabilities[3].maxValue=16;
        probabilities[3].value = 16;
        probabilities[4].maxValue = 55;
        probabilities[4].value = 55;
        probabilities[5].maxValue = 30;
        probabilities[5].value = 30;
        probabilities[6].maxValue = 15;
        probabilities[6].value = 15;
    }
    private void Update() {
        fixvalues();

        if(Input.GetKeyDown(KeyCode.R)){
            resetProbabilities();
        }

        if(Input.GetKeyDown(KeyCode.G)){
            for (int i = 0; i < amountofbiomes; i++)
            {
                points[i]= new Vector2(Random.Range(0f,ancho),Random.Range(0f,alto));
            }
            seed++;
            GeneratePerlinTexture(ancho,alto,1f,worldsize,biomesize,Random.Range(0, 1000000),Random.Range(1000000, 2000000));
        }
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

                float perlinElevationValue = Mathf.PerlinNoise(xCoordE,yCoordE) +  0.5f * Mathf.PerlinNoise(2*xCoordE - ancho,2*yCoordE + alto);
                float perlinBiomeValue = Mathf.PerlinNoise(xCoordB,yCoordB) + 0.5f * Mathf.PerlinNoise(2*xCoordB + ancho,2*yCoordB - alto);

                float dcx=2*x/ancho -1;
                float dcy=2*y/alto -1;

                float d = Mathf.Min(1f, ((float)Math.Pow(dcx,2) + (float)Math.Pow(dcy,2)) / Mathf.Sqrt(2));
                Vector3Int cellPosition = tilemap.WorldToCell(new Vector3(x,y,0));
                tilemap.SetTile(cellPosition,selectTile(perlinElevationValue*(1-d),perlinBiomeValue,d));
            }
        }
    }


    private TileBase selectTile(float e, float b, float d){

        if(e < 1.5f * ((float)probabilities[0].value/100f)){ //agua
            return tile[0];
        }
        
        if(e < 1.5f * ((float)(probabilities[0].value+probabilities[1].value)/100f)){ //playa
            return tile[1];
        }

        if(e < 1.5f * ((float)(probabilities[0].value+probabilities[1].value + probabilities[2].value)/100f)){ //tierra
            if(b < 1.5f * ((float)probabilities[4].value/100f)) return tile[3];
            if(b < 1.5f * ((float)(probabilities[4].value + probabilities[5].value)/100f)) return tile[4];
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
    }
}
