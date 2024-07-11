using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    [SerializeField] MapManager mapManager;

    private int meshWidth;
    private int meshHeight;

    // Start is called before the first frame update
    void Start()
    {
        SetValues();
        mapManager.onValuesChanged += SetValues;
        mapManager.onValuesChanged += GenerateMesh;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetValues()
    {
        meshWidth = mapManager.GetWidth();
        meshHeight = mapManager.GetHeight();
    }

    public void GenerateMesh()
    {
        float[,] noiseMap = mapManager.GetPerlinNoiseMap();
        int vI = 0;
        int tI = 0;
        var mesh = new Mesh();
        var vertices = new Vector3[meshWidth*meshHeight];
        var triangles = new int[(meshWidth-1)*(meshHeight-1)*6];
        var uvs = new Vector2[meshWidth*meshHeight];
        for(int y = 0; y < meshHeight; y++)
        {
            for(int x = 0; x < meshWidth; x++)
            {
                vertices[vI] = (new Vector3(y, noiseMap[x,y]*20,x));
                uvs[vI] = (new Vector2((float)x/meshWidth, (float)y /meshHeight));
                if(x < meshWidth - 1 && y < meshHeight - 1)
                {
                    triangles[tI] = (vI);
                    triangles[tI+1] = (vI + meshWidth + 1);
                    triangles[tI+2] = (vI + meshWidth);

                    triangles[tI + 3] = (vI + meshWidth + 1);
                    triangles[tI+4] = (vI);
                    triangles[tI + 5] = (vI + 1);
                    tI += 6;
                }
                vI++;
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
