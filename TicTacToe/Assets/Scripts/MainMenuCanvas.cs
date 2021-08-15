using UnityEngine.UI;
using UnityEngine;
using UniRx;
using UnityEngine.SceneManagement;

//Данный скрипт организует работу основного меню
public class MainMenuCanvas : MonoBehaviour
{
    [SerializeField] private GameObject fieldSizePanel;
    [SerializeField] private GameObject gameModePanel;
    [SerializeField] private Button button_3x3;
    [SerializeField] private Button button_5x5;
    [SerializeField] private Button button_PlayerVsPlayer;
    [SerializeField] private Button button_PlayerVsComputer;
    [SerializeField] private Button button_ComputerVsComputer;

    private bool fieldSizeSelected;
    private bool gameModeSelected;

    public static FieldSize fieldSize;
    public static GameMode gameMode;

    void Start()
    {
        button_3x3.OnClickAsObservable()
                    .Subscribe(_ => { fieldSize = FieldSize.Field_3x3; fieldSizeSelected = true; })
                    .AddTo(this);

        button_5x5.OnClickAsObservable()
                    .Subscribe(_ => { fieldSize = FieldSize.Field_5x5; fieldSizeSelected = true; })
                    .AddTo(this);

        button_PlayerVsPlayer.OnClickAsObservable()
                                .Subscribe(_ => { gameMode = GameMode.PlayerVsPlayer; gameModeSelected = true; })
                                .AddTo(this);

        button_PlayerVsComputer.OnClickAsObservable()
                                .Subscribe(_ => { gameMode = GameMode.PlayerVsComputer; gameModeSelected = true; })
                                .AddTo(this);

        button_ComputerVsComputer.OnClickAsObservable()
                                    .Subscribe(_ => { gameMode = GameMode.ComputerVsComputer; gameModeSelected = true; })
                                    .AddTo(this);

        this.ObserveEveryValueChanged(_ => gameModeSelected).Subscribe(_ => StartGame());
    }

    private void Update()
    {
        UpdateView();
    }

    //Обновление видимости панелей
    private void UpdateView()
    {
        fieldSizePanel.SetActive(!fieldSizeSelected);
        gameModePanel.SetActive(!gameModeSelected && fieldSizeSelected);
    }

    //Запуск игры, если все параметры выбраны
    private void StartGame()
    {
        if(fieldSizeSelected && gameModeSelected)
            SceneManager.LoadSceneAsync("GameScene");
    }
}

public enum FieldSize
{
    Field_3x3,
    Field_5x5
}

public enum GameMode
{
    PlayerVsPlayer,
    PlayerVsComputer,
    ComputerVsComputer
}
