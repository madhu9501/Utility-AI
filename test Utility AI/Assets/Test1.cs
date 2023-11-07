// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Test1 : MonoBehaviour
// {
//     public bool GetCurrentBlockInfo(List<GameObject> listOfBlocks, Transform entityPosition, out int blockNumber, out GameObject block, float tolerance = 0.1f)
//     {
//         blockNumber = -1;
//         block = null;
//         // Safety check
//         if (listOfBlocks == null || listOfBlocks.Count == 0 || entityPosition == null)
//             return false;
//         block = FindClosestBlock(listOfBlocks, entityPosition, tolerance);
//         blockNumber = GetBlockIndex(listOfBlocks, block);
//         Debug.LogWarning("No block found for entity's current position.");
//         return false;
//     }

//     private GameObject FindClosestBlock(List<GameObject> blocks, Transform position, float tolerance)
//     {
//         return blocks.Find(cube => Mathf.Abs(cube.transform.position.z - position.position.z) < tolerance);
//     }

//     private int GetBlockIndex(List<GameObject> blocks, GameObject block)
//     {
//         return blocks.IndexOf(block);
//     }

//     public int GetCountOfBlocksToWin(List<GameObject> blocksList, GameObject currentBlock) {
//         int currentPosition = GetBlockIndex(blocksList, currentBlock);
//         if (currentPosition == -1)
//         {
//             Debug.LogWarning("Current block not found in blocks list.");
//             return -1;
//         }
//         return blocksList.Count - currentPosition + 1;
//     }

    
// }


// [CreateAssetMenu(menuName = "AI/Consideration/PlayerPawnReleased")]
// public class PlayerPawnReleasedConsideration : AIConsideratinSO
// {
//     public override float ConsiderationScore(NpcController npc)
//     {
//         float maxSteps = 12f; // Maximum steps in the game
//         float currentSteps = npc.BotStepsTaken; // Assuming you have a method or variable for this

//         // Normalize the score between 0 and 1
//         return currentSteps / maxSteps;
//     }
// }

// [CreateAssetMenu(menuName = "AI/Consideration/PlayerUsedReleaseCard")]
// public class PlayerUsedReleaseCardConsideration : AIConsideratinSO
// {
//     public override float ConsiderationScore(NpcController npc)
//     {
//         // If the player has used a release card, this will be high priority for the bot
//         return npc.PlayerUsedReleaseCard() ? 1f : 0f; // Assuming a method or boolean to check this
//     }
// }


// [CreateAssetMenu(menuName = "AI/Consideration/ConsecutiveGamesFirstTimePlayer")]
// public class ConsecutiveGamesFirstTimePlayerConsideration : AIConsideratinSO
// {
//     public override float ConsiderationScore(NpcController npc)
//     {
//         float score = 0f;

//         if (npc.IsSecondConsecutiveGame())
//         {
//             score = 0.6f;
//         }
//         else if (npc.IsThirdConsecutiveGame())
//         {
//             score = 0.8f;
//         }

//         if (npc.IsFirstTimePlayer())
//         {
//             score += 0.2f;
//         }

//         return Mathf.Clamp01(score);
//     }
// }


// public class Test2 : MonoBehaviour
// {

//     public int GetMaxTurnsBasedOnUserHistory()
//     {
//         int baseTurns = 3; // for new users
//         int experiencedUserTurns = 4; // example value for experienced users

//         if (IsFirstTimePlayer())
//         {
//             return baseTurns;
//         }
//         else if (NumberOfGamesPlayed() >= 4) // Assuming you have a method to get number of games played
//         {
//             return experiencedUserTurns;
//         }
//         // ... we can hv multiple conditions
        
//         return baseTurns; // default
//     }


//     public class PlayerReleaseTurnsConsideration : AIConsideratinSO
//     {
//         public override float ConsiderationScore(NpcController npc)
//         {
//             float maxTurns = npc.GetMaxTurnsBasedOnUserHistory();
//             float turnsTaken = npc.TurnsTakenToReleasePlayer(); // Assuming a method for this

//             return 1f - (turnsTaken / maxTurns);
//         }
//     }

// }