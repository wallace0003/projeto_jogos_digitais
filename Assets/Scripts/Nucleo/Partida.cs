namespace CincoEstrelas
{
    /// <summary>
    /// O jogo sempre começa pela tela inicial (com as orientações de como jogar).
    /// Só no Editor dá para pular essa tela, pelo menu
    /// "Cinco Estrelas › Testar a fase direto", para testar mais rápido.
    /// </summary>
    public static class Partida
    {
        public const string ChavePularTelaInicial = "CincoEstrelas.PularTelaInicial";

        public static bool PodePularTelaInicial()
        {
#if UNITY_EDITOR
            return UnityEditor.SessionState.GetBool(ChavePularTelaInicial, false);
#else
            return false;
#endif
        }
    }
}
