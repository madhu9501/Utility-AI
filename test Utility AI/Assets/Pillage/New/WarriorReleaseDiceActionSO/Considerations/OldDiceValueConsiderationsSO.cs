using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OldDiceValueConsiderations", menuName = "ScriptableObject/AI/Consideration/OldDiceValueConsiderations")]

public class OldDiceValueConsiderationsSO : AIConsideratinSO<NpcController>
{
    public float ManipulateDiceMaxProbability;
    public float ManipulateDiceMinProbability;
    public int NumberOfTurnToConsider;

    public override float ConsiderationScore(NpcController npc)
    {
        var playerController = npc.playerController;
        var oldDiceValues = playerController.playerDiceValues; 
        Score = ManipulateDiceMaxProbability;

        for(int i = oldDiceValues.Count - 1; i > oldDiceValues.Count - 1 - NumberOfTurnToConsider; i --)
        {
            if( oldDiceValues[i] == 1 || oldDiceValues[i] == 6 )
            {
                Score = ManipulateDiceMinProbability;
                break;
            }
        }


        return Score;
    }

}
