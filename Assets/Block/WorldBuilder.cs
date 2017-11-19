using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBuilder : MonoBehaviour
{
    public BlockManager blockManager;
    public Palette palette;
    public int InitSize = 100;
    public int InitGroundLevel = 2;
    // Use this for initialization
    void Start()
    {
        blockManager.Initialize();
        float GroundBlockSize = Mathf.Pow(2, InitGroundLevel);
        if(!blockManager.LoadXML())
            for (int i = -InitSize; i < InitSize; i++)
            {
                for (int j = -InitSize; j < InitSize; j++)
                {
                    Block block;
                    blockManager.InsertBlock(new Vector3(i* GroundBlockSize, -GroundBlockSize, j * GroundBlockSize), InitGroundLevel, out block);
                    if ((i + j) % 2 == 0)
                    {
                        //block.Entity.GetComponent<MeshRenderer>().sharedMaterial = palette.Materials[2];
                        block.SetPaletteMaterial(palette, 2);
                    }
                }
            }
    }

    private void OnApplicationQuit()
    {
        blockManager.SaveXML();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
