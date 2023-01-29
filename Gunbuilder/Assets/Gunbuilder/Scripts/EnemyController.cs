using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public GameObject HPBarPrefab;
    GameObject HPBar;
    public GameObject HPSetPos;
    public float HpUISize;
    public GameObject CamPosGO;
    Slider HPSlider;
    public EnemyStats enemyStats;
    public bool isDead;

    public float userDistance;
    Vector3 playerPos;
    Vector3 hpBarPos;

    public float HideBarTime;
    public float nextHideBarTime;

    // Update is called once per frame
    void Start()
    {
        CreateHPBar(); //creates the HP bar and defines the required variables
        HPBar.SetActive(false);
    }

    void Update()
    {
        UpdateHPBarTrans(); //moves and scales it based on where player is

        if (nextHideBarTime <= Time.time && HPBar.activeSelf == true) //hides the hp bar once the time is reached
            HPBar.SetActive(false);

        if(isDead)
            Death(); //kills the enemy
    }

    public void Death() //enemy dies
    {
        //ded
    }

    void CreateHPBar() 
    {
        HPBar = Instantiate(HPBarPrefab); //creates hp bar using prefab

        HPSlider = HPBar.GetComponent<Slider>(); //defines slider

        HPBar.transform.position = HPSetPos.transform.position;

        HPSlider.maxValue = enemyStats.HP; //sets slider values to enemy HP value
        HPBar.transform.localScale = new Vector3(HpUISize, HpUISize);
    }

    void UpdateHPBarTrans() 
    {
        HPBar.transform.LookAt(CamPosGO.transform.position); //turns HP bar to look at camera (has a strange look, doesn't actually look like a UI element but works for now)

        HPBar.transform.position = HPSetPos.transform.position;

        playerPos = CamPosGO.transform.position;
        hpBarPos = HPBar.transform.position;

        userDistance = Vector3.Distance(hpBarPos, playerPos); //finds the distance the user is from the UI

        HPBar.transform.localScale = new Vector3(HpUISize * userDistance,HpUISize * userDistance); //Scales the HPbar based on how far the user is, keeps it the same size on users screen regardless of distance
    }

    public void Hit(float damageTaken) //updates enemy stats and hp bar
    {
        nextHideBarTime = Time.time + HideBarTime; //states when the hp bar should hide

        if (HPBar.activeSelf == false) //shows hp bar if not already shown
        {
            HPBar.SetActive(true);
        }

        enemyStats.HP -= damageTaken; //updates enemy HP stat
        HPSlider.value = enemyStats.HP; //updates enemy HP bar
    }
}
    