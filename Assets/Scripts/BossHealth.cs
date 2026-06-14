using UnityEngine;

/// <summary>
/// Health for boss-tier entities (placeholder + real fights). Damageable by
/// player projectiles (see Projectile). Fires onDeath once when HP hits 0.
/// Set invulnerable to gate damage during boss phases (e.g. whale submerged).
/// </summary>
public class BossHealth : MonoBehaviour
{
    public int maxHealth = 60;
    public bool invulnerable;
    public System.Action onDeath;
    public System.Action<int, int> onHealthChanged; // (current, max)

    public int Health { get; private set; }
    private bool dead;

    void Awake() { Health = maxHealth; }

    public void TakeDamage(int dmg)
    {
        if (dead || invulnerable || dmg <= 0) return;
        Health = Mathf.Max(0, Health - dmg);
        onHealthChanged?.Invoke(Health, maxHealth);
        if (Health <= 0)
        {
            dead = true;
            onDeath?.Invoke();
        }
    }
}
