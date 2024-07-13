using System.Collections;
using System.Collections.Generic;
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

    private RenderTexture renderTexture;
    private Camera _camera;

    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.current;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float[,] GetPerlinNoise(int chunkSize)
    {
        SetValues();
        InitRenderTexture(chunkSize);

        computeShader.SetTexture(0,"Result",renderTexture);
        int threadGroups = Mathf.CeilToInt(chunkSize/8);

        computeShader.Dispatch(0,threadGroups,threadGroups,1);

        renderTarget = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        CopyRenderTextureToTexture2D();

        float[,] floatArray = ConvertTexture2DToFloatArray(renderTarget);

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

    void SetValues()
    {
        computeShader.SetFloat("time",Time.time);
    }
}
