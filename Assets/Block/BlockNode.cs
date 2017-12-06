using System.Collections;
using System.Collections.Generic;
using HyperDB;
using UnityEngine;
/// <summary>
/// 
/// [DBNode]
///     ↑
/// [BlockNode] <>---- [BlockObject]
/// 
/// </summary>
public class BlockNode : HyperDB.DBNode
{
    /// <summary>
    /// 
    /// </summary>
    public static GameObject BlockTemplate;

    public static Vector3 GetPosition(int[] keys, int level)
    {
        return new Vector3(keys[0] / 64.0f, keys[1] / 64.0f, keys[2] / 64.0f);
    }

    public static int[] GetKeys(Vector3 position, int level)
    {
        return new int[] { (int)(position.x * 64.0), (int)(position.y * 64.0), (int)(position.z * 64.0) };
    }
    /// <summary>
    /// 
    /// </summary>
    public BlockObject block;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="level"></param>
    public BlockNode(DBManager manager, int level = -1) : base(manager, level)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userData"></param>
    public override void OnInsert(object userData)
    {
        var obj = GameObject.Instantiate(BlockTemplate);
        block = obj.GetComponent<BlockObject>();
        block.transform.position = GetPosition(Keys, Level);
        if (block != null)
            block.node = this;

        Palette palette = userData as Palette;
        Material mat= palette.Materials[palette.SelectedID];
        obj.GetComponent<MeshRenderer>().sharedMaterial = mat;

    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnDelete()
    {
        block.node = null;
        GameObject.Destroy(block.gameObject);
        block = null;
    }
}
