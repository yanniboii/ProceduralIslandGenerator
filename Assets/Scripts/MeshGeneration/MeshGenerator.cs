using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
/// <summary>
/// The meshGenerator generates a mesh based on the perlin noise, voronoi noise, and animation curve. It also places the trees down.
/// </summary>
public class MeshGenerator : MonoBehaviour
{
    [SerializeField] MapManager mapManager;

    private LSystems lSystems;
    private int meshSize;

    private void Awake()
    {
        lSystems = GetComponent<LSystems>();

    }

    // Start is called before the first frame update
    void Start()
    {
        SetValues();
        mapManager.onValuesChanged += SetValues;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetValues()
    {
        meshSize = mapManager.GetChunkSize();
    }

    public GameObject GenerateMesh(/*int meshWidth, int meshHeight*/Vector2Int offset, float[,] noiseMap, int chunkSize, SimpleVoronoi simpleVoronoi, AnimationCurve curve, string axiom)
    {
        GameObject chunk = new GameObject();

        float effectiveCellSize = Mathf.Abs(simpleVoronoi.cellSize);

        offset *= chunkSize;
        chunk.name = ("Chunk" +offset);
        chunk.transform.position = new Vector3(offset.x,0,offset.y);

        MeshRenderer meshRenderer = chunk.AddComponent<MeshRenderer>();
        //Material material = GetComponent<MeshRenderer>().material;
        //meshRenderer.sharedMaterial = material;
        MeshFilter meshFilter = chunk.AddComponent<MeshFilter>();

        int vI = 0;
        int tI = 0;

        var mesh = new Mesh();

        var vertices = new Vector3[meshSize*meshSize];
        var triangles = new int[(meshSize-1)*(meshSize-1)*6];
        var uvs = new Vector2[meshSize*meshSize];

        for(int y = 0; y < meshSize; y++)
        {
            for(int x = 0; x < meshSize; x++)
            {
                float distance = float.MaxValue;

                for (int i = 0; i < simpleVoronoi.cellAmount; i++)
                {
                    float pointDistance = Vector2.Distance(new Vector2(x, y), simpleVoronoi.cells[i]);
                    distance = Mathf.Min(distance, pointDistance);
                }

                float normalizedDistance = distance / effectiveCellSize;
                float distancePercentage = Mathf.Clamp01(meshSize / distance);
                float treeDistance = Mathf.Clamp01(distance/simpleVoronoi.cellSize);
                float noiseValue = curve.Evaluate(noiseMap[x, y]);
                float heightA = noiseValue * Mathf.Pow(1-distancePercentage, 2);
                float heightB = noiseValue * 60 * Mathf.Pow( distancePercentage, 2);

                float blendFactor = Mathf.SmoothStep(0, 1, normalizedDistance);
                float finalHeight = Mathf.Lerp(heightB, heightA, blendFactor);

                if(treeDistance < 0.004f && finalHeight <= 7)
                {
                    Debug.Log("hey");
                    lSystems.axiom = axiom;
                    lSystems.spawntrees(new Vector3(x - (meshSize / 2), finalHeight, y - (meshSize / 2)), chunk.transform);
                }

                vertices[vI] = new Vector3(x - (meshSize / 2), finalHeight, y - (meshSize / 2));
                uvs[vI] = (new Vector2((float)x/meshSize, (float)y /meshSize));
                if(x < meshSize - 1 && y < meshSize - 1)
                {
                    triangles[tI] = (vI);
                    triangles[tI+1] = (vI + meshSize);
                    triangles[tI+2] = (vI + meshSize + 1);

                    triangles[tI + 3] = (vI + meshSize + 1);
                    triangles[tI+4] = (vI + 1);
                    triangles[tI + 5] = (vI);
                    tI += 6;
                }
                vI++;
            }
        }



        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        return chunk;
    }

    public GameObject GenerateMesh(/*int meshWidth, int meshHeight*/Vector2Int offset, int chunkSize)
    {
        GameObject chunk = new GameObject();

        offset *= chunkSize;
        chunk.name = ("Chunk" + offset);
        chunk.transform.position = new Vector3(offset.x, 0, offset.y);

        MeshRenderer meshRenderer = chunk.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;
        MeshFilter meshFilter = chunk.AddComponent<MeshFilter>();

        int vI = 0;
        int tI = 0;

        var mesh = new Mesh();

        var vertices = new Vector3[meshSize * meshSize];
        var triangles = new int[(meshSize - 1) * (meshSize - 1) * 6];
        var uvs = new Vector2[meshSize * meshSize];

        for (int y = 0; y < meshSize; y++)
        {
            for (int x = 0; x < meshSize; x++)
            {
                vertices[vI] = (new Vector3(x - (meshSize / 2), 0, y - (meshSize / 2)));
                
                uvs[vI] = (new Vector2((float)x / meshSize, (float)y / meshSize));
                if (x < meshSize - 1 && y < meshSize - 1)
                {
                    triangles[tI] = (vI);
                    triangles[tI + 1] = (vI + meshSize);
                    triangles[tI + 2] = (vI + meshSize + 1);

                    triangles[tI + 3] = (vI + meshSize + 1);
                    triangles[tI + 4] = (vI + 1);
                    triangles[tI + 5] = (vI);
                    tI += 6;
                }
                vI++;
            }
        }



        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        return chunk;
    }
}
