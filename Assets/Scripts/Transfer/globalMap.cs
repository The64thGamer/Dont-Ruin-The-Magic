using BayatGames.SaveGameFree;
using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class globalMap : MonoBehaviour
{
    [Header("Variables")]
    public mapFile mapF;
    public InputField inputF;
    public GameObject mapObject;
    public SpriteRenderer mapBackground;

    void Start()
    {
        mapF = this.GetComponent<mapFile>();
        NewMap("newMap", "Unknown");
    }

    public void UpdateName()
    {
        mapF.mapVars.mapName = inputF.text;
    }

    Sprite MapTexToSprite()
    {
        return Sprite.Create(mapF.mapVars.mapTex, new Rect(0.0f, 0.0f, mapF.mapVars.mapTex.width, mapF.mapVars.mapTex.height), new Vector2(0f, 0f), 1);
    }

    public void LoadImage()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Load Image", "", "png", false);
        if (paths.Length > 0)
        {
            byte[] data = File.ReadAllBytes(paths[0]);
            Texture2D temp = new Texture2D(0, 0);
            ImageConversion.LoadImage(temp, data);

            if (temp.width * temp.height == mapF.mapVars.mapTex.width * mapF.mapVars.mapTex.height)
            {
                Debug.Log("Applied New Texture (" + temp.width + ", " + temp.height + ")");
                mapF.mapVars.mapTex.SetPixels(temp.GetPixels());
                mapF.mapVars.mapTex.Apply();
                mapBackground.sprite = MapTexToSprite();
            }
            else
            {
                Debug.Log("New texture was not correct size.");
            }
        }
    }

    public void SaveImage()
    {
        byte[] data = ImageConversion.EncodeToPNG(mapF.mapVars.mapTex);
        var path = StandaloneFileBrowser.SaveFilePanel("Save Image", "", "MyMap", "png");
        Debug.Log("Image Saved: " + path);
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, data);
        }
    }

    public void ResizeMapX(int plusX)
    {
        if (plusX > 0)
        {

        }
        else
        {

        }
    }

    void CreateMap()
    {
        UnloadMap();

        //Create Object
        GameObject background = new GameObject();
        background.name = "background";
        background.transform.position = Vector3.zero;
        background.transform.parent = mapObject.transform;

        //Apply Sprite
        mapBackground = background.AddComponent<SpriteRenderer>();
        mapBackground.sprite = MapTexToSprite();
    }

    public void NewMap(string name, string author)
    {
        //Apply Default Values
        mapF.mapVars = new mapFile.mapFileV1();
        mapF.mapVars.mapName = name;
        mapF.mapVars.mapCreator = author;
        mapF.mapVars.tileSizeX = 64;
        mapF.mapVars.tileSizeY = 64;
        mapF.mapVars.mapTex = new Texture2D(mapF.mapVars.tileSizeX * 16, mapF.mapVars.tileSizeY * 16);
        mapF.mapVars.mapTex.Apply();
        mapF.mapVars.mapTex.filterMode = FilterMode.Point;
        inputF.text = name;

        CreateMap();
    }

    public void LoadMap()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Load Map", "", "plMap", false);
        if (paths.Length > 0)
        {
            UnloadMap();
            //Check Validity
            LoadAsync(paths[0]);
            if (mapF.mapVars != null)
            {
                CreateMap();
            }
        }
    }

    async void LoadAsync(string path)
    {
        mapF.mapVars = BayatGames.SaveGameFree.SaveGame.Load<mapFile.mapFileV1>(path);
    }

    public void SaveMap()
    {
        var path = StandaloneFileBrowser.SaveFilePanel("Save Map", "", mapF.mapVars.mapName, "plMap");
        Debug.Log("Image Saved: " + path);
        if (!string.IsNullOrEmpty(path))
        {
            BayatGames.SaveGameFree.SaveGame.Save(path, mapF.mapVars);
        }
    }

    public void UnloadMap()
    {
        //Destroy all map objects
        foreach (Transform child in mapObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

}
