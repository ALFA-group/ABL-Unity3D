using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Serialization;

namespace Linefy {
    [ExecuteInEditMode]
    public class EditableLines : EditableDrawableBase {
		
        public SerializationData_Lines properties = new SerializationData_Lines();
        public Line[] items = new Line[]{
			new Line(new Vector3(-0.7f, 0.7f), new Vector3(0.7f, -0.7f), Color.red, Color.yellow, 1),
            new Line(new Vector3(0.7f, 0.7f), new Vector3(-0.7f, -0.7f), Color.cyan, Color.green, 1)
		};
 
		Lines _lines;
        Lines lines {
            get {
                if (_lines == null) { 
                    _lines = new Lines(properties);
                }
                return _lines;
            }
        }

        public override Drawable drawable {
			get{
				return lines;
			}
		}  

        protected override void PreDraw() {
            base.PreDraw();
             if (itemsModificationId != lines.itemsModificationId) {
                lines.count = items.Length;
                for (int i = 0; i < items.Length; i++) {
                    lines[i] = items[i];
                }
                lines.itemsModificationId = itemsModificationId;
            }

            if (propertiesModificationId != lines.propertiesModificationId) {
                lines.LoadSerializationData(properties);
                lines.propertiesModificationId = propertiesModificationId;
            }
        }

        public static EditableLines CreateInstance() {
            GameObject go = new GameObject("New EditableLines");
            EditableLines result = go.AddComponent<EditableLines>();
            return result;
        }
    }
}
