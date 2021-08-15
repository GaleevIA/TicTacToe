using UnityEngine;
using UniRx;
using UnityEngine.UI;

//Данный скрипт организует работу верхней панели игрового интерейса
public class TopPanel : MonoBehaviour
{
    [SerializeField] private Text playerNameText;

    public void Init()
    {
        GameController.OnNextTurn.Subscribe(_ => UpdateCurrentPlayer()).AddTo(this);

        UpdateCurrentPlayer();
    }

    //Обновление представления текущего игрока в интерфейсе
    private void UpdateCurrentPlayer()
    {
        playerNameText.text = GameController.currentPlayerName;
    }

}
