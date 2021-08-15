using UnityEngine;
using UnityEngine.UI;

//Данный скрипт организует работу центральной панели игрового интерфейса
public class CenterPanel : MonoBehaviour
{
    [SerializeField] private Text winText;   

    public void Init(bool isDraw)
    {
        if (isDraw)
            winText.text = "Ничья!";
        else
            winText.text = $"Победил: {GameController.currentPlayerName}!";
    }
}
