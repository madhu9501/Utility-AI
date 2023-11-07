using System.Collections.Generic;
using UnityEngine;


public class NpcController : MonoBehaviour
{
    // [HideInInspector]
    // public AIPredictor aiPredictor;
    public PlayerController playerController;
    public bool ActionCompleted {get; private set; }
    public bool DiceRollCompleted {get; private set; }

    // public int DiceValue {get; private set; } 

    [SerializeField] private List<AIActionSO<NpcController>> ActionSO;
    public AIActionSO<NpcController> RollNormalDiceActionSO;
    private List<AIActionSO<NpcController>> DiceActionSO;
    private List<AIActionSO<NpcController>> PowerActionSO;
    
    void Awake()
    {
        // aiPredictor = GetComponent<AIPredictor>();
        playerController = GetComponent<PlayerController>();
        PowerActionSO = ActionSO;
    }

    public void PickPowerAction()
    {
        ActionCompleted = false;
        DiceRollCompleted = false;
        AIPredictor.BestActionToDo(PowerActionSO, RollNormalDiceActionSO, false, this);
    }

    public void PickDiceAction()
    {
        ActionCompleted = false;
        DiceRollCompleted = false;
        AIPredictor.BestActionToDo(DiceActionSO, RollNormalDiceActionSO, true, this);
    }

    public void ExecuteBestAction()
    {
        AIPredictor.actionToDo.ExecuteAction(this);
        // For removing power card after use
        if(AIPredictor.actionToDo.CanRemove)
            PowerActionSO.Remove(AIPredictor.actionToDo);

    }





































    // public void PlayDice()
    // {
    //     aiPredictor.DiceOrCheatDice(PowerActionSO[0], RollNormalDiceActionSO);
    // }

    // #region ParametersForConsideration
    // public float GetPlayerPos()
    // {
    //     return GameManager.Instance.player.transform.position.z;
    // }

    // public float GetPlayerDistToWin()
    // {
    //     return (GetMaxDist() - (GetMaxDist() - GetPlayerPos()));
    // }

    // public float GetBotDistToWin()
    // {
    //     return (GetMaxDist() - GetBotPos());
    // }

    // public float GetBotPos()
    // {
    //     return GameManager.Instance.bot.transform.position.z;
    // }
   
    // public float GetMaxDist()
    // {
    //     return  Mathf.Abs( GameManager.Instance.PlayerWinTrans.position.z  - GameManager.Instance.BotWinTrans.position.z);
    // }
   
    // // public bool PlayerPlus3CardUsed()
    // // {
    //     // return GameManager.Instance.player.plus3CardUsed;
    // // }
    // #endregion
   
    // #region Actions
    
    // public void plus3Steps()
    // {
    //     int powerSteps = 3;
    //     botController.MoveFwd(powerSteps);
    //     Debug.Log("Bot Used plus 3 Steps Card");
    //     ActionCompleted = true;
    //     DiceRollCompleted = false;

    // }

    // public void SkipOppTurn()
    // {
    //     botController.SetPlayerTurnSkip(true);
    //     Debug.Log("Bot Used Skip Opponent Turn Card");
    //     ActionCompleted = true;
    //     DiceRollCompleted = false;

    // }

    // public void RollDice(bool doManipulate, bool doCapture, bool botLoose)
    // {
    //     ActionCompleted = true;
    //     DiceRollCompleted = true;
    //     if(doManipulate)
    //     {
    //         DiceValue = 1;
    //         Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! BotCheated");
    //         return;

    //     }
    //     if(doCapture)
    //     {
    //         int diceValue = (int) (Mathf.Round(Mathf.Abs(GetPlayerPos() - GetBotPos() )));

    //         DiceValue = Mathf.Clamp(diceValue, 1, 3);
    //         // Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! CheatCaptureDice     "+ DiceValue);

    //         return;

    //         // DiceValue = Random.Range(1, 4);
    //     }
    //     if(botLoose)
    //     {
    //         int diceValue = Random.Range(1, 4);
    //         Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! diceValue    " + diceValue);
            
    //         var newPos = new Vector3(transform.position.x , transform.position.y, transform.position.z + diceValue * 1.1f);


    //         if((newPos.z > GameManager.Instance.BotWinTrans.position.z) && (newPos.z < GameManager.Instance.BotWinTrans.position.z + 0.5f))
    //         {
    //             Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Redice to avoid win" );

    //             RollDice(false, false, true);
    //             return;
    //         }
    //         DiceValue = diceValue;
    //     }
        
    //     if(!doManipulate && !doCapture && !botLoose)
    //         DiceValue = Random.Range(1, 4);

    //     // ActionCompleted = true;
    //     // DiceRollCompleted = true;

    // }
    // #endregion
}

