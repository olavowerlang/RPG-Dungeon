using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    bool TakeHit(int dmg, Vector2 hitDir, float knockback = 0f);
}
