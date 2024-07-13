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

    private int size;
    
    private float previousNoiseScale;
    private int previousOctaves;
    private float previousLacunarity;
    private float previousPersistance;
    // Start is called before the first frame update
    void Start()
    {
        SetValues();
        mapManager.onValuesChanged += SetValues;
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
        size = mapManager.GetChunkSize();
    }

    public float[,] GeneratePerlinNoiseMap(Vector2Int offset, int chunkSize)
    {
        offset *= chunkSize;

        noiseMap = new float[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float frequency = 1;
                float amplitude = 1;
                float noise = 0;
                for (int k = 0; k < octaves; k++)
                {
                    float scaledX = j / noiseScale * frequency;
                    float scaledY = i / noiseScale * frequency;
                    scaledX += offset.x;
                    scaledY += offset.y;
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
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                noiseMap[j, i] = Mathf.InverseLerp(minHeight, maxHeight, noiseMap[j,i]);
            }
        }
        return noiseMap;
    }

    public void ApplyTexture()
    {
        noiseTexture = CopyNoiseMapOnTexture();
        GetComponent<MeshRenderer>().material.mainTexture = noiseTexture;
    }

    Texture2D CopyNoiseMapOnTexture()
    {
        Texture2D texture2D = new Texture2D(size,size);
        Color[] colorMap = new Color[size*size];
        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                float color = noiseMap[j, i];
                colorMap[i * size + j] = Color.Lerp(Color.white,Color.black,color);
            }
        }
        texture2D.SetPixels(colorMap);
        texture2D.Apply();
        return texture2D;
    }
}
