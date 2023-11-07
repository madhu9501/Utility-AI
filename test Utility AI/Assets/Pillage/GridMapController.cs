using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
/// <summary>
/// Creates and handles the list of all active cells in game
/// </summary>
public class GridMapController : MonoBehaviour
{
    [Header("Cells -->>")]
    public List<CellHandler> currentActiveCells;
    public Node root;
    //private List<CellHandler> allvisitedCells = new List<CellHandler>();
    [SerializeField] List<Node> allNodesInLevel = new List<Node>();
  

    [System.Serializable]
    public class Node
    {
        public CellHandler cellNode;
        public List<string> childernNode = new List<string>();
        private float _cellMinimumDistance = 0.62f;

        public void CreateNodeTree(CellHandler node, List<CellHandler> allCells,ref List<Node> totalNodesCollection)
        {
            //Logger.Log($"Added Node - {node.name}");
            cellNode = node;
            
            // find closest cell to current node within Range
            List<CellHandler> closestCells = new List<CellHandler>();
            var allCellsDistanceRange = allCells.Where(item =>  Mathf.Abs(Vector3.Distance(cellNode.transform.position, item.transform.position)) < _cellMinimumDistance).ToList();
            foreach (var item in allCellsDistanceRange)
            {
                if(item.cellId != cellNode.cellId && totalNodesCollection.Find(c => c.cellNode.cellId == item.cellId) == null)
                {
                    closestCells.Add(item);
                }
            }

            totalNodesCollection.Add(this);

            if (closestCells.Count > 0)
            {
                //Logger.Log($"{cellNode.name} Closet found - {closestCells.Count}", cellNode);
                foreach (var item in closestCells)
                {
                    //Logger.Log($"D - {Vector3.Distance(cellNode.transform.position, item.transform.position)}" +
                      // $" - N -> {item.name}", item.transform);
                    Node closeNode = new Node();
                    closeNode.CreateNodeTree(item, allCells, ref totalNodesCollection);
                    childernNode.Add(closeNode.cellNode.cellId);
                }
            }

          
        }
    }

    // Start is called before the first frame update
    public void Init(List<CellHandler> allCellsForLevel)
    {
        currentActiveCells = new List<CellHandler>(allCellsForLevel);

        // find the center cell 
        var center = currentActiveCells.Find(c => c.type == CellType.CENTER);
        CreateNaigationTree(center);
    }

    private void CreateNaigationTree(CellHandler centerRoot)
    {
        root = new Node();
        root.CreateNodeTree(centerRoot, currentActiveCells, ref allNodesInLevel);

        //GetPathToTarget(centerRoot, currentActiveCells[12]);
    }

    public List<Node> result = new List<Node>();
    public List<CellHandler> GetPathToTarget(CellHandler source, CellHandler target)
    {
        // calculate the f = g+h for all the cells from source to target
        var sourceNode = allNodesInLevel.Find(n => n.cellNode.cellId == source.cellId);
        var taretNode = allNodesInLevel.Find(n => n.cellNode.cellId == target.cellId);

        var center = currentActiveCells.Find(c => c.type == CellType.CENTER);
        var centerNode = allNodesInLevel.Find(n => n.cellNode.cellId == center.cellId);

        List<Node> centerToTargetResult = new List<Node>();
        TraverseNodeTree(centerNode, taretNode, ref centerToTargetResult);
        centerToTargetResult.Add(centerNode);

        List<Node> centerToSourceResult = new List<Node>();
        TraverseNodeTree(centerNode, sourceNode, ref centerToSourceResult);

        centerToTargetResult.Reverse();

        result = new List<Node>(centerToSourceResult);
        result.AddRange(centerToTargetResult);
        
        List<CellHandler> resultCellsPath = new List<CellHandler>();

        foreach (var item in result)
        {
            resultCellsPath.Add(item.cellNode);
        }

        return resultCellsPath;
        
    }

    private bool TraverseNodeTree(Node currentNode, Node targetNode,ref List<Node> path)
    {
        
        if (currentNode.cellNode.cellId == targetNode.cellNode.cellId)
        {
            return true;
        }
        else if (currentNode.childernNode.Count > 0)
        {
            foreach (var child in currentNode.childernNode)
            {
                // get the node of cell id
                var childNode = allNodesInLevel.Find(n => n.cellNode.cellId == child);
                if (TraverseNodeTree(childNode, targetNode,ref path))
                {
                    path.Add(childNode);
                    return true;
                }
            }
        }

        return false;

    }

    // @Madhu - get path fom center cell
    public List<CellHandler> PathFromCenter(CellHandler cell, bool isIncludeCenterCell = true)
    {
        var node1 = allNodesInLevel.Find(n => n.cellNode.cellId == cell.cellId);

        var center = currentActiveCells.Find(c => c.type == CellType.CENTER);
        var centerNode = allNodesInLevel.Find(n => n.cellNode.cellId == center.cellId);

        List<Node> nodePath = new List<Node>();
        TraverseNodeTree(centerNode, node1, ref nodePath);
        if(isIncludeCenterCell)
        {
            nodePath.Add(centerNode);
        }


        List<CellHandler> cellsPath = new List<CellHandler>();
        foreach (var item in nodePath)
        {
            cellsPath.Add(item.cellNode);
        }

        return cellsPath;

    }
    // Madhu

}
