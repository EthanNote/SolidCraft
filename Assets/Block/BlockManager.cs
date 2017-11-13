using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
        root = new Block();
        root._level = 10;
        root.transform.postion = new Vector3(-0.5f, -0.5f, -0.5f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    Block root;

    //public BlockManager()
    //{
    //}


    public InsertionResult InsertBlock(Vector3 point, int level)
    {
        return InsertBlock(point, level, root);
    }

    public enum InsertionResult { OK, FAILED_INSIDEENTITY, FAILED_BLOCKEXIST }

    InsertionResult InsertBlock(Vector3 point, int level, Block parent)
    {
        if (parent.IsEntity)
            return InsertionResult.FAILED_INSIDEENTITY;

        if (level == parent.Level - 1)
        {
            int id = parent.FindSubArea(point);
            if (parent.subBlocks[id] != null)
                return InsertionResult.FAILED_BLOCKEXIST;

            parent.subBlocks[id] = new Block();
            parent.subBlocks[id]._level = parent.Level - 1;
            parent.subBlocks[id].materialID = 0;
            parent.subBlocks[id].transform.postion = parent.ChildPosisitions[id];
            return InsertionResult.OK;
        }
        else
        {
            int id = parent.FindSubArea(point);
            if (parent.subBlocks[id] == null)
            {
                parent.subBlocks[id] = new Block();
                parent.subBlocks[id]._level = parent.Level - 1;
                parent.subBlocks[id].transform.postion = parent.ChildPosisitions[id];
            }
            return InsertBlock(point, level, parent.subBlocks[id]);
        }
    }

    public enum DropResult { OK, FAILED_NOTEXIST }

    public DropResult DropBlock(Vector3 point, int level)
    {
        return DropBlock(point, level, root);
    }

    DropResult DropBlock(Vector3 point, int level, Block parent)
    {
        if (level == parent.Level - 1)
        {
            int id = parent.FindSubArea(point);
            if (parent.subBlocks[id] != null)
            {
                parent.subBlocks[id].OnDestroy();
                parent.subBlocks[id] = null;
                return DropResult.OK;
            }
            return DropResult.FAILED_NOTEXIST;
        }
        else
        {
            int id = parent.FindSubArea(point);
            if (parent.subBlocks[id] == null)
            {
                return DropResult.FAILED_NOTEXIST;
            }
            return DropBlock(point, level, parent.subBlocks[id]);
        }
    }
    public string Dump()
    {
        return Dump(root, 0);
    }
    public string Dump(Block parent, int indent)
    {
        Vector3 p = parent.transform.postion;
        var str = string.Format("({0} {1} {2})", p.x, p.y, p.z);
        var result = "";
        for (int i = 0; i < indent; i++)
            result += " ";
        result += str;
        if (parent.IsEntity)
            result += string.Format("  MAT-{0}", parent.materialID);
        result += "\n";
        for (int i = 0; i < 8; i++)
            if (parent.subBlocks[i] != null)
                result += Dump(parent.subBlocks[i], indent + 1);
        if (result[result.Length - 1] != '\n')
            result += "\n";
        return result;
    }
}
}
