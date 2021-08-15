using UniRx;
using UnityEngine;

//Данный скрипт организует работу всего игрового интерфейса
public class UIController : MonoBehaviour
{   
    [SerializeField] private GameObject gameField;
    [SerializeField] private CenterPanel centerPanel;
    [SerializeField] private TopPanel topPanel;
    [SerializeField] private GameObject waitIcon;

    void Start()
    {
        topPanel.Init();

        waitIcon.SetActive(false);

        GameController.OnGameEnd.Subscribe(x => UpdateView(isWin: true, isDraw: x)).AddTo(this);
        GameController.OnAI_TurnStart.Subscribe(_ => OnAI_TurnStart()).AddTo(this);
        GameController.OnAI_TurnEnd.Subscribe(_ => OnAI_TurnEnd()).AddTo(this);

        UpdateView(false, false);
    }

    //Обновляем видимость панелей
    private void UpdateView(bool isWin, bool isDraw)
    {
        centerPanel.gameObject.SetActive(isWin);

        if (isWin)
            centerPanel.Init(isDraw);
    }

    //Событие при старте хода компьютерного игрока
    private void OnAI_TurnStart()
    {
        waitIcon.SetActive(true);
    }

    //Событие при окончании хода компьютерного игрока
    private void OnAI_TurnEnd()
    {
        waitIcon.SetActive(false);
    }
}
