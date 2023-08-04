using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapFile : MonoBehaviour
{
    public mapFileV1 mapVars;
    public class mapFileV1
    {
        public string mapName;
        public string mapCreator;
        public int tileSizeX, tileSizeY;
        public int[,] staticCollisions;
        public Texture2D mapTex;
        public List<Texture2D> layersText;
        public List<Vector2> layersPos;
        public List<Vector3> animatedTiles;
        public List<mapAnimTileV1> animTileCache;
    }
    public class mapAnimTileV1
    {
        public List<Texture2D> frames;
        public float animSpeed;
        public bool randomStart;
    }
    public class entityV1
    {
        public string name;
        public Vector2 position;
        public int version;

        public List<Vector2> posList;
        public List<int> intList;
        public List<float> floatList;
        public List<string> stringList;
        public List<bool> boolList;
        public byte[] byteArray;
    }
}
