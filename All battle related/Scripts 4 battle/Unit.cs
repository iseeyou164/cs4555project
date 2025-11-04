using UnityEngine;

public class Unit : MonoBehaviour
{
    public string unitName;
    public int unitLevel;
    public int damage;
    public int currentHealth;
    public int maxHealth;

    public bool TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0)
        {
            return true; // Unit is dead
        }
        else
        {
            return false; // Unit is still alive
        }
    }

    public void item(int a)
    {
        string firstItem = "Health Potion";
        if (firstItem == "Health Potion")
        {
            currentHealth += 20;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
        }
    }
}
