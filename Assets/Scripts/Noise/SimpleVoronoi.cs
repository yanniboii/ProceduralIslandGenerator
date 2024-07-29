using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

/// <summary>
/// This scriptable object is used to generate the voronoi noise.
/// </summary>

[CreateAssetMenu(fileName = "VoronoiData", menuName = "VoronoiData/VoronoiData")]
public class SimpleVoronoi : ScriptableObject
{
    public int cellAmount;
    public float cellSize;
    public float minRiverOffset;
    public float maxRiverOffset;

    public List<Vector2> cells = new List<Vector2>();
    public List<Vector2> riverStartPoints = new List<Vector2>();

    public void Initialize(int size, SimpleVoronoi simpleVoronoi)
    {
        cellAmount = simpleVoronoi.cellAmount;
        cellSize = simpleVoronoi.cellSize;
        minRiverOffset = simpleVoronoi.minRiverOffset;
        maxRiverOffset = simpleVoronoi.maxRiverOffset;
        for (int i = 0; i < cellAmount; i++)
        {
            cells.Add(new Vector2(Random.Range(cellSize,size-cellSize),Random.Range(cellSize, size - cellSize)));
        }
        for(int i = 0; i < cells.Count-Random.Range(0,cellAmount/2); i++)
        {
            Vector2 riverPoint = new Vector2(cells[i].x + Random.Range(minRiverOffset, maxRiverOffset), cells[i].y);

            Vector3 originalPosition = new Vector3(riverPoint.x, 0, riverPoint.y);
            Vector3 pivotPoint = new Vector3(cells[i].x, 0, cells[i].y);

            float angle = Random.Range(0, 360);

            Vector3 direction = originalPosition - pivotPoint;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 rotatedPosition = pivotPoint + rotation * direction;

            riverPoint = new Vector2(rotatedPosition.x, rotatedPosition.z);

            riverStartPoints.Add(riverPoint);
        }
    }
}
