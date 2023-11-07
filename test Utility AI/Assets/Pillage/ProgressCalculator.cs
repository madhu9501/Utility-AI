using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// @Madhu - Calculate progress fo the player in game 
public class ProgressCalculator : MonoBehaviour
{
    List<WarriorController> warriorControllers; 
    private List<CellHandler> _warrriorPath;
    private CellHandler _warriorCell;
    List<int> _warriorTotalCellCount = new List<int>();
    List<int> _warriorProgressCount = new List<int>();
    private float _playerProgress;
    private int _totalCellCount;
    private int _playerProgressCount;
    public int GetPlayerProgress()
    {
        return _playerProgressCount;
    }

    //  Get reference to all the pawns under a player in a list
    public void Init(PlayerController player)
    {
        warriorControllers = new List<WarriorController>(GetComponentsInChildren<WarriorController>());

        CalculateWarriorProgress(player);

    }

    // This method Calculates the progress of the each warior of a player 
    // total cell count is calculated using the length of the warrior path list in the warrior cntroller script
    // progress of the warrior is calculated using the index of the cell in the warrior path list on which the warrior is on  
    public void CalculateWarriorProgress(PlayerController player ) 
    {
        _playerProgress= 0; 
        _totalCellCount = 0;
        _playerProgressCount = 0;

        for( int i = 0; i < warriorControllers.Count; i ++)
        {
            _warrriorPath = warriorControllers[i].currentPath;
            _warriorCell = warriorControllers[i].currentCell;

            _warriorTotalCellCount.Insert( i, _warrriorPath.Count -1);
            _warriorProgressCount.Insert(i, warriorControllers[i].GetCellIndexOfPath(_warrriorPath, warriorControllers[i].targetPlayer, _warriorCell));

            _totalCellCount += _warriorTotalCellCount[i];
            _playerProgressCount += _warriorProgressCount[i];

        }

        _warriorTotalCellCount.Clear();
        _warriorProgressCount.Clear();

        // player.playerUI.UpdateProgressBar( _totalCellCount, _playerProgressCount);
    
    }

}
