using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    [SerializeField] MapManager mapManager;

    [SerializeField] AnimationCurve curve;

    private int meshSize;

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

    public GameObject GenerateMesh(/*int meshWidth, int meshHeight*/Vector2Int offset, float[,] noiseMap, int chunkSize, SimpleVoronoi simpleVoronoi)
    {
        GameObject chunk = new GameObject();

        float effectiveCellSize = Mathf.Abs(simpleVoronoi.cellSize);

        offset *= chunkSize;
        chunk.name = ("Chunk" +offset);
        chunk.transform.position = new Vector3(offset.x,0,offset.y);

        MeshRenderer meshRenderer = chunk.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;
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
                float distancePercentage = ( meshSize / distance);

                if (normalizedDistance < simpleVoronoi.tolerance)
                {
                    vertices[vI] = (new Vector3(x - (meshSize / 2), curve.Evaluate(noiseMap[x, y]) * distancePercentage , y - (meshSize / 2)));
                }
                else
                {
                    vertices[vI] = (new Vector3(x - (meshSize / 2), 0, y - (meshSize / 2)));
                }
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
