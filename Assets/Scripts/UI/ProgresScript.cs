using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgresScript : MonoBehaviour
{
	ButtonScript buttonS;
	HpScript hpS;

	ArmyScript army;                                               //importovani pro pracovani s vojacky
	public GameObject objectArmyP;								//objekt pro propojeni scriptu

	//funkce zakladny ukazuje zkusenosti(%), pocet penez, co se vyrabi, co je ve fronte
	public int experience = 0;									//zkusenosti
	public int experienceinprocents = 0;						//zkusenosti
	public int nextlevelup = 4000;                              //pokud dosahne tolika zkusenosti tak se evolvuje			//potrebuje i prenastavit v unity!!
	public int level = 0;                                       //toto ukazuje level evoluce v zakladu je to 0
	public int money = 175;                                     //penize			
	public int order = 0;                                       //kolik jich vyrabime   //ud�lat poud�ji jako array, protoze bude vyrabet vice jednotek
	public int made = 0;
	public int[] orderv2 = { 0, 0, 0, 0, 0 };                   //poradi jednotek
	public GameObject[] baseAppearance = new GameObject[4];     //vzhled budov v array ohledne nove evoluce

	public Text experienceText;                                 //Prehled ohledne dalsi evoluce v %
	public Text moneyText;                                      //Prehled kolik ma hrac financi

	public GameObject playerSpawner;                            //misto kde se tyto objekty spawnou


	//vyrobnik v procentech graficky
	public Image progBar;
	public int[,] moneyperunit = { { 15, 25, 100 }, { 30, 50, 200 }, { 60, 100, 400 }, { 120, 200, 800 }, { 240, 400, 1600 } };		//v�cerozm�rn� pole pro cenu jednotek	//potrebuje upravu
	private int[] waitTime = { 5, 8, 10 };                      //vyroba soldiera, rangera, tanka
	public float progbarfill = 0f;								//kolik bude vyplnovat v progbaru
	//public float timer = 0;
	public bool canProduce = true;                              //zda muze vyrabet
	public Text[] actionButtonText = new Text[6];               //upraveni textu u buttonu soldier, ranger, tank		//pozdeji budou jeste dalsi dva na dobrovolny update evoluce a pro pohromy

	public Image[] orderVizual = new Image[5];

	public Text trainText;                                      //tento text se bude prepisovat podle toho co se vyrabi
																//

	//Katastrofa
	public GameObject fireBall;
	public GameObject borderL;
    public GameObject borderR;
    public GameObject disasterZone;
	private float waitDisasterFill = 0f;
	private int waitingTimeForDisaster = 40;
	public Image waitDisasterFillBox;
    public bool canDoDisaster = true;

    // Start is called before the first frame update
    void Start()
	{
        buttonS = GetComponent<ButtonScript>();					//propojeni zakladnich scriptu pro funkci UI
		hpS = GetComponent<HpScript>();							//propojeni zakladnich scriptu pro funkci UI

		army = objectArmyP.GetComponent<ArmyScript>();				//propojeni scriptu UniArmy s ProgresScript
																//nastaveni aktualnich penez
		moneyText.text = money.ToString();
		experienceText.text = experienceinprocents.ToString() + "%";
		TrainingText();                         //zapise se co se vyrabi

        for (int i = 0; i < 3; i++)								//na zacatku se definuje co tam bude na tom buttonu
        {
            actionButtonText[i].text = "lvl." + (level + 1);
            actionButtonText[i + (actionButtonText.Length) / 2].text = "Cost " + moneyperunit[level, i] + " $";
        }
    }

	// Update is called once per frame
	void Update()
	{
		OrderView();											//graficke videni fronty
		experienceinprocents = ((100 * experience) / nextlevelup);				//vytvori proceznta ze zkusenosti
		moneyText.text = money.ToString();						//opakovatelne se budou vpisovat penize do textu
		Evolution();							//funkce pro vylepsovani urovne doby
		if (order > 0 && !LogScript.isGameOver)					//zacne se produkce jakmile bude neco v rade a taky se zacne hybat progbar
		{
			//StartCoroutine(Orderfactory());
			OrderFactory();
		}
		if (!canDoDisaster && !LogScript.isGameOver)
		{
			WaitDisaster();
        }
	}
	/*IEnumerator Orderfactory()                                  //bude vyrabet jednoho 5s           //pak udelat na if (aby se menil ten vyrobni cas)              // pozdeji udelat smooth
	{
		if (order > 0 && progbarinprocents != 1f && canProduce == true)
		{
			canProduce = false;
			timer += 1;                                         //je to trosicku opozdene, ale nevadi
			StartCoroutine(TrainingText());                     //zacne se psat co se vyrabi
			progbarinprocents = (timer / waitTime[orderv2[0] - 1]);        //podle toho se urci co se bude vyrabet a jak dlouho pomoci arraye
			//progBar.fillAmount = progbarinprocents;
			yield return new WaitForSecondsRealtime(1);
			progBar.fillAmount = Mathf.Lerp(progBar.fillAmount, progbarinprocents, progbarinprocents);
			//yield return new WaitForSecondsRealtime(1);
			canProduce = true;
			if (progbarinprocents == 1f)
			{
				made += 1;
				timer = 0;
				progbarinprocents = 0f;
				if (orderv2[0] == 1)
				{
					Instantiate(baseS.soldierP, baseS.playerSpawner.transform.position, baseS.playerSpawner.transform.rotation);
					Debug.Log("Byl vyroben Soldier");
				}
				else if (orderv2[0] == 2)
				{
					Instantiate(baseS.rangerP, baseS.playerSpawner.transform.position, baseS.playerSpawner.transform.rotation);
				}
				else if (orderv2[0] == 3)
				{
					Instantiate(baseS.tankP, baseS.playerSpawner.transform.position, baseS.playerSpawner.transform.rotation);
				}
				Debug.Log("Byl vyroben " + order);
				order -= 1;
				if (order >= 0)
				{
					StartCoroutine(OrderSorter());
				}
			}
		}
		if (order == 0)
		{
			timer = 0;
			progbarinprocents = ((100 * timer) / waitTime[orderv2[0]]) / 100;
			progBar.fillAmount = progbarinprocents;
			StartCoroutine(TrainingText());                     //zapise se viditelne ze se nic nevyrabi
		}
	}*/
	private void OrderFactory()									//potrebuje celkove sledovani **********
	{
		if (order > 0 && progBar.fillAmount != 1f && canProduce == true)
		{
			canProduce = false;
			//timer += 1;										//je to trosicku opozdene, ale nevadi
			TrainingText();                     //zacne se psat co se vyrabi
			progbarfill = (Time.deltaTime / waitTime[orderv2[0] - 1]);        //podle toho se urci co se bude vyrabet a jak dlouho pomoci arraye
			//Debug.Log(progbarfill);
			//progBar.fillAmount = progbarinprocents;
			//yield return new WaitForSecondsRealtime(1);
			progBar.fillAmount = Mathf.Lerp(progBar.fillAmount, progBar.fillAmount + 1f, progbarfill);      //min, max, speed
																											//yield return new WaitForSecondsRealtime(1);
            canProduce = true;
			if (progBar.fillAmount >= 1f)
			{
				made += 1;
				//timer = 0;
				progBar.fillAmount = 0f;
				for (int unitType = 1; unitType <= army.armyTypeLayer.Length; unitType++)
				{
					if (orderv2[0] == unitType)
					{
                        army.armyType = army.armyTypeLayer[orderv2[0]-1];
                        Instantiate(buttonS.soldierP, playerSpawner.transform.position, playerSpawner.transform.rotation);
                        Debug.Log("Byl vyroben " + army.armyTypeNum);
                    }
				}
				/*if (orderv2[0] == 1)
				{
					army.armyType = army.armyTypeLayer[orderv2[0]];
					Instantiate(buttonS.soldierP, playerSpawner.transform.position, playerSpawner.transform.rotation);
					Debug.Log("Byl vyroben Soldier");
				}
				else if (orderv2[0] == 2)
				{
					army.armyType = army.ranger;
					Instantiate(buttonS.soldierP, playerSpawner.transform.position, playerSpawner.transform.rotation);
				}
				else if (orderv2[0] == 3)
				{
					army.armyType = army.tank;
					Instantiate(buttonS.soldierP, playerSpawner.transform.position, playerSpawner.transform.rotation);
				}*/
				//Debug.Log("Byl vyroben " + order);
				order -= 1;
				if (order >= 0)
				{
					OrderSorter();
				}
			}
		}
		if (order == 0)
		{
			//timer = 0;
			//progbarinprocents = ((100 * timer) / waitTime[orderv2[0]]) / 100;
			progbarfill = 0;
			progBar.fillAmount = progbarfill;
			TrainingText();                     //zapise se viditelne ze se nic nevyrabi
		}
	}
	private void OrderView()                                    //toto zajistuje vizualni frontu vyroby jednotek
	{
		for (int i = 0; i < 5; i++)
		{
			if (orderv2[i] == 0)
			{
				orderVizual[i].fillAmount = 0;
			}
			else
			{
				orderVizual[i].fillAmount = 1;
			}
		}
	}
	void OrderSorter()                                   //toto serazuje array podle toho co je na rade ve vyrobe
	{
		for (int i = 0; i < 5; i++)
		{
			if (i != 4)
			{
				orderv2[i] = orderv2[i + 1];
			}
			else
			{
				orderv2[i] = 0;
			}
		}
		//yield return order;
	}
	void TrainingText()                                  //slouzi proto, aby clovek videl co se prave vyrabi
	{
		string[] trainingTextWrite = { "Nothing...", "Training Soldier...", "Training Ranger...", "Training Tank..." };
		for (int i = 0; i < 4; i++)								//vepisuje se text do boxu co se prave vyrabi
		{
			if (orderv2[0] == i)
			{
				trainText.text = trainingTextWrite[i];
			}
		}
		//yield return new WaitForSeconds(0);
	}
	void Evolution()										//docasne dokud neni button tak se to evolvuje automaticky
	{
		if (experience >= nextlevelup && level != 4)			//pokud jeho level neni roven 4 coz je nejvysi uroven tak se muze vylepsit
		{
			experience -= nextlevelup;
			level += 1;
			for (int i = 0; i < actionButtonText.Length/2; i++)                         //pise do vsech textu, ktere jsou uchovany v poli				**mozna pak chyba bude tady
			{
				actionButtonText[i].text = "lvl." + (level + 1);
				actionButtonText[i+(actionButtonText.Length)/2].text = "Cost " + moneyperunit[level, i] + " $";
			}
			for (int i = 0; i < baseAppearance.Length; i++)						//zde se zmeni vzhled zakladny
			{
				if (level == i)
				{
					baseAppearance[i].SetActive(true);
				}
				else
				{
					baseAppearance[i].SetActive(false);
				}
			}
			hpS.UpgradeHp();					//pro vylepseni zivotu s tim, ze se zachova %
		}
		else if(level == 4)
		{
			experienceText.text = "Max";
		}
		else
		{
			experienceText.text = experienceinprocents.ToString() + "%";
		}
		//yield return experience;
	}
    public IEnumerator SpawnFireBall()
    {
		waitDisasterFill = 1f;
        waitDisasterFillBox.fillAmount = waitDisasterFill;
        WaitDisaster();
        int i = 0;
        while (i <= 30)
        {
            float randomPosX = Random.Range(borderL.transform.position.x, borderR.transform.position.x);								//
			float randomRotZ = Random.Range(0, 360);																					//
            Quaternion changedRotationZ = Quaternion.Euler(disasterZone.transform.rotation.x, 0, randomRotZ);							//
            Vector3 disasterZonePos = new Vector3(randomPosX, disasterZone.transform.position.y, fireBall.transform.position.z);		//
            Instantiate(fireBall, disasterZonePos, changedRotationZ);
            yield return new WaitForSeconds(0.4f);
            i += 1;
        }
    }

	public void WaitDisaster()
	{
        if (waitDisasterFillBox.fillAmount > 0)
		{
            waitDisasterFill = (Time.deltaTime / waitingTimeForDisaster);				//definice rychlosti klesani (cas snimku/celkova doba cekani)
            waitDisasterFillBox.fillAmount = Mathf.Lerp(waitDisasterFillBox.fillAmount, waitDisasterFillBox.fillAmount -1f, waitDisasterFill);		//min, max, speed
        }
		else													//pokud je to rovno nule ci mensi 
		{
			waitDisasterFill = 0;
			waitDisasterFillBox.fillAmount = waitDisasterFill;
			canDoDisaster = true;
        }
    }
}
