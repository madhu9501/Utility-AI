using System.Collections.Generic;
using UnityEngine;

// AI brain
public static class AIPredictor
{
    // public bool finishDecidingAction;
    [HideInInspector]
    public static AIActionSO<NpcController> actionToDo;
    // private NpcController npcController;

    // void Awake()
    // {
    //     npcController = GetComponent<NpcController>();
    // }

    public static void BestActionToDo(List<AIActionSO<NpcController>> actionsAvilable, AIActionSO<NpcController> RollDiceAction, bool isPickDiceAction, NpcController npcController)
    {  
        float score = 0;
        int nextBestActionIndex = 0;

        for(int i =0; i < actionsAvilable.Count; i++)
        {
            if(ActionScore(actionsAvilable[i], npcController) > score)
            {
                nextBestActionIndex = i;
                score = actionsAvilable[i].Score;
            }
        }
        if(score > RollDiceAction.consideration[0].ConsiderationScore(npcController))
        {
            actionToDo = actionsAvilable[nextBestActionIndex];
        }
        else if(!isPickDiceAction)
        {
            npcController.PickDiceAction();
        }
        else if(isPickDiceAction)
        {
            actionToDo = RollDiceAction;
        }
        
        // npcController.ExecuteBestAction();
        // finishDecidingAction = true;
    }

    static float ActionScore(AIActionSO<NpcController> action, NpcController npcController)
    {
        float score = 1f;
        for(int i=0; i < action.consideration.Length; i++ )
        {
            float considerationScore = action.consideration[i].ConsiderationScore(npcController);
            score *= considerationScore;

            if(score == 0)
            {
                action.Score = 0f;
                return action.Score;
            }
        }

        float originalScore = score;
        float modValue = 1 - (1 /action.consideration.Length);
        float makeUpValue = (1 - originalScore) * modValue;
        action.Score = originalScore + (makeUpValue * originalScore);

        return action.Score;
    }

    // public void DiceOrCheatDice(AIActionSO<NpcController> cheatDiceAction, AIActionSO<NpcController> RollDiceAction)
    // {
    //     // Debug.Log("ActionScore(cheatDiceAction)" + ActionScore(cheatDiceAction));
    //     // Debug.Log("RollDiceAction.Score" + RollDiceAction.Score);
    //     // Debug.Log("(ActionScore(cheatDiceAction) > RollDiceAction.Score)" + (ActionScore(cheatDiceAction) > RollDiceAction.Score));


    //     if(ActionScore(cheatDiceAction) > 0.5f)
    //     {
    //         cheatDiceAction.ExecuteAction(npcController);
    //     }
    //     else
    //     {
    //         RollDiceAction.ExecuteAction(npcController);
    //     }

    // }

}

