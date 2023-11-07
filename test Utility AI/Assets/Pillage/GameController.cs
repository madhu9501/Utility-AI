using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
// using PillageGameplay;
using DG.Tweening;
// using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;
using UnityEngine.UI;
// using AeLa.EasyFeedback;
// using ConstantVariables;
// using ShaderEffects;

/// <summary>
/// Currently Controls Game State Logic(win, lose, quit), game logic(Player turn), cells color logic, 
/// camera logic and some animation logic //TODO needs to be refactored in different classes
/// </summary>
public class GameController : MonoBehaviour
{
    public static GameController instance;

    public bool EnableCheat = false;
    public bool DisableBot = false;
    public bool EnableTimer = true;

    public GameObject mainCamera;
    public Camera _loginCam;

    [Header("Reference")]
    public List<PlayerController> players;
    public List<PlayerData> playerDataList;
    public GridMapController gridMap;
    public DiceController dice;
    public PowerupHandler powerupHandle;

    public GameObject centerCell;

    //TODO to be removed if not needed
    /* [Header("Colors")]
     public Color safeCellColor;
     public Color normalCellColor;*/

    public List<PlayerController> activePlayers => players.FindAll(p => p.isActiveInGame);
    public DiceController activeDice;
    private PlayerController currentPlayer;
    public bool isDirectionRequired =>  currentPlayingPlayerCount > 1;
    public int currentPlayingPlayerCount => activePlayers.Where(x => !x.isCompletedPillage).ToList().Count;
    public Queue<PlayerController> currentPlayingPlayers;

    public Transform powerAnimationCenter;

    public List<PlayerController> rankOrderList = new List<PlayerController>();

    [SerializeField]
    private Button _feedBackButton;
    // [SerializeField]
    // private FeedbackForm _feedBackForm;

    // public FeedbackForm _firstFeedBackForm;
    public CanvasGroup uiCanvas;

    [Header("SkyBox Animation Values")]
    [SerializeField]
    private float _skyboxEndValue = 360f;
    [SerializeField]
    private float _duration = 200;
    private string _skyBoxParam = "_Rotation";
    private float _skyBoxStartValue = 0;


    // [SerializeField]
    // private List<VolumeProfile> postProcessingEffects;

    [SerializeField]
    private Material _grounMat;
    public Texture ground_2p;
    public Texture ground_3p;

    //coomented as random names are not needed
    /*public static List<string> userNames = new List<string>()
    { 
        "Mahesh sharma",
        "mahadev",
        "sanket",
        "samay",
        "salman khan",
        "demo player",
        "naruto",
        "User_8",
    };

    public static List<string> GetRandomUserNames(int _count)
    {
        List<string> randomOppo = new List<string>();
        List<string> totalUsers = new List<string>(userNames);
        for (int i = 0; i < _count; i++)
        {
            var rand = totalUsers[Random.Range(0, totalUsers.Count)];
            totalUsers.Remove(rand);
            randomOppo.Add(rand);
        }

        return randomOppo;
    }*/

    private List<int> PlayerActiveForDuel = new List<int>() { 0, 2 };
    private List<int> PlayerActiveForTriplet = new List<int>() {0,1,3};
    private List<int> PlayerActiveForAll = new List<int>() {0,1,2,3};


    public bool isBotSkipTimerOnce = false;

    public static bool isGameRunning => UserData.gamestate == UserData.GameState.NA;

    private bool IsPlayerActiveForLevelMode(int index, int levelMode)
    {
        if (levelMode == 2) 
            return PlayerActiveForDuel.Contains(index);
        else if (levelMode == 3) 
            return PlayerActiveForTriplet.Contains(index);
        else
            return PlayerActiveForAll.Contains(index);
    }     

    private void Awake()
    {
        instance = this;
        _loginCam = Camera.main;
        Camera.main.enabled = false;
        QualitySettings.SetQualityLevel(5);
        // UserInterface.instance.SetCanvasRenderMode(RenderMode.ScreenSpaceOverlay);

        if(UserData.levelModeSelected == LevelMode.TwoPlayer)
        {
            _grounMat.SetTexture("_BaseMap", ground_2p);
        }
        else
        {
            _grounMat.SetTexture("_BaseMap", ground_3p);
        }
       
        //TODO Create new script if more things are modified in skybox
        RenderSettings.skybox.SetFloat(_skyBoxParam, _skyBoxStartValue);
        RenderSettings.skybox.DOFloat(_skyboxEndValue, _skyBoxParam, _duration).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
    }

    // Start is called before the first frame update
    void Start()
    {
        // _feedBackButton.onClick.AddListener(() => EasyFeedBackButtonPressed());
        OnGameStart();

        int activePlayer = UserData.GetLevelMode;
        List<CellHandler> activePlayerCells = new List<CellHandler>();
        
        players.ForEach(p => p.DeactivePlayer(false));

        

        // Dynamic player list generations...
        playerDataList.ForEach(p => p.isBot = true);
        var choosenWarrior = playerDataList.Find(x => x.warriorType == UserData.warriorTypeChoosen);
        choosenWarrior.isBot = false; // reset all will be bots

        // layout for level mode
        var currnetPlayerLayoutList = new List<PlayerController>();
        for (int p = 0; p < players.Count; p++)
            if (IsPlayerActiveForLevelMode(p, activePlayer))
                currnetPlayerLayoutList.Add(players[p]);

        // get opponent choosen data 
        List<PlayerData> currentPlayerDataList = new List<PlayerData>();

        List<UserData.OpponentData> oppoList = new List<UserData.OpponentData>()
        { new UserData.OpponentData(UserData.warriorTypeChoosen,UserData.playerName,UserData.avatar) }; 
        oppoList.AddRange(UserData.opponentList);

        foreach (var item in oppoList)
        {
            var _data = playerDataList.Find(p => p.warriorType == item.oppoWarriorType);
            currentPlayerDataList.Add(_data);
        }

        for (int p = 0; p < currnetPlayerLayoutList.Count; p++)
        {
            activePlayerCells.AddRange(currnetPlayerLayoutList[p].Init(oppoList[p], currentPlayerDataList[p], p));
            currnetPlayerLayoutList[p].isActiveInGame = true;

        }

        // Grid setup
        gridMap.Init(activePlayerCells);

        activePlayers.ForEach(p => p.SetupWarriors());

        // Set up current playing players 
        currentPlayingPlayers = new Queue<PlayerController>(activePlayers);

        
        // Set up Dice 
        // activeDice = Instantiate(dice, currnetPlayerLayoutList[0].playerUI.diceWaypoint.position, Quaternion.identity, currnetPlayerLayoutList[0].playerUI.diceWaypoint);
        activeDice.transform.localScale = Vector3.one;
        activeDice.ResetPosition();
        currentPlayer = currentPlayingPlayers.Dequeue();


        // handle entrance animation here...
        PrepareForAnimation(activePlayerCells, OnAnimationseqComplete);


        var playerData = UserInterface.instance.playerSaveData;
    }

    public PlayerData GetNonBotPlayerData()
    {
        return playerDataList.Find(x => x.warriorType == UserData.warriorTypeChoosen);
         
    }

    public void FeedBackButtonPressed()
    {
        // EngagementManager.instance.LogFirebaseAnalytics(AllEngagementEvents.GA_FeedBack_Pressed_Gameplay);
    }


    private void OnAnimationseqComplete()
    {
        var powerTurnMove = currentPlayer.isPowerupTurnMove;

        powerupHandle.InitPowers(currentPlayer, false);
        currentPlayer.OnPlayerTurn(OnPlayerTurnComplete, powerTurnMove);
        //for resetting the initial camera settings
        // EventManager.SetInitalCameraEventCaller();

    }

    public string _previousPlayerId;
    private void OnPlayerTurnComplete(PlayerController _currentPlayer)
    {
        // @Madhu - Calc player progress
        activePlayers.ForEach(p => p.progressCalculator.CalculateWarriorProgress(p));
        // Madhu

      
        if (!isGameRunning) return;

        // Logger.Log("OnPlayerTurnComplete");

        // check for Re-Roll-Dice available 
        bool moveToNextPlayer = true;

        // enqueue player again in the list
        if (_currentPlayer.isCompletedPillage)
        {
            // check for level finish condtion 
            if (currentPlayingPlayers.Count <= 1 || !_currentPlayer.isBot())
            {
                var _player = UserInterface.instance.playerSaveData;
                if (!_player._levelPerModeData.Exists(x => x.levelMode == UserData.levelModeSelected))
                {
                    // _firstFeedBackForm.Show();
                }
                else
                {
                    GameOver();
                        return;
                }
                
            }
            else
            {
                moveToNextPlayer = true;
            }
        }
        else if (currentPlayer.canReDice)
        {
            moveToNextPlayer = false;
        }

        bool isPlayerFreshTurn = currentPlayer.userId != _previousPlayerId;

        _previousPlayerId = _currentPlayer.userId;

        if (moveToNextPlayer)
        {
            if (!_currentPlayer.isCompletedPillage)
                currentPlayingPlayers.Enqueue(_currentPlayer);

            currentPlayer = currentPlayingPlayers.Dequeue();
            // @Madhu - End player reminder timer
            // EventManager.PlayerReminderTimerStopEventCaller();

        }

        if (isPlayerFreshTurn)
        {
            currentPlayer.nuberOfTurnsPlayed++;
        }

        var powerTurnMove = currentPlayer.isPowerupTurnMove;

        bool isTwoMoveCycle = currentPlayer.nuberOfTurnsPlayed - currentPlayer.powerCycleCount >=2;
 
        // Logger.Log("isTwoMoveCycle " + isTwoMoveCycle);

        powerupHandle.InitPowers(currentPlayer, isTwoMoveCycle);

        currentPlayer.OnPlayerTurn(OnPlayerTurnComplete, powerTurnMove, isTwoMoveCycle);
    }

    public PlayerController GetRandomTarget(string id)
    {
        var exceptMePlayers = activePlayers.Where(x => x.userId != id).ToList();
        int randomIndex = Random.Range(0, exceptMePlayers.Count);
        return exceptMePlayers[randomIndex];
    }

    public void DirectionInit(PlayerController player, UnityAction<PlayerController> OnDirectionSelected)
    {
        foreach (var item in currentPlayingPlayers)
        {
            if(item.userId != player.userId)
            {
                item.DirectionInit((PlayerController x) => 
                {
                    OnDirectionSelected?.Invoke(x);
                    DirectionReset();
                });
            }
        }
    }

    public void GameOver(bool isQuit = false)
    {
       
        // Enable the level over screen
        // Logger.Log($"Game Over !!!");

        GameOverStopMusic();
        // //to stop tick tock sound
        // AudioController.con.StopTickTockSound();


        // //to stop the music
        // AudioController.con.StopMusic();


        // get the current player warrior 
        var userIdPlayer = activePlayers.Find(x => !x.isBot()).userId;
        if (rankOrderList.Exists(x => x.userId == userIdPlayer))
        {
            // game win with rank
            UserData.rank = rankOrderList.FindIndex(x => x.userId == userIdPlayer) + 1;
            UserData.gamestate = UserData.GameState.WIN;
        }
        else if (isQuit)
        {
            // game quit
            UserData.gamestate = UserData.GameState.QUIT;
        }
        else
        {
            // game lost
            UserData.gamestate = UserData.GameState.LOST;
        }
        uiCanvas.DOFade(0, 0.5f).OnStart(() =>
        {
            // GameHUD.instance.WarriorUIMeshSetActive(false);
            // players[0].playerUI.EnableDisabledDice(false);
            // players[0].playerUI.playerPowers.ForEach(x => x.powerCard.gameObject.SetActive(false));
        }).OnComplete(() =>
        {
            // UserInterface.instance?.SwitchScreen(ScreenNames.GameOver);
        }
       );
        EnableGrayMode(true);

    }

    public void GameOverStopMusic()
    {
        //to stop tick tock sound
        // AudioController.con.StopTickTockSound();


        //to stop the music
        // AudioController.con.StopMusic();
    }

    public void DirectionReset()
    {
        activePlayers.ForEach(a => a.EnableDirection(false));
    }

    public bool CheckIfOtherPlayerIsActive(string playerId)
    {
        bool result = false;
        foreach (var player in activePlayers)
        {
            if(player.userId != playerId)
            {
                result = player.IsAnyWarriorActive();
            }
        }
        return result;
    }

    public List<PlayerController> GetOtherPlayers(string playerId)
    {
        if (string.IsNullOrEmpty(playerId))
            return activePlayers; 
        else
            return activePlayers.Where(x => x.userId != playerId).ToList();
    }

    public PlayerController GetCurrentPlayer => currentPlayer;

    public bool CheckThresholdAngles(float angle)
    { 
        return (angle < thresholdLeftY || angle > thresholdRightY);
    }

    // public void EasyFeedBackButtonPressed()
    // {
    //     if (CommonUtilities.CheckInternetConnection() == false)
    //     {
    //         return;
    //     }
    //     else
    //     {
    //         _feedBackForm.Show();
    //     }
    // }


    #region Animation Handling 

    [Header("[ Animation Content =========================== ]"), Space(5)]

    [SerializeField] Vector3 camZoomOutValue;
    [SerializeField] Vector3 camZoomInValue;
    [SerializeField] RectTransform playerCanvasRect, botCanvasRect;
    [SerializeField] List<CanvasGroup> uiCanvasGroup;
    [SerializeField] GameObject blastEntranceVFX;

    [SerializeField] float blastHoldDelay = 1f;
    [SerializeField] float camZoomspeed = 0.5f;
    [SerializeField] float objDropspeed = 0.2f;
    [SerializeField] float cellStartPosition = 0.03f;

    

    //Runs starting animations of the game(rise-raid-return, cells and warrior droping down on scene)
    private void PrepareForAnimation(List<CellHandler> _cells, UnityAction OnComplete)
    {
        blastEntranceVFX.gameObject.SetActive(true);
        // AudioController.con.PlayGameSFX(entraceBlastClip);

        Sequence animationSeq = DOTween.Sequence();

       /* cameraRotate.transform.position = camZoomOutValue;

        animationSeq.Append(cameraRotate.DOMove(camZoomInValue, camZoomspeed).SetDelay(blastHoldDelay).OnComplete(() => 
        {
            blastEntranceVFX.gameObject.SetActive(false);

        }));
*/

        List<Transform> allAnimatedBodies = new List<Transform>();

        //animate cells (excluding base cells and pillage cell) and warrior to drop down to start position
        Sequence cellSeq = DOTween.Sequence();
        _cells.Where(x => x.type != CellType.BASE && x.type != CellType.PILLAGE).ToList().ForEach(x =>
        {
            allAnimatedBodies.Add(x.transform);
            cellSeq.Join(x.transform.DOLocalMoveZ(cellStartPosition, objDropspeed).OnStart(() => x.gameObject.SetActive(true)));
        });
        animationSeq.Append(cellSeq);

        Sequence warriorSeq = DOTween.Sequence();
        List<WarriorController> allWarriors = new List<WarriorController>();
        activePlayers.ForEach(x => allWarriors.AddRange(x.GetWarriors()));

        foreach (var _obj in allWarriors)
        {
            allAnimatedBodies.Add(_obj.transform);
            warriorSeq.Join(_obj.transform.DOLocalMoveZ(_obj.currentCell.transform.localPosition.z, objDropspeed).OnStart(() => _obj.gameObject.SetActive(true)));
        }

        warriorSeq.AppendCallback(EntranceThudEffect);

        animationSeq.Append(warriorSeq);

        // set everything to default poistion 
        foreach (var _obj in allAnimatedBodies)
        {
            var _loc = _obj.localPosition;
            _obj.localPosition = new Vector3(_loc.x, _loc.y, -0.4f);
            _obj.gameObject.SetActive(false);
        }
        
        // animationSeq.Append(DOVirtual.DelayedCall(0.001f, () => EventManager.WordCollisionTextAnimEventCaller()));
        animationSeq.AppendInterval(1.7f);
        // animationSeq.Append( CanvasEntrance());

        animationSeq.Play().SetDelay(2).AppendCallback(() => 
        {
            OnComplete?.Invoke();
            DOTween.Kill(animationSeq);
            // Logger.Log("Animation Complete");


            //For playing the music
            // AudioController.con.PlayMusic(AudioController.MusicEnum.RANDOM);
        }
        );
    }
    
    private void EntranceThudEffect()
    {
        activePlayers.ForEach(p => p.PlayGroundBlast());
    }


    // private Sequence CanvasEntrance()
    // {
    //     Sequence uiSeq = DOTween.Sequence();

    //     // do tween plannel top bottom 
    //     var initialP = playerCanvasRect.sizeDelta.y;
    //     var initialB = botCanvasRect.sizeDelta.y;

    //     playerCanvasRect.DOAnchorPosY(-initialP - 200f,0f);
    //     botCanvasRect.DOAnchorPosY(initialB,0f);

    //     // alpha section 
    //     uiCanvasGroup.ForEach(c => c.alpha = 0);

    //     uiSeq.Join(playerCanvasRect.DOAnchorPosY(0f, 0.5f).OnComplete(
    //         // () => EventManager.DissolveInWarriorMeshEventCaller())); 
    //     // uiSeq.Join( botCanvasRect.DOAnchorPosY(0f,0.5f));

    //     Sequence uiSeqAlpha = DOTween.Sequence();

    //     uiCanvasGroup.ForEach(cg => 
    //     {
    //         uiSeqAlpha.Join(cg.DOFade(1f, 0.2f));
    //     });

    //     uiSeq.Append(uiSeqAlpha);
    //     return uiSeq;
    // }


    #endregion


    #region [ Audio controller ]

    [Header("[ Audio ] ================")]
    public AudioClip thudEffectClip;
    public AudioClip invalidMoveClip;
    public AudioClip diceRolledClip;
    public AudioClip clockTimerTickClip;
    public AudioClip missedTurnedClip;
    public AudioClip playerTurnClip;
    public AudioClip crownAppearClip;
    public AudioClip enableCardPowClip;
    public AudioClip disableCardPowClip;
    public AudioClip unavailablePowerClip;
    public AudioClip uiPanelSlideInClip;
    public AudioClip entraceBlastClip;
    public AudioClip warriorHop;
    public AudioClip warriorSkipHop;
    public AudioClip warriorFlipHop;
    public AudioClip warriorFreezeAttack;
    public AudioClip warriorFireAttack;
    public AudioClip warriorBlastAttack;
    public AudioClip warriorThorAttack;
    public AudioClip pillageCompleteAudio;
    public AudioClip bubbleEffectAudio;
    public AudioClip safeCellAudio;
    public AudioClip flipCardAudio;
    public AudioClip shieldAudioClip;
    public AudioClip arrowStromAudioClip;
    public AudioClip shakuniLaughAudioClip;
    public AudioClip balramLaughAudioClip;
    public AudioClip arjunLaughAudioClip;
    public AudioClip duryodhanLaughAudioClip;
    public AudioClip gandhariLaughAudioClip;
    public AudioClip draupadiLaughAudioClip;
    public AudioClip arjunScreamAudioClip;
    public AudioClip duryodhanScreamAudioClip;
    public AudioClip gandhariScreamAudioClip;
    public AudioClip draupadiScreamAudioClip;
    public AudioClip reviveAudioClip;
    public AudioClip lockCastleSound;
    public AudioClip cardSelected;
    public AudioClip PlayerKickClip;
    public AudioClip CellExpansionClip;
    public AudioClip CannonClip;
    public AudioClip ChariotClip;
    public AudioClip PowerCardOrbClip;
    public AudioClip HelperTextClip;
    public AudioClip promptTextClip;
    public AudioClip EnemyTurnClip;





    #endregion


    #region Handle Camera 

    [Header("[ Camera Rotating ] ===================== "),Space(5)]

    public Transform cameraRotate;
    public float speed;

    public float thresholdLeftY = 20f;
    public float thresholdRightY = 340f;
    public void RotateLeft()
    {
        // Logger.Log(cameraRotate.localEulerAngles.y);
        if (CheckThresholdAngles(cameraRotate.localEulerAngles.y))
        {
            var speedDelta = speed * Time.deltaTime;
            var nextAngle = (cameraRotate.localEulerAngles.y + speedDelta);

            if (CheckThresholdAngles(nextAngle))
                cameraRotate.Rotate(0f, speedDelta, 0f);
        }
    }

    public void RotateRight()
    {
        // Logger.Log(cameraRotate.localEulerAngles.y);
        if (CheckThresholdAngles(cameraRotate.localEulerAngles.y))
        {
            var speedDelta = -speed * Time.deltaTime;
            var nextAngle = (cameraRotate.localEulerAngles.y + speedDelta);

            if (CheckThresholdAngles(nextAngle))
                cameraRotate.Rotate(0f, speedDelta, 0f);
        }


    }

    public void RotateUp()
    {
        // Logger.Log(cameraRotate.localEulerAngles.y);
        if (CheckThresholdAngles(cameraRotate.localEulerAngles.x))
        {
            var speedDelta = speed * Time.deltaTime;
            var nextAngle = (cameraRotate.localEulerAngles.x + speedDelta);

            if (CheckThresholdAngles(nextAngle))
                cameraRotate.Rotate(speedDelta, 0f, 0f);
        }

        
    }
    public void RotateDown()
    {
        // Logger.Log(cameraRotate.localEulerAngles.y);
        if (CheckThresholdAngles(cameraRotate.localEulerAngles.x))
        {
            var speedDelta = -speed * Time.deltaTime;
            var nextAngle = (cameraRotate.localEulerAngles.x + speedDelta);

            if (CheckThresholdAngles(nextAngle))
                cameraRotate.Rotate(speedDelta, 0f, 0f);
        }
    }


    public Camera uiCamera;
    public Camera _camera;
    public Canvas _gameCanvas;
    public GraphicRaycaster gRaycaster;

    public void OnGameStart()
    {
        //EnableGrayMode(false);

        // save the gameplay start data 
        var _playerData = UserInterface.instance.playerSaveData;
        _playerData.UpdateFavData(_levelSelected: UserData.levelModeSelected, _warriorUsed: UserData.warriorTypeChoosen);

        _playerData.AddXP(10); // start the game
        
        UserInterface.instance.playerSaveData = _playerData;
    }

    public void AddXP(PlayerController _player, int amount)
    {
        if (_player.isBot()) return;

        // save the gameplay start data 
        var _playerData = UserInterface.instance.playerSaveData;
        _playerData.AddXP(amount); // start the game

        UserInterface.instance.playerSaveData = _playerData;
    }


    #region Gray Scale Mode

    // public Volume globalVol;

    [ContextMenu("TT")]
    public void TTTTTT()
    {
       
    }

  
    public void EnableGrayMode(bool isEnable)
    {
        
        //_camera.GetComponent<PostProcess>().enabled = isEnable;
        QualitySettings.SetQualityLevel(4);
        if(isEnable)
        {
            // globalVol.profile = postProcessingEffects[ConstantVars.INT_VAL_ZERO];
            gRaycaster.enabled = false;
            _gameCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            float startVal = 0f;
            float endVal = 1f;
           
            DOVirtual.Float(startVal, endVal, 0.5f, (float val) => 
            {
                // globalVol.weight = val;
            }).SetUpdate(true);
        }
        else
        {
            // globalVol.weight = 0f;
        }

    }

    #endregion

    #endregion

}
public static class UserData
{
    public static string playerName; //=>  UserInterface.instance.spriteDirectory.GetWarriorName(UserData.warriorTypeChoosen) + " (Me)";
    public static string avatar => (UserInterface.instance.playerSaveData._avatarId);
    public static LevelMode levelModeSelected;
    public static WarriorType warriorTypeChoosen;

    public static int warriorsCapture = 0;
    public static int warriorsLost = 0;
    public struct OpponentData
    {
        public WarriorType oppoWarriorType;
        public string oppoName;
        public string avatarId;

        public OpponentData(WarriorType type, string _name, string avatar)
        {
            oppoWarriorType = type;
            oppoName = _name;
            avatarId = avatar;
        }
    }

    public enum GameState
    {
        NA,
        QUIT,
        WIN,
        LOST
    }

    public static int rank = -1;
    public static GameState gamestate;
    public static List<OpponentData> opponentList = new List<OpponentData>();
    public static int GetLevelMode => levelModeSelected == LevelMode.TwoPlayer ? 2 : 3;

    


    public static void ResetData()
    {
        warriorsLost = warriorsCapture = 0;
        gamestate = default;
        rank = -1;
    }

}




#region ENUM --
public enum CellType
{
    NORMAL,
    COMMON_SAFE,
    HOME_SAFE,
    START,
    END,
    BASE,
    CENTER,
    PILLAGE
}

public enum WarriorState
{
    None,
    Active,
    //Attack,
    Pillage,
    PillageDone
}

//Divesh states for blendshapes
public enum WarriorFaces
{
    Squeeze,
    HeadBack,
    HeadFront,
    Angry,
    Sad,
    Laugh,
    Blink
}

[System.Serializable]
public enum PowerType
{
    Release,
    Shield,
    Revive,
    Lock,
    Doubler,
    Twice,
    Arrows,
    NA,
}

[System.Serializable]
public enum PromptType
{
    UseCardOrDice,
    UseDice,
    SelectWarrior,
    SelectDirection,
    CurrentPrompt
}

public enum DialougeType
{
    UserProgressBar,
    EnemyProgressBar,
    UserUseCardOrDice,
    UserUseDice,
    UserSelectWarrior,
    UserSelectDirection,
    UserWaitTurn,
    EnemyPortrait,
    UserDestinationCell,
    UserCardWaitTurn,
    UserCardOnlyOne,
}

[System.Serializable]
public enum WarriorType
{
    Red,
    Bue,
    Green,
    Yellow,
    NA
}

public enum LevelMode
{
    FourPlayer,
    TwoPlayer,
    NA
}

public enum VFXID
{
    GROUNDBLAST,
    SHEILD,
    ARROW,
    HEAL,
    BUBBLE,
    LEVELUP,
    ZONE,
    LOCKHOME,
    SUPERUP,
    BLAST,
    FIRE,
    LIGHTING,
    FREEZE,
    COINSHOWER,
    PILLAGECOIN
}
public enum CastleType
{
    Pentagon,
    Octagon,
    Square
}

public enum FlyInOutAnimType
{
    YourTurn,
    EnenmyTurn1,
    EnenmyTurn2
}

[System.Serializable]
public struct VFXDATA
{
    public VFXID id;
    public GameObject vfxobject;
}

[System.Serializable]
public enum DiceType
{
    Normal,
    HighValue,
    LowValue,
    CaptureValue,
    PillageValue,
    ReleaseValue,
    NA
}

[System.Serializable]
public enum ActionType
{
    PassiveCard,
    ActiveCard,
    Dice,
    NA
}
#endregion
