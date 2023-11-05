using System.Globalization;
using JetBrains.Annotations;
using TMPro;
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
    private int seed;
    private void Start() {
        probabilities[0].value = 30;
        probabilities[1].value = 9;
        probabilities[2].value = 36;
        probabilities[3].value = 25;
        GeneratePerlinTexture(ancho,alto,1f, Random.Range(0, 1000000));
        camera.transform.position = new Vector3(ancho/2,alto/2,camera.transform.position.z);
    }
    private void Update() {
        fixvalues();
        if(Input.GetKeyDown(KeyCode.G)){
            seed++;
            GeneratePerlinTexture(ancho,alto,1f, Random.Range(0, 1000000));
        }
    }
    private void GeneratePerlinTexture(float ancho, float alto, float tilesize, int seed)
    {
        for (float x = 0; x <= ancho; x += tilesize)
        {
            for (float y = 0; y <= alto; y += tilesize)
            {   
                float xCoord;
                if(Random.Range(0, 1) == 1){
                     xCoord = x / size + seed;
                } else {
                    xCoord = x / size - seed;
                }

                float yCoord;
                if(Random.Range(0, 1) == 1){
                     yCoord = y / size + seed;
                } else {
                    yCoord = y / size - seed;
                }
                
                float perlinValue = Mathf.PerlinNoise(xCoord,yCoord);

                Vector3 pos = new Vector3(x,y,0);
                settile(pos,perlinValue);
            }
        }
    }

    private void settile(Vector3 pos, float perlinValue){
        Vector3Int cellPosition = tilemap.WorldToCell(pos);
        perlinValue = perlinValue * 100f;
        if(perlinValue < (float)probabilities[0].value){
            tilemap.SetTile(cellPosition, tile[0]);
        } else if(perlinValue < (float)(probabilities[0].value+probabilities[1].value)){
            tilemap.SetTile(cellPosition, tile[1]);
        } else if(perlinValue < (float)(probabilities[0].value+probabilities[1].value + probabilities[2].value)){
            tilemap.SetTile(cellPosition, tile[2]);
        } else {
            tilemap.SetTile(cellPosition, tile[3]);
        }
    }

    private void fixvalues(){
        probabilities[0].maxValue = 100-(probabilities[1].value + probabilities[2].value + probabilities[3].value);
        probabilities[1].maxValue = 100-(probabilities[0].value + probabilities[2].value + probabilities[3].value);
        probabilities[2].maxValue = 100-(probabilities[0].value + probabilities[1].value + probabilities[3].value);
        probabilities[3].maxValue = 100-(probabilities[0].value + probabilities[1].value + probabilities[2].value);
    }
}
