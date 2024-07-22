using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// this class generates controls the compute shader that generates the Perlin noise. you can change the octaves, lacunarity, noiseScale, and persistance
/// to get your desired results. It also generates a flowfield that you can visualize by calling the renderFlowField function.
/// </summary>
public class PerlinComputeMaster : MonoBehaviour
{
    [SerializeField] float noiseScale;
    [SerializeField] int octaves;
    [Tooltip("Keep this above 1.0")]
    [SerializeField] float lacunarity;
    [Tooltip("Keep this below 1.0")]
    [SerializeField] float persistance;
    [SerializeField] ComputeShader computeShader;
    [SerializeField] Texture2D renderTarget;
    [SerializeField] Material material;

    ComputeBuffer computeBuffer;
    ComputeBuffer octaveBuffer;

    private List<Octave> octaveList;

    private RenderTexture perlinTexture;
    private RenderTexture flowFieldVectors;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float[,] GetPerlinNoise(int chunkSize, Vector2 offset, out Texture2D texture2D, out RenderTexture renderTexture)
    {
        offset *= chunkSize;
        offset = new Vector2 (offset.x, -offset.y);
        SetValues(offset);
        InitRenderTexture(chunkSize);

        computeShader.SetTexture(0,"Result",perlinTexture);
        computeShader.SetTexture(0, "FlowField", flowFieldVectors);

        int threadGroups = Mathf.CeilToInt(chunkSize/8.0f);


        computeShader.Dispatch(0, threadGroups, threadGroups, 1);

        renderTarget = new Texture2D(perlinTexture.width, perlinTexture.height, TextureFormat.RGBAFloat, false);
        CopyRenderTextureToTexture2D();

        float[,] floatArray = ConvertTexture2DToFloatArray(renderTarget);

        octaveBuffer.Dispose();
        computeBuffer.Dispose();

        texture2D = renderTarget;
        renderTexture = flowFieldVectors;

        return floatArray;
    }

    float[,] ConvertTexture2DToFloatArray(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        int width = texture.width;
        int height = texture.height;

        float[,] floatArray = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                floatArray[x, y] = pixels[y * width + x].r;
            }
        }

        return floatArray;
    }
    void CopyRenderTextureToTexture2D()
    {
        RenderTexture.active = perlinTexture;
        renderTarget.ReadPixels(new Rect(0, 0, perlinTexture.width, perlinTexture.height), 0, 0);
        renderTarget.Apply();
    }

    void InitRenderTexture(int chunkSize)
    {
        if (perlinTexture == null)
        {
            if (perlinTexture != null)
            {
                perlinTexture.Release();
            }
            perlinTexture = new RenderTexture(chunkSize, chunkSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            perlinTexture.enableRandomWrite = true;
            perlinTexture.Create();
        }
        if (flowFieldVectors == null)
        {
            if (flowFieldVectors != null)
            {
                flowFieldVectors.Release();
            }
            flowFieldVectors = new RenderTexture(chunkSize, chunkSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            flowFieldVectors.enableRandomWrite = true;
            flowFieldVectors.Create();
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

    void RenderFlowField(int resolution, Texture2D flowfieldData)
    {
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                if(x % 10 == 0 && y % 10 == 0)
                {
                    Color color = flowfieldData.GetPixel(x, y);
                    Vector2 flowDirection = new Vector2(color.r, color.g);
                    Vector3 position = new Vector3(x / (float)resolution, 0, y / (float)resolution);
                    DrawArrow(position, flowDirection);
                }

            }
        }
    }

    void DrawArrow(Vector3 position, Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        arrow.transform.position = position;
        arrow.transform.rotation = Quaternion.Euler(90, 0, angle);
        arrow.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f * direction.magnitude);
        arrow.GetComponent<Renderer>().material = material;
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