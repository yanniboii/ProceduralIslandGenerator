using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class PerlinComputeMaster : MonoBehaviour
{
    [SerializeField] float noiseScale;
    [SerializeField] int octaves;
    [SerializeField] float lacunarity;
    [SerializeField] float persistance;
    [SerializeField] ComputeShader computeShader;
    [SerializeField] Texture2D renderTarget;

    ComputeBuffer computeBuffer;
    ComputeBuffer octaveBuffer;

    private List<Octave> octaveList;

    private RenderTexture renderTexture;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float[,] GetPerlinNoise(int chunkSize, Vector2 offset)
    {
        offset *= chunkSize;
        offset = new Vector2 (offset.x, -offset.y);
        SetValues(offset);
        InitRenderTexture(chunkSize);

        computeShader.SetTexture(0,"Result",renderTexture);
        int threadGroups = Mathf.CeilToInt(chunkSize/8.0f);


        computeShader.Dispatch(0, threadGroups, threadGroups, 1);

        renderTarget = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBAFloat, false);
        CopyRenderTextureToTexture2D();

        float[,] floatArray = ConvertTexture2DToFloatArray(renderTarget);

        //for (int i = 0; i < chunkSize; i++)
        //{
        //    for (int j = 0; j < chunkSize; j++)
        //    {
        //        float noise = floatArray[j, i];
        //        if (noise > maxHeight)
        //        {
        //            maxHeight = noise;
        //        }
        //        else if (noise < minHeight)
        //        {
        //            minHeight = noise;
        //        }
        //    }
        //}
        //for (int i = 0; i < chunkSize; i++)
        //{
        //    for (int j = 0; j < chunkSize; j++)
        //    {
        //        floatArray[j, i] = Mathf.InverseLerp(minHeight, maxHeight, floatArray[j, i]);
        //        //Debug.Log(floatArray[j, i]);
        //    }
        //}
        //Debug.Log("MinHeight " + minHeight + " MaxHeight " + maxHeight);

        octaveBuffer.Dispose();
        computeBuffer.Dispose();

        return floatArray;
    }

    float[,] ConvertTexture2DToFloatArray(Texture2D texture)
    {
        // Get the pixel colors from the texture
        Color[] pixels = texture.GetPixels();
        int width = texture.width;
        int height = texture.height;

        // Initialize the float array
        float[,] floatArray = new float[width, height];

        // Convert the pixel colors to floats
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Assuming the texture is black and white, use the red channel for the value
                floatArray[x, y] = pixels[y * width + x].r;
            }
        }

        return floatArray;
    }
    void CopyRenderTextureToTexture2D()
    {
        RenderTexture.active = renderTexture;
        // Read the pixels from the active RenderTexture and store them in the Texture2D
        renderTarget.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        renderTarget.Apply();
    }

    void InitRenderTexture(int chunkSize)
    {
        if (renderTexture == null)
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
            }
            renderTexture = new RenderTexture(chunkSize, chunkSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }
    }

    void SetValues(Vector2 offset)
    {
        computeBuffer = new ComputeBuffer(1,sizeof(float));
        float[] minMaxFloats = new float[1];
        minMaxFloats[0] = 0;
        computeBuffer.SetData(minMaxFloats);
        computeShader.SetBuffer(0,"minMaxBuffer", computeBuffer);

        octaveList = new List<Octave>();

        float frequency = 1;
        float amplitude = 1;
        for (int k = 0; k < octaves; k++)
        {
            octaveList.Add(new Octave(frequency,amplitude));
            amplitude *= persistance;
            frequency *= lacunarity;
        }
        octaveBuffer = new ComputeBuffer(octaveList.Count,sizeof(float)*2);
        octaveBuffer.SetData(octaveList);
        computeShader.SetBuffer(0, "octaveBuffer",octaveBuffer);
        computeShader.SetInt("octavesCount",octaveList.Count);

        computeShader.SetFloat("seed",MapManager.seed);
        computeShader.SetVector("offset",offset);
        computeShader.SetFloat("noiseScale",noiseScale);
    }
}

public struct Octave{
    float frequency;
    float amplitude;
    public Octave(float freqeuncy, float amplitude)
    {
        this.frequency = freqeuncy;
        this.amplitude = amplitude;
    }
};