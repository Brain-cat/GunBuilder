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

    [Header("Weapon/Consumable Stats")]
    //Define bullet velocity, reload speed, aim speed, weapon damage, Fire rate, Jamming chance, accuracy 
    public float Damage;
    public float Recoil;
    public float Spread; //can be used to control hipfire accuracy and ADS accuracy
    public float BulletVelocity;
    public float ReloadSpeed;
    public float AimSpeed;
    public float FireRateRPM;
    public float JammingChance;
    public string FiringMode;
    public string AmmoType;
    public int MagSize;
    public int AmmoInClip;
    public bool IsWeapon;

    //for healing items or leech weapons, if its a healing consumable just set all other stats to 0  but healing
    public float Healing;
    public float HealingRate;

    [Header("Aiming")]
    public GameObject AimSpot;
    public Vector3 AimSpotOffset;
    public float AimFOV;
}
