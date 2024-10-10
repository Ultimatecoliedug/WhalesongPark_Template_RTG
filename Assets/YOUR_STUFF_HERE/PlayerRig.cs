using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerStateEnum
{
    NOT_PLAYING,
    IN_LOBBY,
    QUEUED,
    PLAYING,
    FINISHED,
}

public enum ZonesEnum
{
    Britian,
    North_Africa,
    South_Africa,
    Oceania,
    Antartic,
    Finished,
}

public enum TutorialsEnum
{
    None = 0,
    RaiseSails = 1,
    LowerSails = 2,
    Left_Right = 4,
    Dolphin = 8,
}


public class PlayerRig : MonoBehaviour
{
    public PlayerStateEnum _MyPlayerState;

    public ZonesEnum Zone;

    [SerializeField]
    public int PlayerIndex;

    TutorialsEnum TutorialsDoneFlag;
    TutorialsEnum CurrentTutorial;

    bool EventTimerActive = false;

    float EventTimer;

    int TimedEventIndex = 0;

    float[] EventTimes = new float[]
    {
        3.5f,
        3.5f,
        3.5f,
        3.5f,
        3.5f,
        3.5f,
    };

    Action[] TimedEvents;

    void RaiseSailsTut() { StartTutorial(TutorialsEnum.RaiseSails); }
    void LowerSailsTut() { StartTutorial(TutorialsEnum.LowerSails); }
    void LeftRightTut() { StartTutorial(TutorialsEnum.Left_Right); }
    void StartObstacleSpawn() { ResetObstacleSpawning(); }
    void StartDolphinSpawn() { StartTutorial(TutorialsEnum.Dolphin); }
    void EndDolphinSpawn() { FinishedTutorial(TutorialsEnum.Dolphin); }

    [SerializeField]
    Image[] TutorialImages;

    static Dictionary<TutorialsEnum, int> FlagToIndex = new Dictionary<TutorialsEnum, int>()
    {
        { TutorialsEnum.RaiseSails, 0},
        { TutorialsEnum.LowerSails, 1},
        { TutorialsEnum.Left_Right, 2},
        { TutorialsEnum.Dolphin, 3},
    };

    // PLAYER
    public enum inputTypes
    {
        primaryFire,
        secondaryFire,
        Directional
    }
    public float playerSpeed;
    [SerializeField] private float playerAcceleration;
    [SerializeField] private float playerSlideSpeed = 5;

    // OBSTACLES
    //The distance obstacles will spawn from the player, in units, through local Z-axis.
    [SerializeField] private int ObstacleSpawnDistance = 30;
    [SerializeField] private int landDistanceOffset = 25;
    GameObject obstacles;
    [SerializeField] private GameObject prefab1x1;
    [SerializeField] private GameObject prefabLand;
    float timeSinceLastSpawn = 0f;
    float timeSinceLastLand = 0f;

    [SerializeField]
    float SpawnRateConst;

    private float spawnRate = 0;
    public LayerMask mask;
    // SCORE
    public float playerScore = 0;

    [SerializeField]
    TextMeshProUGUI ScoreText;

    [SerializeField]
    ProgressBarScript ProgressBarRef;

    [SerializeField]
    Dial_Ui_Setter SpeedDialRef;

    [SerializeField]
    float MaxSpeed = 5;

    [SerializeField]
    float MinSpeed = 7;

    void Start()
    {
        if (SpeedDialRef != null)
        {
            SpeedDialRef.MinSpeed = MinSpeed;
            SpeedDialRef.MaxSpeed = MaxSpeed;
        }

        obstacles = transform.Find("Obstacles").gameObject;

        TimedEvents = new Action[]
        {
            RaiseSailsTut,
            LowerSailsTut,
            LeftRightTut,
            StartObstacleSpawn,
            StartDolphinSpawn,
            EndDolphinSpawn,
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (EventTimerActive)
        {
            EventTimer -= Time.deltaTime;

            if (EventTimer < 0.0f)
            {
                TimedEventTimeOut();
            }
        }

        ProcessObjectSpawning();
    }
    private void FixedUpdate()
    {
        if (_MyPlayerState == PlayerStateEnum.PLAYING)
        {
            AdjustSpeed(Time.fixedDeltaTime * playerAcceleration);
        }
    }

    public void HandleInput(inputTypes type, Vector2 direction = new Vector2())
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        switch (type)
        {
            case inputTypes.primaryFire:
                ProcessMovement(new Vector3(1, 0, 0));
                break;
            case inputTypes.secondaryFire:
                ProcessMovement(new Vector3(-1, 0, 0));
                break;
            case inputTypes.Directional:
                ProcessMovement(new Vector3(-direction.x, 0, 0));
                break;
        }

    }

    void ResetObstacleSpawning()
    {
        timeSinceLastSpawn = 999.0f;
        spawnRate = SpawnRateConst;
    }

    private void ProcessMovement(Vector3 direction)
    {
        obstacles.transform.localPosition = obstacles.transform.localPosition + direction * Time.deltaTime * playerSlideSpeed * (playerSpeed * 0.25f);
    }

    private void ProcessObjectSpawning()
    {
        if (spawnRate == 0.0f)
        {
            return;
        }

        if (timeSinceLastSpawn >= 1.0f / spawnRate / playerSpeed)
        {
            //generate random offset
            int offset = UnityEngine.Random.Range(-15, 15);
            offset *= 2;
            //get base position
            Vector3 spawnPos = transform.TransformPoint(new Vector3(offset, 0, ObstacleSpawnDistance));

            SpawnSingleObject(spawnPos);

            timeSinceLastSpawn = 0;
        }
        timeSinceLastSpawn += Time.deltaTime;
        timeSinceLastLand += Time.deltaTime;
    }
    private void SpawnSingleObject(Vector3 spawnPos)
    {
        Debug.Log("Spawned an object!!!");

        Collider[] overlaps = Physics.OverlapSphere(spawnPos, 0.1f, mask, QueryTriggerInteraction.UseGlobal);
        foreach (Collider collision in overlaps)
        {
            if (collision.gameObject != null) return;
        }
        Instantiate(prefab1x1, spawnPos, transform.rotation, obstacles.transform);
    }
    private void SpawnLand(Vector3 spawnPos)
    {
        timeSinceLastLand = 0;
        //Convert to local space and apply landmass offset
        Vector3 leftPos = obstacles.transform.InverseTransformPoint(spawnPos) - new Vector3(-27.5f, 0, -landDistanceOffset);
        Vector3 rightPos = obstacles.transform.InverseTransformPoint(spawnPos) - new Vector3(27.5f, 0, -landDistanceOffset);
        //Convert back to world space
        leftPos = obstacles.transform.TransformPoint(leftPos);
        rightPos = obstacles.transform.TransformPoint(rightPos);

        Instantiate(prefabLand, leftPos, transform.rotation, obstacles.transform);
        Instantiate(prefabLand, rightPos, transform.rotation, obstacles.transform);
    }

    public void ResetPlayer()
    {
        EventTimerActive = false;
        gameObject.SetActive(false);
        _MyPlayerState = PlayerStateEnum.NOT_PLAYING;
        playerSpeed = 0;
        spawnRate = 0;
    }

    public void FinishedTutorial(TutorialsEnum TutorialFinished)
    {
        if (!TutorialsDoneFlag.HasFlag(TutorialFinished))
        {
            TutorialImages[FlagToIndex[TutorialFinished]].gameObject.SetActive(false);
        }

        TutorialsDoneFlag = TutorialsDoneFlag | TutorialFinished;
    }

    public void StartTutorial(TutorialsEnum NewTutorial)
    {
        FinishedTutorial(CurrentTutorial);

        TutorialImages[FlagToIndex[NewTutorial]].gameObject.SetActive(true);
        CurrentTutorial = NewTutorial;
    }

    public void FinalActivatePlayer(ZonesEnum StartZone)
    {
        Debug.Log("MINE: Activating player " + PlayerIndex);
        gameObject.SetActive(true);
        Zone = StartZone;
        _MyPlayerState = PlayerStateEnum.PLAYING;
        CurrentTutorial = TutorialsEnum.None;
        TutorialsDoneFlag = TutorialsEnum.None;
        playerScore = 0;
        EventTimerActive = true;
        TimedEventIndex = -1;
        playerSpeed = MinSpeed;

        TimedEventTimeOut();
    }

    public void DeActivatePlayer()
    {
        gameObject.SetActive(false);
        _MyPlayerState = PlayerStateEnum.FINISHED;
    }

    public void UpdateScore(float scoreAmount)
    {
        playerScore += scoreAmount;

        if (ScoreText != null)
        {
            ScoreText.text = "Score: " + Mathf.RoundToInt(playerScore).ToString();
        }

        //Debug.Log("Current Score: " + playerScore);
    }

    public void AdjustSpeed(float adjustAmount)
    {
        Debug.Log("Adjusting Speed!");

        playerSpeed += adjustAmount;
        playerSpeed = Mathf.Clamp(playerSpeed, MinSpeed, MaxSpeed);

        UpdateScore((Mathf.Pow(playerSpeed,2.0f)) * Time.fixedDeltaTime * 0.1f);

        Debug.Log("Speed Dial Null state: " + (SpeedDialRef == null).ToString());

        if (SpeedDialRef != null)
        {
            SpeedDialRef.SetSpeed(playerSpeed);
        }
    }
    public void TimedEventTimeOut()
    {
        TimedEventIndex++;

        if (TimedEventIndex < TimedEvents.Length)
        {
            TimedEvents[TimedEventIndex]();
            EventTimer = EventTimes[TimedEventIndex];
        }
        else
        {
            EventTimerActive = false;
        }
    }    
}
