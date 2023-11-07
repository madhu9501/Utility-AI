// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// [CreateAssetMenu(fileName = "WarriorInPillageProximity", menuName = "ScriptableObject/AI/Consideration/WarriorInPillageProximity")]

// public class WarriorInPillageProximityConsiderationsSO : AIConsideratinSO
// {
//     public float ManipulateDiceMaxProbability;
//     public float ManipulateDiceMinProbability;
//     public override float ConsiderationScore(NpcController npc)
//     {
//         Score = ManipulateDiceMinProbability;

//         foreach(var warrior in npc.playerController.GetWarriors())
//         {
//             if(warrior.state == WarriorState.None)
//             {
//                 Score = ManipulateDiceMaxProbability;
//                 break;
//             }

//         }
//         List<WarriorController> warriorControllers = new List<WarriorController>( npc.playerController.GetWarriors());
//         for( int i = 0; i < warriorControllers.Count; i ++)
//         {
//             // _warrriorPath = warriorControllers[i].currentPath;
//             // _warriorCell = warriorControllers[i].currentCell;

//             // _warriorTotalCellCount.Insert( i, _warrriorPath.Count -1);
//             // _warriorProgressCount.Insert(i, warriorControllers[i].GetCellIndexOfPath(_warrriorPath, warriorControllers[i].targetPlayer, _warriorCell));

//             // _totalCellCount += _warriorTotalCellCount[i];
//             // _playerProgressCount += _warriorProgressCount[i];
            
            

//         }
//         return Score;
//     }

// }
