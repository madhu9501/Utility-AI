using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using ConstantVariables;

/// <summary>
/// Currently Controls Dice Logic and Dice Ui logic
/// </summary>
public class DiceController : MonoBehaviour
{ 
    // [SerializeField] GameObject cheatButtons;

    public UnityAction<int,bool> OnDiceRoll;

    private List<int> diceValueProbability = new List<int>() {0, 1, 2, 3, 4, 5};
    // generate a dice value
    private int rollDiceValue  => diceValueProbability[Random.Range(0, diceValueProbability.Count)];

    private string botDelayRoll = "BotDelay";
  

    [Header("[UI]")]
    public Button diceButton;
    public Image DiceImage;
    public Image FillerImage;

    public GameObject turnArrow;

    public List<Sprite> currentPlayerDice;
    public List<Sprite> animatedDiceSprites;

    public Color timeFirstPhaseColor;
    public Color timerMidPhaseColor;
    public Color timerLastPhaseColor;
    private bool isPowerMove;


    // 
    private PlayerController _currentplayerController;
/*    private int RollDice(List<int> diceValueProbability)
    {

        return rollDiceValue = diceValueProbability[Random.Range(0, diceValueProbability.Count)];

    }*/
    // @MADHUSUDAN  

    public void OnDicePressedDown()
    {
        // EventManager.PlayerReminderTimerStopEventCaller();
    }

    public void OnDiceReleased()
    {

        // EventManager.PlayerReminderTimerRestartEventCaller();
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // perform dice animation
        // Roll(rollDiceValue);
    }

    // public void DiceClicked(DiceType type, float highProbabilityThreshold, List<int> value)
    public void DiceClicked(int rollDiceValue)
    {
        
        int diceValue = rollDiceValue; //RollDice(diceValueProbability);
        // int diceValue = DiceManipulatedValue(type, highProbabilityThreshold, value);

        // Logger.Log("diceValue===============" + diceValue);
        Roll(diceValue);

    }

    // private List<int> originalDiceValues = new List<int>() {1, 2, 3, 4, 5, 6};
    // private int DiceManipulatedValue(DiceType type, float highProbabilityThreshold, List<int> value)
    // {
    //     List<int> excludeValues;
    //     int randValue = -1;
    //     int diceValue = -1;
    //     float randomValue = Random.value;

    //     if (randomValue < highProbabilityThreshold)
    //     {
    //         switch(type)
    //         {
    //             // case DiceType.HighValue:

    //             // break;
    //             // case DiceType.LowValue:

    //             // break;
    //             // case DiceType.CaptureValue:

    //             // break;
    //             case DiceType.PillageValue:
    //                 randValue = Random.Range(0, value.Count);
    //                 diceValue = value[randValue];
    //             break;
    //             case DiceType.ReleaseValue:
    //                 diceValue = Random.Range(1, 7) <= 3 ? 1 : 6;
    //             break;
    //             case DiceType.Normal:
    //                 randValue = Random.Range(0, originalDiceValues.Count);
    //                 diceValue = originalDiceValues[randValue];
    //             break;
    //         }

    //     }
    //     else
    //     {
    //         switch(type)
    //         {
    //             case DiceType.PillageValue:
    //                 excludeValues = new List<int>(value);
    //                 diceValue = RemoveCommonValuesFromArray(excludeValues);
    //             break;
    //             case DiceType.ReleaseValue:
    //                 excludeValues = new List<int>() {1,6};
    //                 diceValue = RemoveCommonValuesFromArray(excludeValues);

    //             break;
    //             case DiceType.Normal:
    //                 randValue = Random.Range(0, originalDiceValues.Count);
    //                 diceValue = originalDiceValues[randValue];
    //             break;
    //         }

    //     }

    //     return diceValue;
    // }


    // private int RemoveCommonValuesFromArray(List<int> excludeValues)
    // {
    //     List<int> newDiceValues = new List<int>(originalDiceValues);

    //     for (int i = 0; i < excludeValues.Count; i++)
    //     {
    //         newDiceValues.RemoveAll(value => value == excludeValues[i]);
    //     }

    //     int randValue = Random.Range(0, newDiceValues.Count);
    //     return newDiceValues[randValue];

    // }

    
   
    public void ResetPosition()
    {
        transform.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        transform.GetComponent<RectTransform>().sizeDelta = Vector3.zero;
    }

    private void ToggleDiceImage(bool isEnable)
    {
        DiceImage.gameObject.SetActive(isEnable);
        FillerImage.gameObject.SetActive(isEnable);

    }

    //Divesh - Called for each player turn enables it's dice ui and runs the player's dice logic
    // Bot - either plays a power card or rolls dice bsed on power card condition
    public void Init(PlayerController player, bool skipTheSkipTurn)
    {
        isPowerMove = skipTheSkipTurn;
        _currentplayerController = player;
        _currentplayerController.diceController = this;

        DiceImage.sprite = player.playerData.playerDiceSprite;
        diceButton.spriteState = player.playerData.diceSpritePressed;

        DiceImage.DOFade(ConstantVars.FADE_VAL_ONE, ConstantVars.FADE_VAL_ZERO);

        // Logger.Log("skipTheSkipTurn " + skipTheSkipTurn);

        // transform.SetParent(player.playerUI.diceWaypoint.transform);
        ResetPosition();
        OnDiceRoll = player.OnDiceRolled;
        if(!player.isBot())
        {
            Sequence seq = DOTween.Sequence();
            // For Twice helper text
            // float delay = (player.twoMoveActivvateAnimation)? HelperTextManager.totalAnimDur : 0.0001f;

            if(!skipTheSkipTurn)
            {
                // seq.Append(DOVirtual.DelayedCall(delay, () => EventManager.FlyInFlyOutTextAnimEventCaller(FlyInOutAnimType.YourTurn)));
                // seq.Join(DOVirtual.DelayedCall(.00001f, () => EventManager.PlayerReminderTimerStartEventCaller(PromptType.UseCardOrDice)));
                // seq.Join(DOVirtual.DelayedCall(0.00001f, () => AudioController.con.PlayClipOnTurnTextSource(GameController.instance.playerTurnClip)));
            }
            else
            {
                // seq.Append(DOVirtual.DelayedCall(.00001f, () => EventManager.PlayerReminderTimerStartEventCaller(PromptType.UseDice)));
            }
            seq.Play();

        }
        else
        {
            DiceImage.DOFade(0, 0);
        }


        if (player.isBot(false))
        {
            var nonBotPlayer = GameController.instance.players.Find(p=> p.isBot() == false).userId;
            var prevPlayerId = GameController.instance._previousPlayerId;
            var currentPlayerId = GameController.instance.GetCurrentPlayer.userId;

            if(prevPlayerId != currentPlayerId )
            {
                if(prevPlayerId == nonBotPlayer)
                {
                    // EventManager.FlyInFlyOutTextAnimEventCaller(FlyInOutAnimType.EnenmyTurn1);
                }
                else if(prevPlayerId != nonBotPlayer)
                {
                    // EventManager.FlyInFlyOutTextAnimEventCaller(FlyInOutAnimType.EnenmyTurn2);

                }
                // AudioController.con.PlayClipOnEnemyTurnTextSource(GameController.instance.EnemyTurnClip);

            }
            // Logger.Log("skipTheSkipTurnInsideTheBot " + skipTheSkipTurn);

            var randSkip = Random.Range(0, 10) == 11 && !GameController.instance.isBotSkipTimerOnce;
            float delayToRoll = Random.Range(1, 1.5f);
            if (randSkip)
            {
                GameController.instance.isBotSkipTimerOnce = true;
                // delayToRoll = PlayerUI.maxTime + 1;
                // Logger.Log("Dice BOT skip !!!");
            }

            // Bot - decide to use power
            // Logger.Log("skipTheSkipTurnMoreInsideTheBot " + skipTheSkipTurn);
            //  Bot - gets a power card
            var randomPow = GameController.instance.powerupHandle.PlayerHasPowerup();

            // Logger.Log($"Bot decission - {randomPow} :: {delayToRoll} :: {randSkip}");
            if (randomPow != PowerType.NA && !skipTheSkipTurn)
            {
                // Bot - use power card
                DOVirtual.DelayedCall(Random.Range(1, 3), () => GameController.instance.powerupHandle.BotPowerUpUsed(randomPow)).SetId(botDelayRoll);

            }
            else
            {
                // Bot - use dice
                // DOVirtual.DelayedCall(delayToRoll, () => DiceClicked()).SetId(botDelayRoll);
                // player.aiController.GetBestAction();
                // DOVirtual.DelayedCall(delayToRoll, () => player.aiController.ExecuteTheBestAction()).SetId(botDelayRoll);
            }
        }
        else
        {
            EnableDice(true); // Enable the Dice
        }
            
    }

   

    public void SkipTurn()
    {
        if (!GameController.isGameRunning) return;

        // Logger.Log("Dice Turn Skipped !!!");
        // GameController.instance.GetCurrentPlayer.playerUI.UpdateSkipCount();
        OnRollDisable();
        OnDiceRoll?.Invoke(0,true);
    }

    public void OnRollDisable()
    {
        // disable powers 
        EnableDice(false);
       
    }

    //Divesh - Called when user presses dice button rolls a value
    public void Roll(int targetValue)
    {
        // @MADHUSUDAN - TODO
        // EventManager.PlayerReminderTimerStartEventCaller();
        
        if (!GameController.isGameRunning) return;
        // EventManager.Throw3dDiceEventCaller(targetValue);
        // EventManager.ChangeDiceVisibilityEventCaller(true);

        if (_currentplayerController.isBot() == false)
        {
            // _currentplayerController.playerUI.SetSelectionIndicatorDiceValue(targetValue + 1, true);
        }
        // Logger.Log($"Dice Roll = {targetValue}");
        OnRollDisable();

        //For disabling the powers
        var activePowers = GameController.instance.GetCurrentPlayer.ActivePowerups(false);
        // GameController.instance.GetCurrentPlayer.playerUI.DisablePowers(activePowers);

        // AudioController.con.PlayUISFX(GameController.instance.diceRolledClip);

        // StartCoroutine(DiceAnimationHandler(targetValue));
        DOVirtual.DelayedCall(ConstantVars.FADE_VAL_ONE, () => OnDiceRoll?.Invoke(targetValue, false));
       
    }

    //To handle the dice animation
    public IEnumerator DiceAnimationHandler(int targetValue)
    {
        int i = 0;
        if (_currentplayerController.isBot() == false)
        {
            DiceImage.transform.DOScale(new Vector3(1.25f, 1.25f, 1.25f), 0.2f).
           OnComplete(() => DiceImage.transform.DOScale(new Vector3(1f, 1f, 1f), 0.2f));
         //   _currentplayerController.playerUI.EnableDisabledDice(false);
        }
        while (i < animatedDiceSprites.Count)
        {
            yield return new WaitForSeconds(.02f);
           // DiceImage.sprite = animatedDiceSprites[i];
            i = i + 1;
        }
        
        if(i == animatedDiceSprites.Count)
        {
            yield return new WaitForSeconds(.02f);
           // DiceImage.sprite = currentPlayerDice[targetValue];
            yield return new WaitForSeconds(1f);

            //Send Dice roll value to player controller
            OnDiceRoll?.Invoke(targetValue, false);
        }
    }

    public void EnableDice(bool isEnable)
    {
        diceButton.enabled = isEnable;
        // cheatButtons.SetActive(isEnable && GameController.instance.EnableCheat);
        // GameController.instance.GetCurrentPlayer.playerUI.EnableDisablePlayerUI(isEnable);
        turnArrow.SetActive(isEnable);
       
        if (!isEnable)
        {
            //Logger.LogError("TODO - disable all ui");
            //DOTween.Kill(timerTweenId);
        }

        DOTween.Kill(botDelayRoll);
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);
    }





}
