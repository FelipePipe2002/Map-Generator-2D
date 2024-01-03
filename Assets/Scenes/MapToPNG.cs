using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System;

public class TilemapToPng : MonoBehaviour
{

    public Tilemap tm,bm,rm;
    public TMP_InputField[] wm;
    public Text Loading;
    public Sprite waterSprite,grassSprite;

    private Texture2D Img;
    private int iterationsx,iterationsy,itx,ity;
    private float width, height;
    private bool canceled,exported,generar;
    private float chunksize = 30f;
    private Dictionary<Sprite, Color[]> spriteCache;
    private void Start() {
        canceled = false;
        exported = false;
        generar = false;
        iterationsx = Mathf.CeilToInt(float.Parse(wm[0].text)/chunksize);
        iterationsy = Mathf.CeilToInt(float.Parse(wm[1].text)/chunksize);
        itx=0;
        ity=0;
        spriteCache = new Dictionary<Sprite, Color[]>();
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.E)){
            canceled = false;
            exported = false;
            generar = true;
            iterationsx = Mathf.CeilToInt(float.Parse(wm[0].text)/chunksize);
            iterationsy = Mathf.CeilToInt(float.Parse(wm[1].text)/chunksize);
            itx=0;
            ity=0;
            tm = GetComponent<Tilemap>();
            
            Sprite SpriteCualquiera = tm.GetSprite(new Vector3Int(0, 0, 0));
            width = SpriteCualquiera.rect.width;
            height = SpriteCualquiera.rect.height;
            Img = new Texture2D((int)width * tm.size.x, (int)height * tm.size.y);
        }

        float progress = (float)(ity * iterationsx + itx) / (iterationsx * iterationsy) * 100f;

        if(progress <= 100 && generar == true) {
            Loading.enabled = true;
            Loading.text = "Exporting: " + Mathf.RoundToInt(progress) + "%";
        }

        if((ity < iterationsy) && !canceled && generar){

                Pack(itx*chunksize,Mathf.Min((itx+1)*chunksize-1f,float.Parse(wm[0].text)),ity*chunksize,Mathf.Min((ity+1)*chunksize-1f,float.Parse(wm[1].text)),width,height);

                if (itx < iterationsx - 1) {
                    itx++;
                } else {
                    itx = 0;
                    ity++;
                }
        } else if (ity == iterationsy && !exported){
            Img.Apply();
            ExportAsPng("test");
            exported = true;
            generar = false;
        }
    }

    public void Pack (float xi, float xf, float yi,  float yf, float width, float height)
    {
        
        for (int x = (int)xi; x <= xf; x++)
        {
            for(int y = (int)yi; y <= yf; y++)
            {
                Vector3Int spritepos = new Vector3Int(x,y,0);
                Sprite sprite = tm.GetSprite(spritepos);
                if (sprite != null)
                {
                    if(sprite != waterSprite){
                        Sprite aux = rm.GetSprite(spritepos);
                        if(aux == null && sprite == grassSprite){
                            aux = bm.GetSprite(spritepos);
                        }

                        if(aux != null)
                            sprite = aux;
                    }

                    if (!spriteCache.TryGetValue(sprite, out Color[] spriteTexture))
                    {
                        spriteTexture = GetCurrentSprite(sprite);
                        spriteCache.Add(sprite, spriteTexture);
                    }

                    Img.SetPixels(x * (int)width, y * (int)height, (int)width, (int)height, spriteTexture);
                }
            }
        }
    }

    Color[] GetCurrentSprite(Sprite sprite)
    {
        if (sprite == null)
        {
            return new Color[0];
        }

        var pixels = sprite.texture.GetPixels((int)sprite.textureRect.x, 
                                              (int)sprite.textureRect.y, 
                                              (int)sprite.textureRect.width, 
                                              (int)sprite.textureRect.height);

        return pixels;
    }

     public void ExportAsPng (string name) //metodo que exporta como png
     {
         byte[] bytes = Img.EncodeToPNG();
         var dirPath = Application.dataPath + "/Exported Tilemaps/";
         if (!Directory.Exists(dirPath))
         {
             Directory.CreateDirectory(dirPath);
         }
         File.WriteAllBytes(dirPath + name + ".png", bytes);
        Img = null;
     }

}