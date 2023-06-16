using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using System;

public class MyRoomScreenShot : MonoBehaviour
{
    public static string PATH_ROOT;
    public static string PATH_GALLERY;
    public static string PATH_GALLERY_IMAGE;

    public SpriteRenderer wallRenderer;
    public SpriteRenderer floorRenderer;

    public GameObject[] Tilemaps;
    private bool IsTileMapOn = true;

    // Use this for initialization
    void Start ()
    {
        PATH_ROOT = string.Concat(Application.dataPath, "/UserData/");
        PATH_GALLERY = string.Concat(PATH_ROOT, "Gallery/");
        PATH_GALLERY_IMAGE = string.Concat(PATH_GALLERY, "Image/");

        DirectoryCheck(PATH_ROOT);

        DirectoryCheck(PATH_GALLERY);
        DirectoryCheck(PATH_GALLERY_IMAGE);

        Load();
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.R))
        {
            Load();
        }
    }

    public void Load()
    {
        string WallImageName = "Wall.png";
        string FloorImageName = "Floor.png";

        Sprite wallSprite = ImportSprite(string.Concat(PATH_GALLERY_IMAGE, WallImageName));
        Sprite floorSprite = ImportSprite(string.Concat(PATH_GALLERY_IMAGE, FloorImageName));

        wallRenderer.sprite = Instantiate(wallSprite);
        floorRenderer.sprite = Instantiate(floorSprite);
    }

    public void OnOffTileMap()
    {
        IsTileMapOn = !IsTileMapOn;

        foreach (var tilemap in Tilemaps)
        {
            tilemap.SetActive(IsTileMapOn);
        }
    }

    private void DirectoryCheck(string path)
    {
        if (Directory.Exists(path)) return;
        Directory.CreateDirectory(path);
    }

    public bool ReadDirectory(string path, string extension, out string[] pathList)
    {
        pathList = null;
        DirectoryInfo dInfo = new DirectoryInfo(path);
        if (dInfo.Exists == false) return false;

        var paths = from selectFile in dInfo.GetFileSystemInfos()
                    where (selectFile.Extension == extension)
                    select selectFile.FullName;
        pathList = paths.ToArray();

        return true;
    }

    public Texture2D ImportImage(string fullPath)
    {
        byte[] bytes = ReadFile(fullPath);
        Texture2D temp = new Texture2D(2, 2);
        temp.LoadImage(bytes);
        temp.Apply();
        return temp;
    }

    public Sprite ImportSprite(string fullPath)
    {
        Texture2D texture = ImportImage(fullPath);
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Sprite reVal = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));//,100.0f,0,SpriteMeshType.FullRect);
        
        return reVal;
    }

    private byte[] ReadFile(string fullPath)
    {
        FileStream file = new FileStream(fullPath, FileMode.Open);
        byte[] reVal = new byte[file.Length];
        file.Read(reVal, 0, reVal.Length);
        file.Close();
        return reVal;
    }
}
