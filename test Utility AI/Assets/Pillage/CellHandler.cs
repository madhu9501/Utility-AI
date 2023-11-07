using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;
using ConstantVariables;
// using ShaderEffects;
/// <summary>
/// Controls Logic for a cell(Step), basic logic like changing materials and managing some functionlity of warrior 
/// which are on this particualr cell(//TODO Strip logic in different scripts)
/// </summary>
public class CellHandler : MonoBehaviour
{
    [SerializeField] MeshRenderer cellSprite;
    public MeshRenderer cellModel;
    private Material normalCellMat;
    public GameObject cellModelGameObj;
    float initXScale;
    public MeshRenderer cellExtraSprite;
    public CellType type;
    public string cellId;
    public List<WarriorController> warriorsInCell = new List<WarriorController>();
    public List<Transform> warriorPositions = new List<Transform>();
    public List<Transform> _warriorMinonPositions = new List<Transform>();
    public List<Transform> _warriorCapturePositions = new List<Transform>();


    public Vector3 normalScale = new Vector3(0.14f, 0.14f, 0.14f);
    // public Vector3 minionScale = new Vector3(0.14f, 0.14f, 0.14f);

    //TODO - MAYBE WIRE WITH EVENT MANAGER also make different script for animation
    [Header("Animation Settings")]
    public float onSteppedPosition;
    public float duration;

    void Start()
    {
        if (type != CellType.BASE)
        {
            for (int i = 0; i < 5; i++)
            {
                _warriorMinonPositions.Add(warriorPositions[i]);
            }

            for (int i = 0; i < 4; i++)
            {
                _warriorCapturePositions.Add(warriorPositions[5 + i]);
            }
        }
        if(cellModelGameObj != null)
            initXScale = cellModelGameObj.transform.localScale.x;

        if(type == CellType.PILLAGE) 
        {
            vfxs.Find(x => x.id == VFXID.PILLAGECOIN).vfxobject.transform.DOLocalRotate(Vector3.forward * 360, 8, RotateMode.FastBeyond360).
                SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart).SetId(transform);
        }

        if(cellModel != null)
        {

            normalCellMat = cellModel.material;

        }

        
        if(type == CellType.COMMON_SAFE || type == CellType.START || type == CellType.CENTER )
        {
            // SafeCellPS = SafeCellVfx.GetComponent<ParticleSystem>();
            // MagicCirclePS = MagicCircleVfx.GetComponent<ParticleSystem>();
            // SafeCellEM = SafeCellPS.emission;
            // MagicCircleEM = MagicCirclePS.emission;
            SafeCellEM = SafeCellVfx.GetComponent<ParticleSystem>().emission;
            MagicCircleEM = MagicCircleVfx.GetComponent<ParticleSystem>().emission;

        }
       
    }

    public void SetCellIcon(Texture tex)
    {
        cellExtraSprite.enabled = (false);
        cellExtraSprite.material.mainTexture = tex;

    }

    public void AddWarriorToCell(WarriorController warrior, bool canCaptureEnemy = false)
    {
        //TODO set up the scale and position according to the no.of warriors
        warriorsInCell.Add(warrior);
        UpdateWarriorPositions(canCaptureEnemy);
        warrior.transform.parent = transform;
    }

    public void RemoveWarriorFromCell(WarriorController warrior, bool canCaptureEnemy = false)
    {   
        // if(warriorsInCell.Count > 1)
        // {
            // warrior.CellExpandSFX();
        // }
        var _war = warriorsInCell.Remove(warrior);
        if(_war)ResetScalePositionOnRemoved(warrior);
        UpdateWarriorPositions(canCaptureEnemy);
        warrior.transform.parent = warrior.player.transform;
    }
    public Vector3 GetCellPosition(bool canCaptureEnemy = false)
    {
        return transform.position;
    

        // if (warriorPositions.Count > warriorsInCell.Count)
        //     return warriorPositions[warriorsInCell.Count].position;
        //     return warriorPositions.First().position;


    }

    public void UpdateWarriorPositions(bool canCaptureEnemy = false)
    {

        var positions = new List<Transform>() { transform };

        var _scale = normalScale;

        if((warriorsInCell.Count) > 1)
        {
            // positions = _warriorMinonPositions;
            // _scale = minionScale;
            positions= _warriorMinonPositions;
            _scale = normalScale;           
        }
        if (canCaptureEnemy)
        {
            positions= _warriorCapturePositions;
            _scale = normalScale;
        }

        for (int i = 0; i < warriorsInCell.Count; i++)
        {
            warriorsInCell[i].transform.position = positions[i].position;
            warriorsInCell[i].warriorTransform.DOScale(_scale, 0.1f);
        }
    }

    public void ResetScalePositionOnRemoved(WarriorController warrior)
    {
        warrior.transform.position = transform.position;
        warrior.warriorTransform.DOScale(normalScale, 0f);
        cellModelGameObj.transform.DOScaleX( initXScale , 0.4f);

    }

    // Existing pawn make space for incomping pawn to the cell
    public void MovePawnToMakeSpace(bool canCaptureEnemy)
    {
        var positions = new List<Transform>() { transform };
        positions = (canCaptureEnemy) ? _warriorCapturePositions : _warriorMinonPositions;


        for (int i = 0; i < warriorsInCell.Count; i++)
        {
            warriorsInCell[i].transform.position = positions[i].position;

        }
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
        DOTween.Kill(transform);
    }



    #region VFX 

    public Transform vfxParent;
    public List<VFXDATA> vfxs;
    public GameObject LastCellIndicator;
    public GameObject SafeCellVfx;
    public GameObject MagicCircleVfx;
    public ParticleSystem.EmissionModule SafeCellEM;
    public ParticleSystem.EmissionModule MagicCircleEM;

    public void PlayGroundBlast()
    {
        var _obj = vfxs.Find(x => x.id == VFXID.GROUNDBLAST).vfxobject;
        _obj.SetActive(true);
        DOVirtual.DelayedCall(2f, () => { _obj.SetActive(false); });
        // AudioController.con.PlayGameSFX(GameController.instance.thudEffectClip);
    }

    public void AttackWarriorCaptured(VFXID vfxid, Transform vfxEffectParent, float duration = 2f ) 
    {
        var obj = vfxs.Find(x => x.id == vfxid).vfxobject;
        var vfxObj = Instantiate(obj, vfxEffectParent);
        Destroy(vfxObj,duration);
    }

    private Color LastCellIndicatorRedColor = new Color(0.9f, 0.03882353f, 0.0388f);
    private Color LastCellIndicatorBlueColor = new Color(0.1381f, 0.2937f, 0.7547f);
    private Color LastCellIndicatorGreenColor = new Color(0f, 0.4f, 0.0076f);
    private Color LastCellIndicatorYellowColor = new Color(0.8691f, 0.9339f, 0.1533f);
    private Vector3 normalLastCellIndicatorScale = new Vector3(0.8f, 0.5f, 0.8f);
    [SerializeField] private float blinkSpeed;
    [SerializeField] private Material DestCellMat;
    [ColorUsageAttribute(false, true)] public Color initEmissionColorRed;
    [ColorUsageAttribute(false, true)] public Color emissionColorRed;
    [ColorUsageAttribute(false, true)] public Color initEmissionColorBlue;
    [ColorUsageAttribute(false, true)] public Color emissionColorBlue;
    [ColorUsageAttribute(false, true)] public Color initEmissionColorGreen;
    [ColorUsageAttribute(false, true)] public Color emissionColorGreen;
    [ColorUsageAttribute(false, true)] public Color initEmissionColorYellow;
    [ColorUsageAttribute(false, true)] public Color emissionColorYellow;
    public bool isDestinationCell;

    // @Madhu - activate last cell indicator animation
    public void WarriorLastCellIndicator(bool isActive, WarriorController warrior,  bool isSameCell = false)
    {
        isDestinationCell = isActive;
        LastCellIndicator.transform.localScale = normalLastCellIndicatorScale;

        if(warriorsInCell.Count >= 2)
        {
            LastCellIndicator.transform.localScale = normalLastCellIndicatorScale * 2.25f;
        }

        var cicleUpGameObj = LastCellIndicator.transform.GetChild(ConstantVars.INT_VAL_ZERO).gameObject;
        var meshFloorGameObj = LastCellIndicator.transform.GetChild((int)ConstantVars.FADE_VAL_ONE).gameObject;

        var circleUpParticleSystem = cicleUpGameObj.GetComponent<ParticleSystem>();
        var circleUpParticleSystemMain = circleUpParticleSystem.main;

        var meshFloorParticleSystem = meshFloorGameObj.GetComponent<ParticleSystem>();
        var meshFloorParticleSystemMain = meshFloorParticleSystem.main;

        Color initialEmissionColor = initEmissionColorRed;
        Color finalEmissionColor = emissionColorRed;


        switch (warrior.player.playerData.warriorType)
        {
            case WarriorType.Red:
                circleUpParticleSystemMain.startColor = LastCellIndicatorRedColor;
                meshFloorParticleSystemMain.startColor = LastCellIndicatorRedColor;
                initialEmissionColor = initEmissionColorRed;
                finalEmissionColor = emissionColorRed;
            break;
            case WarriorType.Bue:
                circleUpParticleSystemMain.startColor = LastCellIndicatorBlueColor;
                meshFloorParticleSystemMain.startColor = LastCellIndicatorBlueColor;
                initialEmissionColor = initEmissionColorBlue;
                finalEmissionColor = emissionColorBlue;
            break;
            case WarriorType.Green:
                circleUpParticleSystemMain.startColor = LastCellIndicatorGreenColor;
                meshFloorParticleSystemMain.startColor = LastCellIndicatorGreenColor;
                initialEmissionColor = initEmissionColorGreen;
                finalEmissionColor = emissionColorGreen;
            break;
            case WarriorType.Yellow:
                circleUpParticleSystemMain.startColor = LastCellIndicatorYellowColor;
                meshFloorParticleSystemMain.startColor = LastCellIndicatorYellowColor;
                initialEmissionColor = initEmissionColorYellow;
                finalEmissionColor = emissionColorYellow;
            break;
        }



        // if(!warrior.player.isBot())
        // {
        //     var warriorId1 = $"{warrior.player.userId}_W_0";
        //     var warriorId2 = $"{warrior.player.userId}_W_1";
        //     if(isSameCell || (warrior.warriorId == warriorId2)) //|| (warrior.player.GetActiveWarriors().Count > 1)
        //     {
        //         initialEmissionColor = initEmissionColorRed;
        //         finalEmissionColor = emissionColorRed;
        //         circleUpParticleSystemMain.startColor = LastCellIndicatorRedColor;
        //         meshFloorParticleSystemMain.startColor = LastCellIndicatorRedColor;
        //     }
        //     else if((warrior.warriorId == warriorId1)) 
        //     {
        //         initialEmissionColor = initEmissionColorBlue;
        //         finalEmissionColor = emissionColorBlue;
        //         circleUpParticleSystemMain.startColor = LastCellIndicatorBlueColor;
        //         meshFloorParticleSystemMain.startColor = LastCellIndicatorBlueColor;
        //     }
        // }


        Tween myTween = null;

        if(isActive)
        {

            if(myTween != null)
            {
                myTween.Kill();
            }
            
            if(type == CellType.COMMON_SAFE || type == CellType.START || type == CellType.CENTER )
            {
                SafeCellEM.enabled = !isActive;
                MagicCircleEM.enabled = !isActive;
            }

            // CommonGameObjectUtilities.ChangeMaterial(cellModel, DestCellMat);
            var mat = cellModel.material;
            mat.SetVector(ConstantVars.EMISSION_COLOR, initialEmissionColor);
            mat.color = initialEmissionColor;
            myTween = mat.DOVector(finalEmissionColor , ConstantVars.EMISSION_COLOR, blinkSpeed).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);

        }
        else
        {

            if(myTween != null)
            {
                myTween.Kill();
            }
            
            if(type == CellType.COMMON_SAFE || type == CellType.START || type == CellType.CENTER )
            {
                SafeCellEM.enabled = !isActive;
                MagicCircleEM.enabled = !isActive;
            }

            // CommonGameObjectUtilities.ChangeMaterial(cellModel, normalCellMat);

        }

        LastCellIndicator.SetActive(isActive);
        warrior.isLastCellIndiactorActive = isActive;
        
        // enable or disable manadala from cell
        if (type == CellType.NORMAL || type == CellType.HOME_SAFE || type == CellType.END || type == CellType.PILLAGE)
        {
            cellExtraSprite.gameObject.SetActive(!isActive);
        }


    }

    // @Madhu - cell glow aniation for land strip animation
    public void GlowDestinationPathCells(float blinkspeed , WarriorController warrior)
    {
        if(type == CellType.COMMON_SAFE || type == CellType.START || type == CellType.CENTER )
        {
            SafeCellEM.enabled = false;
            MagicCircleEM.enabled = false;
        }

        if (type == CellType.NORMAL || type == CellType.HOME_SAFE || type == CellType.END || type == CellType.PILLAGE)
        {
            cellExtraSprite.gameObject.SetActive(false);
        }

        Color initialEmissionColor = initEmissionColorRed;
        Color finalEmissionColor = emissionColorRed;

        switch (warrior.player.playerData.warriorType)
        {
            case WarriorType.Red:
                initialEmissionColor = initEmissionColorRed;
                finalEmissionColor = emissionColorRed;
            break;
            case WarriorType.Bue:
                initialEmissionColor = initEmissionColorBlue;
                finalEmissionColor = emissionColorBlue;
            break;
            case WarriorType.Green:
                initialEmissionColor = initEmissionColorGreen;
                finalEmissionColor = emissionColorGreen;
            break;
            case WarriorType.Yellow:
                initialEmissionColor = initEmissionColorYellow;
                finalEmissionColor = emissionColorYellow;
            break;
        }

        // CommonGameObjectUtilities.ChangeMaterial(cellModel, DestCellMat);
        var mat = cellModel.material;
        mat.SetVector(ConstantVars.EMISSION_COLOR, initialEmissionColor);
        mat.color = initialEmissionColor;

        mat.DOVector(finalEmissionColor , ConstantVars.EMISSION_COLOR, blinkspeed).SetEase(Ease.Linear).SetLoops(1, LoopType.Yoyo).OnComplete(() => {
            
            if(type == CellType.COMMON_SAFE || type == CellType.START || type == CellType.CENTER )
            {
                SafeCellEM.enabled = true;
                MagicCircleEM.enabled = true;
            }

            if (type == CellType.NORMAL || type == CellType.HOME_SAFE || type == CellType.END || type == CellType.PILLAGE)
            {
                cellExtraSprite.gameObject.SetActive(true);
            }

            // CommonGameObjectUtilities.ChangeMaterial(cellModel, normalCellMat);
        });
    }

    #endregion

    public void DialougeTrigger()
    {
        // Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!1 CellClicked");
        // if(isDestinationCell)
        // {
        //     switch(PlayerTurnReminderTimer.runningPrompt)
        //     {
        //         case PromptType.SelectWarrior:
        //             EventManager.UserdialougeTextAnimEventCaller(DialougeType.UserSelectWarrior, null, -1);
        //         break;
        //         case PromptType.SelectDirection:
        //             EventManager.UserdialougeTextAnimEventCaller(DialougeType.UserSelectDirection, null, -1);
        //         break;
        //     }
        // }
    }

    #region Animation

    public void AnimateCellExpand()
    {
        cellModelGameObj.transform.DOScaleX( initXScale * 2.25f, 0.4f);
    }

    public void AnimateCell()
    {
        // EventManager.BounceAnimationEventCaller(gameObject, Vector3.forward, onSteppedPosition, ConstantVars.VAL_QUATER, ConstantVars.LOOP_VAL_TWO, null);
    }
    #endregion



    // void Update()
    // {
    //     if (Input.GetMouseButtonDown(0)) // Left mouse button clicked
    //     {
    //         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //         RaycastHit hit;

    //         if (Physics.Raycast(ray, out hit))
    //         {
    //             GameObject hitObject = hit.collider.gameObject;
    //             Collider hitCollider = hit.collider;

    //             Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!11 Raycast hit object: " + hitObject.name);

                
    //         }
    //     }
    // }

}
