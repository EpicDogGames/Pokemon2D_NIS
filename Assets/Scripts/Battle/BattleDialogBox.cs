using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;

    [SerializeField] Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject choiceBox;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Image> actionImages;
    [SerializeField] List<Text> moveTexts;
    [SerializeField] List<Image> moveImages;
    [SerializeField] Image yesImage;
    [SerializeField] Image noImage;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;
    [SerializeField] Text yesText;
    [SerializeField] Text noText;

    [SerializeField] Color highlightedColor;
    [SerializeField] Color selectedTextColor;
    [SerializeField] Color selectedImageColor;
    [SerializeField] Color unselectedTextColor;
    [SerializeField] Color unselectedImageColor;

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;    
    }

    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    public void EnableChoiceBox(bool enabled)
    {
        choiceBox.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for (int i=0; i<actionTexts.Count; i++)
        {
            if (i == selectedAction)
            {
                actionTexts[i].color = selectedTextColor;
                actionImages[i].color = selectedImageColor;
            }
            else
            {
                actionTexts[i].color = unselectedTextColor;
                actionImages[i].color = unselectedImageColor;
            }
        }
    }

    public void UpdateMoveSelection(int selectedMove, Move move) 
    {
        for (int i=0; i<moveTexts.Count; i++)
        {
            if (i == selectedMove)
            {
                moveTexts[i].color = selectedTextColor;
                moveImages[i].color = selectedImageColor;
            }
            else
            {
                moveTexts[i].color = unselectedTextColor;
                moveImages[i].color = unselectedImageColor;
            }
        }

        ppText.text = $"PP {move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();

        if (move.PP == 0)
            ppText.color = Color.red;
        else
            ppText.color = Color.black;
    }

    public void SetMoveNames(List<Move> moves)
    {
        for  (int i=0; i<moveTexts.Count; i++)
        {
            if (i < moves.Count)
            {
                moveTexts[i].text = moves[i].Base.Name;
            }
            else
            {
                moveTexts[i].text = "-";
            }
        }
    }

    public void UpdateChoiceBox(bool yesSelected)
    {
        if (yesSelected)
        {
            yesText.color = selectedTextColor;
            yesImage.color = selectedImageColor;
            noText.color = unselectedTextColor;
            noImage.color = unselectedImageColor;
        }
        else
        {
            yesText.color = unselectedTextColor;
            yesImage.color = unselectedImageColor;
            noText.color = selectedTextColor;
            noImage.color = selectedImageColor;
        }
    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f/lettersPerSecond);
        }
        yield return new WaitForSeconds(1f);
    }
}
