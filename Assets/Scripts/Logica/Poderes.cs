namespace CincoEstrelas
{
    /// <summary>Cartas de poder deste protótipo (GDD, pág. 7).</summary>
    public enum CartaPoder
    {
        Nenhuma,
        /// <summary>P01 - Chute do Canhão: por alguns segundos, os chutes atravessam vários adversários.</summary>
        ChuteDoCanhao,
        /// <summary>P05 - Caneleira de Ouro: anula o próximo dano.</summary>
        CaneleiraDeOuro,
    }

    /// <summary>O que um quiz entrega para quem acerta.</summary>
    public enum Recompensa
    {
        Nenhuma,
        CartaChuteDoCanhao,
        CaneleiraDeOuro,
    }

    public static class Poderes
    {
        /// <summary>Quanto tempo o Chute do Canhão fica ativo depois do Espaço.</summary>
        public const float DuracaoCanhao = 10f;

        public static string Codigo(CartaPoder carta)
        {
            switch (carta)
            {
                case CartaPoder.ChuteDoCanhao: return "P01";
                case CartaPoder.CaneleiraDeOuro: return "P05";
            }
            return "";
        }

        public static string Nome(CartaPoder carta)
        {
            switch (carta)
            {
                case CartaPoder.ChuteDoCanhao: return "Chute do Canhão";
                case CartaPoder.CaneleiraDeOuro: return "Caneleira de Ouro";
            }
            return "";
        }

        public static string Descricao(CartaPoder carta)
        {
            switch (carta)
            {
                case CartaPoder.ChuteDoCanhao:
                    return "Aperte ESPAÇO: por 10 segundos, os chutes atravessam vários adversários.";
                case CartaPoder.CaneleiraDeOuro:
                    return "Já está valendo: anula o próximo dano.";
            }
            return "";
        }

        public static CartaPoder CartaDa(Recompensa r)
        {
            switch (r)
            {
                case Recompensa.CartaChuteDoCanhao: return CartaPoder.ChuteDoCanhao;
                case Recompensa.CaneleiraDeOuro: return CartaPoder.CaneleiraDeOuro;
            }
            return CartaPoder.Nenhuma;
        }
    }
}
