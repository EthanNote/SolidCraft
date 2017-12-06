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
    public abstract bool Do(Vector3 markkerPosition, Vector3 normal, int level, Block hitBlock);
    public virtual ActionParameter Do(ActionParameter parameter)
    {
        Do(parameter.markerPosition, parameter.normal, parameter.level, parameter.pickedBlock);
        return null;
    }
}

public class ActionParameter
{
    public Vector3 markerPosition;
    public Vector3 normal;
    public int level;
    public Block pickedBlock;
}


class CreatingAction : CraftingAction
{
    public CreatingAction(CraftingCamera camera, BlockManager blockManager, int priority)
        : base(camera, blockManager, priority) { }

    Vector3 lastposition;
    Vector3 lastNormal;
    int lastLevel;
    Block lastCreatedBlock;
    bool created = false;

    public override bool Do(Vector3 markerPosition, Vector3 normal, int level, Block hitBlock)
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            lastCreatedBlock = null;
            created = false;
            return false;
        }
        if (Input.GetKeyDown(KeyCode.Mouse0) && hitBlock != null)
        {
            Block block;
            BlockManager.InsertBlock(markerPosition, level, out block);
            if (block != null)
            {
                var palette = CraftingCamera.GetComponent<Palette>();
                block.SetPaletteMaterial(palette, palette.SelectedID);
                lastposition = markerPosition;
                lastNormal = normal;
                lastLevel = level;
                lastCreatedBlock = block;
                created = true;
                return true;
            }
            created = false;
            lastCreatedBlock = null;
            return true;
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            if (created)
            {
                Block block;
                BlockManager.InsertBlock(lastposition + lastNormal * Mathf.Pow(2, lastLevel), lastLevel, out block);
                if (block != null)
                {
                    var palette = CraftingCamera.GetComponent<Palette>();
                    block.SetPaletteMaterial(palette, palette.SelectedID);
                    lastposition = block.Position;
                    lastCreatedBlock = block;
                    created = true;
                    return true;
                }
                created = false;
                lastCreatedBlock = null;
                return true;
            }
        }
        return false;
    }
}

class DropAction : CraftingAction
{
    public DropAction(CraftingCamera camera, BlockManager blockManager, int priority)
        : base(camera, blockManager, priority) { }

    bool DropOrDrill(Vector3 markerPosition, Vector3 normal, int level, Block hitBlock)
    {
        if (Input.GetKey(KeyCode.Mouse0))
            repeatable = false;

        if (Input.GetKeyDown(KeyCode.Mouse1) && hitBlock != null)
        {
            BlockManager.DropResult r;
            if (hitBlock.Level < CraftingCamera.CraftingLevel)
                r = BlockManager.DropBlock(hitBlock.Position, CraftingCamera.CraftingLevel);
            else
                r = BlockManager.DropBlock(markerPosition - normal * Mathf.Pow(2, level), CraftingCamera.CraftingLevel);
            if (r != BlockManager.DropResult.OK)
                BlockManager.DrillBlock(hitBlock, markerPosition - normal * Mathf.Pow(2, level), CraftingCamera.CraftingLevel);

            return true;
        }
        return false;
    }

    Vector3 lastposition;
    Vector3 lastNormal;
    int lastLevel;
    bool repeatable = false;

    public override bool Do(Vector3 markerPosition, Vector3 normal, int level, Block hitBlock)
    {
        if (DropOrDrill(markerPosition, normal, level, hitBlock))
        {
            lastposition = markerPosition - normal * Mathf.Pow(2, level);
            lastNormal = normal;
            lastLevel = level;
            repeatable = true;
            return true;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (repeatable)
            {
                Vector3 nextPosition = lastposition - lastNormal * Mathf.Pow(2, lastLevel);
                if (BlockManager.DropBlock(nextPosition, lastLevel) == BlockManager.DropResult.OK
                    || BlockManager.DrillBlock(nextPosition, lastLevel))
                {
                    lastposition = nextPosition;
                }
                else
                    repeatable = false;

            }
            return repeatable;
        }
        return false;
    }
}

class SettingSizeAction : CraftingAction
{
    public SettingSizeAction(CraftingCamera camera, BlockManager blockManager, int priority)
        : base(camera, blockManager, priority) { }

    public override bool Do(Vector3 markerPosition, Vector3 normal, int level, Block hitBlock)
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

    public override bool Do(Vector3 markerPosition, Vector3 normal, int level, Block hitBlock)
    {
        if (Input.GetMouseButton(2) && hitBlock != null)
        {
            var palette = CraftingCamera.GetComponent<Palette>();
            Block parent = BlockManager.GetParent(hitBlock, level);
            parent.SetPaletteMaterial(palette, palette.SelectedID);
            return true;
        }
        return false;
    }
}