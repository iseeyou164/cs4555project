using System.Collections;
using TMPro;
using UnityEngine;

public class DiceResult : MonoBehaviour
{
    public static DiceResult Instance;
    public TMP_Text resultText;

    private void Awake() => Instance = this;

    public void ShowResult(int value)
    {
        resultText.text = $"{value}";
        resultText.gameObject.SetActive(true);
        StartCoroutine(HideAfterDelay(1.5f));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        resultText.gameObject.SetActive(false);
    }
}
