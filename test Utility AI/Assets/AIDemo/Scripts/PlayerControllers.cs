// using UnityEngine;
// using UnityEngine.Events;


// public class PlayerControllers : MonoBehaviour
// {
//     private bool isPlayerTurn = false;
//     Vector3 initPos;
//     public bool IsShieldOn;
//     public int shiledOnTurnCount = 0;
//     public bool plus3CardUsed = false;
//     public bool botCaptured = false;
//     public int afterCaptureTurnCount = 0;



//     void Start()
//     {
//         initPos = transform.position;

//     }
//     private void Update()
//     {
//         // Check if it's the player's turn
//         if (isPlayerTurn)
//         {
//             // Check for player input
//             if (Input.GetKeyDown(KeyCode.Space))
//             {
//                 RollDice();
//             }

//             if (Input.GetKeyDown(KeyCode.Q))
//             {
//                 ActivateShield();
//             }

//             if (Input.GetKeyDown(KeyCode.W))
//             {
//                 PushEnemyBack();
//             }
//         }
//     }

//     void ActivateShield()
//     {
//         IsShieldOn = true;
//         Debug.Log("Sheild Activated");
//     }
    
//     void PushEnemyBack()
//     {
//         GameManager.Instance.bot.Moveback(false, 3);
//         plus3CardUsed = true;
//         Debug.Log("Pushed Enemy Back");

//     } 

//     private void RollDice()
//     {
//         int diceValue;
//         // Generate a random dice value (1, 2, or 3)
//         if(GameManager.Instance.bot.npcController.GetPlayerDistToWin() < 4.6f )
//         {
//             diceValue = Random.Range(1, 3);
//         }
//         else
//         {
//             diceValue = Random.Range(1, 4);
//         }

//         MoveFwd(diceValue);

//         if(shiledOnTurnCount > 0)
//             IsShieldOn = false;
//         // End the player's turn
//         isPlayerTurn = false;

//         // Notify the game manager that the player has finished their turn
//         GameManager.Instance.PlayerTurnEndedEvent?.Invoke();
//     }

//     public void StartPlayerTurn()
//     {
//         isPlayerTurn = true;
//     }

//     private void MoveFwd(int diceValue)
//     {
//         var newPos = new Vector3(transform.position.x , transform.position.y, transform.position.z - diceValue * 1.1f);
        
//         var canMove = GameManager.Instance.CheckPlayerCanMove(newPos.z);
//         if(canMove)
//         {
//             transform.position = newPos;
//             GameManager.Instance.CheckPlayerWinCondition(newPos.z);
//         }
//         Debug.Log("Player Dice Value: " + diceValue);

//     }

//     public void Moveback(bool backToBase = false , int value = 0)
//     {
//         if(backToBase)
//         {
//             transform.position = initPos;
//         }

//         transform.position = new Vector3(transform.position.x , transform.position.y, transform.position.z + value * 1.1f);
//     }
    
//     void OnEnable()
//     {
//         GameManager.Instance.PlayerTurnStartEvent += StartPlayerTurn;

//     }

//     void OnDisable()
//     {
//         GameManager.Instance.PlayerTurnStartEvent -= StartPlayerTurn;
//     }
    

// }