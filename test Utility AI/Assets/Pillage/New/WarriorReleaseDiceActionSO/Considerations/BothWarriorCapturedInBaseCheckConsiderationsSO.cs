using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BothWarriorCapturedInBaseCheckConsiderations", menuName = "ScriptableObject/AI/Consideration/BothWarriorCapturedInBaseCheckConsiderations")]

public class BothWarriorCapturedInBaseCheckConsiderationsSO : AIConsideratinSO<NpcController>
{
    public float ManipulateDiceMaxProbability;
    public float ManipulateDiceMinProbability;

    public override float ConsiderationScore(NpcController npc)
    {
        var playerController = npc.playerController;

        Score = ManipulateDiceMaxProbability;

        foreach(var warrior in playerController.GetWarriors())
        {
            
            if(warrior.captureData.isCapture != true )
            {
                Score = ManipulateDiceMinProbability;
                break;
            }
        }
        return Score;
    }
}
