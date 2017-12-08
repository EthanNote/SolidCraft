using UnityEngine;
using System.Collections;

public abstract class CamCraftingAction
{
    public BlockDBManager manager;
    public int priority;
    public BlockCraftingCamera camera;
    public CamCraftingAction(BlockCraftingCamera camera,
        BlockDBManager manager, int priority)
    {
        this.camera = camera;
        this.priority = priority;
        this.manager = manager;
        repeatParameter = null;
    }

    public CraftingActionParameter repeatParameter;
    public abstract CraftingActionParameter Do(CraftingActionParameter parameter);
    public void SetUinqueRepeat(CraftingActionParameter param)
    {
        foreach (var a in camera.actions)
        {
            if (a != this)
                a.repeatParameter = null;
        }
        this.repeatParameter = param;
    }

}

public class CraftingActionParameter
{
    public Vector3 markerPosition;
    public Vector3 normal;
    public int level;
    public BlockObject pickedBlock;
}

class CreatingBlockAction : CamCraftingAction
{
    public CreatingBlockAction(BlockCraftingCamera camera, BlockDBManager manager, int priority)
    : base(camera, manager, priority)
    { }

    public override CraftingActionParameter Do(CraftingActionParameter parameter)
    {
        if(Input.GetKeyDown(KeyCode.R) && repeatParameter != null)
        {
            parameter = repeatParameter;
        }
        if (Input.GetKeyDown(KeyCode.Mouse0) && parameter.pickedBlock != null)
        {
            var palette = camera.GetComponent<Palette>();
            var keys = BlockNode.GetKeys(parameter.markerPosition, camera.CraftingLevel);
            var result = manager.DBManager.Insert(keys, camera.CraftingLevel, camera.GetComponent<Palette>());
            if (result.Succeed)
            {
                var rc = new CraftingActionParameter();
                rc.level = parameter.level;
                rc.markerPosition = parameter.markerPosition + parameter.normal * (rc.level / 64.0f);
                rc.normal = parameter.normal;
                rc.pickedBlock = (result.Result as BlockNode).block;

                SetUinqueRepeat(rc);
                return rc;
            }
        }
        return null;
    }
}

class ZoomLevelAction : CamCraftingAction
{
    public ZoomLevelAction(BlockCraftingCamera camera, BlockDBManager manager, int priority) 
        : base(camera, manager, priority)
    {
    }

    public override CraftingActionParameter Do(CraftingActionParameter parameter)
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (wheel > 0)
                camera.CraftingLevel++;
            else if (wheel < 0)
                camera.CraftingLevel--;            
        }
        return null;
    }
}



