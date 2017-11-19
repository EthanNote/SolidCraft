using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Palette : MonoBehaviour
{
    public Material[] Materials;
    public Image MaterialIndicator;
    public int SelectedID { get; private set; }
    // Use this for initialization
    void Start()
    {
        SelectedID = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetKey(KeyCode.LeftControl) && Materials.Length > 0)
        {
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (wheel > 0)
                SelectedID++;
            if (wheel < 0)
                SelectedID--;
            while (SelectedID < 0)
                SelectedID += Materials.Length;
            SelectedID %= Materials.Length;
            MaterialIndicator.color = Materials[SelectedID].color;
        }
    }
    public Material Material
    {
        get
        {
            if (Materials.Length > 0)
                return Materials[SelectedID];
            return null;
        }
    }

}
