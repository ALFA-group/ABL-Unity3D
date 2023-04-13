using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy {
	public class EditableDrawableBase : DrawableComponent {
        [HideInInspector]
        public int propertiesModificationId;
        [HideInInspector]
        public int itemsModificationId;
        [HideInInspector]
        public bool enableOnSceneGUIEdit;
		
		[Tooltip("If enabled, the Properties will be applied always (every frame). To minimize CPU usage, enable this only if you are changing Properties fields with another script or animation.")]
		public bool updatePropertiesAlways;
		
		[Tooltip("If enabled, the Items will be applied always (every frame). To minimize CPU usage, enable this only if you are changing Items with another script.")]
		public bool updateItemsAlways;
		
		public void ApplyProperties(){
			propertiesModificationId ++;			
		}
		
		public void ApplyItems(){
			itemsModificationId ++;
		}
		
		protected override void PreDraw(){
            if (Application.isPlaying) {
                if (updateItemsAlways) {
                    itemsModificationId++;
                }
                if (updatePropertiesAlways) {
                    propertiesModificationId++;
                }
            } else {
                itemsModificationId++;
                propertiesModificationId++;
            }
        }
		
 
	}
}
