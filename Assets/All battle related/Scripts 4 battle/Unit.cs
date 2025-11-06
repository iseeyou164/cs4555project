using UnityEngine;


public class Unit : MonoBehaviour
{
    public string unitName;
    public int unitLevel;
    public int damage;
    public int currentHealth;
    public int maxHealth;
    private Animator animator;

    void Awake()
    {
        // Find the Animator on this same GameObject
        animator = GetComponent<Animator>(); // <-- ADD THIS METHOD
    }

    public void PlayAttackAnimation()
    {
        animator.SetTrigger("Attack");
    }

    public void PlayDeathAnimation()
    {
        animator.SetTrigger("Death");
    }

    public bool TakeDamage(int dmg)
    {
        animator.SetTrigger("DamageTaken");
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
