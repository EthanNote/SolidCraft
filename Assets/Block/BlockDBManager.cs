using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockDBManager : MonoBehaviour
{

    public HyperDB.DBManager DBManager;
    public GameObject BlockTemplate;
    // Use this for initialization
    void Start()
    {
        DBManager = new HyperDB.DBManager(31, 3, typeof(BlockNode));
        BlockNode.BlockTemplate = BlockTemplate;


        //DBManager.Insert(new int[] { 3, 0, 4}, 0);
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                DBManager.Insert(new int[] { i << 6, 0, j << 6 }, 6);
            }
        }
        print(DBManager.Dump(DBManager.Root));
    }


    // Update is called once per frame
    void Update()
    {

    }
}
