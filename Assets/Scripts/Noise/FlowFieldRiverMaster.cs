using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// This class is used to generate the river texture using the flowField texture that i generate in the perlinComputeMaster. Currently this
/// doesn't work and only gives a weird result.
/// </summary>
public class FlowFieldRiverMaster : MonoBehaviour
{
    [SerializeField] ComputeShader computeShader;
    [SerializeField] Texture2D renderTarget;
    [SerializeField] RenderTexture flowField;
    [SerializeField] int maxSteps;
    [SerializeField] int particleAmount;

    ComputeBuffer computeBuffer;
    ComputeBuffer particleBuffer;

    [SerializeField] RenderTexture river;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float[,] GetRiver(int chunkSize, RenderTexture flowFieldVectors, out float[,] texture2D)
    {
        SetValues(chunkSize);
        InitRenderTexture(chunkSize);

        computeShader.SetTexture(0, "river", river);
        computeShader.SetTexture(0, "FlowFieldVectors", flowFieldVectors);
        flowField = flowFieldVectors;

        int threadGroups = Mathf.CeilToInt(chunkSize / 8.0f);


        computeShader.Dispatch(0, threadGroups, threadGroups, 1);

        renderTarget = new Texture2D(river.width, river.height, TextureFormat.RGBAFloat, false);
        CopyRenderTextureToTexture2D();

        float[,] floatArray = ConvertTexture2DToFloatArray(renderTarget);
        
        computeBuffer.Dispose();
        particleBuffer.Dispose();

        texture2D = floatArray;

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

    void SetValues(int chunkSize)
    {
        computeBuffer = new ComputeBuffer(maxSteps, sizeof(float)*2);

        computeShader.SetBuffer(0, "riverPath", computeBuffer);

        particleBuffer = new ComputeBuffer (particleAmount, sizeof(float)*6);

        Particle[] particles = new Particle[particleAmount];
        for (int i = 0; i < particleAmount; i++)
        {
            particles[i].pos = new Vector2(Random.Range(0, chunkSize),Random.Range(0, chunkSize));
            particles[i].vel = new Vector2(Random.Range(0, 4), Random.Range(0, 4));
            particles[i].acc = Vector2.zero;
        }

        particleBuffer.SetData(particles);

        computeShader.SetBuffer(0, "particles", particleBuffer);
        computeShader.SetInt("particlesCount", particles.Length);

        computeShader.SetInt("maxSteps", maxSteps);

    }

}

public struct Particle
{
    public Vector2 pos;
    public Vector2 vel;
    public Vector2 acc;
}