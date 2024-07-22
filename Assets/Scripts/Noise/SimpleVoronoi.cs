using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This scriptable object is used to generate the voronoi noise.
/// </summary>

[CreateAssetMenu(fileName = "VoronoiData", menuName = "VoronoiData/VoronoiData")]
public class SimpleVoronoi : ScriptableObject
{
    public int cellAmount;
    public float cellSize;
    public float tolerance;

    public List<Vector2> cells = new List<Vector2>();

    public void Initialize(int size, SimpleVoronoi simpleVoronoi)
    {
        cellAmount = simpleVoronoi.cellAmount;
        cellSize = simpleVoronoi.cellSize;
        tolerance = simpleVoronoi.tolerance;
        for (int i = 0; i < cellAmount; i++)
        {
            cells.Add(new Vector2(Random.Range(cellSize,size-cellSize),Random.Range(cellSize, size - cellSize)));
        }
    }
}
