using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 100;
    public int damage = 10;

    [HideInInspector] public int currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(int dmg)
    {
        GetComponent<EnemyAI>()?.TakeDamage(dmg);
    }

}
