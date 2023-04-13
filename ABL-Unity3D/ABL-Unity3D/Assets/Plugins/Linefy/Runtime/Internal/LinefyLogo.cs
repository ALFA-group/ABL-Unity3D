using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy.Internal {

    public class LinefyLogo : Drawable {

        public DVector3Value logoCenter;
        public DFloatValue linesWidths;
        public DFloatValue shadowWidths;
        public DFloatValue shadowTransparency;
        public DFloatValue rombusRadius;
        public DFloatValue crossRadius;
        public DVector3Value shadowOffset;

        public DFloatValue crossRotation;

        public DFlag d_positions;

        Polyline rombus;
        Polyline rombusShadow;
        Lines cross;

        Color crossColor00 = Color.yellow;
        Color crossColor01 = Color.red;
        Color crossColor10 = Color.green;
        Color crossColor11 = Color.magenta;

        Texture _linesTexture;
        public Texture linesTexture {
            get {
                return _linesTexture;
            }

            set {
                _linesTexture = value;
                cross.texture = _linesTexture;
                rombus.texture = _linesTexture;
                rombusShadow.texture = _linesTexture;
            }
        }

        LabelsRenderer text;
        public DVector3Value textOffset;
        public DFloatValue textSize;
        DotsAtlas _font;
        public DotsAtlas font {
            get {
                return _font;
            }

            set {
                _font = value;
                text.atlas = _font;
            }
        }

        public LinefyLogo( ) {
            d_positions = new DFlag("positions", true);
            logoCenter = new DVector3Value(new Vector3(-20, 0, 0), d_positions);
            linesWidths = new DFloatValue(1.7f, d_positions);
            rombusRadius = new DFloatValue(5, d_positions);
            crossRadius = new DFloatValue(7, d_positions);
            crossRotation = new DFloatValue(-45, d_positions);
            shadowOffset = new DVector3Value(new Vector3(0.25f, -0.25f, 0), d_positions);
            shadowWidths = new DFloatValue(2.5f, d_positions);
            textOffset = new DVector3Value(new Vector3(6, 0, 0));
            textSize = new DFloatValue(0.04f);
            shadowTransparency = new DFloatValue(0.5f, d_positions);

            rombus = new Polyline(4, true);
            rombus.transparent = true;
            rombus[0] = new PolylineVertex(Vector3.zero, Color.red, 1, 0.2f);
            rombus[1] = new PolylineVertex(Vector3.zero, Color.yellow, 1, 0.21f);
            rombus[2] = new PolylineVertex(Vector3.zero, Color.blue, 1, 0.22f);
            rombus[3] = new PolylineVertex(Vector3.zero, Color.cyan, 1, 0.23f);
            rombus.SetTextureOffset(4, 0.24f);
            rombus.feather = 0.05f;
            rombus.renderOrder = 1;
            rombus.widthMode = WidthMode.WorldspaceXY;

            rombusShadow = new Polyline(4, true);
            rombusShadow.widthMode = WidthMode.WorldspaceXY;
            rombusShadow.transparent = true;
            rombusShadow[0] = new PolylineVertex(Vector3.zero, Color.white, 1, 0.7f);
            rombusShadow[1] = new PolylineVertex(Vector3.zero, Color.white, 1, 0.71f);
            rombusShadow[2] = new PolylineVertex(Vector3.zero, Color.white, 1, 0.72f);
            rombusShadow[3] = new PolylineVertex(Vector3.zero, Color.white, 1, 0.73f);
            rombusShadow.SetTextureOffset(4, 0.74f);
  
            rombusShadow.renderOrder = 0;

            cross = new Lines(12);
            cross.renderOrder = 2;
            cross.widthMode = WidthMode.WorldspaceXY;
            cross.transparent = true;
            cross.colorMultiplier = Color.white;
            cross.feather = 0.05f;

            text = new LabelsRenderer(font, 1);
            text.transparent = true;
            text.widthMode = WidthMode.WorldspaceXY;
            text[0] = new Label("LINEFY", Vector3.zero, Vector2Int.zero);
        }

        public override void DrawNow(Matrix4x4 matrix) {
            throw new System.NotImplementedException();
        }

        public override void GetStatistic(ref int linesCount, ref int totallinesCount, ref int dotsCount, ref int totalDotsCount, ref int polylinesCount, ref int totalPolylineVerticesCount) {
            throw new System.NotImplementedException();
        }

        public override void Draw(Matrix4x4 matrix, Camera cam, int layer) {
            PreDraw();
            rombusShadow.Draw(matrix, cam, layer);
            rombus.Draw(matrix, cam, layer);
            cross.Draw(matrix, cam, layer);
 
            text.Draw(matrix, cam, layer);
        }

        void PreDraw() {

            if (d_positions) {
                Color shadowColor = new Color(0, 0, 0, shadowTransparency);
                for (int i = 0; i < 4; i++) {
                    float _a = i * 0.25f * Mathf.PI * 2;
                    float _x = Mathf.Cos(_a) * rombusRadius;
                    float _y = Mathf.Sin(_a) * rombusRadius;
                    Vector3 pos = new Vector3(_x, _y) + logoCenter;
                    rombus.SetPosition(i, pos);
                    rombusShadow.SetPosition(i, pos + shadowOffset);
                }


                float crossAngle00 = crossRotation * Mathf.Deg2Rad;
                float crossAngle01 = crossAngle00 + Mathf.PI;

                float crossAngle10 = Mathf.PI + crossRotation * Mathf.Deg2Rad + Mathf.PI / 2;
                float crossAngle11 = crossAngle10 + Mathf.PI;

                Vector3 crossPos00 = new Vector3(Mathf.Cos(crossAngle00), Mathf.Sin(crossAngle00), 0) * crossRadius + logoCenter;
                Vector3 crossPos01 = new Vector3(Mathf.Cos(crossAngle01), Mathf.Sin(crossAngle01), 0) * crossRadius + logoCenter;
                Vector3 crossPos10 = new Vector3(Mathf.Cos(crossAngle10), Mathf.Sin(crossAngle10), 0) * crossRadius + logoCenter;
                Vector3 crossPos11 = new Vector3(Mathf.Cos(crossAngle11), Mathf.Sin(crossAngle11), 0) * crossRadius + logoCenter;

                float lv0_0 = linesWidths / (crossRadius * 2);
                float lv0_1 = 1 - lv0_0;

                Vector3 crossPos0_0 = Vector3.LerpUnclamped(crossPos00, crossPos01, lv0_0);
                Vector3 crossPos0_1 = Vector3.LerpUnclamped(crossPos00, crossPos01, lv0_1);

                Vector3 crossPos1_0 = Vector3.LerpUnclamped(crossPos10, crossPos11, lv0_0);
                Vector3 crossPos1_1 = Vector3.LerpUnclamped(crossPos10, crossPos11, lv0_1);

                Color crossColor0_0 = Color.Lerp(crossColor00, crossColor01, lv0_0);
                Color crossColor0_1 = Color.Lerp(crossColor00, crossColor01, lv0_1);

                Color crossColor1_0 = Color.Lerp(crossColor10, crossColor11, lv0_0);
                Color crossColor1_1 = Color.Lerp(crossColor10, crossColor11, lv0_1);

                cross[0] = new Line(crossPos00 + shadowOffset, crossPos0_0 + shadowOffset, shadowColor, shadowColor, shadowWidths, shadowWidths, 0.5f, 0.75f);
                cross[1] = new Line(crossPos0_0 + shadowOffset, crossPos0_1 + shadowOffset, shadowColor, shadowColor, shadowWidths, shadowWidths, 0.75f, 0.751f);
                cross[2] = new Line(crossPos0_1 + shadowOffset, crossPos01 + shadowOffset, shadowColor, shadowColor, shadowWidths, shadowWidths, 0.75f, 1f);

                cross[3] = new Line(crossPos00, crossPos0_0, crossColor00, crossColor0_0, linesWidths, linesWidths, 0, 0.25f);
                cross[4] = new Line(crossPos0_0, crossPos0_1, crossColor0_0, crossColor0_1, linesWidths, linesWidths, 0.25f, 0.25f);
                cross[5] = new Line(crossPos0_1, crossPos01, crossColor0_1, crossColor01, linesWidths, linesWidths, 0.25f, 0.5f);

                cross[6] = new Line(crossPos10 + shadowOffset, crossPos1_0 + shadowOffset, shadowColor, shadowColor, shadowWidths, shadowWidths, 0.5f, 0.75f);
                cross[7] = new Line(crossPos1_0 + shadowOffset, crossPos1_1 + shadowOffset, shadowColor, shadowColor, shadowWidths, shadowWidths, 0.75f, 0.751f);
                cross[8] = new Line(crossPos1_1 + shadowOffset, crossPos11 + shadowOffset, shadowColor, shadowColor, shadowWidths, shadowWidths, 0.75f, 1f);

                cross[9] = new Line(crossPos10, crossPos1_0, crossColor10, crossColor1_0, linesWidths, linesWidths, 0, 0.25f);
                cross[10] = new Line(crossPos1_0, crossPos1_1, crossColor1_0, crossColor1_1, linesWidths, linesWidths, 0.25f, 0.25f);
                cross[11] = new Line(crossPos1_1, crossPos11, crossColor1_1, crossColor11, linesWidths, linesWidths, 0.25f, 0.5f);


                rombus.widthMultiplier = linesWidths;
                rombusShadow.colorMultiplier = shadowColor;
                rombusShadow.widthMultiplier = shadowWidths;
                d_positions.Reset();
            }

            text.SetPosition(0, textOffset);
            text.size = textSize;
        }

        public override void Dispose() {
            rombus.Dispose();
            rombusShadow.Dispose();
            cross.Dispose();
            text.Dispose();
        }

    }
}
