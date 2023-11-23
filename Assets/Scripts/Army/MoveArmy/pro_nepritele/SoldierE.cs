using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoldierE : MonoBehaviour
{
	SoldierP soldierPscript;									//import scriptu protivnika
	[SerializeField] GameObject soldierP;						//import objektu
	BaseScriptP basePscript;									//import script 
	GameObject item;											//import objektu

	public Rigidbody2D rb;										//funkce pro gravitaci
	public LayerMask[] armyTypes = new LayerMask[3];
	//public LayerMask[] armyTypesE = new LayerMask[3];
	public float[] ranges = { 0.5f, 1.4f, 0.5f };
	public float movespeed;										//rychlost pohybu objektu
	public LayerMask armyType;
	public int armyTypeNum;

	//Ohledne HPbaru
	public GameObject hpBar;

	public float[,] maxhp = { { 100, 60, 300 }, { 150, 90, 450 }, { 225, 135, 675 }, { 350, 200, 1000 }, { 400, 300, 1500 } };              //potrebuje sledovani !!!!!!!!!!!!!!!!!!!!!!!!*******
	public float currhp;
	private float hpinprocents = 1f;
	public int level = 0;                                                                                                   //potrebuje sledovani !!!!!!!!!!!!!!!!!!!!!!!!*******

	//Ohledne utoku
	public int[,] dmg = { { 40, 60, 30 }, { 60, 90, 50 }, { 90, 135, 70 }, { 135, 90, 115 }, { 150, 200, 120 } };           //potrebuje sledovani !!!!!!!!!!!!!!!!!!!!!!!!*******
	public bool canGetdmgM = true;      //na blizko
	public bool canGetdmgR = true;      //na dalku
	public bool[] enemies = { false, false, false };
	private bool givemoney = true;								//cooldown na penize

	// Start is called before the first frame update
	void Start()
	{
		soldierPscript = soldierP.GetComponent<SoldierP>();  //import protivnika a jeho proměnných
		//
		GameObject item = GameObject.FindWithTag("baseP");				//toto najde zakladnu hrace pomoci tagu ktery ma
		basePscript = item.GetComponent<BaseScriptP>();
		//
		level = basePscript.level;                              //potrebuje sledovani !!!!!!!!!!!!!!!!!!!!!!!!*******
		for (int i = 0; i < 3; i++)
		{
			if (armyType == armyTypes[i])
			{
				//maxhp = hptype[i];
				currhp = maxhp[level,i];                        //potrebuje sledovani !!!!!!!!!!!!!!!!!!!!!!!!*******
				armyTypeNum = i;
			}
			//Debug.Log(i);
		}
	}
	// Update is called once per frame
	void Update()
	{
		hpinprocents = ((100 * currhp) / maxhp[level,armyTypeNum]) / 100;                                                   //potrebuje sledovani !!!!!!!!!!!!!!!!!!!!!!!!*******
		rb.velocity = new Vector2((movespeed * -1), rb.velocity.y);   //bude se hybyt do leva zatim je to testovaci
		CheckEnemy();
		for (int i = 0; i < 3; i++)
		{
			if(enemies[i] == true)
			{
                if (currhp <= 0 && givemoney == true)
				{
					Death();									//funkce kdyz zemre
				}
				else if (currhp > 0)
				{
					if((i == 0 || i == 2) && canGetdmgM == true)
					{
						StartCoroutine(DmgdealcooldownMelee());
					}
					else if(i == 1 && canGetdmgR == true)
					{
						StartCoroutine(DmgdealcooldownRange());
					}
				}
			}
			else if (currhp <= 0 && givemoney == true)
			{
				Death();										//pojistna funkce jestli je mrtvy
			}
		}
		//pojistka proto kdyz zabije hracovu jednotku drive a nema okolo sebe nikoho jineho a ma 0 hp a nebo mene
		hpBar.transform.localScale = new Vector2(hpinprocents, hpBar.transform.localScale.y);
	}

	public void CheckEnemy()
	{
		for (int i = 0; i < 3; i++)
		{
			enemies[i] = Physics2D.OverlapCircle(transform.position, soldierPscript.ranges[i], soldierPscript.armyTypes[i]) != null;
		}
	}
	public void Death()											//funkce pro zjisteni zda je objekt mrtvy a da penize a zkusenosti za jeho smrt
	{
		givemoney = false;
		basePscript.money += soldierPscript.moneykill[level, armyTypeNum];                                      //zatim to dava penez tolik kdo ho zabil coz je spatne     potreba to dostat do UI z prefabu
		basePscript.experience += soldierPscript.expperkill[armyTypeNum];                                       //zatim to dava penez tolik kdo ho zabil coz je spatne     potreba to dostat do UI z prefabu
		Debug.Log(soldierPscript.moneykill[level, armyTypeNum]);                                                                           //potrebuje sledovani !!!!!!!!!!!!!!!!!!!!!!!!*******
		Debug.Log(soldierPscript.expperkill[armyTypeNum]);                                                                           //potrebuje sledovani !!!!!!!!!!!!!!!!!!!!!!!!*******
		Destroy(gameObject);
	}

	IEnumerator DmgdealcooldownMelee()
	{
		canGetdmgM = false;
		if (enemies[0] == true)
		{
			currhp -= soldierPscript.dmg[soldierPscript.level,0];                                                           //potrebuje sledovani !!!!!!!!!!!!!!!!!!!!!!!!*******
		}
		else if (enemies[2])
		{
			currhp -= soldierPscript.dmg[soldierPscript.level,2];                                                           //potrebuje sledovani !!!!!!!!!!!!!!!!!!!!!!!!*******
		}
		//Debug.Log("Enemy " + currhp);
		yield return new WaitForSecondsRealtime(3);
		canGetdmgM = true;
	}
	IEnumerator DmgdealcooldownRange()
	{
		canGetdmgR = false;
		currhp -= soldierPscript.dmg[soldierPscript.level, 1];                                                              //potrebuje sledovani !!!!!!!!!!!!!!!!!!!!!!!!*******
		//Debug.Log("Enemy " + currhp);
		yield return new WaitForSecondsRealtime(2);
		canGetdmgR = true;
	}
}
