using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gen : MonoBehaviour
{
    public Terrain terrain;
    [Space]
    public int seed;
    [Space]
    public int Iters;
    public int lifetime;
    [Space]
    public float speed;
    public float lerpRadius;
    public float Add;
    public int checkRange;

    private int size;

    void Start()
    {
        size = terrain.terrainData.heightmapResolution;

        float[,] heightmap = GenPerlinHeightmap(size, size, seed);
        terrain.terrainData.SetHeights(0, 0, heightmap);

        for (int i = 0; i < Iters; i++)
        {
            System.Random random = new System.Random(seed);
            int x = random.Next(0, size);
            int y = random.Next(0, size);
            heightmap = Erose(heightmap, new Vector2Int(x, y));
            
            seed += random.Next(0, 1000);
        }
        terrain.terrainData.SetHeights(0, 0, heightmap);

        terrain.Flush();
    }

    private float[,] Erose (float[,] map, Vector2Int droplet_point)
    {
        float[,] newMap = map;
        Vector2Int prev = droplet_point;


        for (int i = 0; i < lifetime; i++)
        {
            //Get near heights
            float N = prev.y + checkRange < size ? map[prev.x, prev.y + checkRange] : 1f;
            float E = prev.x + checkRange < size ? map[prev.x + checkRange, prev.y] : 1f;
            float S = prev.y - checkRange >= 0 ? map[prev.x, prev.y - checkRange] : 1f;
            float W = prev.x - checkRange >= 0 ? map[prev.x - checkRange, prev.y] : 1f;

            //Exit if "Stuck"
            float curh = map[prev.x, prev.y];
            if (N >= curh && E >= curh && S >= curh && W >= curh)
            {
                Debug.Log($"Cur: {curh}, N: {N}, E: {E}, S: {S}, W: {W}");
                return newMap;
            }

            Vector2 normal = GetGrad(N, E, S, W);

            //Change terrain with lerping
            map = AddLerped(map, size, prev.x, prev.y, Add, lerpRadius);

            //New position calculation
            Vector2 velocity = -normal * speed;
            Vector2Int newPos = Vector2Int.RoundToInt(prev + velocity);

            //Exit if "Out of bounds"
            if (newPos.x < 0 || newPos.x >= size || newPos.y < 0 || newPos.y >= size)
                return newMap;


            prev = newPos;
        }

        return newMap;
    }

    private Vector2 GetGrad (float N, float E, float S, float W)
    {
        float gradx = E - W;
        float grady = N - S;
        return (new Vector2(gradx, grady)).normalized;
    }

    private float[,] AddLerped (float[,] map, int size, int x, int y, float add, float radius)
    {
        float[,] res = map;

        for (int _x = (int)(x - radius); _x <= x + radius; _x++)
            for (int _y = (int)(y - radius); _y <= y + radius; _y++)
            {
                if (_x < 0 || _x >= size || _y < 0 || _y >= size)
                    continue;

                int _width = _x - x;
                int _height = _y - y;

                float _rad = Mathf.Sqrt(_width * _width + _height * _height);
                if (_rad > radius)
                    continue;

                res[_x, _y] += add * (1f - _rad / radius);
            }

        return res;
    }

    private float[,] GenPerlinHeightmap (int width, int height, int seed)
    {
        float[,] res = new float[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float n1 = 1f / width;
                float n2 = 2f / width;
                float n3 = 4f / width;
                float n4 = 8f / width;

                float perlin1 = Mathf.PerlinNoise(seed + x * n1, seed + y * n1);
                float perlin2 = Mathf.PerlinNoise(2 * seed + x * n2, 2 * seed + y * n2);
                float perlin3 = Mathf.PerlinNoise(3 * seed + x * n3, 3 * seed + y * n3);
                float perlin4 = Mathf.PerlinNoise(4 * seed + x * n4, 4 * seed + y * n4);

                float num = (perlin1 + perlin2 + perlin3 + perlin4) / 4f;

                res[x, y] = num;
            }

        return res;
    }
}
