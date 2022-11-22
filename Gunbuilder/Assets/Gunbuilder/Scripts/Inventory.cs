using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public GameObject playerCameraHolder;
    public GameObject playerObj;
    public GameObject dropSpot;
    public float PickupDistance;
    public LayerMask interactableLayerMask;
    public InGameUI gameUI;
    public GameObject ItemSpot;
    public GameObject EmptyHands;

    public bool foundInteractable;
    RaycastHit itemHit;
    public GameObject worldGO; //want to change to just the room we're in will have to find a way to detect what room im in, will change later, for now good enough

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(playerCameraHolder.transform.position, playerCameraHolder.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * PickupDistance); //for debugging

        foundInteractable = Physics.Raycast(ray, out itemHit, PickupDistance, interactableLayerMask);

        switch (foundInteractable)
        {
            case true:
                if (gameUI.PickupItemPrompt.activeInHierarchy != true && itemHit.collider.GetComponent<ItemInfo>().PickUpable == true) //checks if display prompt is not up and we're looking at a pickkupable item
                    ItemPickupPrompt("PickUpItem"); //turns on pickup prompt

                if (Input.GetKeyDown(KeyCode.E) && itemHit.collider.GetComponent<ItemInfo>().PickUpable == true) //checks if we're looking at item and user pressed E
                    PickupItem(gameUI.selectedSlot.GetComponent<SlotInfo>(), itemHit.collider.GetComponent<ItemInfo>(), itemHit.collider.gameObject);
                break;

            case false:
                if (gameUI.PickupItemPrompt.activeInHierarchy == true) //checks if display prompt is up and we're not looking at item
                    ItemPickupPrompt("");
                break;
        }

        if (Input.GetKeyDown(KeyCode.G) && gameUI.selectedSlot.GetComponent<SlotInfo>().Full == true) // will only drop item if G is pressed and the selected slot is full
            DropItem(gameUI.selectedSlot.GetComponent<SlotInfo>());
    }
    void ItemPickupPrompt(string tag) 
    {
        switch (tag)
        {
            default:
                gameUI.DisplayPickupPrompt(false, ""); //turns off pickup prompt and sends empty item name (dont know how to make optional class varibles)
                break;

            case "PickUpItem":
                    gameUI.DisplayPickupPrompt(true, itemHit.collider.GetComponent<ItemInfo>().ItemName); //turns on pick up prompt and provides the items name
                break;
        }
    }
    void PickupItem(SlotInfo slotinfo, ItemInfo iteminfo, GameObject itemObject) //transferes all the item info into the slot and deacivates it
    {
        if (slotinfo.Full == false)
        {
            slotinfo.ItemTag = iteminfo.ItemTag;
            slotinfo.ItemName = iteminfo.ItemName;
            slotinfo.SetSlotImg(iteminfo.SlotImg); //updates the slot image
            slotinfo.ItemPrefab = itemObject;

            itemObject.transform.parent = playerObj.transform; //set as parent as later i want to delete previous rooms/worlds as the user progresses like in ror2
            iteminfo.ItemPrefab.SetActive(false);

            slotinfo.Full = true; // sets the slot ot full so no items can go in that slot

            SelectedItemShow(slotinfo, null, "Pickup");
        }
    }
    void DropItem(SlotInfo slotinfo) // drops the item from the selected slot
    {
        SelectedItemShow(slotinfo, null, "Drop"); //we want to remove from the itemslot and enable rigidbody stuff before we add any forces and so on

        slotinfo.ItemPrefab.transform.parent = worldGO.transform; //sets the item back to the child of the world

        Ray ray = new Ray(playerCameraHolder.transform.position, playerCameraHolder.transform.forward); // draw a ray from the users look direction

        slotinfo.ItemPrefab.transform.position = ray.origin + ray.direction; // place the item in the direction the user is looking

        slotinfo.ItemPrefab.SetActive(true); //re enable the item

        slotinfo.ItemPrefab.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0); // sets the velocity back to 0 just in case it was falling while invis 
        slotinfo.ItemPrefab.GetComponent<Rigidbody>().AddForce(ray.direction * 5, ForceMode.Impulse); // pushes tthe item in the direction the user is looking sort of throwing it

        slotinfo.ItemTag = null; // empties the slot ready for an item to be placed into
        slotinfo.ItemName = null;
        slotinfo.SetSlotImg(slotinfo.EmptySlotImg); //updates the slot image to the empty slot
        slotinfo.ItemPrefab = null;
        slotinfo.Full = false;
    }
    public void SelectedItemShow(SlotInfo slotInfo, SlotInfo previousSlotInfo, string tag) //there is probs a better or more optimised way to do this but for now this is good enough
    {
        GameObject equipedItem; //declares if item in itemspot is hands or the item in the slot

        switch (tag) 
        {
            case "Scroll":

                if (slotInfo.Full == true) //turns off rigidbody and collider things also sets thet object in the slot as the equpieditem
                {
                    equipedItem = slotInfo.ItemPrefab;
                    equipedItem.GetComponent<Rigidbody>().isKinematic = true;
                    equipedItem.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None; //having interpolation messes with obj position for some reason
                    equipedItem.GetComponent<Collider>().enabled = false;
                }

                else
                    equipedItem = EmptyHands; //sets as hands if the slot is empty

                if (previousSlotInfo.Full) 
                {
                    previousSlotInfo.ItemPrefab.SetActive(false);
                    previousSlotInfo.ItemPrefab.transform.parent = playerObj.transform;
                }

                else
                    EmptyHands.SetActive(false); //to compact code and save copy pasting

                SelectedItemShowMove(equipedItem, equipedItem.GetComponent<ItemInfo>(), slotInfo);
                break;

            case "Drop": //the slot emptying and item parent is all handled by DropItem() anyway so all we have to do here is enable and disable a few things
                equipedItem = slotInfo.ItemPrefab;
                equipedItem.GetComponent<Rigidbody>().isKinematic = false;//disables kinematic so obj reacts to forces
                equipedItem.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;//add interpolation again as it looks smoother
                equipedItem.GetComponent<Collider>().enabled = true; //enables collider
                EmptyHands.SetActive(true); //enables hands as we're holding nothing now
                break;

            case "Pickup": //gonna use this as we just update what we're holding since the slot we're selected has updated 
                equipedItem = slotInfo.ItemPrefab;

                equipedItem.GetComponent<Rigidbody>().isKinematic = true;
                equipedItem.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None; //having interpolation messes with obj position for some reason
                equipedItem.GetComponent<Collider>().enabled = false;
                EmptyHands.SetActive(false); //hides hands; //enables the item
                SelectedItemShowMove(equipedItem, equipedItem.GetComponent<ItemInfo>(), slotInfo); // to compact code and save copy pasting
                break;
        }
    }
    void SelectedItemShowMove(GameObject equipedItem, ItemInfo iteminfo, SlotInfo slotinfo) // moves the item into the user's hands and applys any offset the item may need for viewing
    {
        if (slotinfo.Full == true)
        {
            equipedItem.transform.parent = ItemSpot.transform; //sets it as its parent so we dont have to keep updating its pos each frame
            equipedItem.transform.position = ItemSpot.transform.position; //sets its pos and rotation to item spot
            equipedItem.transform.rotation = ItemSpot.transform.rotation;

            equipedItem.transform.localPosition += iteminfo.ViewModelPosOffset;//sets viewmodel offest (we use local as position is relative to world pos not its own pos)

            Vector3 localrotationoffset = new Vector3(equipedItem.transform.localRotation.x, equipedItem.transform.localRotation.y, equipedItem.transform.localRotation.z) + iteminfo.ViewModelRotOffset; //have to do this weird work around for adding to a roation
            equipedItem.transform.localRotation = Quaternion.Euler(localrotationoffset); //sets viewmodel rotation offset

            equipedItem.SetActive(true); //shows the item passed through
        }

        else // show hands since its empty
            equipedItem.SetActive(true); //shows the item passed through
    }
}
