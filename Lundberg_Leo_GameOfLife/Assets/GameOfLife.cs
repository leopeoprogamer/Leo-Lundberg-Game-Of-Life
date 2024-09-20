using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOfLife : MonoBehaviour
{
    int rows = 20;
    int columns = 20;
    int generaationCount = 0;
    float updateTime = 0.5f;
    float spawnChance = .5f;
    float cellSize = 1f;
   
    [SerializeField] GameObject cellPrefab;
    [SerializeField] Slider speedSlider;
    [SerializeField] TextMeshProUGUI generationText;
    [SerializeField] TextMeshProUGUI StableText;
    [SerializeField] 
    GameObject[,] grid;
    List<bool[,]> previousStates;

    bool[,] cellStates;
    bool[,] updatedCellStates;
    bool stop = false;

 
   



    void Start()
    {
        Application.targetFrameRate = 30;
        speedSlider.value = updateTime;

        rows = (int)Mathf.Floor((Camera.main.orthographicSize * Camera.main.aspect * 2 / cellSize) + 1);
        columns = (int)Mathf.Floor(Camera.main.orthographicSize * 2 / cellSize - 1);

        grid = new GameObject[rows, columns];
        cellStates = new bool[rows, columns];
        updatedCellStates = new bool[rows, columns];
        previousStates = new List<bool[,]>();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector2 position = new Vector2(i * cellSize - Camera.main.orthographicSize * Camera.main.aspect + cellSize *.5f,
                    j * cellSize - Camera.main.orthographicSize + cellSize * .5f);
                grid[i, j] = Instantiate(cellPrefab, position, Quaternion.identity);
                cellStates[i, j] = Random.value > spawnChance;

                UpdateCells(i, j);
            }
        }

        InvokeRepeating("UpdateGrid", updateTime, updateTime);
    }

    private void Update()
    {
        HandleSpeedControls();
    }

    private void HandleSpeedControls()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) && updateTime > .15f)
        {
            updateTime -= 0.1f;
            RestartSimulation();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && updateTime < 1f)
        {
            updateTime += 0.1f;
            RestartSimulation();
        }
    }

    private void RestartSimulation()
    {
        speedSlider.value = 1 - updateTime;
        CancelInvoke("UpdateGrid");
        InvokeRepeating("UpdateGrid", updateTime, updateTime);
    }

    void UpdateGrid()
    {
        generaationCount++;

        generationText.text = "Generation: " + generaationCount;

        if (IsStable())
        {
            CancelInvoke("UpdateGrid");
            StableText.gameObject.SetActive(true);
        }
        
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                int neighbours = CountNeighbours(i, j);
                bool cellIsAlive = cellStates[i, j];

                if (cellIsAlive && (neighbours < 2 || neighbours > 3))
                {
                    updatedCellStates[i, j] = false;
                }
                else if (!cellIsAlive && (neighbours == 3))
                {
                    updatedCellStates[i, j] = true;
                }
                else
                {
                    updatedCellStates[i, j] = cellStates[i, j];
                }
            }
        }

        StoreCurrentState();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                cellStates[i, j] = updatedCellStates[i, j];
                UpdateCells(i, j);
            }
        }

       
    }

    int CountNeighbours(int x, int y)
    {
        int neighbours = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;

                int neighbourX = (x + i);
                int neighbourY = (y + j);

                if (neighbourX >= 0 && neighbourX < rows && neighbourY >= 0 && neighbourY < columns)
                {
                    if (cellStates[neighbourX, neighbourY]) neighbours++;
                }

            }
        }
        return neighbours;
    }

    void StoreCurrentState()
    {
        bool[,] currentState = new bool[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                currentState[i, j] = cellStates[i, j];
            }
        }

        if (previousStates.Count > 10)
        {
            previousStates.RemoveAt(0);
        }

        previousStates.Add(currentState);
    }

    bool IsStable()
    {
        foreach (var prevState in previousStates)
        {
            if(AreStatesEqual(cellStates, prevState))
            {
                return true;
            }
        }

        return false;
    }

    bool AreStatesEqual(bool[,] state1, bool[,] state2)
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (state1[i, j] != state2[i, j])
                {
                    return false;
                }
            }
        }
        return true;
    }

    void UpdateCells(int x, int y)
    {
        Color color = cellStates[x, y] ? Color.black : Color.white;
        grid[x, y].GetComponent<SpriteRenderer>().color = color;
    }
}
