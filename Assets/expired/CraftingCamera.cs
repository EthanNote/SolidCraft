using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 markerPosition;
        Block pickedBlock;
        RaycastHit hitinfo;
        bool marked = MarkCraftingCenter(out markerPosition, out pickedBlock, out hitinfo);
        float scale = Mathf.Pow(2, CraftingLevel);
        marker.gameObject.SetActive(marked);
        marker.position = markerPosition;
        marker.localScale = Vector3.one * scale;

        DoActions(markerPosition, hitinfo.normal, CraftingLevel, pickedBlock);

        if (CraftinglevelIndicator != null)
            CraftinglevelIndicator.text = "" + CraftingLevel;

    }


    bool MarkCraftingCenter(out Vector3 markerPosition, out Block pickedBlock, out RaycastHit hitinfo)
    {

        float blocksize = Mathf.Pow(2, CraftingLevel);
        float raylength = CraftingDistance;
        if (blocksize > 1)
            raylength *= blocksize;

        if (Physics.Raycast(transform.position, transform.forward, out hitinfo, raylength))
        {
            if (hitinfo.transform.gameObject.name == "Entity")
            {
                pickedBlock = hitinfo.transform.parent.gameObject.GetComponent<Block>();
               
                markerPosition = blockManager.GetBlockCenter(hitinfo.point + hitinfo.normal * (blocksize / 2.1f), CraftingLevel);
            }
            else
            {
                markerPosition = blockManager.GetBlockCenter(hitinfo.point, CraftingLevel);
                pickedBlock = null;
            }
            return true;
        }
        markerPosition = Vector3.zero;
        pickedBlock = null;
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

    public void DoActions(Vector3 position, Vector3 normal, int level, Block hitBlock)
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
