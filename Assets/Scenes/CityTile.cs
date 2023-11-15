using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class CityTile : MonoBehaviour
{
    public Tilemap terrainmap,citymap;
    public Camera camara;
    public TilePlacement world;
    public Tile[] tile,cityobjects;

    private bool mostrando;
    private int iterationsx,iterationsy,it,itx,ity;
    private float chunksize = 50f;
    private TileBase tileworld;
    private Vector3 citypos;
    private Vector3Int highestpoint;
    private List<Vector3Int> area,limitearea;
    private int mapsize=50;
    private void Start() {
        mostrando = false;
        iterationsx = Mathf.CeilToInt(mapsize/chunksize);
        iterationsy = Mathf.CeilToInt(mapsize/chunksize);
        it=0;
        itx=0;
        ity=0;
        area = new List<Vector3Int>();
        limitearea = new List<Vector3Int>();
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0) && camara.GetComponent<Camera>().orthographicSize < 60f){
            citypos = Input.mousePosition;
            citypos.z = 0;
            citypos = Camera.main.ScreenToWorldPoint(citypos);
            if(world.clickOnCiudad(citypos)){
                mostrando = true;

                terrainmap.ClearAllTiles();
                citymap.ClearAllTiles();

                world.guardarCamaraPos();
                tileworld = world.getTilePos(citypos);
                world.MostrarMapas(false);
                citypos = world.getMapPos(citypos);

                terrainmap.GetComponent<TilemapRenderer>().enabled = true;
                citymap.GetComponent<TilemapRenderer>().enabled = true;
                camara.transform.position = new Vector3(mapsize/2,mapsize/2,camara.transform.position.z);
                
                itx=0;
                ity=0;
                it=0;
                highestpointvalue = 0;
                area = new List<Vector3Int>();
                limitearea = new List<Vector3Int>();
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape) && mostrando){
            terrainmap.GetComponent<TilemapRenderer>().enabled = false;
            citymap.GetComponent<TilemapRenderer>().enabled = false;
            world.MostrarMapas(true);
            mostrando = false;
            world.restaurarCamaraPos();
        }

        if((ity < iterationsy) && it<3 && mostrando){
                
                switch (it)
                {
                    case 0:
                        GenerateTerrain(itx*chunksize,Mathf.Min((itx+1)*chunksize-1f,mapsize),ity*chunksize,Mathf.Min((ity+1)*chunksize-1f,mapsize),1f,12f,(int)Mathf.Round(citypos.x * 123f + citypos.y * 321f));
                        break;
                    case 1:
                        GenerateCityArea(highestpoint.x,highestpoint.y,12f,(int)Mathf.Round(citypos.x * 123f + citypos.y * 321f));
                        citymap.SetTile(highestpoint,cityobjects[0]);
                        GenerateWalls();
                        break;
                    case 2:
                        break;
                }

                if (itx < iterationsx - 1) {
                    itx++;
                } else {
                    itx = 0;
                    ity++;
                }
            }

            if(ity == iterationsx && itx == 0 && it<3){
                it++;
                itx = 0;
                ity = 0;
            }
    }

    private float highestpointvalue = 0;
    public void GenerateTerrain(float xi, float xf, float yi,  float yf, float tilesize, float worldsize, int seedElevation)
    {
        for (float x = xi; x <= xf; x += tilesize)
        {
            for (float y = yi; y <= yf; y += tilesize)
            {   
                float xCoordE = x / worldsize + seedElevation;
                float yCoordE = y / worldsize + seedElevation;

                float perlinElevationValue = Mathf.Clamp(Mathf.PerlinNoise(xCoordE,yCoordE),0f,1f);
                Vector3Int cellPosition = terrainmap.WorldToCell(new Vector3(x,y,0));


                if(perlinElevationValue>highestpointvalue && 15<x && x<35 && 15<y && y<35){
                    highestpointvalue = perlinElevationValue;
                    highestpoint = cellPosition;

                }
                
                float aux = (255f - (50*Mathf.Clamp(perlinElevationValue-0.3f,0,1f)))/255f;
                Tile selected = null;
                foreach (Tile t in tile)
                {
                    if(t == tileworld){
                        selected = t;
                        break;
                    }
                }

                if(perlinElevationValue < 0.3){
                    selected = tile[1];
                }

                if(perlinElevationValue < 0.2){
                    selected = tile[0];
                }
                Color elevacion = new Color(aux,aux,aux);
                selected.color = elevacion;
                terrainmap.SetTile(cellPosition,selected);
            }
        }
    }

    private void GenerateCityArea(int x, int y, float w, int s)
    {   
        Vector3Int aux = new Vector3Int(x,y,0);

        if(IsValidPosition(x,y) && !area.Contains(aux) && !limitearea.Contains(aux)){
            float distance = Vector3Int.Distance(aux, highestpoint);
            Backtraking(aux,x,y,-1,0,distance,w,s);
            Backtraking(aux,x,y,1,0,distance,w,s);
            Backtraking(aux,x,y,0,-1,distance,w,s);
            Backtraking(aux,x,y,0,1,distance,w,s);
        }
    }

    private bool IsValidPosition(int x, int y)
    {
        return 0<x && x<mapsize && 0<y && y<mapsize;
    }
    
    private void Backtraking(Vector3Int pos,int x,int y,int h,int v,float distance, float w, int s){
        Vector3Int cellPosition = citymap.WorldToCell(new Vector3(x,y,0));
        if(IsValidPosition(x+h,y+v) && Mathf.Clamp(Mathf.PerlinNoise(calcCoord(x+h,w,s),calcCoord(y+v,w,s)),0f,1f) > highestpointvalue-.4f && distance<10){
            citymap.SetTile(cellPosition,cityobjects[2]);
            area.Add(pos);
            GenerateCityArea(x+h, y+v, w, s);
        } else {
            limitearea.Add(pos);
            citymap.SetTile(cellPosition,cityobjects[2]);
        }
    }

    private void GenerateWalls(){
        foreach (Vector3 limite in limitearea)
        {
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int yOffset = -1; yOffset <= 1; yOffset++)
                {
                    Vector3Int aux = new Vector3Int((int)limite.x + xOffset, (int)limite.y + yOffset, 0);

                    if (citymap.GetTile(aux) != cityobjects[2])
                    {
                        citymap.SetTile(aux, cityobjects[1]);
                    }
                }
            }
        }
    }

    public float calcCoord(int i,float w,float s){
        return i / w + s;
    }

    public bool MostrandoCiudad(){
        return mostrando;
    }
}
