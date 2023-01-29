using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    public GameObject EscapeMenu;
    public GameObject GameplayUI;
    public GameObject OptionsMenu;
    public GameObject PickupItemPrompt;
    public PlayerController playerController;
    public GameObject[] InventorySlots;
    public Inventory playerInventory;
    public float SlotSelectScale;
    public float SlotDeselectScale;
    public WeaponController weaponController;
    public GameObject EquipedAmmoUI;
    public GameObject LightAmmoUI;
    public GameObject MediumAmmoUI;
    public GameObject HeavyAmmoUI;

    public bool MenuEnabled;
    bool optionsMenuEnabled;
    int slotScrollTo;
    public GameObject selectedSlot;
    public GameObject previousSelectedSlot;

    //called at once script is awake
    void Start()
    {
        InventoryScrolling(0); //defaults the selected slot to the first one on boot

        if (selectedSlot.GetComponent<SlotInfo>().Full == false) //checks if first slot is empty on boot
        {
            UpdateEquipedAmmoUI("EmptyHands",0); //updates equiped ammo UI
        }

        else
            UpdateEquipedAmmoUI(selectedSlot.GetComponent<SlotInfo>().ItemPrefab.GetComponent<ItemInfo>().AmmoType, 
                selectedSlot.GetComponent<SlotInfo>().ItemPrefab.GetComponent<ItemInfo>().AmmoInClip); //updates equiped ammo UI with slot info if slot isn't empty
    }

    // Update is called once per frame
    void Update()
    {
        //use buttondown to as it's only called once button is pressed down
        if (Input.GetButtonDown("Escape") && MenuEnabled == false)
        {
            GameplayUI.SetActive(false); //disables in game UI
            PauseMenu(EscapeMenu, true, false, 0, CursorLockMode.None, true, true); //pauses game and enables pause menu
        }

        else if (Input.GetButtonDown("Escape") && MenuEnabled == true)
        {
            switch (optionsMenuEnabled)
            {
                case true: //switches back to the escape menu
                    EscapeMenu.SetActive(true);
                    OptionsMenu.SetActive(false);
                    optionsMenuEnabled = false;
                    break;

                case false:
                    PauseMenu(EscapeMenu, false, true, 1, CursorLockMode.Locked, false, false); //unpuases and disables pause menu if options menu isn't up
                    GameplayUI.SetActive(true); //enables in game UI
                    break;
            }
        }

        if (Input.mouseScrollDelta.y > 0f && MenuEnabled == false || Input.mouseScrollDelta.y < 0f && MenuEnabled == false) //checks if the mouse is being scrolled and menu isn't up;
        { 
            InventoryScrolling(Input.mouseScrollDelta.y);//scrolls to the next or previous slot;
            playerInventory.SelectedItemShow(selectedSlot.GetComponent<SlotInfo>(), previousSelectedSlot.GetComponent<SlotInfo>(), "Scroll"); //puts the item in the users hands
            weaponController.GetStats(selectedSlot.GetComponent<SlotInfo>()); //sets the weapons stats in the controller

            if (weaponController.isReloading == true) 
            {
                weaponController.reloadTime = 0;
                weaponController.isReloading = false;
            }
        }
    }

    void PauseMenu(GameObject defineGO, bool menuGOActive, bool movementActive, float paused, CursorLockMode cursorLock, bool cursorVis, bool isMenuOn)
    {
        defineGO.SetActive(menuGOActive); //opens or closes menu
        playerController.enabled = movementActive; //enable or disable player movement
        Cursor.lockState = cursorLock; //locks or unlocks the cursor
        Cursor.visible = cursorVis; //hides or shows cursor
        Time.timeScale = paused; //not ideal for flexability but is good enough for now, setting to 0 acts like a pause by stopping time and vise versa
        MenuEnabled = isMenuOn; //defines if the menu is on or not
    }

    public void onClick(string tag) //used for UI buttons, can pass through a string to define what we want it do
    {
        switch (tag)
        {
            case "Options":
                optionsMenuEnabled = true; //defines if options are on or not
                break;

            case "OptionsBack": //will be called if player clicks the back button to stop small esc twice bug
                optionsMenuEnabled = false; //goofy way of doing this but whatever it works (there is probs a much better way but this is just ez and less brain rot)
                break;

            case "ResumeGame": //resumes the game
                PauseMenu(EscapeMenu, false, true, 1, CursorLockMode.Locked, false, false);
                GameplayUI.SetActive(true);
                break;
        }
    }

    void InventoryScrolling(float scrollDirction)
    {
        bool downScalePrevSlot = true; //to downscale the slot
        int prevSelectedSlot = slotScrollTo; //declares the previous selected slot, this is so we know where in the slot array we need to reset slots pos from

        previousSelectedSlot = InventorySlots[slotScrollTo]; //to keep track what slot last selected (is used for hiding previous item that was selcted)

        switch (scrollDirction) //gets the scroll dirction from the mouse scroll delta Y and picks one of the cases based of its value.
        {
            case -1: //scrolls up

                slotScrollTo++;
                if (slotScrollTo >= 5)
                    slotScrollTo = 0; //goes back to 0 once outside of the array (hard coded can maybe make flexible but im not thinking about increasing inventory size atm)
                break;

            case 1: //scrolls down
                slotScrollTo--;
                if (slotScrollTo <= -1)
                    slotScrollTo = 4; // goes to the last slot if reached the end of the slot array
                break;

            case 0: //default slot for boot
                slotScrollTo = 0;
                downScalePrevSlot = false; // skips downscaling prev slot as there is no prev selected slot
                break;
        }

        //chooses to downscale previous slot or not
        switch (downScalePrevSlot)
        {
            case false: //will just skip downscaling
                break;

            default: //the default is to downscale slots and move them back 
                Vector2 prevSlotOldHW = selectedSlot.GetComponent<RectTransform>().sizeDelta; //declares previous slots height and width
                Vector2 prevSlotPosOffset = new Vector2(((prevSlotOldHW.x * SlotSelectScale) - prevSlotOldHW.x) / 2f, ((prevSlotOldHW.y * SlotSelectScale) - prevSlotOldHW.y) / 2f); // declares old slots offset

                selectedSlot.transform.localScale = new Vector3(SlotDeselectScale, SlotDeselectScale); // resets selected slots scale
                selectedSlot.transform.localPosition = new Vector3(selectedSlot.transform.localPosition.x - prevSlotPosOffset.x, selectedSlot.transform.localPosition.y - prevSlotPosOffset.y); //resets selected slots pos

                for (int i = prevSelectedSlot + 1; i < InventorySlots.Length; i++) //removes the offset we added to the slots to move them over to the right of the scaled up slot, essencially resetting the slots possision ready for moving again.
                {
                    InventorySlots[i].transform.localPosition = new Vector3(InventorySlots[i].transform.localPosition.x - (prevSlotPosOffset.x * 2), InventorySlots[i].transform.localPosition.y); //move slots to the left
                }

                break;
        }

        selectedSlot = InventorySlots[slotScrollTo]; //declares what slot we have selected

        Vector2 slotOldHW = selectedSlot.GetComponent<RectTransform>().sizeDelta;
        Vector2 slotPosOffset = new Vector2(((slotOldHW.x * SlotSelectScale) - slotOldHW.x) / 2f, ((slotOldHW.y * SlotSelectScale) - slotOldHW.y) / 2f); //finds the offset we need to add to the slots when the selected slot is scaled up

        selectedSlot.transform.localScale = new Vector3(SlotSelectScale, SlotSelectScale); //scales the selected slot up
        selectedSlot.transform.localPosition = new Vector3(selectedSlot.transform.localPosition.x + slotPosOffset.x, selectedSlot.transform.localPosition.y + slotPosOffset.y); //moves the selected slot up and to the right using the offset

        for (int i = slotScrollTo + 1; i < InventorySlots.Length; i++) //adds the offset to each of the slots to the right of the selected slot, we *2 as we're not moving half the offset and instead the whole offset the scale creates
        {
            InventorySlots[i].transform.localPosition = new Vector3(InventorySlots[i].transform.localPosition.x + (slotPosOffset.x * 2), InventorySlots[i].transform.localPosition.y); //moves slots to the right
        }
    }

    public void DisplayPickupPrompt(bool displayPrompt,string itemName)
    {
        switch (displayPrompt) 
        {
            case true:
                PickupItemPrompt.GetComponent<TextMeshProUGUI>().text = new string($"'E' to pick up {itemName}"); //changes the text on screen to the name of the item we're looking at
                PickupItemPrompt.SetActive(true); //turns on the prompt
                break;

            default:
                PickupItemPrompt.SetActive(false); //turns off the prompt
                break;
        }
    }

    public void UpdateAmmoUI(int lightAmmo, int mediumAmmo, int heavyAmmo) //updates the ammo UI //may want to change string if i end up using images to show light, medium, heavy 
    {
        LightAmmoUI.GetComponent<TextMeshProUGUI>().text = new string($"Light: {lightAmmo}");
        MediumAmmoUI.GetComponent<TextMeshProUGUI>().text = new string($"Medium: {mediumAmmo}");
        HeavyAmmoUI.GetComponent<TextMeshProUGUI>().text = new string($"Heavy: {heavyAmmo}");
    }

    public void UpdateEquipedAmmoUI(string ammoType, int ammoInClip) //updates equiped ammo UI
    {
        switch (ammoType) 
        {
            case("Light"):
                EquipedAmmoUI.GetComponent<TextMeshProUGUI>().text = new string($"{ammoInClip} / {playerInventory.lightAmmo}");
                break;

            case ("Medium"):
                EquipedAmmoUI.GetComponent<TextMeshProUGUI>().text = new string($"{ammoInClip} / {playerInventory.mediumAmmo}");
                break;

            case ("Heavy"):
                EquipedAmmoUI.GetComponent<TextMeshProUGUI>().text = new string($"{ammoInClip} / {playerInventory.HeavyAmmo}");
                break;

            case ("Consumable"):
                EquipedAmmoUI.GetComponent<TextMeshProUGUI>().text = new string($"{ammoInClip}"); //shows consumable charges
                break;

            case ("EmptyHands"):
                EquipedAmmoUI.GetComponent<TextMeshProUGUI>().text = new string($""); //hides ammo UI if empty hands
                break;

            default:
                Debug.Log("UpdateEquipedAmmo incorrect ammoType");
                break;
        }
    }
}