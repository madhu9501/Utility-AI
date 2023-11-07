using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using ConstantVariables;

/// <summary>
/// This Class controls play game logic for a particular player,each player has this class attached to it.
/// this player turn logic and player power logic
/// Created by : Divesh
/// </summary>
public class PlayerController : MonoBehaviour
{
    // @Madhu - reference to main camera which will be accessed by the warrior controller for pawns to look as a target
    public Camera MainCamera;
    public Transform FlagPrefab;
    private float flagMinRotation = 20; 
    private float flagMaxRotation = -20;
    private float flagWaveDuration = 1.3f;

    // Madhu
    public string userId => playerData.playerName;
    private bool isPlayerBot => playerData.isBot;
    public bool isBot(bool botEnabled = true) => isPlayerBot && (botEnabled || !GameController.instance.DisableBot);
    public bool isActiveInGame = false;
    public bool isCompletedPillage = false;
    public bool canReDice = false;
    public float score;

    [Header("Reference")]
    //public Transform diceWaypoint;
    public TextMeshPro playerNameText;
    // public EventTrigger directionTrigger;
    public GameObject DirectionObj;
    public Transform fortBase;
    public MeshRenderer fortMesh;
    public Transform cellHolder;
    public MeshRenderer castleTop;
    public GameObject castleLock;
 
    private List<WarriorController> warriorControllers = new List<WarriorController>();
    public CellHandler startCell, endCell;
    public List<CellHandler> baseCells;
    public CellHandler pillageCell;

    public List<PowerType> availablePowers = new List<PowerType>();
    public PowerType playerCurrentPowerClicked = PowerType.NA;
    public CastleType thisCaslteType;

    public bool isPowerupTurnMove = false;
    public bool PowerUpSelectionActive = false;

    public int powerCycleCount = 0;

    public DiceController diceController;
    public PlayerData playerData;
    // public PlayerUI playerUI;
    // public AiController aiController;
    // @Madhu - Reference to ProgressCalculator
    public ProgressCalculator progressCalculator;
    // Madhu

    public UserData.OpponentData playerDetails;

    #region power variables 
    public bool twoMovesPowerEnable = false;
    public bool diceDoublersPowerEnable = false;
    public bool twoMoveActivvateAnimation = false;
    #endregion

    #region blinking material
    public MeshRenderer blinkingMaterial;
    [SerializeField]
    public float glowAnimDuration;
    #endregion

    // public List<ParticleSystem> arrowParticles;

    public int totalWarriorCaptured = 0;
    public int totalWarriorLost = 0;

    public int nuberOfTurnsPlayed = 0;

    public Material stepMatTop;

    #region Callbacks 
    private UnityAction<PlayerController> OnPlayerTurnComplete;
    #endregion
    // @MADHU - for cam zoom, warrior selection
    Transform camParent;
    // public CameraPositionFinder cameraPositionFinder;
    // MADHU - for power text animation
    public bool doublerActive;
    public bool twiceActive;
    public bool releaseActive;
    public bool shieldActive;
    public bool OnPointerDownAutoMove;
    public List<int> playerDiceValues;
    
    public void DeactivePlayer(bool isEnable)
    {
        // playerUI.gameObject.SetActive(isEnable);
        gameObject.SetActive(isEnable);
    }

    public List<WarriorController> GetWarriors()
    {
        return warriorControllers;
    }

    // Start is called before the first frame update
    public List<CellHandler> Init(UserData.OpponentData _details, PlayerData data, int num)
    {
        // Logger.Log($"Player Init - {userId}");

        playerData = data;

        playerDetails = _details;

        //this.userId = userId;
        DeactivePlayer(true);

        // arrowParticles.ForEach((x) =>
        // {
        //     var main = x.main;
        //     main.startColor = data.emmisionColor;
        // });

        // Get all the cells
        var cells = cellHolder.GetComponentsInChildren<CellHandler>().ToList().Where(x => x.gameObject.activeInHierarchy).ToList();
        cells.ForEach(c => c.cellId = $"{userId}_{cells.IndexOf(c)}");
       
        //Divesh - Change color of home_safeCells and end cell to player's color
        cells.FindAll(c => c.type == CellType.HOME_SAFE || c.type == CellType.END).
            ForEach(x => {
                x.cellModel.material.SetTexture(ConstantVars.BASE_MAP, playerData.homeCellsTex);
                x.cellModel.material.SetTexture(ConstantVars.EMISSION_MAP, playerData.homeCellsTex);
                // /* CommonGameObjectUtilities.ChangeMaterialColor(x.cellModel.materials[1], playerData.color);
                //  CommonGameObjectUtilities.ChangeEmmisionColor(x.cellModel.materials[1], playerData.emmisionStartColor);*/
            });

        cells.FindAll(c => c.type == CellType.HOME_SAFE || c.type == CellType.END).
           ForEach(x => x.cellExtraSprite.material.mainTexture = playerData.CellTopTex);

        cells.FindAll(c => c.type == CellType.HOME_SAFE || c.type == CellType.END).
          ForEach(x => x.cellExtraSprite.material.color = Color.white);




        startCell = cells.Find(c => c.type == CellType.START);
        endCell = cells.Find(c => c.type == CellType.END); 

        // Generate the warriors on base cells 
        // init warriors....
        baseCells = cells.FindAll(c => c.type == CellType.BASE);
        pillageCell = cells.Find(p => p.type == CellType.PILLAGE);

        //Divesh -  Fort is not instansiated anymore as only texture(material needs to be dynamic and not the gameobject)
        /*for (int i = 0; i < fortBase.transform.childCount; i++)
          {
              DestroyImmediate(fortBase.GetChild(i).gameObject);
          }*/

        var fort = fortBase.GetChild(ConstantVars.INT_VAL_ZERO);
        if(castleTop != null)
        {
            var castleTexture = playerData.castleTopTextures.FindIndex(x => x.id == thisCaslteType);
            castleTop.material.SetTexture(ConstantVars.BASE_MAP, playerData.castleTopTextures[castleTexture].castleTopVfx);
            castleTop.material.SetTexture(ConstantVars.EMISSION_MAP, playerData.castleTopTextures[castleTexture].castleTopVfx);
            // CommonGameObjectUtilities.ChangeMaterialColor(castleTop.material, playerData.color);
            blinkingMaterial = castleTop;
            // CommonGameObjectUtilities.ChangeEmmisionColor(castleTop.material, playerData.emmisionStartColor);
        }
      

        var fortBaseCells = fortBase.GetComponentsInChildren<CellHandler>().ToList().Where(x => x.gameObject.activeInHierarchy).ToList();
        for (int i = 0; i < baseCells.Count; i++)
        {
            baseCells[i].transform.position = fortBaseCells[i].transform.position;
        }

        availablePowers = new List<PowerType>(playerData.availablePowers);

        // playerUI.InitUI(this);
        // @Madhu
        camParent = MainCamera.transform.parent;
        // // cameraPositionFinder = camParent.GetComponent<CameraPositionFinder>();
        // @Madhu - flag animation
        // EventManager.FlagAnimEventCaller(FlagPrefab, flagMinRotation, flagMaxRotation, flagWaveDuration);
        // Madhu
        // playerUI._directionArrows.gameObject.GetComponent<Image>().sprite = GameController.instance.GetNonBotPlayerData().DirectionArrow;
        // DirectionObj = playerUI._directionArrows.gameObject;
        // // aiController = GetComponent<AiController>();
        return cells;
    }

    public void SetupWarriors()
    {
        for (int w = 0; w < baseCells.Count; w++)
        {
            // Logger.Log("Setup Warrior");
            var warriorPrefab = Instantiate(playerData.warriorPrefab, baseCells[w].transform.position, playerData.warriorPrefab.transform.rotation, transform);
            warriorControllers.Add(warriorPrefab);
            warriorPrefab.Init($"W_{w}", baseCells[w], this);
        }

        foreach(WarriorController warrior in warriorControllers)
        {
            // warrior.warriorAnimation.ResetToIdleAnimation();
        }
        
        //  playerUI._warrior.PlayAnimation(ConstantVars.IDLE);

        if(isBot() == false)
        {
            // playerUI._selectionArrows[0].gameObject.GetComponent<Image>().sprite = playerData.SelectionArrow;
            // playerUI._selectionArrows[1].gameObject.GetComponent<Image>().sprite = playerData.SelectionArrow;

            // warriorControllers[0].selectionIndicator = playerUI._selectionArrows[0].gameObject;
            // warriorControllers[1].selectionIndicator = playerUI._selectionArrows[1].gameObject;

            warriorControllers[0].AddListenerToSelectionArrow();
            warriorControllers[1].AddListenerToSelectionArrow();

        }
    }
       

    // Method to control gameplay on player and bots turn
    public void OnPlayerTurn(UnityAction<PlayerController> OnPlayerTurnComplete, bool powerTurnMove, bool isPowerCardAvilable = false)
    {
        PickIdleBlendShapeAnimation();
        canReDice = false;

        this.OnPlayerTurnComplete = OnPlayerTurnComplete;
        //Divesh - Disabled timer as per new requirment
        // playerUI.StartTimer(powerTurnMove); 

        // Logger.Log("Dice Initialize "+ powerTurnMove);
        GameController.instance.activeDice.Init(this, powerTurnMove);

        //for making the color blink
        ColorBlinkMore();


        // When its enemy/Bot turn
        if (isBot())
        {
            // @Madhu - Enable boundary around bots/enemy card
            // playerUI.EnableOpponentCardBoundary();
            // Madhu
        }
        
        // When its Players turn
        if(!isBot()){
            // @Madhu
            //  Invoke shine effects on player slot card 
            // playerUI.PlayShineEffect();
        }

        if (!powerTurnMove && !isBot())
        {
            // CommonUtilities.VibrateDevice();
            // AudioController.con.PlayClipOnTurnTextSource(GameController.instance.playerTurnClip);
        }

        // playerUI.EnableBorder();
    }

    Tween tween = null;
    private void ColorBlinkMore()
    {
        if(tween != null)
        {
            tween.Kill();
        }
        // Logger.Log("Player Name " + playerData.playerName);
        //  CommonGameObjectUtilities.ChangeMaterialColor(castleTop.material, playerData.color);
        // CommonGameObjectUtilities.ChangeEmmisionColor(blinkingMaterial.material, playerData.emmisionStartColor);
        tween = blinkingMaterial.material.DOVector(playerData.emmisionColor,ConstantVars.EMISSION_COLOR, glowAnimDuration).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }


    // Bot - Decides what to do when dice is rolled based on CheckBotPriority() and on power card used
    public void OnDiceRolled(int diceValue, bool skipTurn)
    {

        if (!GameController.isGameRunning) return;
        
        diceValue += 1;
        // Logger.Log($"Player - {userId} rolled dice - {diceValue} -- skip -> {skipTurn}");

        if (skipTurn)
        {
            OnPlayerTurnOver();
            return;
        }

        if (diceValue == 6) canReDice = true;

        // check for re-dice
        if (twoMovesPowerEnable)
        {
            twoMovesPowerEnable = (canReDice )? true : false;
            twiceActive = true;
            // twoMovesPowerEnable = false;
            canReDice = true;
            twoMoveActivvateAnimation = true;

        }
        
        if (diceDoublersPowerEnable)
        {

            diceDoublersPowerEnable = false;
            doublerActive = true;

            diceValue *= 2; // powerups

            // playerUI.ScalePowerEffect(PowerType.Doubler, () => { OnDiceRolled(diceValue - 1, skipTurn); }, true);
            return;
        }
      
        // player will decide according to dice value which pawn to active
        List<WarriorController> warriorsActive = new List<WarriorController>();

        foreach (var item in warriorControllers)
        {
            if (item.IsValidMove(diceValue)) 
            {
                warriorsActive.Add(item);
                item.IsRestricted = false;

            }
            else{
                item.IsRestricted = true;
            }
        }

        playerDiceValues.Add(diceValue);


        // Logger.Log($"Player {userId} - Warrior valid - {warriorsActive.Count}");

        if (warriorsActive.Count > 1) // then user will choose the warrior to move
        {  
            if (!isBot(false))
            {
                warriorsActive.ForEach(w => w.EnableWarrior(true));

            }
            else
                CheckBotPriority(warriorsActive);
        }
        else if (warriorsActive.Count == 1) // auto move the warrior 
        {
            // @Madhu - Doubler helper text
            if(doublerActive)
            {
                // HelperTextManager.PlayHelperTextAnime(PowerType.Doubler);
                // DOVirtual.DelayedCall(HelperTextManager.totalAnimDur, () =>  warriorsActive.First().MoveWarrior());
                doublerActive = false;
                
            }else
            {
                var activeWarrior  = warriorsActive.First();
                var otherWarrior = GetOtherWarrior(activeWarrior);

                activeWarrior.MoveWarrior();
                otherWarrior.PlayRestrictedVFX(true);

            }

        }
        else
        {

            foreach (var item in warriorControllers)
            {
                item.PlayRestrictedVFX(true , true);
            }
            DOVirtual.DelayedCall(warriorControllers[0].restrictedVFXDuration, () => 
            {
                // AudioController.con.PlayGameSFX(GameController.instance.invalidMoveClip);
                OnPlayerTurnOver();
            });

        }
    }
    
   

// Bot - Prioritise capture > unlock > move random warrior
    private void CheckBotPriority(List<WarriorController> warriorsActive)
    {
        if (warriorsActive.Find(w => w.CheckWarriorCanCapture()) != null)
        {
            // Logger.Log("Bot can capture !!!");
            warriorsActive.Find(w => w.CheckWarriorCanCapture()).MoveWarrior();
        }
        else if (warriorsActive.Find(w => w.CheckWarriorCanUnlock()) != null)
        {
            // Logger.Log("Bot Can unlock !!!");
            warriorsActive.Find(w => w.CheckWarriorCanUnlock()).MoveWarrior();
        }
        else
            warriorsActive[Random.Range(0, warriorsActive.Count)].MoveWarrior();
    }

    public void OnWarriorClicked(WarriorController warrior)
    {
        // disable the warrior clicks 
        warriorControllers.ForEach(w => { w.EnableWarrior(false); });

    }

    public void ResetPlayer()
    {
        warriorControllers.ForEach(w => w.playerDiceValue = 0);
        PowerUpSelectionActive = false;
        twoMovesPowerEnable = false;
        diceDoublersPowerEnable = false;
        twoMoveActivvateAnimation = false;
        
    }

    public void OnWarriorMoveComplete()
    {
        // Logger.Log("OnWarriorMoveComplete");
        PowerUpSelectionActive = false;

        float restrictedVfxdelay = 0; 
        foreach(var warrior in warriorControllers)
        {
            if(warrior.IsRestricted)
            {
                warrior.StopRestrictedVFXCountdown();
                restrictedVfxdelay = warrior.remainingTime;
            }

        }
        DOVirtual.DelayedCall(restrictedVfxdelay, () => OnPlayerTurnOver());

    }

    // Method to control gameplay on player turn  and bots turn is completed
    bool turnReset = false;
    public void OnPlayerTurnOver()
    {

        // Logger.Log("OnPlayerTurnOver");
        if (tween != null)
        {
            tween.Kill();
            // CommonGameObjectUtilities.ChangeMaterialColor(blinkingMaterial.material, playerData.color);
            // CommonGameObjectUtilities.ChangeEmmisionColor(blinkingMaterial.material, playerData.emmisionStartColor);
        }
        
        // Blend shape Animation
        PickIdleBlendShapeAnimation();

        //  when enemy/bot turn is completed
        if (isBot())
        {
           
            // @Madhu - Disable card boundary around enemy/bot card
            // playerUI.DisableOpponentCardBoundary();
            // Madhu
        }
        else
        {
            // playerUI.EnableDisableFadedDice(true);
        }

        OnWarriorClicked(null);
        playerCurrentPowerClicked = PowerType.NA;

        if (!canReDice)
        {
            warriorControllers.ForEach(w => w.CleanCaptureData());
            // playerUI.DisableBorder();
        }
        else
        {
            warriorControllers.ForEach(w => w.CleanCaptureData());
            // @Madhu - Twice helper text
            if (twiceActive && !twoMovesPowerEnable)
            {
                // HelperTextManager.PlayHelperTextAnime(PowerType.Twice);
                twiceActive = false;
            }
        }
        

        if (twoMoveActivvateAnimation && !canReDice)
        {

            // playerUI.ScalePowerEffect(PowerType.Twice, () => { OnPlayerTurnComplete?.Invoke(this); }, true);
            twoMoveActivvateAnimation = false;
        }
        else
        {
            OnPlayerTurnComplete?.Invoke(this);
            // EventManager.ChangeDiceVisibilityEventCaller(false);
        }

       
    }

    public void OnPillageComplete()
    {
        // if all the warrior has status == pillage done then player won
        isCompletedPillage = warriorControllers.FindIndex(w => w.state != WarriorState.PillageDone) == -1;

        if (isCompletedPillage)
        {
            score = GameController.instance.currentPlayingPlayerCount * 100;
            GameController.instance.rankOrderList.Add(this);
            // playerUI.OnPillageCompleteUpdateUI();
        }
    }

    public void DirectionInit(UnityAction<PlayerController> OnDirectionClick)
    {   
        // @Madhu - Direction selction text animation 
        // DOVirtual.DelayedCall(ConstantVars.VAL_HALF, () => EventManager.SelectDirectionTextAnimEventCaller());
        
        var directionTrigger = DirectionObj.GetComponent<EventTrigger>();
        directionTrigger.triggers.Clear();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((data) =>
        {
            OnDirectionClick?.Invoke(this);
        });

        directionTrigger.triggers.Add(entry);
        DOVirtual.DelayedCall(2f, () => EnableDirection(true));
        
    }

    public void EnableDirection(bool isEnable)
    {
        // playerUI.SetDirectionIndicatorPosition(DirectionObj, DirectionObj.transform.position, this);
        DirectionObj.SetActive(isEnable);
    }

    public CellHandler GetBaseCell()
    {
        if (baseCells.Count > 0)
        {
            return baseCells.OrderBy(b => b.warriorsInCell.Count).ToList().First();
        }
        else
        {
            // Logger.LogError("No base cell !!!");
            return startCell;
        }
    }

    public List<CellHandler> GetFortCells()
    {
        List<CellHandler> fortCells = new List<CellHandler>();
        var cells = cellHolder.GetComponentsInChildren<CellHandler>().ToList().Where(x => x.gameObject.activeInHierarchy).ToList();
        fortCells.AddRange(cells.FindAll(x => x.type == CellType.PILLAGE || x.type == CellType.BASE));
        return fortCells;
    }

    public bool CheckCellIsBaseCell(CellHandler cell)
    {
        return baseCells.Exists(c => c.cellId == cell.cellId);
    }

    // Bot - what to do or on which warrior to use when power is selected
    #region Power ups 
    public void PlayerPowerUsed(PowerType type, bool isBot)
    {
        var _playerData = UserInterface.instance.playerSaveData;
        _playerData.UpdateFavData(_powerUsed:type);
        UserInterface.instance.playerSaveData = _playerData;

        GameController.instance.AddXP(this, 20);


        // remove power from the list and trigger power
        playerCurrentPowerClicked = (type);
        availablePowers.Remove(type);

        // check if player has type of power 
        switch (type)
        {
            case PowerType.Release:

                // activate warrior in home base 
                var _homeWarriors = (warriorControllers.FindAll(w => w.state == WarriorState.None));

                if (_homeWarriors != null && _homeWarriors.Count == 1) // then auto move
                {
                    releaseActive = true;
                    _homeWarriors.First().MoveWarrior(1, false);                    
                }
                else if (_homeWarriors != null && _homeWarriors.Count > 1)
                {
                    PowerUpSelectionActive = true;

                    if (!isBot)
                    {
                        _homeWarriors.ForEach(w =>
                        { 
                            if (w.state == WarriorState.None && w.IsValidMove(1))
                            {
                                w.EnableWarrior(true);
                                releaseActive = true;
                                // playerUI.SetSelectionIndicatorDiceValue(1, false);
                            }
                        });

                    }// Bot - select random warrior for release
                    else
                    {
                        releaseActive = true;
                        var randWarrior = _homeWarriors[Random.Range(0, _homeWarriors.Count)];
                        randWarrior.MoveWarrior(1, false);


                    }
                }



                // set the dice value and can-redice
                canReDice = true;
                isPowerupTurnMove = true;


                break;
            case PowerType.Shield:
                // activate warrior in home base 
                _homeWarriors = (warriorControllers.FindAll(w => w.state != WarriorState.None && w.state != WarriorState.PillageDone));


                // set the dice value and can-redice
                canReDice = true;
                isPowerupTurnMove = true;

                if (_homeWarriors != null && _homeWarriors.Count == 1) // then auto move
                {
                    PowerUpSelectionActive = true;
                    OnPointerDownAutoMove = true;
                    _homeWarriors.First().OnPointerDown(null);
                    // EventManager.(PromptType.UseDice);

                   
                }
                else if (_homeWarriors != null && _homeWarriors.Count > 1)
                {
                    PowerUpSelectionActive = true;

                    if (!isBot)
                    {
                        _homeWarriors.ForEach(w =>
                        {
                            // playerUI.SetSelectionIndicatorDiceValue(0, false);
                            w.EnableWarrior(true);
                        });


                    }// Bot -
                    else
                    {
                        //commented due to new feature
                        //_homeWarriors[Random.Range(0, _homeWarriors.Count)].OnPointerDown(null);

                        var otherPlayers = GameController.instance.GetOtherPlayers(userId);
                        bool shouldBreak = false;
                        for (int i = 0; i < otherPlayers.Count; i++)
                        {
                            var otherWarriors = otherPlayers[i].GetActiveWarriors();

                            for (int j = 0; j < otherWarriors.Count; j++)
                            {
                                if (shouldBreak == false)
                                {
                                    var diff = 7;

                                    var currentPath = otherWarriors[j].currentPath;
                                    var currentCell = otherWarriors[j].currentCell;

                                    var currentIndex = currentPath.FindIndex(c => c.cellId == currentCell.cellId);

                                    if (currentIndex < 0) currentIndex = 0;

                                    if ((currentIndex + 7) > currentPath.Count)
                                        diff = currentPath.Count - currentIndex;

                                    var futurePath = currentPath.GetRange(currentIndex, diff);

                                    for (int k = 0; k < _homeWarriors.Count; k++)
                                    {
                                        if (futurePath.Contains(_homeWarriors[k].currentCell) && shouldBreak == false)
                                        {
                                            shieldActive = true;
                                            shouldBreak = true;
                                            OnPointerDownAutoMove = true;
                                            _homeWarriors[k].OnPointerDown(null);

                                        }
                                    }
                                }

                            }

                        }
                    }
                }

                break;
            case PowerType.Revive:

                // set the dice value and can-redice
                canReDice = true;
                isPowerupTurnMove = true;

                // activate warrior in home base 
                _homeWarriors = (warriorControllers.FindAll(w => w.captureData.isCapture));

                if (_homeWarriors != null && _homeWarriors.Count == 1) // then auto move
                {
                    _homeWarriors.First().ReviveWarrior(); 

                }
                else if (_homeWarriors != null && _homeWarriors.Count > 1)
                {
                    PowerUpSelectionActive = true;
                    if (!isBot)
                    {
                        _homeWarriors.ForEach(w =>
                        {
                            w.EnableWarrior(true);
                            // playerUI.SetSelectionIndicatorDiceValue(6, false);
                        });

                    }
                    else
                    {
                        var randWarrior = _homeWarriors[Random.Range(0, _homeWarriors.Count)];
                        randWarrior.ReviveWarrior();
                        
                    }
                }
                break;
            case PowerType.Lock:

                //Play card click
                // AudioController.con.PlayUISFX(GameController.instance.lockCastleSound);
// 
                warriorControllers.ForEach(w => w.AddPower(type));

                // set the dice value and can-redice
                canReDice = true;
                isPowerupTurnMove = true;
                OnWarriorMoveComplete();

                break;
            case PowerType.Doubler:
                // added code to dowble dice value once roll

                canReDice = true;
                isPowerupTurnMove = true;
                OnDiceRolled(0, true);
                diceDoublersPowerEnable = true;

                break;
            case PowerType.Twice:

                // Added code in dice == 6 ||| power up then re - dice
                canReDice = true;
                isPowerupTurnMove = true;
                OnDiceRolled(0, true);
                twoMovesPowerEnable = true;

                break;
            case PowerType.Arrows:

                canReDice = true;
                isPowerupTurnMove = true;

                break;
            default:
                break;
        }

    }

    // Bot - refreshes the list of power card avilable
    public List<PowerType> ActivePowerups(bool isMyOneCycleComplete)
    {
        // check for one cycle complete for player turn...
        if (isMyOneCycleComplete)
        {
            warriorControllers.ForEach(w => w.CleanPowers());
        }

        List<PowerType> activePowers = new List<PowerType>();

        foreach (var item in availablePowers)
        {
            switch (item)
            {
                case PowerType.Release:
                    // check the condition 
                    // Unleash warrior - if any warrior exsists in base else disable
                    if (warriorControllers.Exists(w => w.state == WarriorState.None))
                        activePowers.Add(item);
                    break;

                case PowerType.Shield:
                    // all the warriors should be outside the home base
                    if (warriorControllers.Exists(w => w.state != WarriorState.None && w.state != WarriorState.PillageDone))
                        activePowers.Add(item);
                    break;

                case PowerType.Revive:
                    //TODO
                   // activePowers.Add(item);
                    // Logger.Log("ISCAPTURE==========================" + (warriorControllers.Exists(w => w.captureData.isCapture)));
                    if (warriorControllers.Exists(w => w.captureData.isCapture))
                        activePowers.Add(item);
                    break;

                case PowerType.Lock:
                    activePowers.Add(item);
                    break;

                case PowerType.Doubler:
                    // Need active waariors to move double steps 
                    // toddo = made available on each turn
                    //if (warriorControllers.Exists(w => w.state != WarriorState.None))
                    //{
                    activePowers.Add(item);
                    //}
                    break;
                case PowerType.Twice:
                    activePowers.Add(item);
                    break;
                case PowerType.Arrows:
                    int oppCount = GameController.instance.players.FindAll(p=>p.isPlayerBot).Sum(w=>w.GetActiveWarriors().Count());
                    if(oppCount>0)
                    activePowers.Add(item);
                    break;
                default:
                    break;
            }
        }

        return activePowers;
    }


    public bool IsAnyWarriorActive() => warriorControllers.Exists(x => x.state == WarriorState.Active || x.state == WarriorState.Pillage);

    public bool IsPowerRunning(PowerType power)
    {
        if (power == PowerType.Lock) // check if any player warrior has lock castle
        {
            return warriorControllers.Exists(w => w.runningPowers.Contains(power));
        }

        return playerCurrentPowerClicked.Equals(power);
    }

    public void EnableDisableCastleLock(bool isActive)
    {
        var _obj = vfxs.Find(x => x.id == VFXID.LOCKHOME).vfxobject;
        if (isActive)
        {
            _obj.SetActive(isActive);
            castleLock.SetActive(true);
            castleLock.transform.DOLocalMoveY(0, 0.5f);
            // @Madhu - Lock helper text
            // DOVirtual.DelayedCall(0.5f, () => HelperTextManager.PlayHelperTextAnime(PowerType.Lock));
            // Madhu
        }
        else
        {
            if (_obj.activeSelf)
            {
                castleLock.GetComponent<MeshRenderer>().material.DOFloat(1, ConstantVars.DISSOLVE, 4f).OnComplete(() => castleLock.SetActive(false));
            }
        }
    }


    public List<WarriorController> GetActiveWarriors()
    {
        return warriorControllers.Where(w => w.state != WarriorState.None && w.state != WarriorState.PillageDone).ToList();
    }

    public List<WarriorController> GetLockWarriors()
    {
        return warriorControllers.Where(w => w.state == WarriorState.None).ToList();
    }

    #endregion


    #region VFX 

    public List<VFXDATA> vfxs;

    public void EnablePower(PowerType _type, bool isEnable)
    {
        if (_type == PowerType.Lock)
        {
            
            EnableDisableCastleLock(isEnable);
        }

        
    }
    public void PlayGroundBlast()
    {
        var _obj = vfxs.Find(x => x.id == VFXID.GROUNDBLAST).vfxobject;
        _obj.SetActive(true);
        DOVirtual.DelayedCall(2f, () => { _obj.SetActive(false); });
        // AudioController.con.PlayGameSFX(GameController.instance.thudEffectClip);
    }

    #endregion


    //  @Madhu
    public void PickIdleBlendShapeAnimation()
    {
        // warriorControllers.ForEach(w => w.warriorAnimation.ResetToIdleAnimation());
        // playerUI._warrior.ResetAndPlay(ConstantVars.IDLE);

   /*     var otherPlayers = GameController.instance.GetOtherPlayers(userId);
        foreach(var otherPlayer in otherPlayers)
        {
            if((progressCalculator.GetPlayerProgress() - otherPlayer.progressCalculator.GetPlayerProgress()) >= 20)
            {
               otherPlayer.warriorControllers.ForEach(w => w.warriorAnimation.SadFaceAnimation());
                // otherPlayer.playerUI._warrior.PlayWarriorAnimation(ConstantVars.SADFULL);
            }
            else if((otherPlayer.progressCalculator.GetPlayerProgress() - progressCalculator.GetPlayerProgress()) >= 20 ) 
            {
               warriorControllers.ForEach(w => w.warriorAnimation.SadFaceAnimation());
                playerUI._warrior.SadFaceAnimation();
                break;
            }
            else
            {
               
            }
        }*/

    }

    public void WarriorSelectionCameraView()
    {

        var warrior1 = warriorControllers[0];
        var warrior2 = warriorControllers[1];
        // bool zoom;
        bool bothWarriorInBase = (CheckWarriorInBase(warrior1) && CheckWarriorInBase(warrior2)) ? true : false;
        bool warrior1InBase = (CheckWarriorInBase(warrior1) == true && CheckWarriorInBase(warrior2) == false) ? true : false;
        bool warrior2InBase = (CheckWarriorInBase(warrior2) == true && CheckWarriorInBase(warrior1) == false) ? true : false;
        bool bothWarriorInPillage = (CheckWarriorInPillage(warrior1) && CheckWarriorInPillage(warrior2)) ? true : false;
        bool warrior1InPillage = (CheckWarriorInPillage(warrior1) == true && CheckWarriorInPillage(warrior2) == false) ? true : false;
        bool warrior2InPillage = (CheckWarriorInPillage(warrior2) == true && CheckWarriorInPillage(warrior1) == false) ? true : false;

        if (bothWarriorInBase || bothWarriorInPillage)
        {
            // cameraPositionFinder.WarriorsNearCaseCamView(warrior1.warriorTransform, warrior2.warriorTransform);
        }
        else if (warrior1InBase)
        {
            if (CheckWarriorInProximityToBase(warrior2))
            {
                // cameraPositionFinder.WarriorsNearCaseCamView(warrior1.warriorTransform, warrior2.warriorTransform);
            }
            else
            {
                // Logger.Log("======================Cannot zoom");
                foreach (WarriorController warrior in warriorControllers)
                {
                    warrior.SwitchWarriorSelectionButtons();
                }
            }
        }
        else if (warrior2InBase)
        {
            if (CheckWarriorInProximityToBase(warrior1))
            {
                // cameraPositionFinder.WarriorsNearCaseCamView(warrior1.warriorTransform, warrior2.warriorTransform);
            }
            else
            {
                // Logger.Log("======================Cannot zoom");
                foreach (WarriorController warrior in warriorControllers)
                {
                    warrior.SwitchWarriorSelectionButtons();
                }
            }

        }
        else if (warrior1InPillage)
        {
            if (CheckWarriorInProximityToPillage(warrior2, warrior1))
            {
                // cameraPositionFinder.WarriorsNearCaseCamView(warrior1.warriorTransform, warrior2.warriorTransform);
            }
            else
            {
                // Logger.Log("======================Cannot zoom");
                foreach (WarriorController warrior in warriorControllers)
                {
                    warrior.SwitchWarriorSelectionButtons();
                }
            }
        }
        else if (warrior2InPillage)
        {
            if (CheckWarriorInProximityToPillage(warrior1, warrior2))
            {
                // cameraPositionFinder.WarriorsNearCaseCamView(warrior1.warriorTransform, warrior2.warriorTransform);
            }
            else
            {
                // Logger.Log("======================Cannot zoom");
                foreach (WarriorController warrior in warriorControllers)
                {
                    warrior.SwitchWarriorSelectionButtons();
                }
            }
        }
        else if (CellsBtwWarriors(warrior1, warrior2))
        {
            // cameraPositionFinder.WarriorsNearCaseCamView(warrior1.warriorTransform, warrior2.warriorTransform);
        }
        else
        {
            // Logger.Log("======================Cannot zoom");
            foreach (WarriorController warrior in warriorControllers)
            {
                warrior.SwitchWarriorSelectionButtons();
            }
        }
    }

    // public void WarriorSelectionCameraView()
    // {
    //     var warrior1 = warriorControllers[0];
    //     var warrior2 = warriorControllers[1];

    //     if (bothWarriorInBase() || bothWarriorInPillage())
    //     {
            // cameraPositionFinder.WarriorsNearCaseCamView(warrior1.warriorTransform, warrior2.warriorTransform);
    //         return;
    //     }

    //     if (warrior1InBase() && CheckWarriorInProximityToBase(warrior2))
    //     {
            // cameraPositionFinder.WarriorsNearCaseCamView(warrior1.warriorTransform, warrior2.warriorTransform);
    //         return;
    //     }

    //     if (warrior2InBase() && CheckWarriorInProximityToBase(warrior1))
    //     {
            // cameraPositionFinder.WarriorsNearCaseCamView(warrior1.warriorTransform, warrior2.warriorTransform);
    //         return;
    //     }

    //     if (warrior1InPillage() && CheckWarriorInProximityToPillage(warrior2, warrior1))
    //     {
            // cameraPositionFinder.WarriorsNearCaseCamView(warrior1.warriorTransform, warrior2.warriorTransform);
    //         return;
    //     }

    //     if (warrior2InPillage() && CheckWarriorInProximityToPillage(warrior1, warrior2))
    //     {
            // cameraPositionFinder.WarriorsNearCaseCamView(warrior1.warriorTransform, warrior2.warriorTransform);
    //         return;
    //     }

    //     if (CellsBtwWarriors(warrior1, warrior2))
    //     {
            // cameraPositionFinder.WarriorsNearCaseCamView(warrior1.warriorTransform, warrior2.warriorTransform);
    //         return;
    //     }
    // }

    // private bool bothWarriorInBase()
    // {
    //     return CheckWarriorInBase(warriorControllers[0]) && CheckWarriorInBase(warriorControllers[1]);
    // }

    // private bool warrior1InBase()
    // {
    //     return CheckWarriorInBase(warriorControllers[0]) && !CheckWarriorInBase(warriorControllers[1]);
    // }

    // private bool warrior2InBase()
    // {
    //     return CheckWarriorInBase(warriorControllers[1]) && !CheckWarriorInBase(warriorControllers[0]);
    // }

    // private bool bothWarriorInPillage()
    // {
    //     return CheckWarriorInPillage(warriorControllers[0]) && CheckWarriorInPillage(warriorControllers[1]);
    // }

    // private bool warrior1InPillage()
    // {
    //     return CheckWarriorInPillage(warriorControllers[0]) && !CheckWarriorInPillage(warriorControllers[1]);
    // }

    // private bool warrior2InPillage()
    // {
    //     return CheckWarriorInPillage(warriorControllers[1]) && !CheckWarriorInPillage(warriorControllers[0]);
    // }
    
    // check if the numer of cells bewtween 2 warriors is < 6
    private int _minCellBtwWarrioForZoom = 6;

    private bool CellsBtwWarriors(WarriorController warrior1, WarriorController warrior2)
    {   
        List<CellHandler> centerToWarrior1Path = new List<CellHandler>();
        List<CellHandler> centerToWarrior2Path = new List<CellHandler>();

        centerToWarrior1Path = GameController.instance.gridMap.PathFromCenter(warrior1.currentCell );
        centerToWarrior2Path = GameController.instance.gridMap.PathFromCenter(warrior2.currentCell );

        bool Warrior1PathHasWarrior2 = centerToWarrior1Path.Exists( c => c.cellId == warrior2.currentCell.cellId);
        bool Warrior2PathHasWarrior1 = centerToWarrior2Path.Exists( c => c.cellId == warrior1.currentCell.cellId);

        int totalCellCount;
        totalCellCount = (Warrior1PathHasWarrior2 || Warrior2PathHasWarrior1) ? Mathf.Abs(centerToWarrior1Path.Count - centerToWarrior2Path.Count) : ((centerToWarrior1Path.Count -1) + (centerToWarrior2Path.Count -1));

        
        return (totalCellCount < _minCellBtwWarrioForZoom) ? true : false;
    }

    private bool CheckWarriorInBase(WarriorController warrior)
    {
        return (warrior.currentCell.type == CellType.BASE) ? true : false ;
    }

    private bool CheckWarriorInProximityToBase(WarriorController warrior)
    {
        int warriorDistanceFromBase = _minCellBtwWarrioForZoom -1;
        
        List<CellHandler> centerToStartPath = new List<CellHandler>();
        centerToStartPath = GameController.instance.gridMap.PathFromCenter(startCell);

        int currentIndex;


        if(centerToStartPath.Exists( c => c.cellId == warrior.currentCell.cellId))
        {
            currentIndex = centerToStartPath.FindIndex(c => c.cellId == warrior.currentCell.cellId);
            return (currentIndex < warriorDistanceFromBase) ? true : false ;
        }
        else
        {
            return false;
        }
        
    }

    private bool CheckWarriorInPillage(WarriorController warrior)
    {
        return (warrior.currentCell.type == CellType.PILLAGE) ? true : false ;
    }

    private bool CheckWarriorInProximityToPillage(WarriorController warrior1, WarriorController warrior2)
    {
        // get path from center to start cell
        // check if warrior1 cellid  exists in that list if it does get index and check count
        // var pillageCell = warrior2.
        int warriorDistanceFromPilage = _minCellBtwWarrioForZoom -1;

        List<CellHandler> centerToStartPath = new List<CellHandler>();
        centerToStartPath = GameController.instance.gridMap.PathFromCenter(warrior2.targetPlayer.startCell);

        int currentIndex;


        if(centerToStartPath.Exists( c => c.cellId == warrior1.currentCell.cellId))
        {
            currentIndex = centerToStartPath.FindIndex(c => c.cellId == warrior1.currentCell.cellId);
            return (currentIndex < warriorDistanceFromPilage) ? true : false ;
        }
        else
        {
            return false;
        }
        
    }

    public WarriorController GetOtherWarrior(WarriorController warrior)
    {
        var unSelectedWarrior = GetWarriors().Find(w => w.warriorId != warrior.warriorId);
        return unSelectedWarrior;
    }
    // @MADHU

    public void CastleClicked()
    {
        if(!isBot() && GameController.instance.GetCurrentPlayer == this)
        {
            // switch(PlayerTurnReminderTimer.runningPrompt)
            // {
            //     case PromptType.UseCardOrDice:
            //         // EventManager.UserdialougeTextAnimEventCaller(DialougeType.UserUseCardOrDice, null, -1);
            //     break;
            //     case PromptType.UseDice:
            //         // EventManager.UserdialougeTextAnimEventCaller(DialougeType.UserUseDice, null, -1);
            //     break;
            // }
        }
    }

}

