using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace CincoEstrelas.Testes
{
    /// <summary>
    /// O mapa de verdade (Resources/Fases/Suecia1958.txt): tem tudo o que a
    /// fase precisa e todo pulo é possível com a física do jogo. Se alguém
    /// editar o mapa e deixar um vão impossível, este teste avisa.
    /// </summary>
    [TestFixture]
    public class TestesMapaDaFase
    {
        public const string Caminho = "Assets/Resources/Fases/Suecia1958.txt";

        /// <summary>
        /// Caminho completo do mapa. No Unity e no rodar_testes.ps1 a pasta atual já
        /// é a do projeto; no "dotnet test" é a pasta bin/, então sobe até achar.
        /// </summary>
        public static string CaminhoCompleto()
        {
            foreach (string inicio in new[] { Directory.GetCurrentDirectory(), AppDomain.CurrentDomain.BaseDirectory })
            {
                if (string.IsNullOrEmpty(inicio))
                    continue;
                DirectoryInfo d = new DirectoryInfo(inicio);
                while (d != null)
                {
                    string c = Path.Combine(d.FullName, Caminho);
                    if (File.Exists(c))
                        return c;
                    d = d.Parent;
                }
            }
            return Caminho;
        }

        static string Texto()
        {
            return File.ReadAllText(CaminhoCompleto());
        }

        static MapaFase Mapa()
        {
            return MapaFase.Ler(Texto());
        }

        /// <summary>Troca o caractere da célula (x, y) do mapa (y = 0 é a linha de baixo).</summary>
        static MapaFase Alterado(params object[] trocas)
        {
            var linhas = new List<string>(Texto().Replace("\r", "").Split('\n'));
            var grade = new List<int>();
            for (int i = 0; i < linhas.Count; i++)
            {
                string l = linhas[i];
                if (l.Length > 0 && !l.StartsWith("//") && !l.StartsWith("@"))
                    grade.Add(i);
            }
            for (int t = 0; t < trocas.Length; t += 3)
            {
                int x = (int)trocas[t];
                int y = (int)trocas[t + 1];
                char c = (char)trocas[t + 2];
                int indice = grade[grade.Count - 1 - y];
                char[] cs = linhas[indice].ToCharArray();
                cs[x] = c;
                linhas[indice] = new string(cs);
            }
            return MapaFase.Ler(string.Join("\n", linhas.ToArray()));
        }

        [Test]
        public void ArquivoExiste()
        {
            Assert.IsTrue(File.Exists(CaminhoCompleto()), Caminho);
        }

        [Test]
        public void MapaTemTudoOQueAFasePrecisa()
        {
            CollectionAssert.IsEmpty(Mapa().Validar(), "problemas no mapa");
        }

        [Test]
        public void FaseEhJogavel()
        {
            CollectionAssert.IsEmpty(AnalisadorMapa.Analisar(Mapa()), "a fase não passou na análise");
        }

        [Test]
        public void TextosDasPlacasTemLetraNaFonte()
        {
            MapaFase m = Mapa();
            int placas = 0;
            foreach (int n in m.NumerosComTexto)
            {
                char faltando;
                string t = m.TextoPlaca(n);
                Assert.IsTrue(GlifosFonte.TextoSuportado(t, out faltando), "placa " + n + ": falta '" + faltando + "'");
                placas++;
            }
            Assert.GreaterOrEqual(placas, 8);
        }

        [Test]
        public void NomeDaFase()
        {
            Assert.AreEqual("Suécia 1958", Mapa().Nome);
        }

        [Test]
        public void SemOBarcoOCanalNaoDaParaPular()
        {
            MapaFase m = Mapa();
            Celula barco = m.Encontrar(Legenda.Barco)[1];
            int esq, dir;
            m.LimitesBarco(barco, out esq, out dir);
            MapaFase sem = Alterado(barco.X, barco.Y, '.', esq, barco.Y, '.', dir, barco.Y, '.');
            Assert.AreEqual(1, sem.Encontrar(Legenda.Barco).Count);
            List<string> p = AnalisadorMapa.Analisar(sem);
            Assert.Greater(p.Count, 0, "sem o barco o canal de " + (dir - esq + 1) + " tiles devia ser impossível");
        }

        [Test]
        public void SinoBaixoDemaisEhDetectado()
        {
            MapaFase m = Mapa();
            Celula s = m.Primeira(Legenda.Sino);
            MapaFase baixo = Alterado(s.X, s.Y, '.', s.X, s.Y - 2, 'S');
            Assert.Greater(AnalisadorMapa.Analisar(baixo).Count, 0);
        }

        [Test]
        public void PonteCurtaDemaisEhDetectada()
        {
            MapaFase m = Mapa();
            Celula p = m.Primeira(Legenda.Ponte);
            int ini, fim;
            m.Trecho(p.X, p.Y, out ini, out fim);
            var trocas = new List<object>();
            // aterra o começo do canal: sobra um canal curto, que dá para pular
            for (int x = ini; x <= fim - 3; x++)
            {
                for (int y = 0; y <= p.Y; y++)
                {
                    trocas.Add(x);
                    trocas.Add(y);
                    trocas.Add('#');
                }
            }
            List<string> problemas = AnalisadorMapa.Analisar(Alterado(trocas.ToArray()));
            bool achou = false;
            foreach (string s in problemas)
                achou |= s.Contains("ponte");
            Assert.IsTrue(achou, "um canal de 3 tiles não precisa da ponte");
        }
    }
}
