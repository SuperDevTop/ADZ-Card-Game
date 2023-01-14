using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameEngine : MonoBehaviour
{
    enum WhoseTurn
    {
        Me = 0,
        Opponent = 1
    }

    enum CardType
    {
        attack = 0,
        defence = 1
    }

    public GameObject mainUI;
    public GameObject dialog;
    public GameObject[] myCards;
    public GameObject[] oppCards;
    public GameObject[] myCardOrigin;
    public GameObject[] oppCardOrigin;
    public GameObject[] abyssCard;
    public GameObject[] pointCard;
    public GameObject[] pointCardOrigin;
    public Image meWaitingBar;

    public Sprite[] cardType;

    public Text mePoint;
    public Text oppPoint;
    public Text alertText;

    int meCurrent = 0;
    int oppCurrent = 0;
    int turnIndex = 0;
    int count = 20;
    float time = 0;

    bool isWarning;
    bool isFinishedTurn;
    bool isBlocked;
    bool isOppPushed;

    void Start()
    {
        isWarning = false;
        isFinishedTurn = false;
        isBlocked = false;
        isOppPushed = false;

        for (int i = 0; i < 5; i++)
        {
            RandomMeCard(i);
            RandomOppCardTest(i);
        }
    }
    
    void Update()
    {
        mainUI.transform.localScale = new Vector3(Screen.width / 1170f, Screen.height / 2532f, 1);

        if (isWarning)
        {
            time -= Time.deltaTime;
        }

        if (time < 0)
        {
            isWarning = false;
            alertText.gameObject.SetActive(false);
            dialog.SetActive(false);
            meCurrent = 0;
            time = 10f;
        }

        if (isFinishedTurn)
        { 
            if(meCurrent == 300)
            {
                if(oppCurrent < 0)
                {
                    mePoint.text = (int.Parse(mePoint.text) + oppCurrent + 600).ToString();
                    oppPoint.text = (int.Parse(oppPoint.text) - meCurrent).ToString();
                    AlertShow((oppCurrent + 600).ToString());
                }
                else if(oppCurrent > 0)
                {
                    mePoint.text = (int.Parse(mePoint.text) - (oppCurrent - meCurrent - 600)).ToString();
                    AlertShow((-oppCurrent + meCurrent + 600).ToString());
                }
            }
            else
            {
                if (meCurrent <= 0 && oppCurrent < 0)
                {
                    mePoint.text = (int.Parse(mePoint.text) + oppCurrent).ToString();
                    oppPoint.text = (int.Parse(oppPoint.text) + meCurrent).ToString();
                    AlertShow(oppCurrent.ToString());
                }
                else if (meCurrent > 0 && oppCurrent < 0)
                {
                    oppPoint.text = (int.Parse(oppPoint.text) - oppCurrent - meCurrent).ToString();
                    AlertShow((oppCurrent + meCurrent).ToString());
                }
                else if (meCurrent <= 0 && oppCurrent > 0)
                {
                    mePoint.text = (int.Parse(mePoint.text) - oppCurrent - meCurrent).ToString();
                    AlertShow((0 - oppCurrent - meCurrent).ToString());
                }                
                else if (meCurrent > 0 && oppCurrent > 0)
                {
                    AlertShow("0");
                }
            }                                    

            isFinishedTurn = false;  
        }

        if(turnIndex == (int)WhoseTurn.Opponent && !isOppPushed)
        {
            StartCoroutine(DelayToPushOpp(1.5f));
            isOppPushed = true;
        }
    }
    
    /// <summary>
    ////////////////////////////////////////////////////////////////////////////////////////////////
    /// Push Cards
    /// </summary>
    public void PushMyCard()
    {
        for (int i = 0; i < myCards.Length; i++)
        {
            if (Vector3.Distance(myCards[i].transform.position, abyssCard[0].transform.position) < 1f && myCards[i].active)
            {
                myCards[i].SetActive(false);
                myCards[i].transform.position = new Vector3(10000f, 10000f, 0);
            }
        }

        if (Vector3.Distance(pointCard[0].transform.position, abyssCard[0].transform.position) < 1f)
        {
            pointCard[0].SetActive(false);
            pointCard[0].GetComponent<Animator>().enabled = false;
            pointCard[0].transform.position = pointCardOrigin[0].transform.position;
            pointCard[0].SetActive(true);
        }
        
        EventSystem.current.currentSelectedGameObject.GetComponent<Animator>().enabled = true;
        string[] array = EventSystem.current.currentSelectedGameObject.name.Split("+");

        if (int.Parse(array[0]) == (int)CardType.attack)
        {
            meCurrent = int.Parse(array[1]) * (-1);
        }
        else if (int.Parse(array[0]) == (int)CardType.defence)
        {
            meCurrent = int.Parse(array[1]);
        }
 
        if(turnIndex == (int)WhoseTurn.Me)
        {
            StopAllCoroutines();
            StartCoroutine(DelayToPushOpp(1.5f));
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(DelayToCalculate());
        }
    }

    public void PushOppCard(int index)
    {
        for (int i = 0; i < oppCards.Length; i++)
        {
            if (Vector3.Distance(oppCards[i].transform.position, abyssCard[1].transform.position) < 1f && oppCards[i].active)
            {
                oppCards[i].SetActive(false);
                oppCards[i].transform.position = new Vector3(10000f, 10000f, 0);
            }
        }

        oppCards[index].GetComponent<Animator>().enabled = true;
        oppCardOrigin[index].SetActive(false);
        string[] array = oppCards[index].name.Split("+");

        if (int.Parse(array[0]) == (int)CardType.attack)
        {
            oppCurrent = int.Parse(array[1]) * (-1);
        }
        else if (int.Parse(array[0]) == (int)CardType.defence)
        {
            oppCurrent = int.Parse(array[1]);
        }

        if(turnIndex == (int)WhoseTurn.Me)
        {
            StartCoroutine(DelayToCalculate());
        }
        else
        {
            StartCoroutine(DelayToShowDialog());
            StartCoroutine(WaitingForMePush(6.5f));            
        }                
    }

    public void MePointCardPushed()
    {
        for (int i = 0; i < myCards.Length; i++)
        {
            if (Vector3.Distance(myCards[i].transform.position, abyssCard[0].transform.position) < 1f && myCards[i].active)
            {
                myCards[i].SetActive(false);
                myCards[i].transform.position = new Vector3(10000f, 10000f, 0);
            }
        }

        pointCard[0].GetComponent<Animator>().enabled = true;
        meCurrent = 300;

        if(turnIndex == (int)WhoseTurn.Me)
        {
            StopAllCoroutines();
            StartCoroutine(DelayToPushOpp(1.5f));
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(DelayToCalculate());
        }
    }

    public void MePointOriginCardPushed()
    {
        pointCard[0].SetActive(false);
        pointCard[0].transform.position = pointCardOrigin[0].transform.position;
        pointCard[0].SetActive(true);

        if (turnIndex == (int)WhoseTurn.Me)
        {
            StopAllCoroutines();
            StartCoroutine(DelayToPushOpp(1.5f));
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(DelayToCalculate());
        }
    }

    public void OppPointCardPushed()
    {
        pointCard[1].GetComponent<Animator>().enabled = true;
        oppCurrent = 300;
    }

    /// <summary>
    /// ///////////////////////////////////////////////////////////////////////////////////////
    /// Extra
    /// </summary>
    public void DrawCardClick()
    {
        if(turnIndex == (int)WhoseTurn.Me)
        {
            turnIndex = (int)WhoseTurn.Opponent;

            // Draw my cards
            for (int i = 0; i < 5; i++)
            {
                myCards[i].SetActive(false);

                if (Vector3.Distance(myCards[i].transform.position, myCardOrigin[i].transform.position) < 1f)
                {
                    myCards[i].SetActive(true);
                }
            }

            if (Vector3.Distance(pointCard[0].transform.position, pointCardOrigin[0].transform.position) > 1f)
            {
                pointCard[0].SetActive(false);
                pointCard[0].transform.position = pointCardOrigin[0].transform.position;
                pointCard[0].SetActive(true);
                pointCard[0].GetComponent<Animator>().enabled = false;
            }

            for (int i = 0; i < 5; i++)
            {
                if (Vector3.Distance(myCards[i].transform.position, myCardOrigin[i].transform.position) > 1f)
                {
                    myCards[i].SetActive(true);
                    myCards[i].transform.position = myCardOrigin[i].transform.position;
                    myCards[i].GetComponent<Animator>().enabled = false;
                    count--;
                    RandomMeCard(i);
                    break;
                }
            }
        }
        else
        {
            turnIndex = (int)WhoseTurn.Me;

            // Draw Opp cards
            for (int i = 0; i < 5; i++)
            {
                oppCards[i].SetActive(false);

                if (Vector3.Distance(oppCards[i].transform.position, oppCardOrigin[i].transform.position) < 1f)
                {
                    oppCards[i].SetActive(true);
                    oppCardOrigin[i].SetActive(true);
                }
            }

            for (int i = 0; i < 5; i++)
            {
                if (Vector3.Distance(oppCards[i].transform.position, oppCardOrigin[i].transform.position) > 1f)
                {
                    oppCards[i].SetActive(true);
                    oppCards[i].transform.position = oppCardOrigin[i].transform.position;
                    oppCards[i].GetComponent<Animator>().enabled = false;
                    count--;
                    RandomOppCard(i);
                    break;
                }
            }
        }                                
    }    
        
    public void IsFinishedTurn()
    {
        int pushedCard = 0;

        for(int i = 0; i < myCards.Length; i++)
        {
            if(Vector3.Distance(myCards[i].transform.position, abyssCard[0].transform.position) < 1f)
            {
                pushedCard++;
                break;
            }
        }

        for (int i = 0; i < oppCards.Length; i++)
        {
            if (Vector3.Distance(oppCards[i].transform.position, abyssCard[1].transform.position) < 1f)
            {
                pushedCard++;
                break;
            }
        }

        for(int i = 0; i < abyssCard.Length; i++)
        {
            if(Vector3.Distance(pointCard[i].transform.position , abyssCard[i].transform.position) < 1f)
            {
                pushedCard++;
            }
        }

        if(pushedCard == 2)
        {
            isFinishedTurn = true;
        }

        if (meCurrent == 0)
        {
            isFinishedTurn = true;
        }
    }

    public void RandomMeCard(int index)
    {
        int cardIndex = (int)Random.Range(0, 20f) % 2;

        myCards[index].GetComponent<Image>().sprite = cardType[cardIndex];

        if (cardIndex == (int)CardType.attack)
        {
            myCards[index].name = cardIndex + "+2000";
        }
        else if (cardIndex == (int)CardType.defence)
        {
            myCards[index].name = cardIndex + "+2500";
        }
    }

    public void RandomOppCardTest(int index)
    {
        if(index % 2 == 0)
        {
            oppCards[index].name = 0 + "+2000";
        }
        else
        {
            oppCards[index].name = 1 + "+2500";
        }                
    }

    public void RandomOppCard(int index)
    {
        int cardIndex = (int)Random.Range(0, 20f) % 2;

        oppCards[index].GetComponent<Image>().sprite = cardType[cardIndex];

        if (cardIndex == (int)CardType.attack)
        {
            oppCards[index].name = cardIndex + "+2000";
        }
        else if (cardIndex == (int)CardType.defence)
        {
            oppCards[index].name = cardIndex + "+2500";
        }
    }

    IEnumerator DelayToPushOpp(float delayTime)
    {       
        yield return new WaitForSeconds(delayTime);

        int index = (int)Random.Range(0, 20f) % 5;

        for(int i = 0; i < 15; i++)
        {
            if (Vector3.Distance(oppCards[index].transform.position, oppCardOrigin[index].transform.position) > 1f)
            {
                index = (index + 1) % 5;
            }
            else
            {
                if(turnIndex == (int)WhoseTurn.Me)
                {
                    break;
                }
                else
                {
                    if (int.Parse(oppCards[index].name.Split("+")[0]) == (int)CardType.attack)
                    {
                        break;
                    }
                    else
                    {
                        index = (index + 1) % 5;
                    }
                }
            }
        }
        
        PushOppCard(index);            
    }

    IEnumerator DelayToCalculate()
    {
        if(turnIndex == (int)WhoseTurn.Me)
        {
            yield return new WaitForSeconds(1.5f);
            IsFinishedTurn();
        }
        else
        {
            if (!isBlocked && meCurrent == 0)
            {
                yield return new WaitForSeconds(0);
                IsFinishedTurn();
            }
            else if (isBlocked)
            {
                yield return new WaitForSeconds(1.5f);
                IsFinishedTurn();
            }
        }                
    }

    IEnumerator WaitingForMePush(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        for (int i = 0; i < myCards.Length; i++)
        {
            if (Vector3.Distance(myCards[i].transform.position, abyssCard[0].transform.position) < 1f && myCards[i].active)
            {
                myCards[i].SetActive(false);
                myCards[i].transform.position = new Vector3(10000f, 10000f, 0);
            }
        }

        if (Vector3.Distance(pointCard[0].transform.position, abyssCard[0].transform.position) < 1f)
        {
            pointCard[0].SetActive(false);
            pointCard[0].GetComponent<Animator>().enabled = false;
            pointCard[0].transform.position = pointCardOrigin[0].transform.position;
            pointCard[0].SetActive(true);
        }

        StartCoroutine(DelayToCalculate());
    }

    public void AlertShow(string str)
    {
        alertText.text = str;
        time = 1f;
        alertText.gameObject.SetActive(true);
        isWarning = true;
        StartCoroutine(DelayToChangeTurn());
    }

    IEnumerator DelayToShowDialog()
    {
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < myCards.Length; i++)
        {
            if (Vector3.Distance(myCards[i].transform.position, abyssCard[0].transform.position) < 1f && myCards[i].active)
            {
                myCards[i].SetActive(false);
                myCards[i].transform.position = new Vector3(10000f, 10000f, 0);
            }
        }

        if (Vector3.Distance(pointCard[0].transform.position, abyssCard[0].transform.position) < 1f)
        {
            pointCard[0].SetActive(false);
            pointCard[0].GetComponent<Animator>().enabled = false;
            pointCard[0].transform.position = pointCardOrigin[0].transform.position;
            pointCard[0].SetActive(true);
        }

        time = 5f;
        dialog.SetActive(true);
        isWarning = true;
    }

    IEnumerator DelayToChangeTurn()
    {
        yield return new WaitForSeconds(1f);

        if(turnIndex == (int)WhoseTurn.Me)
        {
            turnIndex = (int)WhoseTurn.Opponent;
            isOppPushed = false;
        }
        else
        {
            turnIndex = (int)WhoseTurn.Me;
        }
    }

    public void DialogYes()
    {
        isBlocked = true;
        time = 0;        

        StartCoroutine(WaitingForMePush(3f));
    }
    
    public void DialogNo()
    {
        isBlocked = false;
        time = 0;
        meCurrent = 0;
        StopAllCoroutines();
        StartCoroutine(DelayToCalculate());
    }
}
