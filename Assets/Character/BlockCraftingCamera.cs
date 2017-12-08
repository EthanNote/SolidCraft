using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockCraftingCamera : MonoBehaviour
{

    public int CraftingLevel { get; set; }
    public Text CraftinglevelIndicator;
    public BlockDBManager manager;
    public float CraftingDistance = 10;
    public GameObject marker;


    public List<CamCraftingAction> actions { get; private set; }

    // Use this for initialization
    void Start()
    {
        CraftingLevel = 6;
        this.actions = new List<CamCraftingAction>()
        {
            new CreatingBlockAction(this, manager, 0),
            new ZoomLevelAction(this, manager, 0)
        };
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hitinfo;
        if (Physics.Raycast(transform.position, transform.forward, out hitinfo, 10))
        {
            var param = new CraftingActionParameter();

            var pickedBlock = hitinfo.transform.gameObject.GetComponent<BlockObject>();
            if (pickedBlock != null)
            {
                var nodekey = pickedBlock.node.Keys;
                int[] keys = new int[3];
                nodekey.CopyTo(keys, 0);

                print(hitinfo.normal);

                if (Vector3.Dot(hitinfo.normal, Vector3.right) > 0.9f)
                    keys[0] += (1 << CraftingLevel);

                if (Vector3.Dot(hitinfo.normal, Vector3.up) > 0.9f)
                    keys[1] += (1 << CraftingLevel);

                if (Vector3.Dot(hitinfo.normal, Vector3.forward) > 0.9f)
                    keys[2] += (1 << CraftingLevel);

                if (Vector3.Dot(hitinfo.normal, Vector3.right) < -0.9f)
                    keys[0] -= (1 << CraftingLevel);

                if (Vector3.Dot(hitinfo.normal, Vector3.up) < -0.9f)
                    keys[1] -= (1 << CraftingLevel);

                if (Vector3.Dot(hitinfo.normal, Vector3.forward) < -0.9f)
                    keys[2] -= (1 << CraftingLevel);

                marker.transform.position = BlockNode.GetPosition(keys, CraftingLevel);
                marker.transform.localScale = Vector3.one * (1 << CraftingLevel)/64.0f;
                marker.SetActive(true);

                param.normal = hitinfo.normal;
                param.level = CraftingLevel;
                param.markerPosition = marker.transform.position;
                param.pickedBlock = pickedBlock;

                foreach(var a in actions)
                {
                    a.Do(param);
                }

            }
            else
                marker.SetActive(false);
        }
        else
            marker.SetActive(false);
    }
}
