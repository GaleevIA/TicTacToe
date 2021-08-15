using UnityEngine;
using UnityEngine.UI;

//Данный скрипт хранит информацию о тайле и организует работу с его дочерними объектами
public class Tile : MonoBehaviour
{
    [SerializeField] private Text buttonText;

    public Vector3 tilePosition;
    
    //Обновляем внешний вид ячейки в зависимости от игрока, который ее нажал
    public void UpdateView(Color color, string text)
    {
        buttonText.color = color;
        buttonText.text = text;
    }
}
