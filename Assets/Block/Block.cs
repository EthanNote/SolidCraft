using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct B3
{
    public bool b1;
    public bool b2;
    public bool b3;
    public B3(bool b1, bool b2, bool b3)
    {
        this.b1 = b1;
        this.b2 = b2;
        this.b3 = b3;
    }
    public static bool operator ==(B3 b31, B3 b32)
    {
        return (b31.b1 == b32.b1) && (b31.b2 == b32.b2) && (b31.b3 == b32.b3);
    }
    public static bool operator !=(B3 b31, B3 b32)
    {
        return (b31.b1 != b32.b1) || (b31.b2 != b32.b2) || (b31.b3 != b32.b3);
    }
}

public interface IBlock
{
    Vector3 Position { get; }
    int Level { get; set; }
    int MaterialID { get; set; }
}

public class BlockUtility
{
    public static float BlockSize(IBlock block)
    {
        return (float)Math.Pow(2, block.Level);
    }

    public static Dictionary<B3, int> indexMap = new Dictionary<B3, int>
        {
            {new B3(false, false, false), 0 },
            {new B3(false, false, true), 1 },
            {new B3(false, true, false), 2 },
            {new B3(false, true, true), 3 },
            {new B3(true, false, false), 4 },
            {new B3(true, false, true), 5 },
            {new B3(true, true, false), 6 },
            {new B3(true, true, true), 7 },
        };

    public static Vector3[] ChildPositions(IBlock block)
    {
        float radius = BlockSize(block) * 0.25f;
        return new Vector3[]
        {
                    block.Position+new Vector3(-radius, -radius,-radius),
                    block.Position+new Vector3(-radius, -radius,radius),
                    block.Position+new Vector3(-radius, radius,-radius),
                    block.Position+new Vector3(-radius, radius,radius),
                    block.Position+new Vector3(radius, -radius,-radius),
                    block.Position+new Vector3(radius, -radius,radius),
                    block.Position+new Vector3(radius, radius,-radius),
                    block.Position+new Vector3(radius, radius,radius),
        };
    }

    public static bool IsInBlock(IBlock block, Vector3 pos)
    {
        float radius = BlockSize(block) * 0.5f;
        Vector3 dir = pos - block.Position;
        if (dir.x > radius || dir.x < -radius)
            return false;
        if (dir.y > radius || dir.y < -radius)
            return false;
        if (dir.z > radius || dir.z < -radius)
            return false;

        return true;
    }

    public static int FindSubArea(IBlock block, Vector3 pos)
    {
        if (!BlockUtility.IsInBlock(block, pos))
            return -1;
        return BlockUtility.indexMap[new B3(pos.x > block.Position.x,
            pos.y > block.Position.y, pos.z > block.Position.z)];
    }
}

public class Block : MonoBehaviour, IBlock
{
    // Use this for initialization
    void Start()
    {

    }

    public void SetEntity()
    {
        Entity = null;
        foreach (Transform t in transform)
        {
            Entity = t.gameObject;
            break;
        }
    }


    // Update is called once per frame
    void Update()
    {

    }

    //void OnDestroy()
    //{
    //    for (int i = 0; i < 8; i++)
    //    {
    //        if (subBlocks[i] != null)
    //            GameObject.Destroy(subBlocks[i]);
    //    }
    //}

    public virtual Vector3 Position { get { return transform.position; } }

    public GameObject Entity { get; private set; }
    public bool IsEntity { get { return Entity.activeSelf; } }
    public int Level { get; set; }
    public Block[] subBlocks = new Block[8];

    public float BlockSize { get { return BlockUtility.BlockSize(this); } }

    public bool IsInBlock(Vector3 pos)
    {
        return BlockUtility.IsInBlock(this, pos);
    }

    public int FindSubArea(Vector3 pos)
    {
        return BlockUtility.FindSubArea(this, pos);
    }

    Vector3[] childPositions = null;
    public Vector3[] ChildPositions
    {
        get
        {
            if (childPositions == null)
            {
                childPositions = BlockUtility.ChildPositions(this);
            }
            return childPositions;
        }
    }

    public int MaterialID { get; set; }

    public void SetPaletteMaterial(Palette palette, int id)
    {
        Entity.GetComponent<MeshRenderer>().sharedMaterial = palette.Materials[id];
        MaterialID = id;
    }

    public void Delete()
    {
        for(int i = 0; i < 8; i++)
        {
            if (subBlocks[i] != null)
                subBlocks[i].Delete();
        }
        GameObject.Destroy(gameObject);
    }
}

public class SimpleBlock : IBlock
{
    Vector3 position;
    public Vector3 Position { get { return position; } }
    public int Level { get; set; }

    public SimpleBlock(Vector3 position, int level)
    {
        this.position = position;
        this.Level = level;
    }
    public int MaterialID { get; set; }
}