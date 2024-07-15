using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MapManager : MonoBehaviour
{
    [SerializeField] public static int seed;
    [Range(1, 1000)]
    [SerializeField] int chunkSize;

    [SerializeField] float renderDistance;

    [SerializeField] Transform playerTransform;
    [SerializeField] List<Chunk> chunkList = new List<Chunk>();


    #region components

    PerlinNoise perlinNoise;
    PerlinComputeMaster perlinComputeMaster;
    MeshGenerator meshGenerator;

    #endregion

    #region events

    public delegate void OnValuesChanged();
    public OnValuesChanged onValuesChanged;

    #endregion

    #region private variables

    public Dictionary<Vector2Int,Chunk> chunks = new Dictionary<Vector2Int, Chunk>();
    private int chunksInRenderDistance;

    private int previousChunkSize;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        SetValues();

        onValuesChanged += () => Debug.Log("Values Changed");
        onValuesChanged += UpdateChunks;
        UpdateChunks();
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfValuesChanged();
        UpdateChunks();
        playerTransform.Translate(new Vector3(0.05f,0,0));
    }

    void UpdateChunks()
    {
        foreach (Chunk item in chunks.Values)
        {
            if (!item.gameObject.active) continue;
            item.gameObject.SetActive(false);
        }
        for (int y = -chunksInRenderDistance; y <= chunksInRenderDistance; y++)
        {
            for(int x = -chunksInRenderDistance; x <= chunksInRenderDistance; x++)
            {
                Vector2Int currentChunk = new Vector2Int((int)playerTransform.position.x+(x*chunkSize), (int)playerTransform.position.z + (y * chunkSize));
                currentChunk /= chunkSize;

                if (!chunks.ContainsKey(currentChunk))
                {
                    Debug.Log("B");
                    chunks.Add(currentChunk,new Chunk(meshGenerator,perlinNoise, perlinComputeMaster, currentChunk, chunkSize));
                    chunkList.Add(new Chunk(meshGenerator, perlinNoise, perlinComputeMaster, currentChunk, chunkSize));
                }
                else
                {
                    Chunk chunk;
                    chunks.TryGetValue(currentChunk,out chunk);
                    chunk.gameObject.SetActive(true);
                }
            }
        }
    }



    void CheckIfValuesChanged()
    {
        if (chunkSize != previousChunkSize)
        {
            onValuesChanged.Invoke();
            SetValues();
        }
    }

    void SetValues()
    {
        previousChunkSize = chunkSize;
        perlinNoise = GetComponent<PerlinNoise>();
        perlinComputeMaster = GetComponent<PerlinComputeMaster>();
        meshGenerator = GetComponent<MeshGenerator>();

        chunksInRenderDistance = Mathf.RoundToInt(renderDistance/chunkSize);

    }

    public int GetChunkSize() { return chunkSize; }

}

[System.Serializable]
public struct Chunk
{
    public Vector2Int pos;
    public GameObject gameObject;
    public Texture2D texture2D;

    public bool initialized;

    MeshGenerator generator;
    PerlinNoise perlinNoise;
    PerlinComputeMaster perlinComputeMaster;

    float[,] perlinNoiseMap;
    int chunkSize;

    public Chunk(MeshGenerator generator, PerlinNoise perlinNoise, PerlinComputeMaster perlinComputeMaster, Vector2Int pos, int chunkSize)
    {
        this.generator = generator;
        this.perlinNoise = perlinNoise;
        this.perlinComputeMaster = perlinComputeMaster;

        this.pos = pos;
        this.chunkSize = chunkSize;

        //perlinNoiseMap = perlinNoise.GeneratePerlinNoiseMap(this.pos,chunkSize);
        perlinNoiseMap = perlinComputeMaster.GetPerlinNoise(this.chunkSize, new Vector2(this.pos.x,this.pos.y), out texture2D);
        gameObject = generator.GenerateMesh(this.pos, perlinNoiseMap, this.chunkSize);

        initialized = true;
    }
}