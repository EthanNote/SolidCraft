using ObjLoader.Loader.Loaders;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class OBJ : MonoBehaviour
{

    LoadResult result;
    public string filename;
    public BlockDBManager manager;
    public Palette palette;
    // Use this for initialization
    void Start()
    {
        var objLoaderFactory = new ObjLoaderFactory();
        var objLoader = objLoaderFactory.Create();
        var fileStream = new FileStream(filename, FileMode.Open);
        result = objLoader.Load(fileStream);
        foreach (var v in result.Vertices)
        {
            manager.DBManager.Insert(new int[] { (int)(v.X * 64 * 0.1) + 640, (int)(v.Y * 64 * 0.1) + 64, (int)(v.Z * 64 * 0.1) + 640 }, 0, palette);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
