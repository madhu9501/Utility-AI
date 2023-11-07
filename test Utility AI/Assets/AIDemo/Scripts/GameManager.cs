// using UnityEngine;
// using UnityEngine.Events;

// public class GameManager : MonoBehaviour
// {
//     public static GameManager Instance { get; private set; }

//     public PlayerController player;
//     public BotController bot;
//     public Transform PlayerWinTrans;
//     public Transform BotWinTrans;
//     public UnityAction PlayerTurnEndedEvent;
//     public UnityAction BotTurnEndedEvent;
//     public UnityAction PlayerTurnStartEvent;
//     public UnityAction BotTurnStartEvent;



//     // public UnityAction<GameObject> CaptureEvent;


//     void Awake()
//     {
//         // Singleton pattern to ensure only one instance of the GameManager exists
//         if (Instance == null)
//             Instance = this;
//         else
//             Destroy(gameObject);
        
//         Debug.Log(Instance);
//     }

//     void OnEnable()
//     {
//         // PlayerTurnEndedEvent += PlayerTurnEnded;
//         BotTurnEndedEvent += BotTurnEnded;
//     }

//     void OnDisable()
//     {
//         // PlayerTurnEndedEvent -= PlayerTurnEnded;
//         BotTurnEndedEvent -= BotTurnEnded;
//         // CaptureEvent -= CapturePawn;
//     }

//     private void Start()
//     {
//         // Start the game with the player's turn
//         StartPlayerTurn();
//     }

//     private void StartPlayerTurn()
//     {
//         // Allow the player to roll the dice and move
//         PlayerTurnStartEvent?.Invoke();

//     }

//     private void StartBotTurn()
//     {
//         // Allow the player to roll the dice and move
//         BotTurnStartEvent?.Invoke();
//     }

//     // public void PlayerTurnEnded()
//     // {
//     //     if(player.IsShieldOn)
//     //         player.shiledOnTurnCount++;
            
//     //     // Switch to the bot's turn
//     //     StartBotTurn();
        
//     // }

//     public void BotTurnEnded()
//     {
//         // Switch to the player's turn
//         if(bot.GetPlayerTurnSkip())
//         {
//             StartBotTurn();
//             bot.SetPlayerTurnSkip(false);
//         }
//         else
//         {
//             StartPlayerTurn();
//         }
//     }


//     public void CheckPlayerWinCondition(float pawnPosition)
//     {
//         if(pawnPosition < PlayerWinTrans.position.z )
//         {
//             var playerPos = bot.transform.position.z ;
//             playerPos = PlayerWinTrans.position.z;
//             PlayerWin();
//         }

//         if(IsInCaptureDist(player.transform.position, bot.transform.position) )
//         {
//             bot.Moveback(true);
//             Debug.Log("Bot Captured!!");
//         }

//     }
    
//     public bool CheckPlayerCanMove(float pawnPosition)
//     {
//         if(pawnPosition < PlayerWinTrans.position.z - 0.5f )
//         {
//             return false;
//         }
//         else
//         {
//             return true;
//         }
//     }

//     public bool CheckBotCanMove(float pawnPosition)
//     {
//         if(pawnPosition > BotWinTrans.position.z + 0.5f)
//         {
//             return false;
//         }
//         else
//         {
//             return true;
//         }
//     }

//     public void CheckBotWinCondition(float pawnPosition)
//     {
//         if(pawnPosition > BotWinTrans.position.z )
//         {
//             var botPos = bot.transform.position.z ;
//             botPos = BotWinTrans.position.z;
//             BotWin();
//         }
        
//         // if(IsInCaptureDist(player.transform.position, bot.transform.position) && player.IsShieldOn == false)
//         // {
//         //     player.Moveback(true);
//         //     Debug.Log("Player Captured!!");
//         // }
//     }

//     private void PlayerWin()
//     {
//         Debug.Log("Player Wins!!!!");
//         EndGame();
        
//     }

//     private void BotWin()
//     {
//         Debug.Log("Bot Wins!!!!!");
//         EndGame();
//     }

//     private void EndGame()
//     {
//         player.enabled = false;
//         bot.enabled = false;
//     }


//     bool IsInCaptureDist(Vector3 pos1, Vector3 pos2)
//     {
//         float diff = Mathf.Abs(pos1.z - pos2.z);
//         return (diff < 0.5f);

//     }



// }