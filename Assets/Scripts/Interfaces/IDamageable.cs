using UnityEngine;

public interface IDamageable
{

    float ROLLBACKhealth { get; set; }

    float health { get; set; }

    void TakeDamage(float damage);

    void Die();

}
