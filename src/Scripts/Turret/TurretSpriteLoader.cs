using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Turret
{
    public class TurretSpriteLoader : MonoBehaviour
    {
        [SerializeField]
        private List<Sprite> TurretSprites = null;

        [SerializeField]
        private SpriteRenderer TurreSpriteRenderer = null;

        // Use this for initialization
        void Start()
        {
            int spriteInd = DataHandler.Instance.CurrentStageLevel % 10;
            spriteInd = spriteInd == 0 ? 9 : spriteInd;
            TurreSpriteRenderer.sprite = TurretSprites[spriteInd];
        }
    }
}
