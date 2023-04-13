using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy {

    public class Matrix4x4InspectorAttribute : PropertyAttribute {
        public bool showValuesGrid = true;
        public bool showInputFields;

        public Matrix4x4InspectorAttribute(bool showInputFields) {

            this.showInputFields = showInputFields;
        }

        public Matrix4x4InspectorAttribute() {
            this.showInputFields = true;
            this.showValuesGrid = true;
        }
    }

    public class InfoStringAttribute : PropertyAttribute {
  
        public InfoStringAttribute( ) {
 
        }

 
    }
}
