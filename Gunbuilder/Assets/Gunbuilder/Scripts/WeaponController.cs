using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class WeaponController : MonoBehaviour
{
    [Header("Equiped Item Stats (Debug)")]
    //Define bullet velocity, reload speed, aim speed, weapon damage, Fire rate, Jamming chance, accuracy   //might not need to define here as we already have seperate script

    public GameObject ItemPrefab;
    public string ItemTag;
    public float Damage;
    public float Recoil;
    public float Spread; //can be used to control hipfire accuracy and ADS accuracy
    public float BulletVelocity;
    public float ReloadSpeed;
    public float AimSpeed;
    public float FireRate;
    public float JammingChance; //might have will implement jamming later
    public string FiringMode;
    public string AmmoType;
    public int MagSize;
    public int AmmoInClip;
    public bool IsWeapon;

    public float Healing;
    public float HealingRate;

    public GameObject AimSpot;
    public Vector3 AimSpotOffset;
    public float AimFOV;

    [Header ("Raycasting")]
    public GameObject playerCameraHolder;
    public InGameUI ingameUI;

    [Header("Rate of fire (Debug)")]
    public float nextShot; //is public for debugging
    public float currentTime; //for debugging

    [Header("Reticle")]
    public GameObject HipfireReticle;
    public Color EnemyColor;
    public Color DefaultColor;

    [Header("Reloading (Debug)")]
    public bool isReloading;
    public float reloadTime;

    [Header("Aiming")]
    public PlayerController playerController;
    bool isAlreadyAiming;

    // Update is called once per frame
    void Update()
    {
        currentTime = Time.time; //for debugging

        Ray ray = new Ray(playerCameraHolder.transform.position, playerCameraHolder.transform.forward);
        //Debug.DrawRay(ray.origin, ray.direction,Color.red,2f); //for debugging direction

        ReticleCheck(ray); //checks if reticle is pointed at enemy or not, will change color accordingly

        if (ingameUI.MenuEnabled == false) //checks if main menu is open
        {
            switch (FiringMode) 
            {
                case "FullAuto":

                    if (Input.GetButton("Fire1") && Time.time >= nextShot) //Mouse 0 Left Click and ready to fire // get button makes full auto
                    {
                        if (AmmoInClip > 0) //checks if ammo in clip
                        {
                            Shoot(ItemTag, ray);  //Fires the weapon
                            nextShot = Time.time + (60 / FireRate); //turns from per min to per second
                            AmmoInClip -= 1; //removes 1 from clip

                            ingameUI.UpdateEquipedAmmoUI(AmmoType,AmmoInClip); //updates UI
                            ItemPrefab.GetComponent<ItemInfo>().AmmoInClip = AmmoInClip; //updates the actual item stats so while scrolling or dropping item ammo isn't reset
                        }

                        else if (isReloading == false) //checks if we're not already reloading
                            Reload(GetComponent<Inventory>(), true); // reloads if out of ammo
                    }

                    break;

                case "SemiAuto":

                    if (Input.GetButtonDown("Fire1") && Time.time >= nextShot) //Mouse 0 Left Click and ready to fire // buttondown makes semi auto
                    {
                        if (AmmoInClip > 0) //checks if ammo in clip
                        {
                            Shoot(ItemTag, ray);  //Fires the weapon
                            nextShot = Time.time + (60 / FireRate); //turns from per min to per second
                            AmmoInClip -= 1; //removes 1 from clip

                            ingameUI.UpdateEquipedAmmoUI(AmmoType, AmmoInClip);//updates UI
                            ItemPrefab.GetComponent<ItemInfo>().AmmoInClip = AmmoInClip; //updates the actual item stats so while scrolling or dropping item ammo isn't reset
                        }

                        else if (isReloading == false) //checks if we're not arleady reloading
                            Reload(GetComponent<Inventory>(), true); // reloads if out of ammo
                    }

                    break;

                case "Burst":
                    //will implement later
                    break;

                default:
                    break;
            }

            if (Input.GetButton("Fire2") && IsWeapon == true && isAlreadyAiming == false) //Mouse 1 Right Click
            {
                AimDownSights(true);
                isAlreadyAiming = true; //Aims the weapon, reduces spread.
            }

            if (Input.GetButtonUp("Fire2") && IsWeapon == true && isAlreadyAiming == true) 
            {
                AimDownSights(false);
                isAlreadyAiming = false;
            }

            if (Input.GetButtonDown("Reload") && IsWeapon == true) //reload the weapon
            {
                Reload(GetComponent<Inventory>(),true);
            }

            if (isReloading == true) 
            {
                Reload(GetComponent<Inventory>(),false);
            }
        }
    }

    void ReticleCheck(Ray ray) 
    {
        RaycastHit raycastHit;
        bool ObjectHit = Physics.Raycast(ray.origin, ray.direction,out raycastHit);

        if (ObjectHit == true && raycastHit.collider.gameObject.layer == 8)
        {
            HipfireReticle.GetComponent<Graphic>().color = EnemyColor; //changes red if enemy is aimed at
        }

        else 
        {
            HipfireReticle.GetComponent<Graphic>().color = DefaultColor; //returns to white if no enemy in crosshair
        }
    }

    public void GetStats(SlotInfo slotInfo) // is called in the InGameUI script after scrolling and picking up / dropping item in Inventory script // WANT TO REVIEW THIS SET OF CODE LATER DEFO COULD BE DONE BETTER I JUST DONT WANNA DO IT NOW (added to todo list)
    {
        if (slotInfo.Full == true)
        {
            ItemInfo stats = slotInfo.ItemPrefab.GetComponent<ItemInfo>(); // I have no idea how to call large amounts of data at once to transfer over to this from that specific weapon/object stats, for now this will do for unity until i figure that out
            SetStats(stats.ItemTag, stats.Damage, stats.Recoil, stats.Spread, 
                stats.BulletVelocity, stats.ReloadSpeed, stats.AimSpeed, 
                stats.FireRateRPM, stats.JammingChance, stats.Healing, 
                stats.HealingRate, stats.FiringMode,stats.AmmoType,
                stats.MagSize,stats.AmmoInClip, stats.IsWeapon, 
                stats.ItemPrefab,stats.AimSpot,stats.AimSpotOffset,stats.AimFOV); //quite long probs a way better way of doing this
        }

        else 
        {
            SetStats("",0, 0, 0, 0, 0, 0, 0, 0, 0, 0,"","",0,0,false,null,null,new Vector3(0,0,0),0); //again probs a better way of resetting everything to 0 but this will do for now, may come back later time to try refine
        }
    }

    void SetStats(string itemTag, float damage, float recoil, float spread, float bulletVelocity,
        float reloadSpeed, float aimSpeed, float fireRate, float jammingChance, float healing,
        float healingRate, string firingMode, string ammoType, int magSize, int ammoInClip,
        bool isWeapon, GameObject itemPrefab, GameObject aimSpot, Vector3 aimSpotOffset, float aimFOV)
    {
        //sets all the stats so i dont have to call from the seperate script each time to use equiped slot stats, not sure if this impacts performance much
        ItemTag = itemTag;
        Damage = damage;
        Recoil = recoil;
        Spread = spread;
        BulletVelocity = bulletVelocity;
        ReloadSpeed = reloadSpeed;
        AimSpeed = aimSpeed;
        FireRate = fireRate;
        JammingChance = jammingChance;
        Healing = healing;
        HealingRate = healingRate;
        FiringMode = firingMode;
        AmmoType = ammoType;
        MagSize = magSize;
        AmmoInClip = ammoInClip;
        IsWeapon = isWeapon;
        ItemPrefab = itemPrefab;
        AimSpot = aimSpot;
        AimSpotOffset = aimSpotOffset;
        AimFOV = aimFOV;
    }

    void Shoot(string weaponType, Ray ray) //will use/shoot whatever the user has in their hand
    {
        switch (weaponType) //checks what weapon type we have in our hand
        {
            case "HitScan": //Hit scan weapons are instant dmg dealers, no bullet travel.

                RaycastHit raycastHit;
                GameObject objectHit;

                bool bulletHit = Physics.Raycast(ray.origin, ray.direction, out raycastHit); //checks if we hit something (not sure if range is needed, we'll see)

                if (bulletHit) //will only run if bullet hits something
                {
                    objectHit = raycastHit.collider.gameObject;  //gets the gameobject from the collider, make sure enemy script and other scripts are where the collider is, again probs a better way but good enough
                    Debug.DrawLine(ray.origin, raycastHit.point,Color.red,2f); //for debugging hit

                    //checks what we hit using the layer the gameobject is on
                    if (objectHit.layer == 8 && objectHit.GetComponent<EnemyController>().isDead == false) //layer 8 is enemy
                    {
                        objectHit.GetComponent<EnemyController>().Hit(Damage); //takes away dmg done will add armour later

                        if (objectHit.GetComponent<EnemyStats>().HP <= 0)  //if hp 0 then ded
                        {
                            objectHit.GetComponent<EnemyController>().isDead = true; //ded
                        }
                    }
                }

                CreateBulletHole();

                break;

            case "Consumable":
                //will work on this later
                break;

            default:
                break;
        }
    }

    void AimDownSights(bool isAiming) 
    {
        if (isAiming)
        {
            HipfireReticle.SetActive(false);

            ItemPrefab.transform.position = playerController.PlayerCamera.transform.position;

            //there is a high chance of this causing some issue later down the line :)
            if(AimSpot.transform.localPosition.y > 0) //use negative value for the y axis if aimspot y is positive //had to do it this way so for some reason parenting objects was messing with the x and z values, hence why way earlier i had to use waeponspot offest in inventory
                ItemPrefab.transform.localPosition = ItemPrefab.transform.localPosition + (new Vector3(AimSpot.transform.localPosition.z + AimSpotOffset.x, 
                    0 - (AimSpot.transform.localPosition.y + AimSpotOffset.y), 
                    AimSpot.transform.localPosition.x + AimSpotOffset.z));

            else //use positive value for the y axis if aimspot y is negative
                ItemPrefab.transform.localPosition = ItemPrefab.transform.localPosition + (new Vector3(AimSpot.transform.localPosition.z + AimSpotOffset.x, 
                    0 + (AimSpot.transform.localPosition.y + AimSpotOffset.y), 
                    AimSpot.transform.localPosition.x + AimSpotOffset.z));


            playerController.PlayerCamera.fieldOfView = playerController.PlayerCamera.fieldOfView - AimFOV;// changes the FOV
        }

        else 
        {
            HipfireReticle.SetActive(true);
            playerController.PlayerCamera.fieldOfView = playerController.CameraFOV; //reverts fov
            GetComponent<Inventory>().SelectedItemShowMove(ItemPrefab,ItemPrefab.GetComponent<ItemInfo>(),GetComponent<InGameUI>().selectedSlot.GetComponent<SlotInfo>()); //returns weapon back to idle pos
        }
    }

    void CreateBulletHole() 
    {
        //want to add little bullet holes can be temporary random coloured plane for now but will help debug bullet spread and so on.
    }

    void Reload(Inventory inventory, bool StartReload) //reloads the weapon, takes ammo from inventory and puts it in ammoinclip
    {
        switch (StartReload) //checks if we are starting the reload or in the process of reloading
        {
            case true: //will start the reload counter

                switch (AmmoType) 
                {
                    case "Light":
                        if (inventory.lightAmmo > 0)//checks if ammo in stock pile is empty or not to decide to even bother reloading
                        {
                            reloadTime = Time.time + ReloadSpeed;
                            isReloading = true;
                        }
                        break;

                    case "Medium":
                        if (inventory.mediumAmmo > 0)
                        {
                            reloadTime = Time.time + ReloadSpeed;
                            isReloading = true;
                        }
                        break;

                    case "Heavy":
                        if (inventory.HeavyAmmo > 0)
                        {
                            reloadTime = Time.time + ReloadSpeed;
                            isReloading = true;
                        }
                        break;
                }
                
                break;

            case false:

                switch (AmmoType)
                {
                    default:
                        Debug.Log("couldn't fine ammo type"); //debugging incase of misspell ammotype
                        break;

                    case ("Light"): //takes from light ammo inventory

                        if (reloadTime <= Time.time) //checks if reload time is over
                        {
                            inventory.lightAmmo = AmmoInClip + inventory.lightAmmo; // takes ammo out of the clip and puts it in stockpile
                            AmmoInClip = 0; //empties clip

                            if (inventory.lightAmmo <= MagSize) //if ammo stockpile is smaller or equal to mag size
                            {
                                AmmoInClip = inventory.lightAmmo; //ammo in clip will take all remaining ammo from stockpile
                                inventory.lightAmmo = 0; //empties the stockpile
                            }

                            else //we have enough ammo in stockpile to fill mag
                            {
                                AmmoInClip = MagSize; //fills mag
                                inventory.lightAmmo = inventory.lightAmmo - MagSize; //takes ammo away from stockpile
                            }

                            ItemPrefab.GetComponent<ItemInfo>().AmmoInClip = AmmoInClip; //updates the actual item stats so while scrolling or dropping item ammo isn't reset

                            //updates the ingame ammo UI
                            ingameUI.UpdateAmmoUI(inventory.lightAmmo, inventory.mediumAmmo, inventory.HeavyAmmo);
                            ingameUI.UpdateEquipedAmmoUI(AmmoType, AmmoInClip);

                            reloadTime = 0;
                            isReloading = false;
                        }

                        break;

                    case ("Medium"):  //takes from medium ammo inventory same as above ^

                        if (reloadTime <= Time.time)
                        {
                            inventory.mediumAmmo = AmmoInClip + inventory.mediumAmmo;
                            AmmoInClip = 0;

                            if (inventory.mediumAmmo <= MagSize)
                            {
                                AmmoInClip = inventory.mediumAmmo;
                                inventory.mediumAmmo = 0;
                            }

                            else
                            {
                                AmmoInClip = MagSize;
                                inventory.mediumAmmo = inventory.mediumAmmo - MagSize;
                            }

                            ItemPrefab.GetComponent<ItemInfo>().AmmoInClip = AmmoInClip;

                            ingameUI.UpdateAmmoUI(inventory.lightAmmo, inventory.mediumAmmo, inventory.HeavyAmmo);
                            ingameUI.UpdateEquipedAmmoUI(AmmoType, AmmoInClip);

                            reloadTime = 0;
                            isReloading = false;
                        }

                        break;

                    case ("Heavy"): //takes from heavy ammo inventory same as above ^

                        if (reloadTime <= Time.time)
                        {
                            inventory.HeavyAmmo = AmmoInClip + inventory.HeavyAmmo;
                            AmmoInClip = 0;

                            if (inventory.HeavyAmmo <= MagSize)
                            {
                                AmmoInClip = inventory.HeavyAmmo;
                                inventory.HeavyAmmo = 0;
                            }

                            else
                            {
                                AmmoInClip = MagSize;
                                inventory.HeavyAmmo = inventory.HeavyAmmo - MagSize;
                            }

                            ItemPrefab.GetComponent<ItemInfo>().AmmoInClip = AmmoInClip;

                            ingameUI.UpdateAmmoUI(inventory.lightAmmo, inventory.mediumAmmo, inventory.HeavyAmmo);
                            ingameUI.UpdateEquipedAmmoUI(AmmoType, AmmoInClip);

                            reloadTime = 0;
                            isReloading = false;
                        }
                        break;
                }
                break;
        }
    }
}