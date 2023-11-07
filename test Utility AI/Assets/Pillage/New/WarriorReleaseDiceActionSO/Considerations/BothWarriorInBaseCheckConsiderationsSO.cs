using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BothWarriorInBaseCheckConsiderations", menuName = "ScriptableObject/AI/Consideration/BothWarriorInBaseCheckConsiderations")]

public class BothWarriorInBaseCheckConsiderationsSO : AIConsideratinSO<NpcController>
{
    public float ManipulateDiceMaxProbability;
    public float ManipulateDiceMinProbability;

    public override float ConsiderationScore(NpcController npc)
    {
        var playerController = npc.playerController;

        Score = ManipulateDiceMaxProbability;

        foreach(var warrior in playerController.GetWarriors())
        {
            if(warrior.state != WarriorState.None )
            {
                Score = ManipulateDiceMinProbability;
                break;
            }
        }
        return Score;
    }

}
