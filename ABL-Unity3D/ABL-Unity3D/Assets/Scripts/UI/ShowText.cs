using TMPro;
using UnityEngine;

namespace UI
{
    public class ShowText : MonoBehaviour
    {
        public TextMeshProUGUI textMesh;

        public virtual string TextSource => "UNSET";

        protected virtual void Start()
        {
            if (!this.textMesh) this.textMesh = this.GetComponent<TextMeshProUGUI>();
        }

        protected virtual void Update()
        {
            if (this.textMesh) this.textMesh.SetText(this.TextSource);
        }
    }
}