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
        float offset = (1 << level) / 128.0f;
        return new Vector3(keys[0] / 64.0f + offset, keys[1] / 64.0f + offset, keys[2] / 64.0f + offset);
    }

    public static int[] GetKeys(Vector3 position, int level)
    {
		return new int[] { 
			(((int)(position.x * 64.0))>>level)<<level, 
			(((int)(position.y * 64.0))>>level)<<level, 
			(((int)(position.z * 64.0))>>level)<<level
		};
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
        Material mat = palette.Materials[palette.SelectedID];
        obj.GetComponent<MeshRenderer>().sharedMaterial = mat;
        obj.transform.localScale = Vector3.one * ((1 << Level) / 64.0f);
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
