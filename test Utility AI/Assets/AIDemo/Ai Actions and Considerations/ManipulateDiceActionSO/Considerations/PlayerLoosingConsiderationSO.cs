// using UnityEngine;

// [CreateAssetMenu(fileName = "PlayerLoosingConsideration", menuName = "ScriptableObject/AI/Consideration/PlayerLoosingConsideration")]

// public class PlayerLoosingConsiderationSO : AIConsideratinSO
// {
//     public float ManipulateDiceMaxProbability;
//     public float ManipulateDiceMinProbability;

//     public override float ConsiderationScore(NpcController npc)
//     {
//         Score = ( (npc.GetBotDistToWin() < (npc.GetMaxDist()/2.5f))) ? ManipulateDiceMaxProbability : ManipulateDiceMinProbability; //(npc.GetBotDistToWin() < npc.GetPlayerDistToWin()) &&
//         // Debug.Log("!!!!!!!!!!!!!!!!!!! GetBotDistToWin" + npc.GetBotDistToWin());
//         // Debug.Log("!!!!!!!!!!!!!!!!!!! (npc.GetMaxDist()/2.5f)" + (npc.GetMaxDist()/2.5f));
//         // Debug.Log("!!!!!!!!!!!!!!!!!!! (npc.GetBotDistToWin() < (npc.GetMaxDist()/2.5f))" + (npc.GetBotDistToWin() < (npc.GetMaxDist()/2.5f)));
//         // Debug.Log("!!!!!!!!!!!!!!!!!!! Cheat Dice Score" + Score);

//         return Score;
//     }
// }

