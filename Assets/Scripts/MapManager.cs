using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Range(1, 1000)]
    [SerializeField] int width;
    [Range(1, 1000)]
    [SerializeField] int height;

    #region components

    PerlinNoise perlinNoise;
    MeshGenerator meshGenerator;

    #endregion

    #region events

    public delegate void OnValuesChanged();
    public OnValuesChanged onValuesChanged;

    #endregion

    #region private variables

    private float[,] perlinNoiseMap;

    private int previousWidth;
    private int previousHeight;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        onValuesChanged += () => Debug.Log("Values Changed");
        SetValues();
        perlinNoise.GeneratePerlinNoiseMap();
        meshGenerator.GenerateMesh();
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfValuesChanged();
    }

    void CheckIfValuesChanged()
    {
        if (width != previousWidth ||
            height != previousHeight)
        {
            onValuesChanged.Invoke();
            SetValues();
        }
    }

    void SetValues()
    {
        previousHeight = height;
        previousWidth = width;
        perlinNoise = GetComponent<PerlinNoise>();
        meshGenerator = GetComponent<MeshGenerator>();
    }

    public int GetWidth() { return width; }
    public int GetHeight() { return height; }
    public void SetPerlinNoieMap(float[,] perlinNoiseMap) { this.perlinNoiseMap = perlinNoiseMap; }
    public float[,] GetPerlinNoiseMap() {  return perlinNoiseMap; }

}
