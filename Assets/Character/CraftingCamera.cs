using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class CraftingAction
{
    public BlockManager BlockManager { get; protected set; }
    public int Priority { get; protected set; }
    public CraftingCamera CraftingCamera { get; protected set; }
    public CraftingAction(CraftingCamera camera, BlockManager blockManager, int priority)
    {
        this.CraftingCamera = camera;
        this.BlockManager = blockManager;
        this.Priority = priority;
    }
    public abstract bool Do(Vector3 position, Vector3 normal, int level, Block hitBlock);
}

class CreatingAction : CraftingAction
{
    public CreatingAction(CraftingCamera camera, BlockManager blockManager, int priority)
        : base(camera, blockManager, priority) { }

    public override bool Do(Vector3 position, Vector3 normal, int level, Block hitBlock)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && hitBlock != null)
        {
            Block block;
            BlockManager.InsertBlock(position, level, out block);
            if (block != null)
            {
                var palette = CraftingCamera.GetComponent<Palette>();
                block.SetPaletteMaterial(palette, palette.SelectedID);
            }
            return true;
        }
        return false;
    }
}

class DropAction : CraftingAction
{
    public DropAction(CraftingCamera camera, BlockManager blockManager, int priority)
        : base(camera, blockManager, priority) { }

    public override bool Do(Vector3 position, Vector3 normal, int level, Block hitBlock)
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && hitBlock != null)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (hitBlock.Level < CraftingCamera.CraftingLevel)
                    BlockManager.DropBlock(hitBlock.Position, CraftingCamera.CraftingLevel);
                else
                    BlockManager.DropBlock(position - normal * Mathf.Pow(2, level), CraftingCamera.CraftingLevel);
            }
            else
            {
                BlockManager.DrillBlock(hitBlock, position - normal * Mathf.Pow(2, level), CraftingCamera.CraftingLevel);
            }
            return true;
        }
        return false;
    }
}

class SettingSizeAction : CraftingAction
{
    public SettingSizeAction(CraftingCamera camera, BlockManager blockManager, int priority)
        : base(camera, blockManager, priority) { }

    public override bool Do(Vector3 position, Vector3 normal, int level, Block hitBlock)
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (wheel > 0)
                CraftingCamera.CraftingLevel++;
            else if (wheel < 0)
                CraftingCamera.CraftingLevel--;
            if (wheel != 0)
                return true;
        }
        return false;
    }
}

class PaintingAction : CraftingAction
{
    public PaintingAction(CraftingCamera camera, BlockManager blockManager, int priority)
        : base(camera, blockManager, priority) { }

    public override bool Do(Vector3 position, Vector3 normal, int level, Block hitBlock)
    {
        if (Input.GetMouseButtonDown(2) && hitBlock != null)
        {
            var palette = CraftingCamera.GetComponent<Palette>();
            hitBlock.SetPaletteMaterial(palette, palette.SelectedID);
            return true;
        }
        return false;
    }
}

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(Palette))]
public class CraftingCamera : MonoBehaviour
{

    public int CraftingLevel { get; set; }
    public Text CraftinglevelIndicator;
    public BlockManager blockManager;
    public float CraftingDistance = 10;
    // Use this for initialization
    void Start()
    {
        this.actions = new List<CraftingAction>() {
            new CreatingAction(this, blockManager, 5),
            new DropAction(this, blockManager, 5),
            new SettingSizeAction(this, blockManager, 5),
            new PaintingAction(this, blockManager, 5)
        };
        //actions.Sort((CraftingAction a, CraftingAction b) => { return a.Priority > b.Priority ? 1 : -1; });
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 markpos;
        Block hited;
        RaycastHit hitinfo;
        bool marked = MarkCraftingCenter(out markpos, out hited, out hitinfo);
        float scale = Mathf.Pow(2, CraftingLevel);
        marker.gameObject.SetActive(marked);
        marker.position = markpos;
        marker.localScale = Vector3.one * scale;

        Do(markpos, hitinfo.normal, CraftingLevel, hited);

        ////Create block
        //if (Input.GetKeyDown(KeyCode.Mouse0) && marked)
        //{
        //    Block block;
        //    blockManager.InsertBlock(markpos, CraftingLevel, out block);
        //    if (block != null)
        //    {
        //        //block.Entity.GetComponent<MeshRenderer>().sharedMaterial = GetComponent<Palette>().Material;
        //        //block.MaterialID = GetComponent<Palette>().SelectedID;
        //        var palette = GetComponent<Palette>();
        //        block.SetPaletteMaterial(palette, palette.SelectedID);
        //    }
        //}

        ////Drill or remove block
        //if (Input.GetKeyDown(KeyCode.Mouse1) && marked)
        //{
        //    if (Input.GetKey(KeyCode.LeftShift))
        //    {
        //        if (hited.Level < CraftingLevel)
        //            blockManager.DropBlock(hited.Position, CraftingLevel);
        //        else
        //            blockManager.DropBlock(marker.position - hitinfo.normal * scale, CraftingLevel);
        //    }
        //    else
        //    {
        //        blockManager.DrillBlock(hited, marker.position - hitinfo.normal * scale, CraftingLevel);
        //    }
        //}

        ////Set crafting size
        //if (Input.GetKey(KeyCode.LeftControl))
        //{
        //    float wheel = Input.GetAxis("Mouse ScrollWheel");
        //    if (wheel > 0)
        //        CraftingLevel++;
        //    else if (wheel < 0)
        //        CraftingLevel--;
        //}

        ////Reassign block material
        //if (Input.GetMouseButtonDown(2) && hited != null)
        //{
        //    //hited.Entity.GetComponent<MeshRenderer>().sharedMaterial = GetComponent<Palette>().Material;
        //    //hited.MaterialID = GetComponent<Palette>().SelectedID;
        //    var palette = GetComponent<Palette>();
        //    hited.SetPaletteMaterial(palette, palette.SelectedID);
        //}

        if (CraftinglevelIndicator != null)
            CraftinglevelIndicator.text = "" + CraftingLevel;

    }


    bool MarkCraftingCenter(out Vector3 center, out Block block, out RaycastHit hitinfo)
    {

        float blocksize = Mathf.Pow(2, CraftingLevel);
        float raylength = CraftingDistance;
        if (blocksize > 1)
            raylength *= blocksize;

        if (Physics.Raycast(transform.position, transform.forward, out hitinfo, raylength))
        {
            if (hitinfo.transform.gameObject.name == "Entity")
            {
                block = hitinfo.transform.parent.gameObject.GetComponent<Block>();
                //if (block.Level < craftingLevel)
                //{
                //    //if (Input.GetKey(KeyCode.LeftShift))
                //    //    center = blockManager.GetBlockCenter(block.Position, craftingLevel) + hitinfo.normal * blocksize;
                //    //else
                //        center = blockManager.GetBlockCenter(block.Position, craftingLevel);

                //}
                //else
                //{
                center = blockManager.GetBlockCenter(hitinfo.point + hitinfo.normal * (blocksize / 2.1f), CraftingLevel);
                //}
            }
            else
            {
                center = blockManager.GetBlockCenter(hitinfo.point, CraftingLevel);
                block = null;
            }
            return true;
        }
        center = Vector3.zero;
        block = null;
        return false;
    }

    public Transform marker;


    List<CraftingAction> actions;
    public CraftingCamera craftingCamera;
    public void AddAction(CraftingAction action)
    {
        actions.Add(action);
        actions.Sort((CraftingAction a, CraftingAction b) => { return a.Priority > b.Priority ? 1 : -1; });
    }

    public void Do(Vector3 position, Vector3 normal, int level, Block hitBlock)
    {
        bool result = true;
        int currentPriority = int.MaxValue;
        foreach (var a in actions)
        {
            if (a.Priority < currentPriority)
                if (!result)
                    break;
            if (!a.Do(position, normal, level, hitBlock))
                result = false;
            currentPriority = a.Priority;

        }
    }


}
