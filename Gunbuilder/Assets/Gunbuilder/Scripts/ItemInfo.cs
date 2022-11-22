using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfo : MonoBehaviour //this will be used for all interactables now
{
    public bool PickUpable;
    public string ItemTag;
    public string ItemName;
    public Sprite SlotImg;
    public GameObject ItemPrefab;

    [Header("ViewModelOffset")]
    public Vector3 ViewModelPosOffset;
    public Vector3 ViewModelRotOffset;
}
