using System.Globalization;
using JetBrains.Annotations;
using TMPro;
using Unity.Collections;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TilePlacement : MonoBehaviour
{
    public float ancho = 50f;
    public float alto = 50f;
    public float size = 15f;
    public Tilemap tilemap;
    public TileBase[] tile;

    public Slider[] probabilities;

    public GameObject camera;

    private int amountofbiomes;
    private Vector2[] points;
    private int seed;
    private void Start() {
        probabilities[0].value = 30;
        probabilities[1].value = 9;
        probabilities[2].value = 45;
        probabilities[3].value = 16;
        probabilities[4].value = 55;
        probabilities[5].value = 30;
        probabilities[6].value = 15;

        amountofbiomes= (int)(ancho *alto/70f);
        points = new Vector2[amountofbiomes];
        for (int i = 0; i < amountofbiomes; i++)
        {
            points[i]= new Vector2(Random.Range(0f,ancho),Random.Range(0f,alto));
        }
        GeneratePerlinTexture(ancho,alto,1f, Random.Range(0, 1000000),Random.Range(1000000, 2000000));
        camera.transform.position = new Vector3(ancho/2,alto/2,camera.transform.position.z);
    }
    private void Update() {
        fixvalues();
        if(Input.GetKeyDown(KeyCode.G)){
            for (int i = 0; i < amountofbiomes; i++)
            {
                points[i]= new Vector2(Random.Range(0f,ancho),Random.Range(0f,alto));
            }
            seed++;
            GeneratePerlinTexture(ancho,alto,1f, Random.Range(0, 1000000),Random.Range(1000000, 2000000));
        }
    }

    private void GeneratePerlinTexture(float ancho, float alto, float tilesize, int seedElevation, int seedBiome)
    {
        for (float x = 0; x <= ancho; x += tilesize)
        {
            for (float y = 0; y <= alto; y += tilesize)
            {   
                float xCoordE = Random.Range(0, 1) == 1 ? x / size + seedElevation : x / size - seedElevation;
                float yCoordE = Random.Range(0, 1) == 1 ? y / size + seedElevation : y / size - seedElevation;
                float xCoordB = Random.Range(0, 1) == 1 ? x / size + seedBiome : x / size - seedBiome;
                float yCoordB = Random.Range(0, 1) == 1 ? y / size + seedBiome : y / size - seedBiome;

                float perlinElevationValue = Mathf.PerlinNoise(xCoordE,yCoordE);
                float perlinBiomeValue = Mathf.PerlinNoise(xCoordB,yCoordB);

                Vector3Int cellPosition = tilemap.WorldToCell(new Vector3(x,y,0));
                tilemap.SetTile(cellPosition,selectTile(perlinElevationValue,perlinBiomeValue));
            }
        }
    }


    private TileBase selectTile(float e, float b){
        e = e * 100f;
        b = b * 100f;

        if(e < (float)probabilities[0].value){ //agua
            return tile[0];
        }
        
        if(e < (float)(probabilities[0].value+probabilities[1].value)){ //playa
            return tile[1];
        }

        if(e < (float)(probabilities[0].value+probabilities[1].value + probabilities[2].value)){ //tierra
            if(b < (float)probabilities[4].value) return tile[3];
            if(b < (float)(probabilities[4].value + probabilities[5].value)) return tile[4];
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
