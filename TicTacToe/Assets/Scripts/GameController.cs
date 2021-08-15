using System;
using System.Linq;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject gameField;
    [SerializeField] private GameObject tilePrefab;

    [Space, SerializeField] private Color playerOneColor;
    [SerializeField] private Color playerTwoColor;
    [SerializeField] private int playerOneId;
    [SerializeField] private int playerTwoId;

    [Space, SerializeField] private int gameFieldSize;
    [SerializeField] private int tileSize;

    public static Subject<bool> OnNextTurn = new Subject<bool>();
    public static Subject<bool> OnGameEnd = new Subject<bool>();
    public static Subject<bool> OnAI_TurnStart = new Subject<bool>();
    public static Subject<bool> OnAI_TurnEnd = new Subject<bool>();

    public static string currentPlayerName;

    private bool playerOneIsAI;
    private bool playerTwoIsAI;
    private int currentPlayerId;
    private bool isAI_Move;

    //В данном списке будут храниться тайлы и ID игроков, которым они принадлежат
    private List<Tuple<Tile, int>> tiles;  
    //В данном списке будут храниться приоритетные тайлы для хода компьютерного игрока
    private List<Tile> tilesPriority;
    

    void Start()
    {
        tiles = new List<Tuple<Tile, int>>();

        GameParametersInit();
        GameFieldInit();
        GameModeInit();

        FirstTurn();
    }

    #region GameInit
    //Инициализация параметров, связанных с размером поля и режимом игры
    private void GameParametersInit()
    {
        switch (MainMenuCanvas.fieldSize)
        {
            case FieldSize.Field_3x3:
                gameFieldSize = 3;
                break;
            case FieldSize.Field_5x5:
                gameFieldSize = 5;
                break;
        }

        switch (MainMenuCanvas.gameMode)
        {
            case GameMode.PlayerVsComputer:
                playerTwoIsAI = true;
                break;
            case GameMode.ComputerVsComputer:
                playerOneIsAI = true;
                playerTwoIsAI = true;
                break;
        }
    }

    //Инициализация игрового поля и создание ячеек
    private void GameFieldInit()
    {
        gameField.GetComponent<RectTransform>().sizeDelta = new Vector2(gameFieldSize * tileSize, gameFieldSize * tileSize);

        for (int i = 0; i < gameFieldSize; i++)
        {
            for (int j = 0; j < gameFieldSize; j++)
            {
                var tile = Instantiate(tilePrefab, gameField.transform);
                tile.transform.localPosition = new Vector3(i * tileSize, j * tileSize);

                tile.GetComponent<Button>().OnClickAsObservable().Where(_ => !isAI_Move).Subscribe(_ => OnTileClick(tile)).AddTo(tile);

                var cTile = tile.GetComponent<Tile>();
                cTile.tilePosition = new Vector3(i, j);

                tiles.Add(new Tuple<Tile, int>(cTile, 0));

            }
        }
    }

    //Инициализация игрового режима, запуск компьютерных игроков
    private void GameModeInit()
    {
        if (playerOneIsAI || playerTwoIsAI)
            InitTilesPriority();

        if (playerOneIsAI)
            OnNextTurn.Where(_ => currentPlayerId == playerOneId).Subscribe(_ => NextMoveAI());

        if (playerTwoIsAI)
            OnNextTurn.Where(_ => currentPlayerId == playerTwoId).Subscribe(_ => NextMoveAI());
    }
    #endregion

    #region GameLoop
    //Обработка надатия на ячейку игрового поля
    private void OnTileClick(GameObject tile)
    {
        var curTuple = tiles.First(x => x.Item1.gameObject == tile);

        if (curTuple.Item2 != 0) return;

        tile.GetComponent<Tile>().UpdateView(GetPlayerColor(), GetPlayerText());

        tiles.Add(new Tuple<Tile, int>(curTuple.Item1, currentPlayerId));
        tiles.Remove(curTuple);

        if (CheckWinState())
            EndGame(isDraw: false);
        else if (CheckDrawState())
            EndGame(isDraw: true);
        else
            NextTurn();
    }

    //Определение первого хода
    private void FirstTurn()
    {
        var index = UnityEngine.Random.Range(0, 2);

        currentPlayerId = index == 0 ? playerOneId : playerTwoId;

        UpdateCurrentPlayerName();

        OnNextTurn.OnNext(true);
    }

    //Определение следующего хода
    private void NextTurn()
    {
        currentPlayerId = currentPlayerId == playerOneId ? playerTwoId : playerOneId;

        UpdateCurrentPlayerName();

        OnNextTurn.OnNext(true);
    }

    //Проверка выигрышного состояния
    private bool CheckWinState()
    {
        var isWin = false;

        for (int i = 0; i < gameFieldSize; i++)
        {
            var lineSum = tiles.Where(e => e.Item1.tilePosition.x == i).Sum(e => e.Item2);

            if (Mathf.Abs(lineSum) == gameFieldSize)
            {
                isWin = true;
                break;
            }
        }

        for (int j = 0; j < gameFieldSize; j++)
        {
            var columnSum = tiles.Where(e => e.Item1.tilePosition.y == j).Sum(e => e.Item2);

            if (Mathf.Abs(columnSum) == gameFieldSize)
            {
                isWin = true;
                break;
            }              
        }

        var mainDiagonalSum = tiles.Where(e => e.Item1.tilePosition.x == e.Item1.tilePosition.y).Sum(e => e.Item2);

        if (Mathf.Abs(mainDiagonalSum) == gameFieldSize)
            isWin = true;

        var secondDiagonalSum = tiles.Where(e => e.Item1.tilePosition.x == gameFieldSize - e.Item1.tilePosition.y - 1).Sum(e => e.Item2);

        if (Mathf.Abs(secondDiagonalSum) == gameFieldSize)
            isWin = true;

        return isWin;
    }

    //Проверка ничьей
    private bool CheckDrawState()
    {
        return !tiles.Any(e => e.Item2 == 0);
    }

    //Событие окончания игры
    private void EndGame(bool isDraw)
    {
        OnGameEnd.OnNext(isDraw);

        Time.timeScale = 0;
    }
    #endregion

    #region Common
    //Получает цвет символы текущего игрока для вывода в ячейку
    private Color GetPlayerColor()
    {
        return currentPlayerId == playerOneId ? playerOneColor : playerTwoColor;
    }

    //Получает символ текущего игрока для вывода в ячейку
    private string GetPlayerText()
    {
        return currentPlayerId == playerOneId ? "X" : "O";
    }

    //Обновляет представление текущего игрока, для вывода на экран
    private void UpdateCurrentPlayerName()
    {
        if (currentPlayerId == playerOneId)
            currentPlayerName = playerOneIsAI ? $"Игрок 1 (Комп.)" : "Игрок 1";
        else
            currentPlayerName = playerTwoIsAI ? $"Игрок 2 (Комп.)" : "Игрок 2";
    }
    #endregion

    #region AI
    //Определим список с приоритетом ячеек для компьютера
    private void InitTilesPriority()
    {
        tilesPriority = new List<Tile>();

        //Сначала отберем элементы по главное диагонали, у них будет самый высокий приоритет при поиске пустой клетки
        foreach (var tile in tiles.Where(e => e.Item1.tilePosition.x == e.Item1.tilePosition.y).Select(e => e.Item1))
            tilesPriority.Add(tile);

        //Затем отберем элементы по обратной диагонали, у них будет второй по важности приоритет при поиске пустой клетки
        foreach (var tile in tiles.Where(e => e.Item1.tilePosition.x == gameFieldSize - e.Item1.tilePosition.y - 1).Select(e => e.Item1))
            tilesPriority.Add(tile);

        //Добавим все оставшиеся элементы
        foreach (var tile in tiles.Where(e => !tilesPriority.Contains(e.Item1)).Select(e => e.Item1))
            tilesPriority.Add(tile);
    }

    //Компьютер делает свой ход. Добавил небольшую задержку, чтобы моментально не делались ходы
    private async void NextMoveAI()
    {
        isAI_Move = true;

        OnAI_TurnStart.OnNext(true);

        await Task.Delay(1000);

        Tile tile = null;

        //Проверяем наличие двух своих клеток в ряд
        tile = FindNextTile(currentPlayerId, 2);

        //Проверяем наличие двух вражеских клеток в ряд
        if (tile == null)
            tile = FindNextTile(currentPlayerId == playerOneId ? playerTwoId : playerOneId, 2);

        //Проверяем наличие уже поставленной клетки
        if (tile == null)
            tile = FindNextTile(currentPlayerId, 1);

        //Ищем подходящую свободную точку по приоритету
        if (tile == null)
            tile = FindFreeTile();

        OnAI_TurnEnd.OnNext(true);

        if (tile != null)
            OnTileClick(tile.gameObject);

        isAI_Move = false;
    }

    //Ищем следующую клетку для хода
    private Tile FindNextTile(int id, int checkCount)
    {
        Tuple<Tile, int> tuple = null;

        //Проверяем есть ли уже несколько одинаковых знаков в строках, но при этом в этой строке еще нет ни одного противоположного знака
        for (int i = 0; i < gameFieldSize; i++)
        {
            var tilesInLine = tiles.Where(e => e.Item1.tilePosition.x == i);

            var lineCount = tilesInLine.Count(e => e.Item2 == id);
            var lineCount2 = tilesInLine.Count(e => e.Item2 != id && e.Item2 != 0);

            if (lineCount >= checkCount && lineCount2 == 0)
            {
                tuple = tiles.FirstOrDefault(e => e.Item1.tilePosition.x == i && e.Item2 == 0);
                
                if(tuple != null) return tuple.Item1;
            }
        }

        //Проверяем есть ли уже несколько одинаковых знаков в столбцах, но при этом в этом столбце еще нет ни одного противоположного знака
        for (int j = 0; j < gameFieldSize; j++)
        {
            var tilesInColumn = tiles.Where(e => e.Item1.tilePosition.y == j);

            var columnCount = tilesInColumn.Count(e => e.Item2 == id);
            var columnCount2 = tilesInColumn.Count(e => e.Item2 != id && e.Item2 != 0);

            if (columnCount >= checkCount && columnCount2 == 0)
            {
                tuple = tiles.FirstOrDefault(e => e.Item1.tilePosition.y == j && e.Item2 == 0);

                if (tuple != null) return tuple.Item1;
            }
        }

        //Проверяем есть ли уже несколько одинаковых знаков в диагоналях, но при этом в этой диагонали еще нет ни одного противоположного знака
        var tilesInDiagonal = tiles.Where(e => e.Item1.tilePosition.x == e.Item1.tilePosition.y);

        var mainDiagonalCount = tilesInDiagonal.Count(e => e.Item2 == id);
        var mainDiagonalCount2 = tilesInDiagonal.Count(e => e.Item2 != id && e.Item2 != 0);

        if (mainDiagonalCount >= checkCount && mainDiagonalCount2 == 0)
        {
            tuple = tiles.FirstOrDefault(e => e.Item1.tilePosition.x == e.Item1.tilePosition.y && e.Item2 == 0);

            if (tuple != null) return tuple.Item1;
        }

        tilesInDiagonal = tiles.Where(e => e.Item1.tilePosition.x == gameFieldSize - e.Item1.tilePosition.y - 1);

        var secondDiagonalCount = tilesInDiagonal.Count(e => e.Item2 == id);
        var secondDiagonalCount2 = tilesInDiagonal.Count(e => e.Item2 != id && e.Item2 != 0);

        if (secondDiagonalCount >= checkCount)
        {
            tuple = tiles.FirstOrDefault(e => e.Item1.tilePosition.x == gameFieldSize - e.Item1.tilePosition.y - 1 && e.Item2 == 0);

            if (tuple != null) return tuple.Item1;
        }           

        return tuple?.Item1;
    }

    //Выбираем точку для хода среди приоритетных точек
    private Tile FindFreeTile()
    {
        var rnd = new System.Random();

        Tile tile = null;

        tile = tilesPriority.FirstOrDefault(e => tiles.Any(r => r.Item1 == e && r.Item2 == 0));

        return tile;
    }
    #endregion
}
