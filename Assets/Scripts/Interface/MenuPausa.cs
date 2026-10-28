using UnityEngine;
using UnityEngine.UI;

namespace CincoEstrelas
{
    /// <summary>Menu de pausa (Esc / P / Start): continuar, reiniciar a fase ou voltar ao menu.</summary>
    public class MenuPausa : MonoBehaviour
    {
        GameObject conteudo;
        Button continuar;

        public bool Aberto
        {
            get { return conteudo != null && conteudo.activeSelf; }
        }

        public static MenuPausa Criar(GerenciadorFase g)
        {
            Canvas canvas = FabricaUI.CriarCanvas("Menu de pausa", 50);
            MenuPausa m = canvas.gameObject.AddComponent<MenuPausa>();
            Image cortina = FabricaUI.Cortina(canvas.transform, new Color(0.01f, 0.03f, 0.08f, 0.65f));
            m.conteudo = cortina.gameObject;
            Transform t = cortina.transform;
            Vector2 c = new Vector2(0.5f, 0.5f);

            FabricaUI.Painel(t, c, Vector2.zero, new Vector2(880f, 440f));
            FabricaUI.Texto(t, Textos.Pausa, c, new Vector2(0f, 180f), 4f, FabricaUI.CorDestaque).contorno = true;

            m.continuar = FabricaUI.Botao(t, Textos.Continuar, c, new Vector2(-250f, 70f), new Vector2(300f, 48f), g.AlternarPausa);
            Button reiniciar = FabricaUI.Botao(t, Textos.ReiniciarFase, c, new Vector2(-250f, 10f), new Vector2(300f, 48f), g.ReiniciarFase);
            Button menu = FabricaUI.Botao(t, Textos.MenuPrincipal, c, new Vector2(-250f, -50f), new Vector2(300f, 48f), g.IrParaMenu);
            FabricaUI.NavegacaoVertical(m.continuar, reiniciar, menu);
            FabricaUI.Texto(t, Textos.DicaSom, c, new Vector2(-250f, -110f), 2f, FabricaUI.CorApagada, TextAnchor.MiddleCenter,
                new Vector2(340f, 24f));

            // lembrete dos controles (os mesmos da tela inicial)
            FabricaUI.Texto(t, Textos.ComoJogar, c, new Vector2(170f, 130f), 2.5f, FabricaUI.CorDestaque, TextAnchor.MiddleCenter,
                new Vector2(460f, 30f));
            for (int i = 0; i < Textos.Controles.Length / 2; i++)
            {
                float y = 100f - i * 22f;
                FabricaUI.Texto(t, Textos.Controles[i * 2], c, new Vector2(40f, y), 1.5f, FabricaUI.CorVerde,
                    TextAnchor.MiddleRight, new Vector2(200f, 20f)).quebrarLinhas = false;
                FabricaUI.Texto(t, Textos.Controles[i * 2 + 1], c, new Vector2(270f, y), 1.5f, FabricaUI.CorTexto,
                    TextAnchor.MiddleLeft, new Vector2(240f, 20f)).quebrarLinhas = false;
            }
            m.conteudo.SetActive(false);
            return m;
        }

        public void Mostrar(bool mostrar)
        {
            conteudo.SetActive(mostrar);
            if (mostrar)
                FabricaUI.Selecionar(continuar);
        }
    }
}
