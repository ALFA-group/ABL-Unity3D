using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;
using Linefy.Internal;
using Linefy.Serialization;
 
namespace LinefyExamples {
    [DefaultExecutionOrder(71)]
    [ExecuteInEditMode]
    public class LinefyDemo_PolylineGraph : MonoBehaviour {      

        [System.Serializable]
        public class Axis {
            public int gridLinesCount = 5;
            public float valuesRangeFrom = 0;
            public float valuesRangeTo = 1;
            public Vector2Int labelOffset;
            public string labelFormat = @"{0:0.00}";
            public int labelsCount = 3;
            public bool padLabelsRight;
            public Color gridColor = new Color(1,1,1,0.4f);
            public Color labelColor = new Color(1, 1, 1, 0.6f);
            public int labelTextSize = 10;
            //GUIStyle labelsStyle;
            Lines gridLines;
            LabelsRenderer labels;
            bool textsetted;
            public float range {
                get {
                    return Mathf.Abs(valuesRangeFrom - valuesRangeTo);
                }
            }

            public void DrawGrid( Rect screenRect, int axisId, int orthoAxisId, Matrix4x4 screenTM, int renderOrder ) {
                if (gridLines == null) {
                    gridLines = new Lines(this.gridLinesCount);
                }
                gridLines.transparent = true;
                gridLines.feather = 0;
                gridLines.count = this.gridLinesCount;
                gridLines.renderOrder = renderOrder;
                float _step = screenRect.size[axisId]/((int)gridLinesCount - 1);
                float length = screenRect.size[orthoAxisId];
                int labelsSpace = (gridLinesCount - 1) / (labelsCount-1);
                for (int i = 0; i<gridLinesCount; i++) {
                    Vector2 posA = screenRect.position;
                    posA[axisId] += _step * i;
                    Vector2 posB = posA;
                    posB[orthoAxisId] += length;
                    gridLines.SetPosition(i, posA, posB);
                    bool labeled = (i % labelsSpace) == 0;
                    if (labeled) {
                        gridLines.SetColor(i, labelColor);
                    } else {
                        gridLines.SetColor(i, gridColor);
                    }
                }

                gridLines.Draw(screenTM);
            }

            public void DrawLabels(LinefyDemo_PolylineGraph parent, Vector2 a, Vector2 b) {
 
                labelsCount = Mathf.Clamp(labelsCount, 0, gridLinesCount - 1);
                if (labels == null) {
                    labels = new LabelsRenderer(labelsCount);
                }
                labels.size = labelTextSize;
                labels.count = labelsCount;
                labels.zTest = UnityEngine.Rendering.CompareFunction.Always;

                float linesStep =   (float)(gridLinesCount - 1) / (labelsCount-1);
                float lvStep = 1f/ (float)(gridLinesCount - 1);

                for (int i = 0; i < labelsCount; i++) {
                    int lineIdx =  (int)(i *linesStep) ;
                    float persentage = lineIdx * lvStep;
                    Vector2 labelPos = Vector2.LerpUnclamped(a, b, lineIdx * lvStep);
                    float val = Mathf.LerpUnclamped(valuesRangeFrom, valuesRangeTo, persentage);

                    if (textsetted == false) {
                        string text = string.Format(labelFormat, val);
                        labels[i] = new Label(text, labelPos, labelOffset);
                        textsetted = true;
                    }

                    if (Application.isPlaying) {
                        labels.SetPosition(i, labelPos);
                        labels.SetOffset(i, labelOffset);
                    }  
                }
                labels.Draw(parent.nearClipPlane.gui);
            }
        }

        public NearClipPlaneMatrix nearClipPlane;
        public Rect viewportRect;
        public int renderOrder;

        [Header("Graph")]
        [Range(8, 256)]
        public int valuesCount = 32;

        public float alphaMult = 1;
        public Color color = Color.gray;
        public Color outlineColor = Color.white;
        public float outlineWidth = 3;
 
        [SerializeField]
        List<float> valuesList;
        Rect frameScreenRect;

        [Header("Axis")]
        public Axis xAxis ;
        public Axis yAxis ;

        [Header("HeaderText")]
        public string headerText;
        public float headerSize;
        public Color headerColor;
        public Vector2  headerOffset;
        LabelsRenderer headerLabelRenderer;

        Polyline graphPolyline;
        PolygonalMesh gradientFill;

        void LateUpdate() {
            ValidateValuesList();

            if (graphPolyline == null) {
                graphPolyline = new Polyline(valuesCount, true, 1, false);
                graphPolyline.name = "Graph";
            }

            if (gradientFill == null) {
                CreateGraphPolymesh();
            }

            graphPolyline.count = valuesCount;
            frameScreenRect = nearClipPlane.cameraPixelRect.Multiply(viewportRect);

            float xStep = frameScreenRect.width / (float)(valuesList.Count - 1);
            float xPos = frameScreenRect.position.x;
            float yPos = frameScreenRect.position.y;

            Color fillColor = color ;
            fillColor.a = 0;

            for (int i = 0; i < valuesList.Count; i++) {
                float normalizedToMaxScaleValue = Mathf.InverseLerp(yAxis.valuesRangeFrom,   yAxis.valuesRangeTo,  valuesList[i] );
                float valueYPos = normalizedToMaxScaleValue * frameScreenRect.height;
                Vector3 pos = new Vector3(xPos + xStep * i, yPos + valueYPos, 0);
                graphPolyline.SetPosition(i, pos);
                gradientFill.SetPosition(i, new Vector3(pos.x, yPos, 0));
                gradientFill.SetPosition(i+valuesCount, pos);
                gradientFill.SetColor(i, fillColor);
                gradientFill.SetColor(i + valuesCount, new Color(fillColor.r, fillColor.g, fillColor.b, color.a * normalizedToMaxScaleValue * alphaMult )  );
            }

            graphPolyline.colorMultiplier = outlineColor ;
            graphPolyline.widthMultiplier = outlineWidth;
            graphPolyline.feather = 1;
            gradientFill.Draw(nearClipPlane.screen);
            graphPolyline.Draw(nearClipPlane.screen);
            gradientFill.renderOrder = renderOrder;
            graphPolyline.renderOrder = renderOrder + 1;
            xAxis.DrawGrid(frameScreenRect, 0, 1, nearClipPlane.screen, renderOrder + 12);
            yAxis.DrawGrid(frameScreenRect, 1, 0, nearClipPlane.screen, renderOrder + 12);
            Rect guiRect = frameScreenRect;
            Vector2 guiRectPos = guiRect.position;
            guiRectPos.y = Screen.height - guiRectPos.y - guiRect.height;
            guiRect.position = guiRectPos;
            xAxis.DrawLabels(this, guiRect.Point1(), guiRect.Point2());
            yAxis.DrawLabels(this, guiRect.Point1(), guiRect.Point0());

            if (string.IsNullOrEmpty(headerText) == false) {
                if (headerLabelRenderer == null) {
                    headerLabelRenderer = new LabelsRenderer(1);
                }
                Vector2 pixelPos = new Vector2(frameScreenRect.x + frameScreenRect.width / 2, frameScreenRect.y+ frameScreenRect.height) + headerOffset;
                headerLabelRenderer.size = headerSize;
                headerLabelRenderer.textColor = headerColor;
                headerLabelRenderer[0] = new Label(headerText, pixelPos, Vector2Int.zero);
                headerLabelRenderer.Draw(nearClipPlane.screen);
            }
        }
 
        void ValidateValuesList() {
            if (valuesList == null) {
                valuesList = new List<float>();
                Debug.Log("clear values");
            }
 
            if (valuesList.Count != valuesCount) {
                OnChangeValuesCount();
            }
        }

        void OnChangeValuesCount() {
            float[] arr = valuesList.ToArray();
            System.Array.Resize(ref arr, valuesCount);
            valuesList = new List<float>(arr);
            CreateGraphPolymesh();
        }

        void CreateGraphPolymesh() {
 
            int polygonsCount = valuesCount - 1;
            int pointsCount = valuesCount * 2;
            Polygon[] polygons = new Polygon[polygonsCount];
 
            for (int i = 0; i<polygons.Length; i++) {
                Polygon polygon = new Polygon(0, 0, 4);
                int idx0 = i;
                int idx1 = i + valuesCount;
                int idx2 = idx1 +1;
                int idx3 = idx0 + 1;
                polygon.SetCorner(0, idx0, -1, idx0);
                polygon.SetCorner(1, idx1, -1, idx1);
                polygon.SetCorner(2, idx2, -1, idx2);
                polygon.SetCorner(3, idx3, -1, idx3);
                polygons[i] = polygon;
            }
            SerializedPolygonalMesh serializedPolygonalMesh = SerializedPolygonalMesh.GetProcedural(new Vector3[pointsCount], null, new Color[pointsCount], polygons);
            gradientFill = new PolygonalMesh(serializedPolygonalMesh );
            gradientFill.lighingMode = LightingMode.Unlit;
            gradientFill.ambient = 1;
            gradientFill.transparent = true;
        }

        public void AddValueRight(float value) {
            ValidateValuesList();
            valuesList.RemoveAt(0);
            valuesList.Add(value);
        }

        public void GetValuesInfo(ref float average, ref float minValue, ref float maxValue ) {
            minValue = float.MaxValue;
            maxValue = float.MinValue;
            average = 0;
            for (int i = 0; i<valuesList.Count; i++) {
                float v = valuesList[i];
                minValue = Mathf.Min(minValue, v);
                maxValue = Mathf.Max(maxValue, v);
                average += v;
            }
            average = average / (float)(valuesList.Count);
        }

        float timer = 0;
        float prevValue; 
        public void AddValueRightRealtime(float val) {
            float updateRate = xAxis.range / (float)(valuesCount - 1);
            float time = Time.time;
            float prevTime = time - Time.deltaTime;
            float _timer = timer;
            for (float i = timer; i<time; i+=updateRate) {
                float lv = Mathf.InverseLerp(prevTime, time, i);
                AddValueRight(Mathf.Lerp(prevValue, val, lv));
                _timer += updateRate;
            }
            timer = _timer;
            prevValue = val;
 
        }
 
    }
}
