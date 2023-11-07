using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WarriorInHeavenCheckConsideration", menuName = "ScriptableObject/AI/Consideration/WarriorInHeavenCheckConsideration")]

public class WarriorInHeavenCheckConsiderationSO : AIConsideratinSO<NpcController>
{
    public float ManipulateDiceMaxProbability;
    public float ManipulateDiceMinProbability;

    public override float ConsiderationScore(NpcController npc)
    {
        var playerController = npc.playerController;
        Score = ManipulateDiceMinProbability;

        foreach(var warrior in playerController.GetWarriors())
        {
            if(warrior.state == WarriorState.PillageDone )
            {
                Score = ManipulateDiceMaxProbability;
                break;
            }
        }
        return Score;
    }
}
