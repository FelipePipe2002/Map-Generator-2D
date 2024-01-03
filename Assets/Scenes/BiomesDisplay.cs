using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Tilemaps;


public class BiomesDisplay : MonoBehaviour
{
    int columns = 5, rows = 5, lineThickness=7;
    float mintemperature=-10, maxtemperature=30,maxprecipitation=400;
    float cellWidth = 80f, cellHeight = 50f;
    float centerX,centerY;
    float minXPOS=float.MaxValue,maxXPOS=float.MinValue,minYPOS=float.MaxValue,maxYPOS=float.MinValue;
    string csvFilePath = "Assets/Scenes/Biomes.csv";
    GameObject[,] grid;

    public Tile[] biomes;

    GameObject selectedLine,selectedText;
    bool isDragging = false;
    List<GameObject> horizontalLines = new List<GameObject>();
    List<GameObject> verticalLines = new List<GameObject>();
    List<float> horizontalLinesPos = new List<float>();
    List<float> verticalLinesPos = new List<float>();
    string[,] csvData;
    Dictionary<string, Color> biomeColors = new Dictionary<string, Color>
    {
        { "0", Color.white },
        { "1", new Color(180f/255f,95f/255f,6f/255f) },
        { "2", new Color(39f/255f, 78f/255f, 19f/255f) },
        { "3", new Color(56f/255f,118f/255f,29f/255f) },
        { "4", new Color(106f/255f,168f/255f,79f/255f) },
        { "5", new Color(241f/255f,195f/255f,50f/255f) },
        { "6", new Color(191f/255f,144f/255f,0f) }
    };
    
    private void Awake() {
        csvData = ReadCSVFile(csvFilePath);
        centerX = columns * cellWidth / 2f;
        centerY = rows * cellHeight / 2f;

        grid = new GameObject[columns,rows];
        if (csvData != null)
        {   
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    GameObject cube = new GameObject("p:"+i+"-"+j+" b:"+csvData[i, j]);

                    Image cubeImageComponent = cube.AddComponent<Image>();
                    cubeImageComponent.transform.SetParent(transform, false);
                    float xPos = j * cellWidth - centerX;
                    float yPos = -i * cellHeight + centerY - cellHeight;
                    cubeImageComponent.transform.localPosition = new Vector3(xPos, yPos, 0f);

                    if(xPos>maxXPOS) maxXPOS = xPos + cellWidth;
                    if(xPos<minXPOS) minXPOS = xPos;
                    if(yPos>maxYPOS) maxYPOS = yPos + cellHeight;
                    if(yPos<minYPOS) minYPOS = yPos;

                    RectTransform cuberectTransform = cube.GetComponent<RectTransform>();
                    cube.GetComponent<RectTransform>().pivot = new Vector2(0f, 0f);
                    cuberectTransform.sizeDelta = new Vector2(cellWidth, cellHeight);
                    string biome = csvData[i, j];
                    if (biomeColors.ContainsKey(biome))
                    {
                        cubeImageComponent.color = biomeColors[biome];
                    }
                    else
                    {
                        cubeImageComponent.color = Color.gray;
                    }
                    grid[i,j] = cube;
                }

                
            }

            for (int i = 0; i < rows - 1; i++)
            {
                CreateHorizontalLine(i, centerY);
            }

            for (int j = 0; j < columns - 1; j++)
            {
                CreateVerticalLine(j, centerX);
            }
        }
    }

    private GameObject CreateHorizontalLine(int i, float centerY)
    {
        GameObject line = new GameObject("HorizontalLine" + (i));
        Image lineImageComponent = line.AddComponent<Image>();
        lineImageComponent.transform.SetParent(transform, false);
        lineImageComponent.color = Color.black; // Set the color of the line
        lineImageComponent.rectTransform.sizeDelta = new Vector2(columns * cellWidth, lineThickness);
        float lineYPos = -(i + 0.5f) * cellHeight + centerY - cellHeight / 2f;
        lineImageComponent.rectTransform.localPosition = new Vector3(0f, lineYPos, 0f);
        horizontalLinesPos.Add(lineImageComponent.rectTransform.localPosition.y);

        // Add text component
        GameObject textObject = new GameObject("LineText" + i);
        TextMeshProUGUI lineText = textObject.AddComponent<TextMeshProUGUI>();
        lineText.transform.SetParent(line.transform, false);
        float percentage = ((Mathf.Round(lineImageComponent.rectTransform.localPosition.y)/centerY)+1f)/2;
        lineText.text = MathF.Round(maxprecipitation*percentage) + "cm";
        //lineText.text = MathF.Round(maxprecipitation*percentage + minprecipitation*(1-percentage)) + "cm";
        lineText.color = Color.white;
        lineText.fontSize = 20;
        lineText.alignment = TextAlignmentOptions.MidlineRight;
        lineText.rectTransform.pivot = new Vector2(1f, 0.5f);
        lineText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        lineText.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        textObject.transform.position = new Vector3(textObject.transform.position.x - 3f,textObject.transform.position.y);


        // Add EventTrigger for mouse down event
        EventTrigger eventTrigger = line.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((data) => { OnLineClick(line,textObject); });
        eventTrigger.triggers.Add(entry);
        horizontalLines.Add(line);
        return line;
    }

    private GameObject CreateVerticalLine(int j, float centerX)
    {
        GameObject line = new GameObject("VerticalLine" + j);
        Image lineImageComponent = line.AddComponent<Image>();
        lineImageComponent.transform.SetParent(transform, false);
        lineImageComponent.color = Color.black; // Set the color of the line
        lineImageComponent.rectTransform.sizeDelta = new Vector2(lineThickness, rows * cellHeight);
        float lineXPos = (j + 0.5f) * cellWidth - centerX + cellWidth / 2f;
        lineImageComponent.rectTransform.localPosition = new Vector3(lineXPos, 0f, 0f);
        verticalLinesPos.Add(lineImageComponent.rectTransform.localPosition.x);

        // Add text component
        GameObject textObject = new GameObject("LineText" + j);
        TextMeshProUGUI lineText = textObject.AddComponent<TextMeshProUGUI>();
        lineText.transform.SetParent(line.transform, false);
        float percentage = ((Mathf.Round(lineImageComponent.rectTransform.localPosition.x)/centerX)+1f)/2;
        lineText.text = MathF.Round(maxtemperature*percentage + mintemperature*(1-percentage),1) + "°";
        //lineText.text = MathF.Round(maxtemperature*percentage + mintemperature*(1-percentage),1) + "*";
        lineText.color = Color.white;
        lineText.fontSize = 20;
        lineText.alignment = TextAlignmentOptions.Top;
        lineText.rectTransform.pivot = new Vector2(0.5f, 1f);
        lineText.rectTransform.anchorMin = new Vector2(0f, 0f);
        lineText.rectTransform.anchorMax = new Vector2(1f, 0f);
        textObject.transform.position = new Vector3(textObject.transform.position.x,textObject.transform.position.y - 3f);

        // Add EventTrigger for mouse down event
        EventTrigger eventTrigger = line.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((data) => { OnLineClick(line,textObject); });
        eventTrigger.triggers.Add(entry);
        verticalLines.Add(line);
        return line;
    }

    private Vector2 lineleftdown, linerightup;
    private void OnLineClick(GameObject line,GameObject text)
    {
        // Set the selected line for dragging
        selectedLine = line;
        selectedText = text;
        isDragging = true;
        lineleftdown = new Vector2(float.MinValue,float.MinValue);
        linerightup = new Vector2(float.MaxValue,float.MaxValue);
        // Inicializa la posición del ratón cuando comienza el arrastre
        lastMousePosition = Input.mousePosition;

        RectTransform lineRectTransform = selectedLine.GetComponent<RectTransform>();
        foreach (GameObject linee in verticalLines)
        {
            RectTransform otherLineRectTransform = linee.GetComponent<RectTransform>();
            float otherXPos = otherLineRectTransform.localPosition.x;

            if (otherXPos < lineRectTransform.localPosition.x && otherXPos > lineleftdown.x)
            {
                lineleftdown.x = otherXPos;
            }
            else if (otherXPos > lineRectTransform.localPosition.x && otherXPos < linerightup.x)
            {
                linerightup.x = otherXPos;
            }
        }
        lineleftdown.x += lineRectTransform.sizeDelta.x;
        linerightup.x -= lineRectTransform.sizeDelta.x;

        foreach (GameObject linee in horizontalLines)
        {
            RectTransform otherLineRectTransform = linee.GetComponent<RectTransform>();
            float otherYPos = otherLineRectTransform.localPosition.y;

            if (otherYPos < lineRectTransform.localPosition.y && otherYPos > lineleftdown.y)
            {
                lineleftdown.y = otherYPos;
            }
            else if (otherYPos > lineRectTransform.localPosition.y && otherYPos < linerightup.y)
            {
                linerightup.y = otherYPos;
            }
        }
        lineleftdown.y += lineRectTransform.sizeDelta.y;
        linerightup.y -= lineRectTransform.sizeDelta.y;

    }

    private Vector2 lastMousePosition;
    void Update()
    {
        // Handle line dragging
        if (isDragging && selectedLine != null)
        {
            RectTransform lineRectTransform = selectedLine.GetComponent<RectTransform>();
            TextMeshProUGUI textText = selectedText.GetComponent<TextMeshProUGUI>();
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 mouseDelta = currentMousePosition - lastMousePosition;
            lastMousePosition = currentMousePosition;
            
            // Constrain movement based on line type
            if (selectedLine.name.StartsWith("HorizontalLine"))
            {
                // Vertical movement only
                float oldYPos = lineRectTransform.localPosition.y;
                float newYPos = HorizontalPosition(lineRectTransform.localPosition.y + mouseDelta.y);
                lineRectTransform.localPosition = new Vector3(0f, newYPos, 0f);
                horizontalLinesPos[int.Parse(lineRectTransform.name.Substring(lineRectTransform.name.Length-1))] = lineRectTransform.localPosition.y;
                float percentage = ((Mathf.Round(newYPos)/centerY)+1f)/2;
                textText.text = MathF.Round(maxprecipitation*percentage) + "cm";
                
                ChangeBiomeCubeWidthY(lineRectTransform.name.Substring(lineRectTransform.name.Length-1),newYPos - oldYPos);
            }
            else if (selectedLine.name.StartsWith("VerticalLine"))
            {
                float oldXPos = lineRectTransform.localPosition.x;
                float newXPos = VerticalPosition(lineRectTransform.localPosition.x + mouseDelta.x);
                lineRectTransform.localPosition = new Vector3(newXPos, 0f, 0f);
                verticalLinesPos[int.Parse(lineRectTransform.name.Substring(lineRectTransform.name.Length-1))] = lineRectTransform.localPosition.x;
                float percentage = ((Mathf.Round(newXPos)/centerX)+1f)/2;
                textText.text = MathF.Round(maxtemperature*percentage + mintemperature*(1-percentage),1) + "°";
                ChangeBiomeCubeWidthX(lineRectTransform.name.Substring(lineRectTransform.name.Length-1),newXPos - oldXPos);
            }
        }

        // Release the line when the mouse button is up
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            selectedLine = null;
        }
    }

    private void ChangeBiomeCubeWidthX(String linenumber, float delta)
    {
        if(delta != 0){
            for (int i = 0; i < columns; i++)
            {
                RectTransform cubesize = grid[i,int.Parse(linenumber)].GetComponent<RectTransform>();
                cubesize.sizeDelta = new Vector2(cubesize.sizeDelta.x + delta, cubesize.sizeDelta.y);
            }
            for (int i = 0; i < columns; i++)
            {
                RectTransform cubesize = grid[i,int.Parse(linenumber)+1].GetComponent<RectTransform>();
                grid[i,int.Parse(linenumber)+1].transform.position = new Vector3(grid[i,int.Parse(linenumber)+1].transform.position.x + delta, grid[i,int.Parse(linenumber)+1].transform.position.y,grid[i,int.Parse(linenumber)+1].transform.position.z);
                cubesize.sizeDelta = new Vector2(cubesize.sizeDelta.x - delta, cubesize.sizeDelta.y);
            }
        }
    }

    private void ChangeBiomeCubeWidthY(String linenumber, float delta)
    {
        if(delta != 0){
            for (int i = 0; i < rows; i++)
            {
                RectTransform cubesize = grid[int.Parse(linenumber),i].GetComponent<RectTransform>();
                cubesize.sizeDelta = new Vector2(cubesize.sizeDelta.x, cubesize.sizeDelta.y - delta);
                grid[int.Parse(linenumber),i].transform.position = new Vector3(grid[int.Parse(linenumber),i].transform.position.x, grid[int.Parse(linenumber),i].transform.position.y + delta,grid[int.Parse(linenumber),i].transform.position.z);
            }
            for (int i = 0; i < rows; i++)
            {
                RectTransform cubesize = grid[int.Parse(linenumber)+1,i].GetComponent<RectTransform>();
                cubesize.sizeDelta = new Vector2(cubesize.sizeDelta.x,cubesize.sizeDelta.y + delta);
            }
        }
    }

    float VerticalPosition(float newXPos)
    {

        if (newXPos < minXPOS)
        {
            newXPos = minXPOS;
        } else if (newXPos > maxXPOS)
        {
            newXPos = maxXPOS;
        } else if (newXPos < lineleftdown.x)
        {
            newXPos = lineleftdown.x;
        } else if (newXPos > linerightup.x)
        {
            newXPos = linerightup.x;
        }

        return newXPos;
    }
    float HorizontalPosition(float newYPos)
    {
        if (newYPos < minYPOS)
        {
            newYPos = minYPOS;
        }
        else if (newYPos > maxYPOS)
        {
            newYPos = maxYPOS;
        } else if (newYPos < lineleftdown.y)
        {
            newYPos = lineleftdown.y;
        } else if (newYPos > linerightup.y)
        {
            newYPos = linerightup.y;
        }

        return newYPos;
    }
    private string[,] ReadCSVFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("CSV file not found: " + filePath);
            return null;
        }

        string[] lines = File.ReadAllLines(filePath);
        string[,] data = new string[lines.Length, columns];

        for (int i = 0; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');

            for (int j = 0; j < Mathf.Min(columns, values.Length); j++)
            {
                data[i, j] = values[j];
            }
        }

        return data;
    }

    public Tile getBiome(float temperature, float precipitation){
        float percentageh,percentagev,minrangev,maxrangeh = 1f;
        for (int i = rows-2; i >= -1 ; i--) {
            if(i >= 0) {
                percentageh = ((horizontalLinesPos[rows-2-i]/centerX)+1f)/2;
            } else {
                percentageh = 0;
            }
            if(percentageh <= temperature && temperature <= maxrangeh) {
                minrangev = 0f;
                for (int j = 0; j < columns; j++) {
                    if(j < verticalLinesPos.Count) {
                        percentagev = ((verticalLinesPos[j]/centerY)+1f)/2;
                    } else {
                        percentagev = 1;
                    }
                    if(minrangev <= precipitation && precipitation <= percentagev) {
                        return biomes[int.Parse(csvData[rows-2-i,j])];
                    }

                    minrangev = percentagev;
                }
            }

            maxrangeh = percentageh;
        }
    
        return biomes[6];
    }
}
