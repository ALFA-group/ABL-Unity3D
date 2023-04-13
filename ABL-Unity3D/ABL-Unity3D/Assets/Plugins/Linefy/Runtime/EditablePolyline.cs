using UnityEngine;
using Linefy.Serialization;

namespace Linefy {

    [ExecuteInEditMode]
    public class EditablePolyline : EditableDrawableBase {
		
		[Tooltip("Polyline properties")]
        public SerializationData_Polyline properties = new SerializationData_Polyline(1);

		[Tooltip("Polyline vertices")]
        public PolylineVertex[] items = new PolylineVertex[] {
              new PolylineVertex(new Vector3(-0.5f, 0, 0), Color.red, 1, 0),
              new PolylineVertex(new Vector3(0, 0.5f, 0), Color.yellow, 1, 0.25f),
              new PolylineVertex(new Vector3(0.5f, 0, 0), Color.blue, 1, 0.5f),
              new PolylineVertex(new Vector3(0, -0.5f, 0), Color.cyan, 1, 0.75f)
        };

        Polyline _polyline;
        Polyline polyline {
            get {
                if (_polyline == null) {
                    _polyline = new Polyline(properties);
                }
                return _polyline;
            }
         }

        public override Drawable drawable {
			get{
				return polyline;
			}
		}  

        protected override void PreDraw() {
            base.PreDraw();
            if (itemsModificationId != polyline.itemsModificationId) {
                polyline.count = items.Length;
                for (int i = 0; i<items.Length; i++) {
                    polyline[i] = items[i];
                }
                polyline.itemsModificationId = itemsModificationId;
            }

            if (propertiesModificationId != polyline.propertiesModificationId) {
                polyline.LoadSerializationData(properties);
                polyline.propertiesModificationId = propertiesModificationId;
            }
        }
 
        public static EditablePolyline CreateInstance() {
            GameObject go = new GameObject("New EditablePolyline");
            EditablePolyline result = go.AddComponent<EditablePolyline>();
            return result;
        }
    }
}
