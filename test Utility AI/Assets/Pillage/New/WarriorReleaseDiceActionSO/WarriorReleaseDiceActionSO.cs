using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WarriorReleaseDice", menuName = "ScriptableObject/AI/Action/WarriorReleaseDice")]
public class WarriorReleaseDiceActionSO : AIActionSO<NpcController>
{
    // public override bool CanRemove { get; set; } = false;
    public float highProbabilityThreshold;
    public override void ExecuteAction(NpcController npc)
    {
        npc.playerController.diceController.DiceClicked(6); //(DiceType.ReleaseValue, highProbabilityThreshold, new List<int> {-1}); // RollDice(false, false, true);

    }
}
