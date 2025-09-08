namespace FSM
{
    /// <summary>
    /// Claves de comandos para HandleInput(...) en los estados.
    /// Evita strings mágicos dispersos.
    /// </summary>
    public static class CommandKeys
    {
        public const string Jump = "Jump";
        public const string Interact = "Interact";
        public const string AttackPressed = "AttackPressed";
        public const string AttackHeavyReleased = "AttackHeavyReleased";
    }
}