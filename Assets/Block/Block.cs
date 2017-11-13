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

public class Block : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public int materialID = -1;
    public bool IsEntity { get { return materialID >= 0; } }
    public int _level;
    public int Level { get { return _level; } }
    public Block[] subBlocks = new Block[8];

    public float BlockSize { get { return (float)Math.Pow(2, Level); } }

    public bool IsInBlock(Vector3 pos)
    {
        float radius = BlockSize * 0.5f;
        Vector3 dir = pos - transform.position;
        if (dir.x > radius || dir.x < -radius)
            return false;
        if (dir.y > radius || dir.y < -radius)
            return false;
        if (dir.z > radius || dir.z < -radius)
            return false;

        return true;
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

    public int FindSubArea(Vector3 pos)
    {
        if (!IsInBlock(pos))
            return -1;
        return indexMap[new B3(pos.x > transform.position.x,
            pos.y > transform.position.y, pos.z > transform.position.z)];
    }

    Vector3[] childPositions = null;
    public Vector3[] ChildPosisitions
    {
        get
        {
            if (childPositions == null)
            {
                float radius = BlockSize * 0.25f;
                childPositions = new Vector3[]
                {
                    transform.position+new Vector3(-radius, -radius,-radius),
                    transform.position+new Vector3(-radius, -radius,radius),
                    transform.position+new Vector3(-radius, radius,-radius),
                    transform.position+new Vector3(-radius, radius,radius),
                    transform.position+new Vector3(radius, -radius,-radius),
                    transform.position+new Vector3(radius, -radius,radius),
                    transform.position+new Vector3(radius, radius,-radius),
                    transform.position+new Vector3(radius, radius,radius),
                };
            }
            return childPositions;
        }
    }

    public void OnCreate()
    {

    }

    public void OnDestroy()
    {

    }
}
