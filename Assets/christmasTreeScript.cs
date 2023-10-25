using System.Collections;
using System.Linq;
using UnityEngine;
using KModkit;

public class christmasTreeScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public Renderer[] giftBoxes;
    public Material[] wrappingPaper;
    public KMSelectable clockButton;
    public Transform cModule;
    public int[] numberOfGifts;
    private int valueX = 0;
    private int valueY = 0;
    private int valueZ = 0;
    private int ports = 0;
    private int indicators = 0;
    private int batteries = 0;

    public TextMesh timerText;
    private int hour = 7;
    private int minute = 0;

    public int correctTime;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        clockButton.OnInteract += delegate () { PressButton(); return false; };
    }

    void Start()
    {
        for(int i = 0; i <= 4; i++)
        {
            numberOfGifts[i] = 0;
        }
        WrapPresents();
        CalculateCorrectTime();
        StartCoroutine(StartClock());
    }

    void WrapPresents()
    {
        for(int i = 0; i <= 7; i++)
        {
            int index = UnityEngine.Random.Range(0,5);
            giftBoxes[i].material = wrappingPaper[index];
            numberOfGifts[index]++;
        }

        int index2 = UnityEngine.Random.Range(0,5);
        giftBoxes[8].material = wrappingPaper[index2];
        numberOfGifts[index2] += 2;

        int index3 = UnityEngine.Random.Range(0,5);
        giftBoxes[9].material = wrappingPaper[index3];
        numberOfGifts[index3] += 3;
        Debug.LogFormat("[Christmas Presents #{0}] You have received {1} presents from Auntie Marge, {2} from Uncle Simon, {3} from Cousin Bob, {4} from Granny May & {5} from Great Uncle Bertie.", moduleId, numberOfGifts[0], numberOfGifts[1], numberOfGifts[2], numberOfGifts[3], numberOfGifts[4]);
    }

    void CalculateCorrectTime()
    {
        valueX = numberOfGifts[0] + numberOfGifts[1] - numberOfGifts[2];
        if(valueX < 0)
        {
            valueX = valueX * -1;
        }

        valueY = (numberOfGifts[3] - numberOfGifts[4]);
        if(valueY < 0)
        {
            valueY = valueY * -1;
        }
        else if(valueY == 0)
        {
            valueY = 1;
        }
        ports = Bomb.GetPortCount();
        indicators = Bomb.GetIndicators().Count();
        batteries = Bomb.GetBatteryCount();

        valueX = valueX + indicators;
        valueY = valueY + ports;
        valueZ = valueX * valueY + batteries;
        Debug.LogFormat("[Christmas Presents #{0}] Value X is {1}. Value Y is {2}. Value Z is {3}.", moduleId, valueX, valueY, valueZ);
        correctTime = (valueZ % 14) + 7;
        Debug.LogFormat("[Christmas Presents #{0}] Open your presents when the hour time is {1}.", moduleId, correctTime);
    }

    IEnumerator StartClock()
    {
        while(!moduleSolved)
        {
            yield return new WaitForSeconds(0.02f);
            minute++;
            if(minute == 60)
            {
                minute = 0;
                hour++;
                if(hour == 21)
                {
                    hour = 7;
                }
            }
            if(moduleSolved)
            {
                break;
            }
            timerText.text = hour.ToString("00") + ":" + minute.ToString("00");
        }
    }

    public void PressButton()
    {
        if(moduleSolved)
        {
            return;
        }
        clockButton.AddInteractionPunch();
        if(hour == correctTime)
        {
            moduleSolved = true;
            GetComponent<KMBombModule>().HandlePass();
            Audio.PlaySoundAtTransform("bells", transform);
            Debug.LogFormat("[Christmas Presents #{0}] You opened your presents at {1}. That is correct. Module disarmed & Merry Christmas!", moduleId, timerText.text);
        }
        else
        {
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Christmas Presents #{0}] Strike! You tried to open your presents at {1}. That is incorrect.", moduleId, timerText.text);
        }
    }

    #region TwitchPlays

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = "To press the clock use '!{0} <hour>'! (Number should be in a range of 7-20)";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        int hours = -1;
        if(!int.TryParse(command, out hours))
        {
            yield return null;  
            yield return "sendtochaterror Number not valid!";
            yield break;
        }
        if(hour == hours)
        {
            yield return null;
            clockButton.OnInteract();
            yield break;
        }
        if (!(hours>=7 && hours<=20))
        {
            yield return null;
            yield return "sendtochaterror Number out of range!";
            yield break;
        }
        do
        {
            yield return "trycancel Button wasn't pressed due to a request to cancel.";
        } while (hour != hours);
        yield return null;
        yield return new WaitForSeconds(0.02f);
        clockButton.OnInteract();
    }

    #endregion
}
