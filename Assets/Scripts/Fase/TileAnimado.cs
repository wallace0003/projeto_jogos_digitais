using UnityEngine;
using UnityEngine.Tilemaps;

namespace CincoEstrelas
{
    /// <summary>
    /// Tile que troca de sprite sozinho (animação feita pelo próprio Tilemap).
    /// Usado na água dos canais de Estocolmo.
    /// </summary>
    public class TileAnimado : TileBase
    {
        public Sprite[] quadros;
        public float velocidade = 6f;

        public override void GetTileData(Vector3Int posicao, ITilemap tilemap, ref TileData dados)
        {
            dados.sprite = quadros != null && quadros.Length > 0 ? quadros[0] : null;
            dados.color = Color.white;
            dados.transform = Matrix4x4.identity;
            dados.flags = TileFlags.LockTransform | TileFlags.LockColor;
            dados.colliderType = Tile.ColliderType.None;
        }

        public override bool GetTileAnimationData(Vector3Int posicao, ITilemap tilemap, ref TileAnimationData dados)
        {
            if (quadros == null || quadros.Length == 0)
                return false;
            dados.animatedSprites = quadros;
            dados.animationSpeed = velocidade;
            // cada tile começa num quadro diferente para a água não "piscar" em bloco
            dados.animationStartTime = (posicao.x * 0.37f) % 1f;
            return true;
        }
    }
}
