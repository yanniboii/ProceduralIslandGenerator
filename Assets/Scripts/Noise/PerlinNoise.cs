using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    [SerializeField] MapManager mapManager;
    [SerializeField] Texture2D noiseTexture;
    [SerializeField] float noiseScale;
    [SerializeField] int octaves;
    [SerializeField] float lacunarity;
    [SerializeField] float persistance;


    private float minHeight;
    private float maxHeight;


    float[,] noiseMap;

    private int width;
    private int height;

    private float previousNoiseScale;
    private int previousOctaves;
    private float previousLacunarity;
    private float previousPersistance;
    // Start is called before the first frame update
    void Start()
    {
        SetValues();
        GeneratePerlinNoiseMap();
        ApplyTexture();
        mapManager.onValuesChanged += SetValues;
        mapManager.onValuesChanged += GeneratePerlinNoiseMap;
        mapManager.onValuesChanged += ApplyTexture;
    }

    // Update is called once per frame
    void Update()
    {
        if(noiseScale != previousNoiseScale ||
           octaves != previousOctaves||
           lacunarity != previousLacunarity ||
           persistance != previousPersistance)
        {
            mapManager.onValuesChanged.Invoke();
        }
    }
    void SetValues()
    {
        minHeight = Mathf.Infinity;
        maxHeight = Mathf.NegativeInfinity;
        previousNoiseScale = noiseScale;
        previousOctaves = octaves;
        previousLacunarity = lacunarity;
        previousPersistance = persistance;
        width = mapManager.GetWidth();
        height = mapManager.GetHeight();
    }

    public void GeneratePerlinNoiseMap()
    {

        noiseMap = new float[width, height];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float frequency = 1;
                float amplitude = 1;
                float noise = 0;
                for (int k = 0; k < octaves; k++)
                {
                    float scaledX = j / noiseScale * frequency;
                    float scaledY = i / noiseScale * frequency;
                    float perlinNoise = Mathf.PerlinNoise(scaledX, scaledY);
                    noise += perlinNoise * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                if (noise > maxHeight)
                {
                    maxHeight = noise;
                }
                else if (noise < minHeight)
                {
                    minHeight = noise;
                }
                noiseMap[j, i] = noise;
            }
        }
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                noiseMap[j, i] = Mathf.InverseLerp(minHeight, maxHeight, noiseMap[j,i]);
            }
        }
        mapManager.SetPerlinNoieMap(noiseMap);
    }

    void ApplyTexture()
    {
        noiseTexture = CopyNoiseMapOnTexture();
        GetComponent<MeshRenderer>().material.mainTexture = noiseTexture;
    }

    Texture2D CopyNoiseMapOnTexture()
    {
        Texture2D texture2D = new Texture2D(width,height);
        Color[] colorMap = new Color[width*height];
        for(int i = 0; i < height; i++)
        {
            for(int j = 0; j < width; j++)
            {
                float color = noiseMap[j, i];
                colorMap[i * width + j] = Color.Lerp(Color.white,Color.black,color);
            }
        }
        texture2D.SetPixels(colorMap);
        texture2D.Apply();
        return texture2D;
    }
}
