using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class perlin_terrain : MonoBehaviour {

    public Terrain terrain;

    public int gridResolution = 512; // Will be adjusted to be divisible by the subdivision value. 
    public int gridSubdivision = 4;  // Value must be a power of 2.

    [Range(0, 1)]
    public float steepness = 0.12f;

    Grid grid;

    private static float[,] heights;

    // Use this for initialization
    void Start()
    {
        if (gridResolution < terrain.terrainData.heightmapResolution)
            throw new ArgumentException("Given resolution must be equal to or exceed terrain heightmap resolution.");

        grid = new Grid(gridResolution, gridSubdivision);
        grid.SteepnessCoefficient = steepness;

        heights = new float[terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution];

        determineTerrain();
    }

    // Update is called once per frame
    void Update()
    {
        // Do nothing.
    }

    void determineTerrain()
    {
        for (int y = 0; y < terrain.terrainData.heightmapResolution; y++)
            for (int x = 0; x < terrain.terrainData.heightmapResolution; x++)
            {
                heights[y, x] = grid.getAmplitude(x, y);
            }

        terrain.terrainData.SetHeights(0, 0, heights);
    }
}

public class Grid
{
    public float steepnessCoefficient = 0.12f; // The closer it is to one, the steeper the terrain. Has to be greater than or equal to zero.

    private static int resolution;
    private static int subdivision;
    private int subdivisionInterval;

    private Point[] points;

    public Grid(int _resolution, int _subdivision)
    {
        if (Mathf.Log(_subdivision, 2) % 1 != 0)
            throw new ArgumentException("Subdivision value MUST be a power of 2.");

        while (_resolution % _subdivision != 0)
        {
            _resolution++;
        }

        resolution = _resolution;
        subdivision = _subdivision;
        subdivisionInterval = resolution / subdivision;

        points = new Point[(int)Mathf.Pow(subdivision + 1, 2)];

        generatePoints();
    }

    private void generatePoints()
    {
        int n = 0;

        for (int i = 0; i <= subdivision; i++)
            for (int j = 0; j <= subdivision; j++)
            {
                points[n] = new Point(j * subdivisionInterval, i * subdivisionInterval);
                n++;
            }
    }

    public float getAmplitude(int x, int y)
    {
        float pointX = ((float)x / (float)subdivisionInterval) % 1f , pointY = ((float)y / (float)subdivisionInterval) % 1f;

        Vector2[] distanceVectors = { new Vector2(pointX, pointY), new Vector2(pointX - 1, pointY), new Vector2(pointX, pointY - 1), new Vector2(pointX - 1, pointY - 1) };

        float[] values = new float[4];
        float[] weighed = new float[2];
        float amplitude = 0;

        List<Point> unitPoints = new List<Point>();

        unitPoints = getUnitPoints(x, y);

        int n = 0;

        foreach (Point point in unitPoints)
        {
            switch (n)
            {
                case 0:
                    values[0] = Vector2.Dot(point.GradientVector, distanceVectors[0]);
                    break;
                case 1:
                    value[1] = Vector2.Dot(point.GradientVector, distanceVectors[1]);
                    break;
                case 2:
                    value[2] = Vector2.Dot(point.GradientVector, distanceVectors[2]);
                    break;
                case 3:
                    value[3] = Vector2.Dot(point.GradientVector, distanceVectors[3]);
                    break;
            }

            n++;
        }

        weighed[0] = Mathf.Lerp(values[0], values[1], fadeValue(pointX));
        weighed[1] = Mathf.Lerp(values[2], values[3], fadeValue(pointX));

        amplitude = Mathf.Lerp(weighed[0], weighed[1], fadeValue(pointY));

        return ((amplitude + 1) / 2) * steepnessCoefficient; // Normalize amplitude to range from 0 to 1. (and apply steepness coefficient)
    }

    private float fadeValue(float value)
    {
        return 6 * Mathf.Pow(value, 5) - 15 * Mathf.Pow(value, 4) + 10 * Mathf.Pow(value, 3);
    }

    private List<Point> getUnitPoints(int x, int y)
    {
        int unitNumberX = (int)(x / subdivisionInterval), unitNumberY = (int)(y / subdivisionInterval);
        List<Point> unitPoints = new List<Point>();

        foreach (Point point in points)
        {
            if (point.X == unitNumberX * subdivisionInterval && point.Y == unitNumberY * subdivisionInterval)
            {
                unitPoints.Add(point);
            }
            else if (point.X == unitNumberX * subdivisionInterval + subdivisionInterval && point.Y == unitNumberY * subdivisionInterval)
            {
                unitPoints.Add(point);
            }
            else if (point.X == unitNumberX * subdivisionInterval && point.Y == unitNumberY * subdivisionInterval + subdivisionInterval)
            {
                unitPoints.Add(point);
            }
            else if (point.X == unitNumberX * subdivisionInterval + subdivisionInterval && point.Y == unitNumberY * subdivisionInterval + subdivisionInterval)
            {
                unitPoints.Add(point);
            }
        }

        return unitPoints;
    }

    //------------------------------ Property declarations ------------------------------

    public static int Resolution
    {
        get { return resolution; }
    }

    public static int Subdivision
    {
        get { return subdivision; }
    }

    public float SteepnessCoefficient
    {
        get { return steepnessCoefficient; }
        set { steepnessCoefficient = value; }
    }
}

public struct Point
{
    private static Vector2[] vectors = { new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(-1, -1) };
    private Vector2 gradientVector;

    private int x, y;

    public Point(int _x, int _y)
    {
        x = _x;
        y = _y;

        gradientVector = vectors[UnityEngine.Random.Range(0, 4)];
    }

    //------------------------------ Property declarations ------------------------------

    public int X
    {
        get { return x; }
    }

    public int Y
    {
        get { return y; }
    }

    public Vector2 GradientVector
    {
        get { return gradientVector; }
    }
}
