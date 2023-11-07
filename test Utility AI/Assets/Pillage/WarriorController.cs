using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;
using ConstantVariables;
using UnityEngine.UI;

public class WarriorController : MonoBehaviour, IPointerDownHandler
{
    // @Madhu - for stunned, trail, coin shower VFX
    public float restrictedVFXDuration;
    public bool IsRestricted = false;
    public bool restrictedVFXStopped = false;
    public GameObject RestrictedVfxEffect;
    public GameObject starsOnHeadVfxEffect;
    public GameObject WarriorSparkleTrail;
    public GameObject WarriorSparkleTrail2;
    public GameObject CoinShowerVfxEffect;
    // public WarriorAnimation warriorAnimation;

    // Madhu
    private const string playerTweenId = "PlayerMove";
    public CellHandler currentCell;
    public List<CellHandler> currentPath;
    // @Madhu - variable for cell index of the warrior
    public int currentIndex;
    public List<CellHandler> path1;
    public List<CellHandler> path2;
    public CellHandler warriorBaseCell;
    // Madhu
    public PlayerController targetPlayer;
    public PlayerController player;
    public WarriorState state;
    public Vector3 _moveOffset;
    public string warriorId;
    public int playerDiceValue;

    [Header("UI position elements")]
    public GameObject selectionIndicator;
    public GameObject selectionIndicatorParent;
    public Transform infoPosition;
    private string checkpoint = "CHECK\nPT.";
    private Vector2 overrideInfoOffest = new Vector2(140, -50);


    public List<PowerType> runningPowers = new List<PowerType>();
    private const float jumpDefaultPow = 0.2f;
    private  float lastJumpPow = 0.7f;
    // @Madhu - Reference to main camera which is the target for pawns to look at
    private Transform _cam;
    private GameObject _camParent;
    // private CameraZoomController cameraZoomController;
    // @Madhu - for Capture animation 
    // bool to chceck if pawn should look at camera
    public bool canLookAtCam = false;
    private float _captureWarriorCamZoom = 40;
    private float _captureWarriorCamZoomSpeed = 0.5f;
    private float _warriorGoHeavenCamZoom = 100;
    private float _slowMoTimeScale = 0.7f;
    // @Madhu - for Revive text animation
    private bool reviveActive;

    //Divesh - Actual Mesh object of warrior
    public Transform warriorMesh;

    [Header("Shield PowerUp Settings")]
    [SerializeField]
    private float _jumpValue;
    [SerializeField]
    private float _jumpForce;
    [SerializeField]
    private float _jumpDuration;
    [SerializeField]
    private Transform _shieldParent;
    [SerializeField]
    private float _shieldRotationDuration;
    [SerializeField]
    private float _shieldDisableDuration;
    Vector3 vector = new Vector3(0, 360f, 0);

    public bool canMove;
    // @Madhu - for destination path indicator
    private bool warriorReleased = false;
    private bool warriorReleasedOnlyOnce = false;
    private bool isPillaged = false;
    // Madhu

    public struct WarriorCaptureData
    {
        public bool isCapture;
        public PlayerController targetPlayer;
        public List<CellHandler> targetPath;
        public CellHandler cellIndex;
        //public WarriorState state;

        public WarriorCaptureData(bool _isCapture, PlayerController _target, List<CellHandler> _path, CellHandler _current)
        {
            isCapture = _isCapture;
            targetPlayer = _target;
            targetPath = _path;
            cellIndex = _current;
        }
    }

    public WarriorCaptureData captureData = new WarriorCaptureData();
    // @Madhu - Event to trigger pawn rotation as camera will turn
    void OnEnable()
    {
        // EventManager.PawnLookAtCameraAnimationEvent += WarriorLookAtCam;
    }

    void OnDisable()
    {
        // EventManager.PawnLookAtCameraAnimationEvent -= WarriorLookAtCam;
    }
    // Madhu

    public void AddListenerToSelectionArrow()
    {
        selectionIndicator.GetComponent<Button>().onClick.AddListener(() => OnWarriorClicked());

        var directionTrigger = selectionIndicator.GetComponent<EventTrigger>();
        directionTrigger.triggers.Clear();

        EventTrigger.Entry Pressed = new EventTrigger.Entry();
        Pressed.eventID = EventTriggerType.PointerDown;
        Pressed.callback.AddListener((data) =>
        {
            // EventManager.PlayerReminderTimerStopEventCaller();

        });

        directionTrigger.triggers.Add(Pressed);

        EventTrigger.Entry released = new EventTrigger.Entry();
        released.eventID = EventTriggerType.PointerUp;
        released.callback.AddListener((data) =>
        {
            // EventManager.PlayerReminderTimerRestartEventCaller();

        });

        directionTrigger.triggers.Add(released);
    }
    
    public void SetUpSelectionArrowSprite()
    {
        var selectionIndicatorSprite = selectionIndicator.GetComponent<Image>().sprite;
        selectionIndicatorSprite = player.playerData.SelectionArrow;
    }
    public void Init(string id, CellHandler cell, PlayerController player)
    {
        // @Madhu - cache reference to camera
        _cam = player.MainCamera.transform;
        _camParent = _cam.parent.gameObject;
        // // cameraZoomController = _camParent.GetComponent<CameraZoomController>();
        // // warriorAnimation = GetComponentInChildren<WarriorAnimation>();

        warriorId = $"{player.userId}_{id}";
        SetWarriorToDefaultState();
        currentCell = cell;
        warriorBaseCell = cell;
        this.player = player;

        // add warrior to cell slot
        currentCell.AddWarriorToCell(this);

        // get the path move the from current cell to destination
        targetPlayer = GameController.instance.GetRandomTarget(player.userId);
        
        UpdateCurrentPath(player.startCell, targetPlayer.startCell, cell, targetPlayer.pillageCell , true);
        UpdateCurrentPath(targetPlayer.startCell, player.endCell, null, null, false);

        
        // @Madhu - initialize progress calculator 
        player.progressCalculator.Init(player);
        // Madhu

        EnableWarrior(false);
        GetAllPossiblePaths(cell);
        restrictedVFXDuration = GetParticleSystemDuration(RestrictedVfxEffect);

        // @Madhu - setup warrior selection indicator color
        // var warriorId1 = $"{player.userId}_W_0";
        // var warriorId2 = $"{player.userId}_W_1";
        // if(player.isBot() == false)
        // {
        //     if(warriorId == warriorId1)
        //     {
        //         selectionIndicator = player.playerUI._selectionArrows[0].gameObject;  // Blue
        //     }
        //     else if(warriorId == warriorId2)
        //     {
        //         selectionIndicator = player.playerUI._selectionArrows[1].gameObject;  // Red
        //     }
        // AddListenerToSelectionArrow();
        // }
        var colliders = transform.GetComponentsInChildren<BoxCollider>().ToList();
        colliders.ForEach(c => c.enabled = true);
    }

    // This method checks for valid moves using the dice value and warrior state, cell type and lock castle power card usage
    public bool IsValidMove(int diceValue)
    {
        //diceValue += 1;
        playerDiceValue = diceValue;

        // if state = none, and dice value != 6 -> false
        if (state == WarriorState.None &&( playerDiceValue == 6 || playerDiceValue == 1))
        {
            if (runningPowers.Contains(PowerType.Lock))
                return false;
            return true;
        }

        else if (state == WarriorState.PillageDone) return false;

        else if (state != WarriorState.None && currentCell.type != CellType.BASE)
        {
            // if currnetCell + dice value is > path count -> false 
            var pillageIndex = currentPath.FindIndex(c => c.cellId == targetPlayer.pillageCell.cellId);
            var afterPillagePath = currentPath.GetRange(pillageIndex, currentPath.Count - pillageIndex);
            int currentIndex = currentPath.FindIndex(c => c.cellId == currentCell.cellId);

            if (currentIndex == pillageIndex)
            {
                var baseCells = afterPillagePath.FindAll(c => c.type == CellType.PILLAGE);

                if (baseCells != null && GameController.instance.powerupHandle.CheckLockCastlePower(baseCells))
                    return false;

            }

            if (state == WarriorState.Pillage)
            {
                currentIndex = pillageIndex + afterPillagePath.FindIndex(c => c.cellId == currentCell.cellId);
            }

            // if (currentIndex < 0) Logger.LogError($"Current Cell not found - {currentCell.cellId}");

            //var path = currentPath.GetRange(currentIndex, playerDiceValue);
            currentIndex++;
            if (currentPath.Count >= (currentIndex + playerDiceValue))
            {
                //TODO - re-cehck the calculation
                var futurePath = currentPath.GetRange(currentIndex, diceValue);
                var baseCells = futurePath.FindAll(c => c.type == CellType.PILLAGE);

                if (baseCells != null && GameController.instance.powerupHandle.CheckLockCastlePower(baseCells))
                    return false;

                return true;
            }
            else
                return false;
        }

        else return false;
    }

    public void EnableWarrior(bool isEnable)
    {
        canMove = isEnable;
        //disable the text
        // player.playerUI.ShowText("SELECT A WARRIOR BY TAPPING ITS ARROW", 22f);
        if (isEnable)
        {
            player.WarriorSelectionCameraView();
        }
      
    }

    public void SwitchWarriorSelectionButtons()
    {   
        // @Madhu - setup warrior selection indicator color
        // bool isSameCell = false;

        // if(player.isBot() == false)
        // {
        //     var warriorId1 = $"{player.userId}_W_0";
        //     if(warriorId == warriorId1)
        //     {
        //         selectionIndicator = player.playerUI._selectionArrows[0].gameObject;  // Blue
        //     }
        //     // @Madhu activate last cell vfx indicator
        //     if(player.playerCurrentPowerClicked != PowerType.Shield)
        //     {

        //         var otherWarrior = player.GetWarriors().Find(w => w.warriorId != warriorId);
        //         var otherWarrioLastCells = otherWarrior.GetPathLastCell();
        //         var thisWarrioLastCells = GetPathLastCell();
        //         for(int i = 0; i < otherWarrioLastCells.Count; i++)
        //         {
        //             for(int j = 0; j < thisWarrioLastCells.Count; j++)
        //             {
        //                 if(otherWarrioLastCells[i].cellId == thisWarrioLastCells[j].cellId)
        //                 {
        //                     if(warriorId == warriorId1)
        //                     {
        //                         selectionIndicator = player.playerUI._selectionArrows[2].gameObject;  // Red
        //                     }
        //                     isSameCell = true;
        //                 }
        //             }
        //         }
        //     }
        // }


        if (selectionIndicator != null)
        {
            // player.playerUI.SetSelectionIndicatorPosition(selectionIndicator, selectionIndicatorParent.transform.position);

            selectionIndicator.SetActive(canMove);
            // EventManager.PlayerReminderTimerStartEventCaller(PromptType.SelectWarrior);

        }

        // var colliders = transform.GetComponentsInChildren<BoxCollider>().ToList();
        // colliders.ForEach(c => c.enabled = canMove);

        ///todo turn on selection player ui

        // @Madhu activate last cell vfx indicator
        if(player.playerCurrentPowerClicked != PowerType.Shield)
        {
            ActivateWarriorLastCellIndicator(); //isSameCell

        }


        // Logger.Log("==============================================================CAN MOVE");

    }

    void ResetCam()
    {
        canLookAtCam = false;
        DOTween.Kill(warriorTransform);
        // if (!player.isBot() && player.cameraPositionFinder.IsZoomedIn)
        {
            // player.cameraPositionFinder.Reset();
           
        }
    } 

   
    // @MADHUSUDAN
    public void OnWarriorClicked()
    {
        // @MADHUSUDAN TODO
        // EventManager.PlayerReminderTimerRestartEventCaller();

        // if(!isAutoSelect)
        // {
        //     player.GetOtherWarrior(this).PlayRestrictedVFX();
        //     player.IsAutoSelect = false;
        // }
        // foreach(var warrior in player.GetWarriors())
        // {
        //     if(warrior.RestrictedVfxEffect.GetComponent<ParticleSystem>().isPlaying)
        //     {
        //         warrior.PlayRestrictedVFX(false);
        //     }
        // }
        
        player.OnWarriorClicked(this);
        var warriors = player.GetWarriors();
        warriors.ForEach(x => x.SwitchWarriorSelectionButtons());

        // @Madhu - Disable last cell vfx for for unselected warrior

        if (!player.isBot())
        {
            var otherWarrior = player.GetWarriors().Find(w => w.warriorId != warriorId);
            for (int i = 0; i < otherWarrior.lastCells.Count; i++)
            {
                for (int j = 0; j < lastCells.Count; j++)
                {
                    if (otherWarrior.lastCells[i].cellId != lastCells[j].cellId)
                    {
                        otherWarrior.lastCells[i].WarriorLastCellIndicator(false, otherWarrior);
                    }
                }

            }
        }

        // MADHU
        ResetCam();
        // Perform move animation and disable
        // Logger.Log($"Warrior click - {warriorId}");

        // power up checks
        if (player.PowerUpSelectionActive)
        {
            if (player.playerCurrentPowerClicked.Equals(PowerType.Shield))
            {
                //Play card click
                // AudioController.con.PlayUISFX(GameController.instance.shieldAudioClip);

                AddPower(PowerType.Shield);
                 player.OnWarriorMoveComplete();
                return;
            }
            else if (player.playerCurrentPowerClicked.Equals(PowerType.Revive))
            {
                ReviveWarrior();
                return;
            }
        }
        
        if(player.doublerActive)
        {
            // @Madhu - Doubler helper text
            // HelperTextManager.PlayHelperTextAnime(PowerType.Doubler);
            // DOVirtual.DelayedCall(HelperTextManager.totalAnimDur, () => MoveWarrior());
            player.doublerActive = false;
            return;
        }

        DOVirtual.DelayedCall(0.8f, () => MoveWarrior());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(!player.isBot() && (GameController.instance.GetCurrentPlayer != this.player ))
        {
            // EventManager.UserdialougeTextAnimEventCaller(DialougeType.UserWaitTurn, null, -1);
        }

        if(GameController.instance.GetCurrentPlayer == this.player && (canMove || player.OnPointerDownAutoMove))
        {
            player.OnPointerDownAutoMove = false;
            //for resetting the info text
            // player.playerUI.ResetInfoText();
            OnWarriorClicked();
        }
        
    }


    public void MoveWarrior(int overrideDiceValue, bool isRevive = false)
    {
        playerDiceValue = overrideDiceValue;
        MoveWarrior(isRevive);
    }

    float landinStripAnimationdelayForRelease = 0;
    // Set up warrior move path based on dice value
    public void MoveWarrior(bool skipDirection = false, bool isRevive =false)
    {   

        // EventManager.PlayerReminderTimerStopEventCaller();
        // @Madhu - for Revive text animation
        reviveActive = isRevive;
        // @Madhu - disable warrior looking at cam when moving

        // @Madhu if vfx already active, from SwitchWarriorSelectionButtons() 
        if(!isLastCellIndiactorActive ) 
        {
            // @Madhu activate last cell vfx indicator
            ActivateWarriorLastCellIndicator();
        }


        // Madhu
        int diceValue = playerDiceValue;
        List<CellHandler> path = new List<CellHandler>();

        int pillagePrevIndex = currentPath.FindIndex(c => c.type == CellType.PILLAGE) - 1;

        // if state is none
        if (state == WarriorState.None && (diceValue == 6 || diceValue == 1))
        {
            state = WarriorState.Active;
            player.canReDice = true;
            int currentIndex = currentPath.FindIndex(c => c.cellId == currentCell.cellId);
            // if (currentIndex < 0) Logger.LogError($"Current Cell not found - {currentCell.cellId}");
            if(!warriorReleasedOnlyOnce)
            {
                warriorReleased = true;
                warriorReleasedOnlyOnce = true;
            } 
            path.Add(currentPath[++currentIndex]);
        }
        else
        {
            path.AddRange(GetForwardPath(currentPath, targetPlayer, currentCell, diceValue));
        }

        if (!skipDirection)
        {
            if (((currentCell.type == CellType.CENTER || (path.Exists(x => x.type == CellType.CENTER) && path.Last().type != CellType.CENTER)) && (currentIndex <= currentPath.FindIndex( c => c.type == CellType.CENTER))) && ShowDirection() ) //
            {
                return;
            }
        }


            var otherWarrior = player.GetOtherWarrior(this);
            if(otherWarrior.IsRestricted)
            {
                DOVirtual.DelayedCall(2, () => otherWarrior.PlayRestrictedVFX(false));
            } 


        // stop the player time 
        if(player.playerCurrentPowerClicked == PowerType.NA)
            // player.playerUI.StopTimer();

        // Set jump power and poisition on the cell for each cell in the path
        OnWarriorMoveComplete(path.Last(), (List<WarriorController> captureWarriors, bool canCaptureEnemy) =>
        {
            // @MADHU
            if(diceValue ==1 && (canCaptureEnemy == false && path.Last().type!=CellType.PILLAGE && path.Last().cellId != player.endCell.cellId))
            {
                ResetCam();
            }
            // MADHU

            Sequence moveSeq = DOTween.Sequence().SetId(warriorTransform);

            Dictionary<int, float> jumpPower = new Dictionary<int, float>();
            float jumForce = 0.2f;

            //Dictionary<string, bool> flipCells = new Dictionary<string, bool>();

            for (int i = 0; i < path.Count; i++)
            {
                if(path[i].warriorsInCell.Count > 0 && i != path.Count - 1 && path[i].type!=CellType.PILLAGE)
                {
                    jumForce += 0.4f;
                    continue;
                }

                jumpPower.Add(i, jumForce);
                jumForce = jumpDefaultPow;
            }

            foreach (var _keyVal in jumpPower)
            {
                var item = path[_keyVal.Key];
                var jumPow = _keyVal.Value;

                var _pos = item.GetCellPosition(canCaptureEnemy);

                var _index = jumpPower.Keys.ToList().FindIndex(x => x == _keyVal.Key);

                bool shouldFlip = false;
                bool shouldSafeLand = false;
                bool isReviveLland = false;
                bool isLastJump = false;

                CellHandler _currentCell = null;
                CellHandler _nextCell = null;
                
                // condition for first element
                if (_index == 0)
                {
                    // Logger.Log($"warrior path first index - {_index}");
                    _currentCell = currentCell;
                    _nextCell = path[jumpPower.ElementAt(_index).Key];  
                }
                else
                {
                    // Logger.Log($"warrior path non-first index - {_index}");
                    _currentCell =  path[jumpPower.ElementAt(_index - 1).Key];
                    _nextCell =  path[jumpPower.ElementAt(_index).Key];
                }

                //shouldSafeLand = (currentPath[pillagePrevIndex].cellId == _nextCell.cellId || _nextCell.type == CellType.COMMON_SAFE || _nextCell.type == CellType.CENTER) && path.Last().cellId == _nextCell.cellId;
                shouldSafeLand = (_nextCell.type == CellType.START || _nextCell.type == CellType.COMMON_SAFE || _nextCell.type == CellType.CENTER) && path.Last().cellId == _nextCell.cellId;
                isReviveLland = _nextCell.cellId == path.Last().cellId && isRevive;
                isLastJump = path.Last().cellId == _nextCell.cellId;
                shouldFlip = CheckForFlip(_currentCell, _nextCell);

                if (shouldFlip) jumPow += 0.4f;
                if (isLastJump) jumPow = lastJumpPow;
                var duration = shouldFlip ? 1f : isLastJump ? 0.75f : 0.25f;
               
                Sequence _seq = null;
                if (_nextCell.type == CellType.PILLAGE)
                {
                    
                    _seq = DOTween.Sequence();
                    //  _seq.Join(DOVirtual.DelayedCall(0.0001f, () => targetPlayer.playerUI._warrior.ResetAndPlay(ConstantVars.ANGRYFULL)));
                    //  _seq.Join(DOVirtual.DelayedCall(0.0001f, () => player.playerUI._warrior.ResetAndPlay(ConstantVars.LAUGHFULL))); 
                    _seq.Append(PlayPillageDoneVFX(0.1f,3f, true));                    
                    // _seq.Join(DOVirtual.DelayedCall(0.0001f, () => warriorAnimation.AnimateBlendShapeFace(WarriorFaces.Laugh)));


                    // @Madhu - Cam zoome in and out
                    // _seq.Join(DOVirtual.DelayedCall(0.0001f, () => cameraZoomController.CamZoomAt(transform, _captureWarriorCamZoom, _captureWarriorCamZoomSpeed) ));
                    // _seq.Join(DOVirtual.DelayedCall(.8f, () => EventManager.StompEffectTextAnimEventCaller() ));
                    _seq.Join(DOVirtual.DelayedCall(0.00001f, () => PlayCoinShowerVFX(true) ));
                    _seq.Join(DOVirtual.DelayedCall(2f, () => PlayCoinShowerVFX(false) ));
                    // _seq.Join(DOVirtual.DelayedCall(2f, () => player.playerUI.DisplayInfo(checkpoint, infoPosition.position, overrideInfoOffest)));
                    // @Madhu - Blend shape anim
                    _seq.Append(DOVirtual.DelayedCall(0.01f, () => player.PickIdleBlendShapeAnimation()));
                    _seq.Join(DOVirtual.DelayedCall(0.01f, () => targetPlayer.PickIdleBlendShapeAnimation()));
                    // _seq.Append(DOVirtual.DelayedCall( 0.0001f,() => cameraZoomController.ResetCamZoom(_captureWarriorCamZoom, _captureWarriorCamZoomSpeed) ));

                    // Madhu
                    // _seq.Append(DOVirtual.DelayedCall(3f, () => Logger.Log($"Pillage VFX done")));
                }

                // Logger.Log($"Jump Power = {jumPow}");
               
                // play warrior move animation    
                moveSeq.Append(GetWarriorMoveTween( path.Last(), captureWarriors, isReviveLland, shouldSafeLand, _pos, jumPow, isLastJump, duration, shouldFlip, _seq, canCaptureEnemy)
                    .OnStart(() =>
                    {
                        if(lastCells.Count > 1)
                        {
                            lastCells.Find(c => c.cellId != path.Last().cellId).WarriorLastCellIndicator(false, this);
                        }
                        if (jumPow > 0.2f)
                        {
                            // AudioController.con.PlayGameSFX(GameController.instance.warriorSkipHop);
                            // @Madhu
                           // canLookAtCam = true;
                            //lookat 
                        }
                        else
                        {
                            // AudioController.con.PlayGameSFX(GameController.instance.warriorHop);
                            // look to the front of moving direction when it is not the last jump 
                            RotateWarrior(item.transform.position, transform.position);


                        }
                        RotateWarrior(item.transform.position, transform.position);
                    })
                    .OnComplete(() =>
                    {
                       
                        OnEveryMove(item);
                      
                    }));
            }
            

            moveSeq.Play()
            .OnComplete(() =>
            {
                WarriorLookAtCam();
                landinStripAnimationdelayForRelease = 0f; 
                if(player.releaseActive)
                {
                    // HelperTextManager.PlayHelperTextAnime(PowerType.Release);
                    landinStripAnimationdelayForRelease = 1f;
                    player.releaseActive = false;
                }
                


                // Madhu
                if(reviveActive)
                {
                    // HelperTextManager.PlayHelperTextAnime(PowerType.Revive);
                    reviveActive = false;
                }
                
                if (currentCell.type == CellType.BASE && path.Last().cellId == player.startCell.cellId)
                {
                    // Logger.Log("msg in debug");
                    // player.playerUI.ScalePopupEffect(0, () => { Logger.Log("message is shown"); }, true);
                    
                }
                else
                {
                    bool canShowBackToHome = false;

                    foreach (var _keyVal in jumpPower)
                    {
                        var item = path[_keyVal.Key];

                        if(item.cellId == targetPlayer.pillageCell.cellId)
                        {
                            canShowBackToHome = true;
                            break;
                        }
                    }

                    if (canShowBackToHome)
                    {
                        // player.playerUI.ScalePopupEffect(1, () => { Logger.Log("message is shown"); }, true);
                    }


                    // Logger.Log("msg is not in debug");
                }

                currentCell = path.Last();
                currentCell.AddWarriorToCell(this, canCaptureEnemy);

                // @Madhu - play capture enemy warrior Animation 
                WarriorCaptureAnimation(path.Last(), captureWarriors, canCaptureEnemy );


                if(!canCaptureEnemy ) 
                {
                    // DOVirtual.DelayedCall(delayForCheckPillageComplete, () => CheckPillageComplete(path.Last().cellId == player.endCell.cellId));
                    CheckPillageComplete(path.Last().cellId == player.endCell.cellId);

                }


                if (currentCell.type == CellType.PILLAGE)
                {
                    var _obj = currentCell.vfxs.Find(x => x.id == VFXID.PILLAGECOIN).vfxobject;
                    _obj.SetActive(false);
                }

                if (currentCell.type == CellType.COMMON_SAFE || currentCell.type == CellType.START || 
                currentCell.type == CellType.CENTER || currentCell.type == CellType.PILLAGE) 
                {
                    // player.playerUI.DisplayInfo("SAFE", infoPosition.position);
                }

                // Madhu
                
                // @Madhu - disable last cell vfx indicator
                currentCell.WarriorLastCellIndicator(false, this);
                

            })
           .OnStart(() =>
           {
               currentCell.RemoveWarriorFromCell(this, canCaptureEnemy);
               //Check if this cell is safe cell if yes enable healvfx for cell
               if (currentCell.type == CellType.PILLAGE)
               {
                   var _obj = currentCell.vfxs.Find(x => x.id == VFXID.PILLAGECOIN).vfxobject;
                   _obj.SetActive(true);
               }

           })
           .SetId(playerTweenId);

        });

        // Logger.Log($"Warrior move Start - {path.First().cellId}", path.First());
        // Logger.Log($"Warrior move End - {path.Last().cellId}", path.Last());        
    }

    // Next player turn 
    public void CheckPillageComplete(bool isPillageComplete)
    {

        Sequence enableWarSeq = DOTween.Sequence();
        // @Madhu - destination path indicator
        var delayForCheckPillageComplete = 0f;


        if(warriorReleased ) // && UserInterface.instance.playerSaveData._levelNumber < 4
        {
            enableWarSeq.Append( DOVirtual.DelayedCall(landinStripAnimationdelayForRelease, () => PathIndicatorOnWarriorUnlocked()));
            delayForCheckPillageComplete= totalLandinStripAnimationSeqDuration;
            warriorReleased = false;
        }

        if(isPillaged) //
        {
            enableWarSeq.Append( DOVirtual.DelayedCall(0.0001f, () =>  PathIndicatorOnPillage()));
            delayForCheckPillageComplete= totalLandinStripAnimationSeqDuration;
            isPillaged = false;
        }
        
        enableWarSeq.Append( DOVirtual.DelayedCall(delayForCheckPillageComplete, null));

        enableWarSeq.Play().OnComplete(() => 
        {
            if (isPillageComplete)
            {
                // OnPillageComplete(() => player.OnWarriorMoveComplete());
                OnPillageComplete(() => GameController.instance.GetCurrentPlayer.OnWarriorMoveComplete());
                
            }
            else
            {
                // player.OnWarriorMoveComplete();
                GameController.instance.GetCurrentPlayer.OnWarriorMoveComplete();

            }
        });
    }

    // Get future path for warrior to move
    public List<CellHandler> GetForwardPath(List<CellHandler> _currentPath,PlayerController _targetPlayer, CellHandler _currentCell, int _diceValue)
    {
        var futurePath = new List<CellHandler>();

        var pillageIndex = _currentPath.FindIndex(c => c.cellId == _targetPlayer.pillageCell.cellId);
        var afterPillagePath = _currentPath.GetRange(pillageIndex, _currentPath.Count - pillageIndex);
        int currentIndex = _currentPath.FindIndex(c => c.cellId == _currentCell.cellId);
        if (state == WarriorState.Pillage)
        {
            currentIndex = pillageIndex + afterPillagePath.FindIndex(c => c.cellId == _currentCell.cellId);
        }

        // if (currentIndex < 0) Logger.LogError($"Current Cell not found - {_currentCell.cellId}");

        currentIndex++;
        if ((currentIndex + _diceValue) <= currentPath.Count)
        {
            futurePath = _currentPath.GetRange(currentIndex, _diceValue);
            if (futurePath.Count == _diceValue)
            {
                //return (futurePath);
            }
        }
        else
        {
            // not enough dice value to move
            // Logger.Log($"Cannnot Move with dice value - {_diceValue}");
            
        }

        return futurePath;
    }


    public void OnWarriorMoveComplete(CellHandler end, UnityAction<List<WarriorController>, bool> onCompleteMove)
    {


        if (end.type == CellType.NORMAL) // check capturing if only cell is not safe state
        {
            // check if capture anyone
            var allWarriorFromCell = end.warriorsInCell;

            if (allWarriorFromCell.Count > 0)
            {
                var otherPlayerWarriors = allWarriorFromCell.Where(w => w.player.userId != player.userId 
                && !w.runningPowers.Contains(PowerType.Shield)).ToList();
                
                if (otherPlayerWarriors != null && otherPlayerWarriors.Count > 0 )
                {
                    // Logger.Log($"Found Other warriors -> {otherPlayerWarriors.Count}");
                    player.totalWarriorCaptured += otherPlayerWarriors.Count;
                    GameController.instance.AddXP(player,40);

                    onCompleteMove?.Invoke(otherPlayerWarriors, true);

                    player.canReDice = true;
                    if(player.twiceActive)
                    {
                        player.twoMovesPowerEnable = true;
                    }


                if (!player.isBot())
                {
                    // if (UserData.levelModeSelected == LevelMode.FourPlayer)
                    //     EngagementManager.instance.LogFirebaseAnalytics("warrior_captured_4_player");
                    // else
                    //     EngagementManager.instance.LogFirebaseAnalytics("warrior_captured_2_player");
                }

                }
                else
                {
                    onCompleteMove?.Invoke(null, false);
                }


            }
            else
            {
                onCompleteMove?.Invoke(null, false);
            }
        }
        else
        {
            // win condition
            if(end.cellId == player.endCell.cellId)
            {
                //OnPillageComplete();
            }

            // pillage condition
            if(targetPlayer.CheckCellIsBaseCell(end))
            {
               
            }

            onCompleteMove?.Invoke(null, false);
        }
    }

    // @Madhu - Will play the capture enemy Animation
    private void WarriorCaptureAnimation(CellHandler end, List<WarriorController> captureWarriors, bool canCaptureEnemy = false){

        if (canCaptureEnemy)
        {
            Sequence captureSequence = DOTween.Sequence();

            if (captureWarriors != null && captureWarriors.Count > 0 )
            {


                float enemyInAirDuration = 0;
                var warriorType = player.playerData.warriorType;
                switch (warriorType)
                {
                    case WarriorType.Red: // duryothan
                        enemyInAirDuration = 2f;

                        break;
                    case WarriorType.Bue: // arjun
                        enemyInAirDuration = 1f;
                        break;
                    case WarriorType.Green: // balram
                        enemyInAirDuration = 1.5f;

                        break;
                    case WarriorType.Yellow: // shakuni
                        enemyInAirDuration = 1.5f;

                        break;
                    case WarriorType.NA:
                        break;
                    default:
                        break;
                }

                foreach (var enemy in captureWarriors)
                {
                    enemy.WarriorSparkleTrail.SetActive(false);
                    enemy.WarriorSparkleTrail2.SetActive(false);

                    Transform enemyPrefab = enemy.warriorTransform.GetChild(0);
                    Vector3 pos = enemy.warriorTransform.position;
                    Vector3 enemyFallDir = (pos - warriorTransform.position).normalized;
                    Vector3 enemyBeforeKickPos = new Vector3(pos.x, pos.y + 0.1f, pos.z);
                    Vector3 enemyInAirPos = new Vector3(pos.x, pos.y +1, pos.z);
                    Vector3 enemyFallToGroundPos = new Vector3(pos.x, pos.y, pos.z);
                    Vector3 kickAngle;
                    if(enemyFallDir.x < 0)
                        kickAngle = new Vector3(0, 0, -45);
                    else
                        kickAngle = new Vector3(0, 0, 45);

                    float animStartDelay = 0.2f;
                    float kickDuration = 0.15f;
                    float enemyWarriorFlyDuration = 0.5f;
                    float enemyWarriorRollDuration = 0.4f;

                    captureSequence.Join(DOVirtual.DelayedCall(animStartDelay, null));
                    // Blend Shape anim
                    // captureSequence.Join(DOVirtual.DelayedCall(0.001f, () => warriorAnimation.AnimateBlendShapeFace(WarriorFaces.Laugh)));
                    //  captureSequence.Join(DOVirtual.DelayedCall(0.001f, () => player.playerUI._warrior.ResetAndPlay(ConstantVars.LAUGHFULL)));
                    // captureSequence.Join(DOVirtual.DelayedCall(0.001f, () => enemy.warriorAnimation.AnimateBlendShapeFace(WarriorFaces.Angry)));
                    //  captureSequence.Join(DOVirtual.DelayedCall(0.001f, () => enemy.player.playerUI._warrior.ResetAndPlay(ConstantVars.ANGRYFULL)));

                    // Spring effect
                    captureSequence.Join(enemyPrefab.DOMove(pos, animStartDelay));
                    // Kick VFX
                    captureSequence.Insert(animStartDelay + 0.1f, warriorTransform.GetChild(0).DOLocalRotate(kickAngle, kickDuration, RotateMode.Fast ));
                    // Kick SFX
                    captureSequence.Insert(animStartDelay + kickDuration/2, DOVirtual.DelayedCall( 0.0001f, () => PlayKickSFX()));  
                    // Vibrate Phone
                    // captureSequence.Insert(animStartDelay + kickDuration/2, DOVirtual.DelayedCall( 0.0001f,() => CommonUtilities.VibrateDevice()));
                    // player back to normal stand pos
                    captureSequence.Insert(animStartDelay + 0.1f + kickDuration ,warriorTransform.GetChild(0).DOLocalRotate(Vector3.zero, kickDuration, RotateMode.Fast ));
                    // Throw enemy to air
                    captureSequence.Insert(animStartDelay + kickDuration, enemyPrefab.DOMove(enemyInAirPos + enemyFallDir * 0.3f, enemyWarriorFlyDuration));
                    // Roll enenmy x2
                    captureSequence.Insert(animStartDelay + kickDuration + 0.1f, enemyPrefab.DOLocalRotate(new Vector3(360, 0, 0), enemyWarriorRollDuration, RotateMode.FastBeyond360));
                    // Capture VFX
                    captureSequence.Insert(animStartDelay + kickDuration + enemyWarriorFlyDuration, DOVirtual.DelayedCall( 0.01f,() =>PlayCaptureVFX(end, enemy)));
                    captureSequence.Insert(animStartDelay + kickDuration + enemyWarriorFlyDuration, DOVirtual.DelayedCall( 0.01f,() =>PlayScreamSFX(enemy.player)));
                    // Keep enemy in air
                    captureSequence.Insert(animStartDelay + kickDuration + enemyWarriorFlyDuration, DOVirtual.DelayedCall(enemyInAirDuration, null));
                    // enemy fall to ground
                    captureSequence.Insert(animStartDelay + kickDuration + enemyWarriorFlyDuration + enemyInAirDuration, enemyPrefab.DOMove(pos, enemyWarriorFlyDuration));
                    // Zoom Out
                    // captureSequence.Insert(animStartDelay + kickDuration + enemyWarriorFlyDuration + enemyInAirDuration,  DOVirtual.DelayedCall( 0.01f,() => cameraZoomController.ResetCamZoom(_captureWarriorCamZoom, _captureWarriorCamZoomSpeed) ));
                    // End Slow mo
                    // captureSequence.Insert(animStartDelay + kickDuration + enemyWarriorFlyDuration + enemyInAirDuration,  DOVirtual.DelayedCall( 0.01f,() => ShaderEffects.CommonVfxEffect.SlowMotion(ConstantVars.FADE_VAL_ONE) ));
                    // Stars on head VFX
                    captureSequence.Insert(animStartDelay + kickDuration + enemyWarriorFlyDuration + enemyInAirDuration - 1.5f, DOVirtual.DelayedCall( 0.001f,() => enemy.PlayStarsOnHeadVFX(true) ));
                    // captureSequence.Insert(animStartDelay + kickDuration + enemyWarriorFlyDuration + enemyInAirDuration - 1.5f, DOVirtual.DelayedCall( 0.001f,() => enemy.warriorAnimation.BoobleHeadAnimation() ));
                    // Laugh SFX
                    captureSequence.Insert(animStartDelay + kickDuration + enemyWarriorFlyDuration + enemyInAirDuration - 0.5f, DOVirtual.DelayedCall( 0.01f,() => PlayLaughSFX(player) ));
                    captureSequence.Append(DOVirtual.DelayedCall(0.1f,null));
                    // Reset enemy position
                    captureSequence.Append(enemyPrefab.DOMove(pos, 0.1f ));

                }


            }
            captureSequence.Play()
            .AppendCallback(() => 
            {
                /*foreach (var enemy in captureWarriors)
                {
                    enemy.MoveBackOnCapture(end, captureWarriors);
                }*/
                if(captureWarriors.Count > 0)
                {
                    for(int i = 0; i <= captureWarriors.Count; i++)
                    {
                        if(i == 0)
                        {
                            captureWarriors[i].MoveBackOnCapture(end, captureWarriors, true, this);
                        }
                        else
                        {
                            captureWarriors[i].MoveBackOnCapture(end, captureWarriors, false, this);
                        }
                        
                    }

                    player.twiceActive = false;
                    player.PickIdleBlendShapeAnimation();


                }
               
            });

        }
        
    }



    
    private void OnEveryMove(CellHandler cell)
    {
        if(cell.cellId == targetPlayer.pillageCell.cellId)
        {
            // toggle pillage
            OnPillage();
        }
        cell.AnimateCell();
        if (cell.cellId == player.startCell.cellId)
            cell.PlayGroundBlast();
    }

    private bool ShowDirection()
    {
        if (state == WarriorState.Active)
        {
            if (GameController.instance.isDirectionRequired)
            {
                // Logger.Log($"Direction Init Player Paused - cell.cellId - {warriorId}");

                if (player.isBot(false))
                {

                    OnDirectionSelected(GameController.instance.GetRandomTarget(player.userId));
                }
                else
                {
                    var playingPlayers = GameController.instance.activePlayers.Where(x => !x.isCompletedPillage).ToList();

                    if (playingPlayers.Count > 2)
                    {
                        GameController.instance.DirectionInit(player, OnDirectionSelected);

                    }
                    else
                    {
                        OnDirectionSelected(playingPlayers.Find(p=>p.userId!=player.userId));
                    }
                }

                // EventManager.PlayerReminderTimerStartEventCaller(PromptType.SelectDirection);
                return true;
            }
        }
        return false;
    }

    private void OnDirectionSelected(PlayerController targetPlayer)
    {
        foreach(var warrior in player.GetWarriors())
        {
            if(warrior.RestrictedVfxEffect.GetComponent<ParticleSystem>().isPlaying)
            {
                warrior.PlayRestrictedVFX(false);
            }
        }

        // Logger.Log($"Direction Target selected  - {targetPlayer.userId}");
        // get the path move the from current cell to destination
        this.targetPlayer = targetPlayer;
        
        UpdateCurrentPath(player.startCell, targetPlayer.startCell, currentPath[0], targetPlayer.pillageCell, true);
        UpdateCurrentPath(targetPlayer.startCell, player.endCell, null, null, false);
        // targetPlayer.playerUI.KillDirectionArrowTween();
        MoveWarrior(true);
    }

    private void OnPillageComplete(UnityAction callback)
    {
        // Logger.Log($"Warrior has completed Pillage - {warriorId}");
        player.canReDice = true;
        state = WarriorState.PillageDone;
        player.OnPillageComplete();
        
        GoHeaven(0f, 3f,callback);
    }

    private void OnPillage()
    {
        // Logger.Log($"Warrior {warriorId} has pillaged player {targetPlayer.userId} -- state = {state} ");
        
        state =  state == WarriorState.Active ? WarriorState.Pillage : WarriorState.Active;
        PlayPillageVFX(state == WarriorState.Pillage);
    }


    public void MoveBackOnCapture(CellHandler end, List<WarriorController> captureWarriors, bool runOnceOnly, WarriorController capturingWarrior)
    {
        player.totalWarriorLost++;

        // store the capture data 

        captureData = new WarriorCaptureData(true, targetPlayer, currentPath, currentCell);

        // Logger.Log($"Warrior Capture - {warriorId}", transform);

        var pillageIndex = currentPath.FindIndex(c => c.cellId == targetPlayer.pillageCell.cellId);
        var afterPillagePath = currentPath.GetRange(pillageIndex, currentPath.Count - pillageIndex);
        int currentIndex = currentPath.FindIndex(c => c.cellId == currentCell.cellId);
        if (state == WarriorState.Pillage)
        {
            currentIndex = pillageIndex + afterPillagePath.FindIndex(c => c.cellId == currentCell.cellId);
        }

        // if (currentIndex < 0) Logger.LogError($"Current Cell not found - {currentCell.cellId}");

        int moveBackCaptureIndex = 0;

        if (state == WarriorState.Active)
        {
            SetWarriorToDefaultState();
            moveBackCaptureIndex = 0;
        }
        else if (state == WarriorState.Pillage)
        {
            moveBackCaptureIndex = pillageIndex;
        }


        var futurePath = currentPath.GetRange(moveBackCaptureIndex, (currentIndex - moveBackCaptureIndex));
        futurePath.Reverse();

        List<Vector3> reversePathPositions = new List<Vector3>();
        futurePath.ForEach(f => reversePathPositions.Add(f.GetCellPosition()));

        // @Madhu - drag captured pawn back to base
        float duration = 0;
        for(int i=0; i < futurePath.Count; i++){
            duration += 0.2f;

        }
 
        transform.DOPath(reversePathPositions.ToArray(), duration).OnStart(() =>
            {
                currentCell.RemoveWarriorFromCell(this);
                warriorStunned = true;

            }).OnComplete(() =>
            {
                currentCell = futurePath.Last();
                currentCell.AddWarriorToCell(this);
                DOVirtual.DelayedCall(2f,() => PlayStarsOnHeadVFX(false));
                if (runOnceOnly)
                {
                    capturingWarrior.CheckPillageComplete(end.cellId == player.endCell.cellId);
                }
                player.PickIdleBlendShapeAnimation();
                WarriorSparkleTrail.SetActive(true);
                WarriorSparkleTrail2.SetActive(true);

            });
        // Madhu


    }

    public Sequence AfterCapture()
    {
        Sequence seq = DOTween.Sequence();
       
        seq.Append(DOVirtual.DelayedCall(0.001f, () => 
        {
            // currentCell.UpdateWarriorPositions(true);
            // RotateWarrior(Vector3.zero, Vector3.zero,true);

        }));
        seq.Append(PlayReDissolveEffectAfterCapture(null));

        return seq;
    }

    public Tween MoveBackward(int backSteps = 3)
    {
        // Logger.Log($"Moving backward -> {warriorId}");

        Tween reset = null;
        // back to home base
        int currentIndex = currentPath.FindIndex(c => c.cellId == currentCell.cellId);
        // if (currentIndex < 0) Logger.LogError($"Current Cell not found - {currentCell.cellId}");

        var futurePath = new List<CellHandler>();
        if (state == WarriorState.Active || (state == WarriorState.Pillage && currentPath[currentIndex].type == CellType.HOME_SAFE))
        {
            currentIndex = Mathf.Clamp((currentIndex - backSteps), 0, currentPath.Count);
            futurePath = currentPath.GetRange(currentIndex, backSteps);
            futurePath.Reverse();
        }
        else if(state == WarriorState.Pillage && currentPath[currentIndex].cellId != targetPlayer.pillageCell.cellId)
        {
            int prevIndex = currentIndex+1;
            currentIndex = Mathf.Clamp((currentIndex + backSteps), 0, currentPath.Count);
            futurePath = currentPath.GetRange(prevIndex, backSteps);

            if (futurePath.Exists(x=>x.cellId == targetPlayer.pillageCell.cellId))
            {
                int targetIndex = currentPath.FindIndex(x => x.cellId == targetPlayer.pillageCell.cellId);
                futurePath = currentPath.GetRange(prevIndex, targetIndex-prevIndex+1);
            }

        }


        if (state == WarriorState.Active && currentIndex == 0)
            SetWarriorToDefaultState();


        if (state == WarriorState.Active && futurePath.Exists(x => x.cellId == targetPlayer.pillageCell.cellId))
                    OnEveryMove(futurePath.Find(x => x.cellId == targetPlayer.pillageCell.cellId));



        List<Vector3> reversePathPosition = new List<Vector3>();
        futurePath.ForEach(f => reversePathPosition.Add(f.GetCellPosition()));

        var start = Vector3.zero;
        var end = Vector3.zero;
        if (reversePathPosition.Count >= 2)
        {
            end = reversePathPosition.Last();
            start = reversePathPosition[reversePathPosition.Count - 2];
        }

        reset = arrows.FireArrow(() =>
        {
            transform.DOPath(reversePathPosition.ToArray(), 1f).OnStart(() =>
            {
                currentCell.RemoveWarriorFromCell(this);

            }).OnComplete(() =>
            {
                WarriorLookAtCam();
                currentCell = futurePath.Last();
                currentCell.AddWarriorToCell(this);
                RotateWarrior(end, start);
                WarriorSparkleTrail.SetActive(true);
                WarriorSparkleTrail2.SetActive(true);
                

                //TODO 
                if (currentCell.warriorsInCell.Count > 0 && warriorId != currentCell.warriorsInCell[0].warriorId)
                {
                    var seq = DOTween.Sequence();
                    seq.Join(DOVirtual.DelayedCall(0.25f, () => CellExpandSFX(currentCell))).SetEase(Ease.Linear);
                    seq.Join(DOVirtual.DelayedCall(0.4f, () => currentCell.AnimateCellExpand())).SetEase(Ease.Linear);
                    seq.Join(DOVirtual.DelayedCall(0.5f, () => currentCell.MovePawnToMakeSpace(false))).SetEase(Ease.Linear);
                    // @Madhu - Arrow helper text
                    // seq.Append(DOVirtual.DelayedCall(0.1f, () =>  HelperTextManager.PlayHelperTextAnime(PowerType.Arrows))).SetEase(Ease.Linear);
                   
                }
                else{
                    // @Madhu - Arrow helper text
                    // HelperTextManager.PlayHelperTextAnime(PowerType.Arrows);
                }
                

            });

        });

        return reset;
    }
    
    private void UpdateCurrentPath(CellHandler start, CellHandler target, CellHandler baseStart, CellHandler baseEnd, bool clearPath)
    {
        if(clearPath) currentPath = new List<CellHandler>();

        if (baseStart != null) currentPath.Add(baseStart);

        currentPath.AddRange(GameController.instance.gridMap.GetPathToTarget(start, target));

        if (baseEnd != null) currentPath.Add(baseEnd);
    }

    // @Madhu - get all possible paths to hack dice value in such a way that there are no 3 pawn in a cell  
    public List<List<CellHandler>> paths = new List<List<CellHandler>>();

    public void GetAllPossiblePaths(CellHandler cell)
    {
        List<List<CellHandler>> pathTemp = new List<List<CellHandler>>();
        pathTemp.Add(path1);
        pathTemp.Add(path2);
        
        var otherPlayers = GameController.instance.GetOtherPlayers(player.userId);
        for(int i = 0; i < otherPlayers.Count; i++)
        {
            paths.Add(pathTemp[i]);
            GetPath(player.startCell, otherPlayers[i].startCell, cell, otherPlayers[i].pillageCell, true, paths[i]);
            GetPath(otherPlayers[i].startCell, player.endCell, null, null, false, paths[i]);
        }

    }

    private void GetPath(CellHandler start, CellHandler target, CellHandler baseStart, CellHandler baseEnd, bool clearPath, List<CellHandler> path)
    {
        if(clearPath) path.Clear();

        if (baseStart != null) path.Add(baseStart);

        path.AddRange(GameController.instance.gridMap.GetPathToTarget(start, target));

        if (baseEnd != null) path.Add(baseEnd);
    }

    public Transform warriorTransform;

    // @Madhu - Methods to rotate pawns toward target/Camera smoothly 
    public void WarriorLookAtCam()
    {
        float targetXPos = _cam.position.x;
        float targetYPos = warriorTransform.position.y;
        float targetZPos = _cam.position.z;

        Vector3 targetPostition = new Vector3( targetXPos,  targetYPos, targetZPos );
        warriorTransform.DOLookAt(targetPostition, 0.5f).SetId(warriorTransform.transform);
    }
    private bool warriorStunned;
    void Update()
    {
        if(canLookAtCam)
        {
            WarriorLookAtCam();
        }
    

    }

    // Madhu

    private void RotateWarrior(Vector3 end, Vector3 start, bool rotateDefault = false)
    {
        // float zero = ConstantVars.FADE_VAL_ZERO;

        if (rotateDefault)
        {
            // warriorTransform.DOLocalRotate(new Vector3(zero, zero, zero), zero);
            WarriorLookAtCam();
            return;
        }
        //Rotate the warrior so that it looks at the last next cell in it's path
        warriorTransform.LookAt(end);

        //Set Rotation to look foward and not down or up at cell
        var baserotation_y = warriorTransform.rotation.eulerAngles.y;
        var baserotation_z = warriorTransform.rotation.eulerAngles.z;
        // warriorTransform.rotation = Quaternion.Euler(ConstantVars.FADE_VAL_ZERO, baserotation_y, baserotation_z);

        //warriorTransform.DORotate(new Vector3(0f, -90f, 0f), 0.1f);
        // Logger.LogError("Rotate");

        /*
        var direction = end - start;
        string directionString = direction.ToString();
        direction = StringToVector(directionString);
        float x = direction.x;
        float z = direction.z;
        Logger.Log($"Direction - {x} --- {z}");
        if (Mathf.Abs(x) > Mathf.Abs(z)) // check for X direction 
        {
            if (x < 0)
            {
                warriorTransform.DOLocalRotate(new Vector3(0f, -90f, 0f), 0.1f);
            }
            else if (x > 0)
            {
                warriorTransform.DOLocalRotate(new Vector3(0f, 90f, 0f), 0.1f);

            }
        }
        else if (Mathf.Abs(x) < Mathf.Abs(z))// check for Z
        {
            if (z > 0)
            {
                warriorTransform.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.1f);

            }
            else if (z < 0)
            {
                warriorTransform.DOLocalRotate(new Vector3(0f, 180f, 0f), 0.1f);

            }

        }
        else
            warriorTransform.DOLocalRotate(new Vector3(0f, 180f, 0f), 0.1f);*/

    }

    private Vector3 StringToVector(string _vectorString)
    {
        // split the items
        string[] sArray = _vectorString.Replace("(",string.Empty).Replace(")",string.Empty).Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }


    // Bot - FutureTask
    public bool CheckWarriorCanCapture()
    {
        var diceValue = playerDiceValue;

        if (state != WarriorState.None)
        {
            var pillageIndex = currentPath.FindIndex(c => c.cellId == targetPlayer.pillageCell.cellId);
            var afterPillagePath = currentPath.GetRange(pillageIndex, currentPath.Count - pillageIndex);
            int currentIndex = currentPath.FindIndex(c => c.cellId == currentCell.cellId);
            if (state == WarriorState.Pillage)
            {
                currentIndex = pillageIndex + afterPillagePath.FindIndex(c => c.cellId == currentCell.cellId);
            }

            // if (currentIndex < 0) Logger.LogError($"Current Cell not found - {currentCell.cellId}");

            currentIndex++;
            if ((currentIndex + diceValue) <= currentPath.Count)
            {
                var futurePath = currentPath.GetRange(currentIndex, diceValue);
                if (futurePath.Count == diceValue)
                {
                    if (futurePath.Last().type == CellType.NORMAL && futurePath.Last().warriorsInCell.Count > 1)
                        return true;
                }
            }

        }

        return false;
    }

    public bool CheckWarriorCanUnlock()
    {
        // if state = none, and dice value != 6 -> false
        if (state == WarriorState.None && (playerDiceValue == 6 || playerDiceValue == 1))
        {
            if (runningPowers.Contains(PowerType.Lock))
                return false;
            return true;
        }
        return false;
    }

    //Enable Shield and run it's animation
    public void EnableShield()
    {
       
        warriorTransform.DOJump(transform.position, _jumpForce, 1, _jumpDuration).SetEase(Ease.Linear).OnComplete(() =>
        { _shieldParent.gameObject.SetActive(true);
            currentCell.PlayGroundBlast();
            _shieldParent.DOLocalRotate(vector, _shieldRotationDuration, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
            // DOVirtual.DelayedCall(1.2f, () => HelperTextManager.PlayHelperTextAnime(PowerType.Shield));
        });

    }
    //Disable Shield and run it's animation
    public void DisableShield()
    {
        if (_shieldParent.gameObject.activeSelf)
        {
            _shieldParent.DOLocalMoveY(1f, _shieldDisableDuration).SetEase(Ease.Flash).OnComplete(() => _shieldParent.gameObject.SetActive(false));
        }

    }  
        

    public void CleanPowers()
    {
        // clear vfx 
        player.EnablePower(PowerType.Lock, false);
        EnablePower(PowerType.Shield, false);
        runningPowers.Clear();
    }
    public void AddPower(PowerType pow)
    {
        // add vfx 
        player.EnablePower(pow, true);
        EnablePower(pow, true);
        runningPowers.Add(pow);
    }

    public void ReviveWarrior()
    {
        // get difference 
        var _lastReviveIndex = GetCellIndexOfPath(captureData.targetPath, captureData.targetPlayer, captureData.cellIndex);
        var _currentIndex = GetCellIndexOfPath(captureData.targetPath, captureData.targetPlayer, currentCell);

        var _diceValue = _lastReviveIndex - _currentIndex;

        if (state == WarriorState.None) state = WarriorState.Active;

        currentPath = captureData.targetPath;
        targetPlayer = captureData.targetPlayer;
        // Sprite reviveIcon = UserInterface.instance.spriteDirectory.goldenPowerupsData.Find(x => x.powerType == PowerType.Revive).powerIcon;

        // player.playerUI.DisplayInfo(reviveIcon, infoPosition.position);

        playerDiceValue = _diceValue < 0 ? 0 : _diceValue;
        DOVirtual.DelayedCall(ConstantVars.FADE_VAL_ONE, () => MoveWarrior(true, true)).OnComplete( () => PlayReviveVFX());
        CleanCaptureData();
    }

   
    public void CleanCaptureData()
    {
        captureData.isCapture = false;
    }

    // Calaculate current cell index of the warrior
    public int GetCellIndexOfPath(List<CellHandler> _currentPath, PlayerController _targetPlayer, CellHandler _currentCell)
    {
        var pillageIndex = _currentPath.FindIndex(c => c.cellId == _targetPlayer.pillageCell.cellId);
        var afterPillagePath = _currentPath.GetRange(pillageIndex, _currentPath.Count - pillageIndex);
        // @Madhu - converted local currentIndex var to class var for progress calculator
        currentIndex = _currentPath.FindIndex(c => c.cellId == _currentCell.cellId);
        // Madhu
        if (state == WarriorState.Pillage)
        {
            currentIndex = pillageIndex + afterPillagePath.FindIndex(c => c.cellId == _currentCell.cellId);
        }

        return currentIndex;
    }


    public void SetWarriorToDefaultState()
    {
        state = WarriorState.None;
        RotateWarrior(Vector3.zero, Vector3.zero, true);
    }


    public Sequence GetWarriorMoveTween(CellHandler end, List<WarriorController> warriors, bool isRevive , bool shouldSafeLand,Vector3 _pos, float jumPow, bool lastJump ,float duration = 0.25f, bool doFlip = false, Sequence extraAnim = null, bool canCaptureEnemy = false)
    {
        AudioClip _clip = shouldSafeLand ? GameController.instance.safeCellAudio: isRevive ? GameController.instance.reviveAudioClip : GameController.instance.warriorHop;
        //_clip = isRevive ? GameController.instance.reviveAudioClip : GameController.instance.warriorHop;

        if (jumPow > jumpDefaultPow && !shouldSafeLand)
            _clip = GameController.instance.warriorSkipHop;

       
        Sequence seq = DOTween.Sequence();
        // @Madhu - If pawn already exists in the cell jump to side
        if (lastJump && (end.warriorsInCell.Count > 0)) 
        {
            if(canCaptureEnemy)
            {
                _pos = end._warriorCapturePositions[end.warriorsInCell.Count].position;

            }
            else
            {
                _pos = end._warriorMinonPositions[end.warriorsInCell.Count].position;

            }
        }

        // Madhu
        
        seq.Join(transform.DOJump(_pos, jumPow, 1, duration).OnStart(() => 
        {
            // AudioController.con.PlayGameSFX(_clip);

            if (shouldSafeLand)
                PlayReviveVFXForLongTime(true);
            else
                PlayReviveVFXForLongTime(false);

        }));
        
        if(doFlip)
            seq.Join(warriorTransform.GetChild(0).DOLocalRotate(new Vector3(360, 0, 0), duration, RotateMode.FastBeyond360).OnStart(() => 
            {
                WarriorLookAtCam();
                // AudioController.con.PlayGameSFX(GameController.instance.warriorFlipHop);
                if (lastJump == true)
                {
                    if (end.warriorsInCell.Count > 0 && warriorId != end.warriorsInCell[0].warriorId)
                    {

                        seq.Join(DOVirtual.DelayedCall(0.25f, () => CellExpandSFX(end))).SetEase(Ease.Linear);
                        seq.Join(DOVirtual.DelayedCall(0.4f, () => end.AnimateCellExpand())).SetEase(Ease.Linear);
                        seq.Join(DOVirtual.DelayedCall(0.5f, () => end.MovePawnToMakeSpace(canCaptureEnemy))).SetEase(Ease.Linear);
                    }
                }

            }).OnComplete(() => {
                // @Madhu - Revive Helper Text
                if(isRevive)
                    // HelperTextManager.PlayHelperTextAnime(PowerType.Revive);
                WarriorLookAtCam();
                // Madhu

            }));

        if (extraAnim != null)
            seq.Append(extraAnim);


        // Last jump
        if (lastJump == true && doFlip == false)
        {

            seq.Join(warriorTransform.GetChild(0).DOLocalRotate(new Vector3(ConstantVars.INT_VAL_ZERO, ConstantVars.FLOAT_VAL_360, ConstantVars.INT_VAL_ZERO), duration, RotateMode.FastBeyond360).
                    OnStart(() => {
                     WarriorLookAtCam();
                    }));//.OnComplete(() =>
                    // { AudioController.con.PlayGameSFX(GameController.instance.warriorHop); }
                    // ));
            // @Madhu - if a pawn (pawn1) exists in the cell to which the another pawn (pawn2) is moving, move existing pawn (pawn1) to the side, to make space for the other pawn (pawn2)
            // Enable camera zoom in and slow motion 
            if(end.warriorsInCell.Count > 0 && warriorId != end.warriorsInCell[0].warriorId)
            {
                seq.Join(DOVirtual.DelayedCall(0.25f, () => CellExpandSFX(end))).SetEase(Ease.Linear);
                seq.Join(DOVirtual.DelayedCall(0.5f, () =>  end.AnimateCellExpand())).SetEase(Ease.Linear);
                seq.Join( DOVirtual.DelayedCall(0.5f, () =>  end.MovePawnToMakeSpace(canCaptureEnemy))).SetEase(Ease.Linear);

            }
            if(canCaptureEnemy)
            {
                // seq.Join(DOVirtual.DelayedCall(0.0001f, () => cameraZoomController.CamZoomAt(transform, _captureWarriorCamZoom, _captureWarriorCamZoomSpeed) ));
                // seq.Join(DOVirtual.DelayedCall(0.0001f, () => ShaderEffects.CommonVfxEffect.SlowMotion(_slowMoTimeScale) )); 

            }
            // Madhu
            
        }

        return seq;
    }

    public bool CheckForFlip(CellHandler currentCell, CellHandler nextCell)
    {
        bool needFlip = false;

        List<CellHandler> homeBaseCells = new List<CellHandler>();
        homeBaseCells.AddRange(player.GetFortCells());
        homeBaseCells.AddRange(targetPlayer.GetFortCells());

        if(homeBaseCells.Exists(x => x.cellId == currentCell.cellId))
        {
            // Logger.Log($"need to flip cell -{currentCell.cellId}");
            needFlip = true;
        }
        else if (homeBaseCells.Exists(x => x.cellId == nextCell.cellId))
        {
            // Logger.Log($"need to flip cell -{nextCell.cellId}");
            needFlip = true;
        }

        return needFlip;
    }

    private void OnDestroy()
    {
        DOTween.Kill(warriorTransform.transform);
    }


    #region VFX 

    public List<VFXDATA> vfxs;
    public ArrowVFX arrows;
    

    public void EnablePower(PowerType _type, bool isEnable)
    {
        if (_type == PowerType.Shield)
        {
            var _obj = vfxs.Find(x => x.id == VFXID.SHEILD).vfxobject;
            if(isEnable == false)
            {
                DisableShield();
            }
            else
            {
                EnableShield();
            }
            // @Madhu - Shield helper text 
           /* if(player.shieldActive)
            {
                // DOVirtual.DelayedCall(0.5f, () => HelperTextManager.PlayHelperTextAnime(PowerType.Shield));
                player.shieldActive = false;
            }*/
            
            // Madhu

        }
        else if (_type == PowerType.Revive)
        {
            var _obj = vfxs.Find(x => x.id == VFXID.HEAL).vfxobject;
            _obj.SetActive(isEnable);
        }
    }

    public void PlayReviveVFX()
    {
        var _obj = vfxs.Find(x => x.id == VFXID.HEAL).vfxobject;
        _obj.SetActive(true);
        DOVirtual.DelayedCall(4f, () => { _obj.SetActive(false); });
    }

    public void PlayReviveVFXForLongTime(bool isEnable)
    {
        var _obj = vfxs.Find(x => x.id == VFXID.HEAL).vfxobject;
        _obj.SetActive(isEnable);
    }

    public void PlayPillageVFX(bool isEnable)
    {
        var _obj = vfxs.Find(x => x.id == VFXID.LEVELUP).vfxobject;
        _obj.SetActive(isEnable);
        //DOVirtual.DelayedCall(4f, () => { _obj.SetActive(false); });
    }

    public Sequence PlayPillageDoneVFX(float delay, float duration = 10f, bool canPlayClip=false)
    {


        Sequence seq = DOTween.Sequence();
        var _obj = vfxs.Find(x => x.id == VFXID.SUPERUP).vfxobject;
       
        seq.Append(DOVirtual.DelayedCall(duration, () => { _obj.SetActive(false); }).OnStart(() =>
        {
            PlayPillageVFX(false);
            _obj.SetActive(true);
            // AudioController.con.PlayClipOnExtraSource(GameController.instance.pillageCompleteAudio);

            // @Madhu - 
            isPillaged = true;

            //if (canPlayClip)
                //PlayCharacterLaughterSound(player);
        }));

        return seq;
    }

    public Sequence GoHeaven(float dealy, float duration, UnityAction callback)
    {
        if(player.twiceActive)
        {
            player.twoMovesPowerEnable = true;
        }
        //Crown appear click
        // AudioController.con.PlayUISFX(GameController.instance.crownAppearClip);
        WarriorSparkleTrail.gameObject.SetActive(false);
        WarriorSparkleTrail2.gameObject.SetActive(false);
        _shieldParent.gameObject.SetActive(false);
        var otherPlayers = GameController.instance.GetOtherPlayers(player.userId);


        Sequence seq = DOTween.Sequence();

        // position root motion bottom to  top 
        var pos = warriorTransform.position;
        seq.Join(PlayPillageDoneVFX(0f, duration));
        seq.Join(warriorTransform.DOMoveY(pos.y + 3f, duration));

        // @Madhu -blend shape animations
        // seq.Join(DOVirtual.DelayedCall(0.0001f, () => warriorAnimation.AnimateBlendShapeFace(WarriorFaces.Laugh)));
        //  seq.Join(DOVirtual.DelayedCall(0.0001f, () => player.playerUI._warrior.ResetAndPlay(ConstantVars.LAUGHFULL)));
        foreach(var otherplayer in otherPlayers)
        {
            // seq.Join(DOVirtual.DelayedCall(0.0001f, () => otherplayer.playerUI._warrior.ResetAndPlay(ConstantVars.SADFULL)));
        }
        
        // @Madhu - Cam zoom out
        // seq.Join(DOVirtual.DelayedCall(0.0001f, () => cameraZoomController.CamZoomAt(transform, _warriorGoHeavenCamZoom, duration) ));

        seq.Join(warriorTransform.GetChild(0).DOLocalRotate(new Vector3(360 * 2, 0, 0), duration, RotateMode.FastBeyond360).OnStart(() => 
        {
            // AudioController.con.PlayGameSFX(GameController.instance.warriorFlipHop);

        }));

        // @Madhu - Go Heaven Text animation
        // seq.Insert(0.7f, DOVirtual.DelayedCall(0.001f, () => EventManager.GoHeavenTextAnimEventCaller()));


        //seq.Join(Dissolve(false, duration - 2f, null));
        seq.AppendCallback(() => 
        {
            currentCell.RemoveWarriorFromCell(this);
            // @Madhu - reset cam zoom
            // DOVirtual.DelayedCall( 0.0001f,() => cameraZoomController.ResetCamZoom(_warriorGoHeavenCamZoom, _captureWarriorCamZoomSpeed) );
            // Madhu
            callback?.Invoke();
            // @Madhu - Blend Shape animations
            player.PickIdleBlendShapeAnimation();
            foreach(var otherPlayer in otherPlayers)
            {
                otherPlayer.PickIdleBlendShapeAnimation();

            }
            warriorTransform.gameObject.SetActive(false);
            // AudioController.con.PlayGameSFX(GameController.instance.warriorFlipHop);

        });
        return seq;

    }

    public void PlayCaptureVFX(CellHandler cell, WarriorController enemy)
    {
        VFXID _vfx = VFXID.BLAST;
        var warriorType = player.playerData.warriorType;
        var duration = 2f;

        AudioClip clip = GameController.instance.warriorThorAttack;

        switch (warriorType)
        {
            case WarriorType.Red: // duryothan
                _vfx = VFXID.FIRE;
                clip = GameController.instance.warriorFireAttack;

                break;
            case WarriorType.Bue: // arjun
                _vfx = VFXID.LIGHTING;
                clip = GameController.instance.warriorThorAttack;
                break;
            case WarriorType.Green: // balram
                _vfx = VFXID.BLAST;
                clip = GameController.instance.warriorBlastAttack;

                break;
            case WarriorType.Yellow: // shakuni
                _vfx = VFXID.FREEZE;
                clip = GameController.instance.warriorFreezeAttack;

                break;
            case WarriorType.NA:
                break;
            default:
                break;
        }

        // AudioController.con.PlayGameSFX(clip);
        cell.AttackWarriorCaptured(_vfx, enemy.warriorTransform.GetChild(0), duration);

    }

    // @Madhu - play enemy pawn stunned star effect on warrior capture
    private void PlayStarsOnHeadVFX(bool isEnabled)
    {
        starsOnHeadVfxEffect.SetActive(isEnabled);
    }

    private float GetParticleSystemDuration(GameObject obj)
    {
        float duration;
        var ps = obj.GetComponent<ParticleSystem>();
        var main = ps.main;
        return duration = main.duration;
    }
    private Coroutine countdownCoroutine;
    public float remainingTime;
    public IEnumerator RestrictedVFX()
    {
        remainingTime = restrictedVFXDuration;
        // remainingTime = 1.3f;
        
        while (remainingTime >= 0)
        {
            remainingTime-= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

    }
    public void StopRestrictedVFXCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
    }
    public void PlayRestrictedVFX(bool isEnable, bool isAutoDisable = false)
    {
        RestrictedVfxEffect.SetActive(isEnable);
        if(isAutoDisable)
        {
            DOVirtual.DelayedCall(restrictedVFXDuration, () => RestrictedVfxEffect.SetActive(false));
        }
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
        countdownCoroutine = StartCoroutine(RestrictedVFX());
    }

    // @Madhu - play Coin shower effect for pillage 
    private void PlayCoinShowerVFX(bool isEnabled)
    {
        CoinShowerVfxEffect.SetActive(isEnabled);
    }

    // @Madhu - Kick enemy pawn SFX on warrior capture
    private void PlayKickSFX()
    {
        AudioClip clip = GameController.instance.PlayerKickClip;
        // AudioController.con.PlayGameSFX(clip);
    }

    // @Madhu - Cell expand SFX when cell expands for multiple warrior on a cell
    public void CellExpandSFX(CellHandler cell)
    {
        if(cell.type == CellType.PILLAGE)
        {
            return;
        }
        AudioClip clip = GameController.instance.CellExpansionClip;
        // AudioController.con.PlayClipOnExtraSource1(clip);
    }

    // @Madhu - Character laugh SFX when when enemy warrior is captured
    private void PlayLaughSFX(PlayerController player)
    {
        PlayCharacterLaughterSound(player);
    }

    private void PlayScreamSFX(PlayerController player)
    {
        PlayCharacterScreamSound(player);
    }
    // Madhu

    public Material dissolveMatCap;
    private Material cloneMatCap;
    public List<MeshRenderer> _meshRenderer = new List<MeshRenderer>();

// @Madhu - commented lines as enemy pawn is draged back to base not portaled back to base
    public Sequence Dissolve(bool isVisible, float delay, UnityAction callback)
    {
        // Logger.Log($"Dissolve triggered - {warriorId} :: {isVisible}");
        // callback += () => Logger.Log($"dissolve effect ended - {warriorId} :: {isVisible}");
        Sequence seq = DOTween.Sequence();
        // var invisible = 1f;
        // var visible = -0.05f;
        // var duration = 2f;
        if(isVisible)
        {
            // cloneMatCap.SetFloat("_dissolve", invisible);

            // seq.Append(DOVirtual.Float(invisible, visible, duration, (float val) => 
            // {
            //     cloneMatCap.SetFloat("_dissolve", val);
            // }).SetDelay(delay).OnComplete(() => callback?.Invoke()));
            seq.Append(DOVirtual.DelayedCall(0.001f, null)).SetDelay(delay).OnComplete(() => callback?.Invoke());
        }
        else
        {
            // cloneMatCap.SetFloat("_dissolve", visible);
            // seq.Append(DOVirtual.Float(visible, invisible, duration, (float val) =>
            // {
            //     cloneMatCap.SetFloat("_dissolve", val);
            // }).SetDelay(delay).OnComplete(() => callback?.Invoke()));
            seq.Append(DOVirtual.DelayedCall(0.001f, null)).SetDelay(delay).OnComplete(() => callback?.Invoke());
        }

        return seq;
    }
    // Madhu

    public void PlayCharacterLaughterSound(PlayerController player)
    {
        // if (player.isPowerupTurnMove)
        //     return;
        // if (player.playerData.playerName == "INDRAPRASTHA")
        //     // AudioController.con.PlayClipOnExtraSource(GameController.instance.arjunLaughAudioClip);
        // else if (player.playerData.playerName == "HASTINAPUR")
        //     // AudioController.con.PlayClipOnExtraSource(GameController.instance.duryodhanLaughAudioClip);
        // else if (player.playerData.playerName == "DWARIKA")
        //     // AudioController.con.PlayClipOnExtraSource(GameController.instance.gandhariLaughAudioClip);
        // else if (player.playerData.playerName == "GANDHAR")
            // AudioController.con.PlayClipOnExtraSource(GameController.instance.draupadiLaughAudioClip);
    }

    public void PlayCharacterScreamSound(PlayerController player)
    {
        // if (player.isPowerupTurnMove)
        //     return;
        // if (player.playerData.playerName == "INDRAPRASTHA")
        //     // AudioController.con.PlayClipOnScreamSource(GameController.instance.arjunScreamAudioClip);
        // else if (player.playerData.playerName == "HASTINAPUR")
        //     // AudioController.con.PlayClipOnScreamSource(GameController.instance.duryodhanScreamAudioClip);
        // else if (player.playerData.playerName == "DWARIKA")
        //     // AudioController.con.PlayClipOnScreamSource(GameController.instance.gandhariScreamAudioClip);
        // else if (player.playerData.playerName == "GANDHAR")
        //     // AudioController.con.PlayClipOnScreamSource(GameController.instance.draupadiScreamAudioClip);
    }

    public Sequence PlayDissolveEffectOnAttack(PlayerController player,  UnityAction callback)
    {
        return Dissolve(false, 0.1f, callback);
    }

    public GameObject BubbleVFX;
    public Sequence PlayReDissolveEffectAfterCapture(UnityAction callback)
    {
        BubbleVFX.SetActive(true);
        // AudioController.con.PlayClipOnExtraSource(GameController.instance.bubbleEffectAudio);
        return Dissolve(true, 0f, () => 
        {
            BubbleVFX.SetActive(false);
            callback?.Invoke();
        });
    }

    private void Start()
    {
        cloneMatCap = new Material(dissolveMatCap); 
      /*  foreach (var item in _meshRenderer)
        {
            item.material = cloneMatCap;
        }*/
    }

    #endregion
    // @Madhu 
    #region DestinationCellIndicator
    public bool isLastCellIndiactorActive;
    private List<CellHandler> lastCells;

    // Activate destination cell indicator animation 
    private void ActivateWarriorLastCellIndicator( bool isSameCell = false)
    {
        lastCells = GetPathLastCell();
        foreach(var cell in lastCells)
        {
            cell.WarriorLastCellIndicator(true, this, isSameCell);
        }
    }

    // Get last cell of all possible path for the warrior
    public List<CellHandler> GetPathLastCell()
    {
        int warriorDiceValue = playerDiceValue;
        List<CellHandler> warriorPath = new List<CellHandler>();

        if (state == WarriorState.None && (warriorDiceValue == 6 || warriorDiceValue == 1))
        {
            int currentIndex = currentPath.FindIndex(c => c.cellId == currentCell.cellId);
            warriorPath.Add(currentPath[++currentIndex]);
        }
        else
        {
            for (int i = 0; i < paths.Count; i++)
            {
                var path = paths[i];
                if ( path.Exists(c => c.cellId == currentCell.cellId) && !warriorPath.Exists(c => c.cellId == path[currentIndex + warriorDiceValue].cellId))
                {   
                    warriorPath.Add(path[currentIndex + warriorDiceValue]);
                }
            }
        }

        return warriorPath;
    }
    #endregion

    #region DestinationPathIndicator

    Sequence landinStripAnimationSeq; // = DOTween.Sequence();
    private float blinkspeed = 0.15f;
    private float nextCellGlowDelay = 0.1f;
    private float sequenceDelay = 0;
    private float totalLandinStripAnimationSeqDuration = 1.7f;

    // logic for landing strip animation when warriors are unlocked   
    private void PathIndicatorOnWarriorUnlocked()
    {
        var pathToPlayer = GameController.instance.gridMap.PathFromCenter(player.startCell);
        var otherPlayers = GameController.instance.GetOtherPlayers(player.userId);

        var pathToTarget1 = GameController.instance.gridMap.PathFromCenter(otherPlayers[0].startCell, false);
        pathToTarget1.Reverse();
        pathToTarget1.Add(otherPlayers[0].pillageCell);
        
        sequenceDelay = 0;
        // totalLandinStripAnimationSeqDuration = 0;

        if(landinStripAnimationSeq != null)
        {
            landinStripAnimationSeq = null;
        }

        landinStripAnimationSeq.Join( CreateLandingStripAnimSeq(pathToPlayer, ref sequenceDelay, nextCellGlowDelay, blinkspeed));
        float newSequenceDelay1 = sequenceDelay;
        float newSequenceDelay2 = sequenceDelay;
        landinStripAnimationSeq.Join( CreateLandingStripAnimSeq(pathToTarget1, ref newSequenceDelay1, nextCellGlowDelay, blinkspeed));
        if(otherPlayers.Count > 1)
        {
            var pathToTarget2 = GameController.instance.gridMap.PathFromCenter(otherPlayers[1].startCell, false);
            pathToTarget2.Reverse();
            pathToTarget2.Add(otherPlayers[1].pillageCell);
            landinStripAnimationSeq.Join( CreateLandingStripAnimSeq(pathToTarget2, ref newSequenceDelay2, nextCellGlowDelay, blinkspeed));

        }
        landinStripAnimationSeq.Play();
        // totalLandinStripAnimationSeqDuration = (blinkspeed * 2 - nextCellGlowDelay) + ( nextCellGlowDelay * (pathToPlayer.Count + pathToTarget1.Count));
    }

    // logic for landing strip animation after warriors pillaged enemy castle   
    private void PathIndicatorOnPillage()
    {   var cellIndexNextToWarrior = currentIndex + playerDiceValue;
        var remaingPathCellCount =  currentPath.Count - currentIndex - playerDiceValue;
        var pathToHome = currentPath.GetRange(cellIndexNextToWarrior, remaingPathCellCount);

        sequenceDelay = 0;
        // totalLandinStripAnimationSeqDuration = 0;

        if(landinStripAnimationSeq != null)
        {
            landinStripAnimationSeq = null;
        }

        landinStripAnimationSeq.Join( CreateLandingStripAnimSeq(pathToHome, ref sequenceDelay, nextCellGlowDelay, blinkspeed));


        landinStripAnimationSeq.Play();
        // totalLandinStripAnimationSeqDuration = (blinkspeed *2 - nextCellGlowDelay) + (nextCellGlowDelay * pathToHome.Count);

    }

    // Create sequence for landing strip animation
    public Sequence CreateLandingStripAnimSeq(List<CellHandler> pathList, ref float sequenceDelay, float nextCellGlowDelay, float blinkspeed)
    {
        
        Sequence seq = DOTween.Sequence();

        foreach(var cell in pathList)
        {
            var _blinkSpeed = blinkspeed;
            sequenceDelay += nextCellGlowDelay;
            if((cell.type == CellType.PILLAGE ) || (cell.type == CellType.END) )
            {
                _blinkSpeed = 0.35f;
            }
            seq.Join(DOVirtual.DelayedCall(sequenceDelay, () => cell.GlowDestinationPathCells(_blinkSpeed, this)));

        }
        return seq;
    }
    #endregion
    // Madhu
    
}
