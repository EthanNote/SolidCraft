using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;


public class BlockManager : MonoBehaviour
{

    public Block BlockTemplate;
    public Palette palette;
    public Block CreateBlock(int level, bool isSolid)
    {
        Block block = GameObject.Instantiate(BlockTemplate).GetComponent<Block>();
        if (isSolid)
            block.name = "Solid Block";
        else
            block.name = "Oct Node Block";
        block.name += " - Level " + level;

        float scale = Mathf.Pow(2, level);
        block.SetEntity();
        block.Entity.transform.localScale = new Vector3(scale, scale, scale);
        block.Entity.SetActive(isSolid);

        block.Level = level;
        return block;
    }

    // Use this for initialization
    void Start()
    {
        //root = CreateBlock(16, false);
        //root.transform.position = new Vector3(-0.5f, -0.5f, -0.5f);


        //for (int i = -20; i <= 20; i++)
        //{
        //    for (int j = -20; j <= 20; j++)
        //    {
        //        Block block;
        //        InsertBlock(new Vector3(i, -1, j), 0, out block);

        //    }
        //}

    }

    public void Initialize(int topLevel = 16)
    {
        root = CreateBlock(topLevel, false);
        root.transform.position = new Vector3(-0.5f, -0.5f, -0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftControl))
            SaveXML();

    }

    Block root;


    public InsertionResult InsertBlock(Vector3 point, int level, out Block block)
    {
        return InsertBlock(point, level, root, out block);
    }

    public InsertionResult InsertBlock(Vector3 point, int level)
    {
        Block block = null;
        return InsertBlock(point, level, out block);
    }

    public enum InsertionResult { OK, FAILED_INSIDEENTITY, FAILED_BLOCKEXIST }

    InsertionResult InsertBlock(Vector3 point, int level, Block parent, out Block block)
    {
        if (parent.IsEntity)
        {
            block = null;
            return InsertionResult.FAILED_INSIDEENTITY;
        }

        if (level == parent.Level - 1)
        {
            int id = parent.FindSubArea(point);
            if (parent.subBlocks[id] != null)
            {
                block = null;
                return InsertionResult.FAILED_BLOCKEXIST;
            }

            parent.subBlocks[id] = CreateBlock(parent.Level - 1, true);
            parent.subBlocks[id].transform.position = parent.ChildPositions[id];
            block = parent.subBlocks[id];
            return InsertionResult.OK;
        }
        else
        {
            int id = parent.FindSubArea(point);
            if (parent.subBlocks[id] == null)
            {
                parent.subBlocks[id] = CreateBlock(parent.Level - 1, false);

                parent.subBlocks[id].transform.position = parent.ChildPositions[id];
            }
            return InsertBlock(point, level, parent.subBlocks[id], out block);
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
                //parent.subBlocks[id].OnDestroy();
                //GameObject.Destroy(parent.subBlocks[id]);
                parent.subBlocks[id].Delete();
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

    DropResult DropBlock(Block block)
    {
        return DropBlock(root, block);
    }

    DropResult DropBlock(Block parent, Block block)
    {
        if (parent == null || block == null || parent.Level <= block.Level)
            return DropResult.FAILED_NOTEXIST;


        if (parent.Level == block.Level + 1)
        {
            for (int i = 0; i < 8; i++)
            {
                if (parent.subBlocks[i] == block)
                {
                    //GameObject.Destroy(block.gameObject);
                    block.Delete();
                    parent.subBlocks[i] = null;
                    return DropResult.OK;
                }
            }
            return DropResult.FAILED_NOTEXIST;
        }
        int area = parent.FindSubArea(block.Position);
        return DropBlock(parent.subBlocks[area], block);
    }

    Block GetParent(Block block, int parentLevel)
    {
        return null;
    }

    void SplitBlock(Block block)
    {
        if (block != null && block.IsEntity)
        {
            for (int id = 0; id < 8; id++)
            {
                block.subBlocks[id] = CreateBlock(block.Level - 1, true);
                block.subBlocks[id].transform.position = block.ChildPositions[id];
                block.subBlocks[id].Entity.GetComponent<MeshRenderer>().sharedMaterial = block.Entity.GetComponent<MeshRenderer>().sharedMaterial;
                block.subBlocks[id].MaterialID = block.MaterialID;
            }
        }
        block.Entity.SetActive(false);
    }

    public void DrillBlock(Block block, Vector3 position, int level)
    {
        if (block.IsEntity && BlockUtility.IsInBlock(block, position))
        {
            if (block.Level <= level)
                DropBlock(block);
            else
            {
                SplitBlock(block);
                for (int i = 0; i < 8; i++)
                {
                    DrillBlock(block.subBlocks[i], position, level);
                }
            }
        }
    }


    public Vector3 GetBlockCenter(Vector3 query, int level)
    {
        return GetBlockCenter(query, level, root);
    }

    public Vector3 GetBlockCenter(Vector3 query, int level, IBlock block)
    {
        if (level == block.Level - 1)
        {
            int id = BlockUtility.FindSubArea(block, query);
            return BlockUtility.ChildPositions(block)[id];
        }
        else
        {
            int id = BlockUtility.FindSubArea(block, query);
            Vector3 subcenter = BlockUtility.ChildPositions(block)[id];
            IBlock b = new SimpleBlock(subcenter, block.Level - 1);
            return GetBlockCenter(query, level, b);
        }
    }

    public string Dump()
    {
        return Dump(root, 0);
    }
    public string Dump(Block parent, int indent)
    {
        Vector3 p = parent.transform.position;
        var str = string.Format("({0} {1} {2})", p.x, p.y, p.z);
        var result = "";
        for (int i = 0; i < indent; i++)
            result += " ";
        result += str;
        if (parent.IsEntity)
            result += "  SOLID";
        result += "\n";
        for (int i = 0; i < 8; i++)
            if (parent.subBlocks[i] != null)
                result += Dump(parent.subBlocks[i], indent + 1);
        if (result[result.Length - 1] != '\n')
            result += "\n";
        return result;
    }

    public static void ConvertToXML(Block block, int childID, XmlDocument doc, XmlElement parentNode)
    {
        var e = doc.CreateElement("block");
        e.SetAttribute("level", "" + block.Level);
        e.SetAttribute("child_id", "" + childID);
        if (block.IsEntity)
            e.SetAttribute("material_id", "" + block.MaterialID);

        parentNode.AppendChild(e);
        for (int i = 0; i < 8; i++)
        {
            if (block.subBlocks[i] != null)
                ConvertToXML(block.subBlocks[i], i, doc, e);
        }
    }
    public void SaveXML()
    {
        XmlDocument doc = new XmlDocument();
        var root = doc.CreateElement("tree");
        doc.AppendChild(root);
        ConvertToXML(this.root, 0, doc, root);
        doc.Save("world.xml");
    }

    public static void ReadFromXml(XmlElement xmlElement, BlockManager blockManager, Block parent)
    {
        int id = int.Parse(xmlElement.Attributes["child_id"].Value);
        if (parent.subBlocks[id] != null)
            return;

        int level = int.Parse(xmlElement.Attributes["level"].Value);

        if (level != parent.Level - 1)
            return;

        bool issolid = xmlElement.HasAttribute("material_id");
        int material = 0;

        if (issolid)
            material = int.Parse(xmlElement.Attributes["material_id"].Value);
        Block block = blockManager.CreateBlock(level, issolid);
        block.transform.position = parent.ChildPositions[id];
        if (issolid)
            block.SetPaletteMaterial(blockManager.palette, material);

        parent.subBlocks[id] = block;

        foreach(XmlElement e in xmlElement.ChildNodes)
        {
            ReadFromXml(e, blockManager, block);
        }
    }

    public bool LoadXML()
    {
        try
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("world.xml");
            XmlElement tree = null;
            foreach (XmlElement e in doc.ChildNodes)
            {
                if (e.Name == "tree")
                    tree = e;
            }
            XmlElement root = (XmlElement)tree.FirstChild;
            foreach(XmlElement e in root.ChildNodes)
            {
                ReadFromXml(e, this, this.root);
            }
            //ReadFromXml(tree.FirstChild.FirstChild, this, root);
            return true;
        }
        catch
        {
            foreach(var b in root.subBlocks)
            {
                DropBlock(b);
            }
        }
        return false;
    }
}

