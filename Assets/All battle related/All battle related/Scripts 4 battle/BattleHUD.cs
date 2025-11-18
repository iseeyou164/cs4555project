using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class BattleHUD : MonoBehaviour
{
    //public TextMeshProUGUI nameText;
    //public TextMeshProUGUI levelText;
    public int s = 0;
    public Slider hpSlider;

    public void SetHUD(Unit unit)
    {
        if (s == 1)
        {
            DialogManager.Instance.ShowPlayerBattleStats(unit.unitName, unit.unitLevel);
        }
        else
        {
            DialogManager.Instance.ShowEnemyBattleStats(unit.unitName, unit.unitLevel);
        }
        //DialogManager.Instance.ShowEnemyBattleStats(unit.unitName, unit.unitLevel);
            //nameText.text = unit.unitName;
            //levelText.text = "Lvl " + unit.unitLevel;
        hpSlider.maxValue = unit.maxHealth;
        hpSlider.value = unit.currentHealth;
    }

    public void SetHP(int hp)
    {
        hpSlider.value = hp;
    }
}
