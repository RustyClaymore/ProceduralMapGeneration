using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainSurfaceSmoothing : MonoBehaviour {

    Terrain terrain;

    private int xResolution;
    private int zResolution;
    private float[,] heights;
    private float[,] heightMapBackup;
    protected int alphaMapWidth;
    protected int alphaMapHeight;
    protected int numOfAlphaLayers;
    private float[,,] alphaMapBackup;

    void Start()
    {
        terrain = this.GetComponent<Terrain>();

        xResolution = terrain.terrainData.heightmapWidth;
        zResolution = terrain.terrainData.heightmapHeight;
        alphaMapWidth = terrain.terrainData.alphamapWidth;
        alphaMapHeight = terrain.terrainData.alphamapHeight;
        numOfAlphaLayers = terrain.terrainData.alphamapLayers;

        if (Debug.isDebugBuild)
        {
            heights = terrain.terrainData.GetHeights(0, 0, xResolution, zResolution);
            heightMapBackup = terrain.terrainData.GetHeights(0, 0, xResolution, zResolution);
            alphaMapBackup = terrain.terrainData.GetAlphamaps(0, 0, alphaMapWidth, alphaMapHeight);
        }
    }

    void OnApplicationQuit()
    {
        if (Debug.isDebugBuild)
        {
            terrain.terrainData.SetHeights(0, 0, heightMapBackup);
            terrain.terrainData.SetAlphamaps(0, 0, alphaMapBackup);
        }
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                // area middle point x and z, area width, area height, smoothing distance, area height adjust
                SmoothTerrainSurfaceAtPoint(hit.point, 10, 10);
                // area middle point x and z, area size, texture ID from terrain textures
                //TextureDeformation(hit.point, 10 * 2f, DeformationTextureNum);
            }
        }
    }

    public void SmoothTerrainSurfaceAtPoint(Vector3 point, int lenX, int lenZ)//, int smooth, float incdec)
    {
        GetRelativeTerrainPositionFromPos(point, terrain, xResolution, zResolution);
        int areaX;
        int areaZ;

        int smooth = 1;
        float smoothingStep;

        int terX = (int)((point.x / terrain.terrainData.size.x) * xResolution);
        int terZ = (int)((point.z / terrain.terrainData.size.z) * zResolution);

        lenX += smooth;
        lenZ += smooth;

        terX -= (lenX / 2);
        terZ -= (lenZ / 2);

        if (terX < 0) terX = 0;
        if (terX > xResolution) terX = xResolution;
        if (terZ < 0) terZ = 0;
        if (terZ > zResolution) terZ = zResolution;

        float[,] heights = terrain.terrainData.GetHeights(terX, terZ, lenX, lenZ);

        float y = heights[lenX / 2, lenZ / 2];
        y += 0.0001f;

        for (smoothingStep = 0; smoothingStep < smooth + 1; smoothingStep++)
        {
            float multiplier = smoothingStep / smooth;
            for (areaX = (int)(smoothingStep / 2); areaX < lenX - (smoothingStep / 2); areaX++)
            {
                for (areaZ = (int)(smoothingStep / 2); areaZ < lenZ - (smoothingStep / 2); areaZ++)
                {
                    if ((areaX > -1) && (areaZ > -1) && (areaX < xResolution) && (areaZ < zResolution))
                    {
                        heights[areaX, areaZ] = Mathf.Clamp((float)y * multiplier, 0, 1);
                    }
                }
            }
        }
        terrain.terrainData.SetHeights(terX, terZ, heights);
    }

    protected Vector3 GetNormalizedPositionRelativeToTerrain(Vector3 pos, Terrain terrain)
    {
        print(terrain.gameObject.transform.position);
        Vector3 tempCoord = (pos - terrain.gameObject.transform.position);
        Vector3 coord;
        coord.x = tempCoord.x / terrain.terrainData.size.x;
        coord.y = tempCoord.y / terrain.terrainData.size.y;
        coord.z = tempCoord.z / terrain.terrainData.size.z;
        return coord;
    }

    protected Vector3 GetRelativeTerrainPositionFromPos(Vector3 pos, Terrain terrain, int mapWidth, int mapHeight)
    {
        Vector3 coord = GetNormalizedPositionRelativeToTerrain(pos, terrain);
        return new Vector3((coord.x * mapWidth), 0, (coord.z * mapHeight));
    }
}
