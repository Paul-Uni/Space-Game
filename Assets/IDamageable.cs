// Alles, was Schaden nehmen kann (Gegner, Spieler).
public interface IDamageable
{
    bool IsDead { get; }

    // Gibt true zurück, wenn der Schaden tatsächlich angewendet wurde
    // (Gegner: wenn dieser Treffer ihn getötet hat -> siehe Enemy.TakeDamage).
    bool TakeDamage(float amount);
}
