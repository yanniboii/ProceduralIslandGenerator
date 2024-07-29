using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;
/// <summary>
/// This is the main script of the whole generator. This needs to have access to all the components connected to the generation.
/// </summary>
public class MapManager : MonoBehaviour
{
    [SerializeField] public static int seed;
    [Range(1, 1000)]
    [SerializeField] int chunkSize;

    [SerializeField] float renderDistance;

    [SerializeField] Transform playerTransform;

    [SerializeField] SimpleVoronoi simpleVoronoi;

    [SerializeField] List<Biomes> biomes = new List<Biomes>();

    [SerializeField] List<Chunk> chunkList = new List<Chunk>();

    #region components

    PerlinComputeMaster perlinComputeMaster;
    PerlinWormMaster flowFieldRiverMaster;
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
    private bool stopUpdating = false;

    #endregion

    private void Awake()
    {
        Random.InitState(seed);
    }

    // Start is called before the first frame update
    void Start()
    {
        SetValues();

        onValuesChanged += () => Debug.Log("Values Changed");
        onValuesChanged += () => stopUpdating = true;
        onValuesChanged += () => biomes.Clear();
        onValuesChanged += () => stopUpdating = false;
        onValuesChanged += UpdateChunks;
        UpdateChunks();
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfValuesChanged();
        if(!stopUpdating) UpdateChunks();
        //playerTransform.Translate(new Vector3(0.05f,0,0));
    }

    void UpdateChunks()
    {
        if(stopUpdating) return;

        foreach (Chunk item in chunks.Values)
        {
            if (item.gameObject == null) continue;
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
                    Chunk chunk = new Chunk(meshGenerator, perlinComputeMaster, flowFieldRiverMaster, simpleVoronoi, currentChunk, chunkSize,
                                                   biomes[Random.Range(0, biomes.Count)], GetComponent<MeshRenderer>().sharedMaterial);
                    chunks.Add(currentChunk, chunk);
                    chunkList.Add(chunk);
                }
                else
                {
                    Chunk chunk;
                    chunks.TryGetValue(currentChunk,out chunk);
                    if (chunk.gameObject == null) continue;
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
        perlinComputeMaster = GetComponent<PerlinComputeMaster>();
        flowFieldRiverMaster = GetComponent<PerlinWormMaster>();
        meshGenerator = GetComponent<MeshGenerator>();

        chunksInRenderDistance = Mathf.RoundToInt(renderDistance/chunkSize);

    }

    public int GetChunkSize() { return chunkSize; }

}

[System.Serializable]
public struct Chunk
{
    public Vector2Int pos;
    public GameObject? gameObject;

    public Texture2D meshTexture;
    public Texture2D riverTex;
    public SimpleVoronoi simpleVoronoi;

    public bool initialized;

    MeshGenerator generator;
    PerlinComputeMaster perlinComputeMaster;
    PerlinWormMaster perlinWormMaster;

    float[,] perlinNoiseMap;
    int chunkSize;

    public Chunk(MeshGenerator generator, PerlinComputeMaster perlinComputeMaster, PerlinWormMaster perlinWormMaster
        , SimpleVoronoi simpleVoronoi,Vector2Int pos, int chunkSize, Biomes biome, Material baseMaterial)
    {
        perlinNoiseMap = null;
        gameObject = null;

        meshTexture = null;
        riverTex = null;

        this.simpleVoronoi = null;
        float islandChance = Random.Range(0f,3f);

        this.generator = generator;
        this.perlinComputeMaster = perlinComputeMaster;
        this.perlinWormMaster = perlinWormMaster;


        this.pos = pos;
        this.chunkSize = chunkSize;
        if (islandChance > 2.5)
        {
            SimpleVoronoi pasteVoronoi = ScriptableObject.CreateInstance<SimpleVoronoi>();

            pasteVoronoi.Initialize(chunkSize, simpleVoronoi);
            this.simpleVoronoi = pasteVoronoi;

            Texture2D texture2D = null;
            RenderTexture renderTexture = null;

            //perlinNoiseMap = perlinNoise.GeneratePerlinNoiseMap(this.pos,chunkSize);
            perlinNoiseMap = perlinComputeMaster.GetPerlinNoise(this.chunkSize, new Vector2(this.pos.x, this.pos.y), out texture2D, out renderTexture);
            
            this.perlinWormMaster.GetRiver(this.chunkSize, pasteVoronoi, renderTexture, out riverTex);
            
            gameObject = generator.GenerateMesh(this.pos, perlinNoiseMap, this.chunkSize, pasteVoronoi, biome.curve, biome.axiom, out meshTexture);
            gameObject.transform.Translate(new Vector3(0,-1.9f,0));
            
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            Material terrainMaterial;
            terrainMaterial = baseMaterial;
            meshRenderer.material = terrainMaterial;

            meshRenderer.material.SetTexture("_Perlin", texture2D);

            var sortedRegions = biome.regions.OrderBy(x => x.height).ToArray();

            meshRenderer.sharedMaterial.SetInt("regionsCount", sortedRegions.Length - 1);
            meshRenderer.sharedMaterial.SetColorArray("regions", sortedRegions.Select(x => x.color).ToArray());
            meshRenderer.sharedMaterial.SetFloatArray("regionHeights", sortedRegions.Select(x => x.height).ToArray());
        }
        initialized = true;
    }

}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
    public Texture2D texture;
}


[System.Serializable]
public struct Biomes
{
    public string name;
    public AnimationCurve curve;
    public TerrainType[] regions;
    public string axiom;
}