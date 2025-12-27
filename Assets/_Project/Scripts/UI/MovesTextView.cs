using TMPro;
using UnityEngine;

public class MovesTextView : MonoBehaviour
{
    [SerializeField] private TMP_Text movesText;

    public void SetMoves(int movesLeft)
    {
        if (movesText == null) return;
        movesText.text = $"Moves: {movesLeft}";
    }
}
