using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SlotInfo : MonoBehaviour
{
    public string ItemTag;
    public string ItemName;
    public Sprite SlotImg;
    public GameObject ItemPrefab;
    public bool Full;
    public Sprite EmptySlotImg;

    public void SetSlotImg(Sprite slotimg) //sets and declares item image
    {
        SlotImg = slotimg;
        GetComponent<Image>().sprite = slotimg;
    }
}
