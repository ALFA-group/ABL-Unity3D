using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Serialization;


namespace Linefy {

    [ExecuteInEditMode]
    public class EditableDots : EditableDrawableBase {

        public SerializationData_Dots properties = new SerializationData_Dots();
		public Dot[] items = new Dot[]{
			new Dot(new Vector3(-0.7f, 0, 0), 1, 3, Color.green),
            new Dot(new Vector3(0, 0.7f, 0), 1, 19, Color.red),
            new Dot(new Vector3(0.7f, 0, 0), 1, 35, Color.cyan),
            new Dot(new Vector3(0, -0.7f, 0), 1, 51, Color.yellow)
		};


        Dots _dots;
        Dots dots {
            get {
                if (_dots == null) {
                    _dots = new Dots(properties);
                }
                return _dots;
            }
        }
        public override Drawable drawable {
			get{
				return dots;
			}
		}  

        protected override void PreDraw() {
            base.PreDraw();
            if (itemsModificationId != dots.itemsModificationId) {
                dots.count = items.Length;
                for (int i = 0; i < items.Length; i++) {
                    dots[i] = items[i];
                }
                dots.itemsModificationId = itemsModificationId;
            }

            if (propertiesModificationId != dots.propertiesModificationId) {
                dots.LoadSerializationData(properties);
                dots.propertiesModificationId = propertiesModificationId;
            }
        }
 

        public static EditableDots CreateInstance() {
            GameObject go = new GameObject("New EditableDots");
            EditableDots result = go.AddComponent<EditableDots>();
            return result;
        }
    }
}
