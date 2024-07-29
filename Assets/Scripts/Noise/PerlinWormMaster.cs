using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// This class is used to generate the river texture using the flowField texture that i generate in the perlinComputeMaster. Currently this
/// doesn't work and only gives a weird result.
/// </summary>
public class PerlinWormMaster : MonoBehaviour
{
    [SerializeField] ComputeShader computeShader;
    [SerializeField] Texture2D renderTarget;

    [SerializeField] RenderTexture river;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float[,] GetRiver(int chunkSize, SimpleVoronoi voronoi, RenderTexture perlin, out Texture2D texture2D)
    {
        SetValues(chunkSize, voronoi);
        InitRenderTexture(chunkSize);

        computeShader.SetTexture(0, "river", river);
        computeShader.SetTexture(0, "perlin", perlin);

        int threadGroups = Mathf.CeilToInt(chunkSize / 8.0f);


        computeShader.Dispatch(0, threadGroups, threadGroups, 1);

        renderTarget = new Texture2D(river.width, river.height, TextureFormat.RGBAFloat, false);
        CopyRenderTextureToTexture2D();

        float[,] floatArray = ConvertTexture2DToFloatArray(renderTarget);
        
        texture2D = renderTarget;

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
        RenderTexture.active = river;
        // Read the pixels from the active RenderTexture and store them in the Texture2D
        renderTarget.ReadPixels(new Rect(0, 0, river.width, river.height), 0, 0);
        renderTarget.Apply();
    }

    void InitRenderTexture(int chunkSize)
    {
        if (river == null)
        {
            if (river != null)
            {
                river.Release();
            }
            river = new RenderTexture(chunkSize, chunkSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            river.enableRandomWrite = true;
            river.Create();
        }
    }

    void SetValues(int chunkSize, SimpleVoronoi voronoi)
    {
        Vector4[] startPoints = new Vector4[voronoi.riverStartPoints.Count];
        for (int i = 0; i < startPoints.Length; i++)
        {
            startPoints[i] = new Vector4(voronoi.riverStartPoints[i].x, voronoi.riverStartPoints[i].y, 0, 0);
        }
        Vector4[] endPoints = new Vector4[voronoi.riverStartPoints.Count];
        for (int i = 0; i < startPoints.Length; i++)
        {
            float side = Random.Range(0f, 1f);
            if(side < 0.25f)
            {
                endPoints[i] = new Vector4(Random.Range(0,chunkSize), chunkSize, 0, 0);
            }
            else if(side < 0.5f)
            {
                endPoints[i] = new Vector4(0, Random.Range(0, chunkSize), 0, 0);
            }
            else if (side < 0.75f)
            {
                endPoints[i] = new Vector4(chunkSize, Random.Range(0, chunkSize), 0, 0);
            }
            else
            {
                endPoints[i] = new Vector4(Random.Range(0, chunkSize), 0, 0, 0);
            }
        }
        computeShader.SetFloat("cellSize", Mathf.Abs(voronoi.cellSize));

        computeShader.SetVectorArray("startPoints", startPoints);
        computeShader.SetVectorArray("endPoints", endPoints);
        computeShader.SetInt("pointCount", voronoi.riverStartPoints.Count);
    }

}