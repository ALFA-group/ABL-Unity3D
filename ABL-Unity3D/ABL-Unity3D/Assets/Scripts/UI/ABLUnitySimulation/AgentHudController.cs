using System;
using ABLUnitySimulation;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#nullable enable

namespace UI.ABLUnitySimulation
{
    [DefaultExecutionOrder(100)] // after SimAgentFollower!
    public class AgentHudController : MonoBehaviour
    {
        public SimAgentFollower? follower;
        public bool showObservedPosition;

        [FoldoutGroup("UI Wiring")] public Image? panelHealth;

        [FoldoutGroup("UI Wiring")] public Image? panelHealthBackground;

        [FoldoutGroup("UI Wiring")] public Image? iconImage;

        [FormerlySerializedAs("armorSprite")] [FoldoutGroup("UI Wiring")] public Sprite? typeASprite;
        [FormerlySerializedAs("vehicleSprite")] [FoldoutGroup("UI Wiring")] public Sprite? typeBSprite;
        [FormerlySerializedAs("humanSprite")] [FoldoutGroup("UI Wiring")] public Sprite? typeCSprite;
        [FoldoutGroup("UI Wiring")] public Sprite? typeDSprite;

        private Canvas? _hud;

        private void Start()
        {
            this._hud = this.GetComponentInChildren<Canvas>();
            if (this._hud) this._hud.worldCamera = Camera.main;
        }

        private void Update()
        {
            if (null == this._hud ||
                null == this.follower ||
                null == this.follower.myAgent ||
                null == this.panelHealthBackground) return;

            var SimAgent = this.follower.myAgent;

            if (SimAgent.positionObservedByBlue.Position == Vector2.negativeInfinity)
            {
                this.gameObject.SetActive(false);
                return;
            }

            this.gameObject.SetActive(true);

            this.UpdateHealthBar(SimAgent);
            this.UpdateUnitIcon(SimAgent);
        }

        private void LateUpdate()
        {
            if (this.showObservedPosition &&
                this.follower != null &&
                this.follower.myAgent != null &&
                !this.follower.myAgent.GetPositionObservedByEnemy().Position.Equals(Vector2.negativeInfinity))
            {
                var drawingPosition =
                    this.follower.SimToUnityCoordsHelper(this.follower.myAgent.GetPositionObservedByEnemy());
                if (this.transform.position != drawingPosition) this.transform.position = drawingPosition;
            }
        }

        private void UpdateUnitIcon(SimAgent simAgent)
        {
            if (null == this.iconImage) return;

            var icon = this.GetPreferredIconSprite(simAgent);
            if (null != icon && icon != this.iconImage.sprite) this.iconImage.sprite = icon;
        }

        private Sprite? GetPreferredIconSprite(SimAgent simAgent)
        {
            return simAgent.HighestPriorityType() switch
            {
                SimAgentType.TypeC => this.typeCSprite,
                SimAgentType.TypeB => this.typeBSprite,
                SimAgentType.TypeA => this.typeASprite,
                SimAgentType.TypeD => this.typeDSprite,

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void UpdateHealthBar(SimAgent simAgent)
        {
            if (null == this.panelHealth || null == this.panelHealthBackground) return;

            float maxHealth = simAgent.TotalMaxHealth();
            float currentHealth = simAgent.TotalCurrentHealth();

            bool isDamaged = currentHealth < maxHealth && currentHealth > 0;
            this.panelHealth.gameObject.SetActive(isDamaged);
            this.panelHealthBackground.gameObject.SetActive(isDamaged);
            if (isDamaged)
            {
                var anchorMax = this.panelHealth.rectTransform.anchorMax;
                anchorMax.x = maxHealth > 0 ? currentHealth / maxHealth : 0;
                this.panelHealth.rectTransform.anchorMax = anchorMax;
            }
        }
    }
}