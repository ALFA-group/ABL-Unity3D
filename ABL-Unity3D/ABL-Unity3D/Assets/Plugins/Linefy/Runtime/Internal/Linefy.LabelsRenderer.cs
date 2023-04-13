using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;
using System;
using Linefy.Internal;

namespace Linefy{

    /// <summary>
    ///  See <see href="https://polyflow.xyz/content/linefy/documentation-1-1/linefy-documentation.html#LabelsRenderer"/> 
    /// </summary>
    public class LabelsRenderer : Drawable {
        DFlag d_background = new DFlag("d_background", true);
        DFlag d_anyText = new DFlag("d_anyText", true);
        DFlag d_anyPixelOffset = new DFlag("d_anyPixelOffset", true);
        DFlag d_anyColors = new DFlag("d_anyColors", true);
        DFlag d_anyPositions = new DFlag("d_anyPositions", true);
        DIntValue  atlasInstanceID;
        DIntValue fontSettingsHash;
        public int itemsModificationId = -1;
        public int propertiesModificationId = -1;

        public UnityEngine.Rendering.CompareFunction zTest {
            get {
                return dots.zTest;
            }

            set {
                dots.zTest = value;
            }
        }

        /// <summary>
        /// Render queue
        /// </summary>
        public int renderOrder {
            get {
                return dots.renderOrder;
            }

            set {
                  dots.renderOrder = value;
            }
        }
 
        TextAlignment _horizontalAlignment = TextAlignment.Center;
        /// <summary>
        /// The positioning of the text reliative its center
        /// </summary>
        public TextAlignment horizontalAlignment {
            get {
                return _horizontalAlignment;
            }

            set {
                if (_horizontalAlignment != value) {
                    _horizontalAlignment = value;
                    d_anyPixelOffset.Set();
                }
            }
        }


        float _sizeMultiplier = 1;
        /// <summary>
        /// multiplier of all size properties 
        /// </summary>
        public float size {
            get {
                return _sizeMultiplier;
            }

            set {
                if (_sizeMultiplier != value) {
                    d_anyPixelOffset.Set();
                    _sizeMultiplier = value;
                }
            }
        }
 
        bool _drawBackground;
        /// <summary>
        /// Displays background rect under text. Background is calculated using 9-grid slice technique. The indices of background rects defines in DotAtlas settings.
        /// </summary>
        public bool drawBackground {
            get {
                return _drawBackground;
            }

            set {
                if (_drawBackground != value) {
                    _drawBackground = value;
                    d_anyText.Set();
                    d_background.Set();
                }
            }
        }

        Color _textColor = Color.white;
        public Color textColor {
            get {
                return _textColor;
            }

            set {
                if (_textColor != value) {
                    _textColor = value;
                    d_anyColors.Set();
                }
            }
        }

        Color _backgroundColor = Color.gray;
        public Color backgroundColor {
            get {
                return _backgroundColor;
            }

            set {
                if (_backgroundColor != value) {
                    _backgroundColor = value;
                    d_anyColors.Set();
                }
            }
        }

        WidthMode _widthMode;
        public WidthMode widthMode {
            get {
                return _widthMode;
            }

            set {
                if (value != _widthMode) {
                    _widthMode = value;
                    if (_widthMode == WidthMode.WorldspaceBillboard) {
                        dots.widthMode = WidthMode.PixelsBillboard;
                        return;
                    }
 
                    dots.widthMode = _widthMode;
                }
            }
        }

        Vector2 _backgroundExtraSize = new Vector2(0,0);
        /// <summary>
        /// The size to be added to the automatically calculated background size.  
        /// </summary>
        public Vector2 backgroundExtraSize { 
            get {
                return _backgroundExtraSize;
            }

            set {
                if (_backgroundExtraSize != value) {
                    _backgroundExtraSize = value;
                    d_anyPixelOffset.Set();
                }
            }
        }

        public bool transparent {
            get {
                return dots.transparent;
            }

            set {
                dots.transparent = value;
            }
        }

        /// <summary>
        /// Snaps the position of glyphs to the nearest pixels on the screen so that guarantee exact pixel dimensions of glyphs.
        /// </summary>
        public bool pixelPerfect {
            get {
                return dots.pixelPerfect;
            }

            set {
                dots.pixelPerfect = value;
            }
        }

        public float fadeAlphaDistanceFrom {
            get {
                return dots.fadeAlphaDistanceFrom;
            }

            set {
                  dots.fadeAlphaDistanceFrom = value;
            }
        }

        public float fadeAlphaDistanceTo {
            get {
                return dots.fadeAlphaDistanceTo;
            }

            set {
                dots.fadeAlphaDistanceTo = value;
            }
        }

        class label {

            public struct GlyphInfo {
                public int rectIdx;
                public int dotidx;
                public Vector2 pixelOffset;
                public bool isWhitespace;

                public GlyphInfo(int rectIdx, bool isWhitespace) {
                    this.rectIdx = rectIdx;
                    this.dotidx = 0;
                    this.pixelOffset = new Vector2();
                    this.isWhitespace = isWhitespace;
                }
            }

            public GlyphInfo[] glyphInfos;
            public float linePixelWidth = 0;

            LabelsRenderer root;
            public DFlag d_text = new DFlag("d_text", true);
            public DFlag d_positions = new DFlag("d_positions", true);
            public DFlag d_pixelOffset = new DFlag("d_pixelOffset", true);

            string _text = string.Empty;
            public string text {
                get {
                    return _text;
                }

                set {
                    if (value != _text) {
                        d_text.Set();
                        root.d_anyText.Set();
                        _text = value;
                        //parceText();
                    }
                }
            }

            Vector3 _position;
            public Vector3 position {
                get {
                    return _position;
                }

                set {
                    if (value != _position) {
                        root.d_anyPositions.Set();
                        d_positions.Set();
                        _position = value;
                    }
                }
            }

            Vector2 _offset;
            public Vector2 offset { 
                get {
                    return _offset;
                }
            
                set {
                    if (value != _offset) {
                        _offset = value;
                        d_pixelOffset.Set();
                        root.d_anyPixelOffset.Set();
                    }

                }
            }

            public void ParceText() {
                if (root._drawBackground) {
                    if (_text.Length == 0) {
                        System.Array.Resize(ref glyphInfos, 0);
                        return;
                    }
 
                    System.Array.Resize(ref glyphInfos, _text.Length+9);
                    glyphInfos[0] = new GlyphInfo(root.atlas.background9SliseIndices.topLeft, false);
                    glyphInfos[1] = new GlyphInfo(root.atlas.background9SliseIndices.top, false);
                    glyphInfos[2] = new GlyphInfo(root.atlas.background9SliseIndices.topRight, false);
                    glyphInfos[3] = new GlyphInfo(root.atlas.background9SliseIndices.left, false);
                    glyphInfos[4] = new GlyphInfo(root.atlas.background9SliseIndices.center, false);
                    glyphInfos[5] = new GlyphInfo(root.atlas.background9SliseIndices.right, false);
                    glyphInfos[6] = new GlyphInfo(root.atlas.background9SliseIndices.bottomLeft, false);
                    glyphInfos[7] = new GlyphInfo(root.atlas.background9SliseIndices.bottom, false);
                    glyphInfos[8] = new GlyphInfo(root.atlas.background9SliseIndices.bottomRight, false);
 
                    for (int i = 0; i < _text.Length; i++) {
                        int utfPoint = char.ConvertToUtf32(_text, i);
                        glyphInfos[i + 9] = (new GlyphInfo(root.atlas.GetRectByUtf32(utfPoint), utfPoint == 32));
                    }
                 } else {
                    System.Array.Resize(ref glyphInfos, _text.Length);
                    for (int i = 0; i < _text.Length; i++) {
                        int utfPoint = char.ConvertToUtf32(_text, i);
                        glyphInfos[i] = (new GlyphInfo(root.atlas.GetRectByUtf32(utfPoint), utfPoint == 32));
                    }
                }
            }

            public label(LabelsRenderer root, Vector3 position, string text) {
                this.root = root;
                this.position = position;
                this.text = text;

            }

            public void UpdatePixelOffset() {
                if (_text.Length == 0) {
                    return;
                }
 
                float _sizeMultiplier =  root._sizeMultiplier;
                float horizontalSpacing = root.atlas.horizontalSpacing * _sizeMultiplier;
                float whitespaceWidth = root.atlas.whitespaceWidth * _sizeMultiplier;
                Vector2 scaledPixelSize = (Vector2)root.atlas.rectPixelSize * _sizeMultiplier;
                Vector2 pixelPosAccum = new Vector2(0, 0);
 
                if (glyphInfos.Length > 0) {
                    pixelPosAccum.x += scaledPixelSize.x / 2;
                }

                int charsStartIdx = root._drawBackground ? 9 : 0;

                if (root.atlas.monowidth) {
                    for (int i = charsStartIdx; i < glyphInfos.Length; i++) {
                        if (glyphInfos[i].isWhitespace) {
                            pixelPosAccum.x += whitespaceWidth;
                            continue;
                        }
                        glyphInfos[i].pixelOffset = new Vector2(pixelPosAccum.x , pixelPosAccum.y);
                        pixelPosAccum.x += horizontalSpacing;
                    }
                    linePixelWidth = pixelPosAccum.x;
                } else {
                    for (int i = charsStartIdx; i < glyphInfos.Length; i++) {
                        if (glyphInfos[i].isWhitespace) {
                            pixelPosAccum.x += whitespaceWidth;
                            continue;
                        }
                        int rectIdx = glyphInfos[i].rectIdx;
                        DotsAtlas.Rect atlasRect = root.atlas.rects[rectIdx % root.atlas.rects.Length];
                        glyphInfos[i].pixelOffset = new Vector2(pixelPosAccum.x - atlasRect.bounds.xMin * _sizeMultiplier, pixelPosAccum.y);
                        float charWidth = atlasRect.bounds.width * _sizeMultiplier;
                        pixelPosAccum.x += charWidth;
                        pixelPosAccum.x += horizontalSpacing;
                    }
                    linePixelWidth = pixelPosAccum.x - horizontalSpacing - scaledPixelSize.x / 2;
                }

      
                float halfLineWidth =  (linePixelWidth / 2f);

                if (root._horizontalAlignment == TextAlignment.Center) {
                    float lineOffset = halfLineWidth;
                    for (int i = charsStartIdx; i < glyphInfos.Length; i++) {
                        glyphInfos[i].pixelOffset.x -= lineOffset;
                    }
                } else if (root._horizontalAlignment == TextAlignment.Right) {
                    float lineOffset = linePixelWidth;
                    for (int i = charsStartIdx; i < glyphInfos.Length; i++) {
                        glyphInfos[i].pixelOffset.x -= lineOffset;
                    }
                }

                for (int i = charsStartIdx; i < glyphInfos.Length; i++) {
                    int dotIdx = glyphInfos[i].dotidx;
                    root.dots.SetPixelOffset(glyphInfos[i].dotidx, glyphInfos[i].pixelOffset+offset);
                    root.dots.SetSize(dotIdx, scaledPixelSize);
                }

                if (root._drawBackground) {
                    Vector2 centerRectSize = new Vector2(linePixelWidth, scaledPixelSize.y) + root._backgroundExtraSize * _sizeMultiplier ;
                    float x0 = - (centerRectSize.x/2 + scaledPixelSize.x/2 );
                    float x1 = 0;
                    float x2 = -x0 ;
                    float y1 = 0;
                    float y0 = - centerRectSize.y/2 - scaledPixelSize.y/2;
                    float y2 = -y0;

                    if (root._horizontalAlignment == TextAlignment.Left) {
                        x0 += halfLineWidth;
                        x1 += halfLineWidth;
                        x2 += halfLineWidth;
                    } else if (root._horizontalAlignment == TextAlignment.Right) {
                        x0 -= halfLineWidth;
                        x1 -= halfLineWidth;
                        x2 -= halfLineWidth;
                    }

                    //0  
                    root.dots.SetPixelOffset(glyphInfos[0].dotidx, new Vector2(x0,y0)+offset);
                    root.dots.SetSize(glyphInfos[0].dotidx, scaledPixelSize);

                    //1 
                    root.dots.SetPixelOffset(glyphInfos[1].dotidx, new Vector2(x1, y0) + offset);
                    root.dots.SetSize(glyphInfos[1].dotidx, new Vector2(centerRectSize.x, scaledPixelSize.y));

                    //2
                    root.dots.SetPixelOffset(glyphInfos[2].dotidx, new Vector2(x2, y0) + offset);
                    root.dots.SetSize(glyphInfos[2].dotidx, scaledPixelSize);

                    //3   
                    root.dots.SetPixelOffset(glyphInfos[3].dotidx, new Vector2(x0, y1) + offset);
                    root.dots.SetSize(glyphInfos[3].dotidx, new Vector2(scaledPixelSize.x, centerRectSize.y));

                    //4    center                
                    root.dots.SetPixelOffset(glyphInfos[4].dotidx, new Vector2(x1, y1) + offset);
                    root.dots.SetSize(glyphInfos[4].dotidx, centerRectSize);

                    //5  
                    root.dots.SetPixelOffset(glyphInfos[5].dotidx, new Vector2(x2, y1) + offset);
                    root.dots.SetSize(glyphInfos[5].dotidx, new Vector2(scaledPixelSize.x, centerRectSize.y));

                    //6 
                    root.dots.SetPixelOffset(glyphInfos[6].dotidx, new Vector2(x0, y2) + offset);
                    root.dots.SetSize(glyphInfos[6].dotidx, scaledPixelSize);

                    //7  
                    root.dots.SetPixelOffset(glyphInfos[7].dotidx, new Vector2(x1, y2) + offset);
                    root.dots.SetSize(glyphInfos[7].dotidx, new Vector2(centerRectSize.x, scaledPixelSize.y));

                    //8  
                    root.dots.SetPixelOffset(glyphInfos[8].dotidx, new Vector2(x2, y2) + offset);
                    root.dots.SetSize(glyphInfos[8].dotidx, scaledPixelSize);
 
                }
            }

            public void UpdateRectIndices() {
                for (int i = 0; i < glyphInfos.Length; i++) {
                    GlyphInfo info = glyphInfos[i];
                    root.dots.SetRectIndex(info.dotidx, info.rectIdx);
                    root.dots.SetEnabled(info.dotidx, info.isWhitespace==false);
                }
            }

            public void UpdateColors(Color textColor ) {
                for (int i = 0; i < glyphInfos.Length; i++) {
                    GlyphInfo info = glyphInfos[i];
                    root.dots.SetColor(info.dotidx, textColor);
                }
            }

            public void UpdateColors(Color textColor, Color backgroundColor) {
                if (_text.Length == 0) {
                    return;
                }
                root.dots.SetColor(glyphInfos[0].dotidx, backgroundColor);
                root.dots.SetColor(glyphInfos[1].dotidx, backgroundColor);
                root.dots.SetColor(glyphInfos[2].dotidx, backgroundColor);
                root.dots.SetColor(glyphInfos[3].dotidx, backgroundColor);
                root.dots.SetColor(glyphInfos[4].dotidx, backgroundColor);
                root.dots.SetColor(glyphInfos[5].dotidx, backgroundColor);
                root.dots.SetColor(glyphInfos[6].dotidx, backgroundColor);
                root.dots.SetColor(glyphInfos[7].dotidx, backgroundColor);
                root.dots.SetColor(glyphInfos[8].dotidx, backgroundColor);

                for (int i = 9; i < glyphInfos.Length; i++) {
                    GlyphInfo info = glyphInfos[i];
                    root.dots.SetColor(info.dotidx, textColor);
                }
            }

            public void UpdatePositions() {
                if (d_positions) {
                     for (int i = 0; i < glyphInfos.Length; i++) {
                         GlyphInfo info = glyphInfos[i];
                         root.dots.SetPosition(info.dotidx, _position);
                     }
                    d_positions.Reset();
                }
            }
        }

        /// <summary>
        /// Font atlas.
        /// </summary>
        public DotsAtlas atlas {
            get {
                return dots.atlas;
            }

            set {
                if (value == null) {
                    value = DotsAtlas.DefaultFont11px;
                }
                dots.atlas = value;
            }
        }
 
        Dots dots;

        label[] labels = new label[0];

        public LabelsRenderer( int labelsCount ) {
            InternalCtor(null, labelsCount);
        }

        public LabelsRenderer(DotsAtlas atlas, int labelsCount) {
            InternalCtor(atlas, labelsCount);
        }

        void InternalCtor(DotsAtlas atlas, int labelsCount) {
            dots = new Dots(16);
            dots.capacityChangeStep = 8;
            dots.widthMode = WidthMode.PixelsBillboard;
            dots.widthMultiplier = 1;
            dots.colorMultiplier = Color.white;
            dots.pixelPerfect = false;
            this.atlas = atlas;
            fontSettingsHash = new DIntValue(-1, d_anyText);
            atlasInstanceID = new DIntValue(-1, d_anyPixelOffset);
            this.count = labelsCount;
        }

        /// <summary>
        /// Labels count
        /// </summary>
        public int count {
            get {
                return labels == null ? 0 : labels.Length;
            }

            set {
                value = Mathf.Max(0, value);
                int prevCount = labels == null ? 0 : labels.Length;
                if (prevCount != value) {
                    System.Array.Resize(ref labels, value);
                    for (int i = prevCount; i < value; i++) {
                        labels[i] = new label(this, Vector3.zero, string.Empty);
                    }
                    d_anyText.Set();
                }
            }
        }

        public Label this [int idx] {
        
            get {
                if (validateLabelIdx(idx)) {
                    label l = labels[idx];
                    return new Label(l.text, l.position, l.offset);
                } else {
                    return new Label();
                }
            }

            set {
                if (validateLabelIdx(idx)) {
                    label l = labels[idx];
                    l.position = value.position;
                    l.text = value.text;
                    l.offset = value.offset;
                }
            }
        }

        public void SetPosition(int idx, Vector3 position) {
            if (validateLabelIdx(idx)) {
                labels[idx].position = position;
            }
        }

        public void SetText(int idx, string text) {
            if (validateLabelIdx(idx)) {
                labels[idx].text = text;
            }
        }

        public void SetOffset(int idx, Vector2 offset) {
            if (validateLabelIdx(idx)) {
                labels[idx].offset = offset;
            }
        }

        bool validateLabelIdx(int idx) {
            bool result = true;
#if UNITY_EDITOR
            if (labels.Length == 0 || idx < 0 || idx >= labels.Length) {
                result = false;
                Debug.LogWarningFormat("labelIdx {0} is out of range. Labels count {1}", idx, labels.Length);
            }
#endif
            return result;

        }

        public override void DrawNow(Matrix4x4 matrix) {
            PreDraw();
            dots.DrawNow(matrix);
        }

        public override void Draw(Matrix4x4 matrix, Camera cam, int layer) {
            PreDraw();
            dots.Draw(matrix, cam, layer);
        }

        void PreDraw() {
         
            fontSettingsHash.SetValue(atlas.fontSettingsHash);
            atlasInstanceID.SetValue(atlas.GetInstanceID());
 
            if (d_background) {
                foreach (label label in labels) {
                    label.d_text.Set();
                }
                d_background.Reset();
            }

            if (d_anyText) {
                d_anyColors.Set();
                d_anyPixelOffset.Set();
                d_anyPositions.Set();

                int totalCharctersCounter = 0;
                foreach (label label in labels) {
                    label.ParceText();
                    label.d_positions.Set();
                    for (int c = 0; c < label.glyphInfos.Length; c++) {
                        label.glyphInfos[c].dotidx = totalCharctersCounter;
                        totalCharctersCounter++;
                    }
                }

                dots.count = totalCharctersCounter;
                foreach (label label in labels) {
                    label.UpdateRectIndices();
                    label.d_text.Reset();
                }

                d_anyText.Reset();
            }

            if (d_anyPositions) {
                foreach (label label in labels) {
                    label.UpdatePositions();
                }
                d_anyPositions.Reset();
            }

            if (d_anyPixelOffset) {
                foreach (label label in labels) {
                    label.UpdatePixelOffset();
                }
                d_anyPixelOffset.Reset();
            }

            if (d_anyColors) {
                if (_drawBackground) {
                    foreach (label label in labels) {
                        label.UpdateColors(_textColor, _backgroundColor);
                    }
                } else {
                    foreach (label label in labels) {
                        label.UpdateColors(_textColor);
                    }
                }

                d_anyColors.Reset();
            }
         }

        public override void Dispose() {
            if (dots != null) {
                dots.Dispose();
            }
        }

        public override void GetStatistic(ref int linesCount, ref int totallinesCount, ref int dotsCount, ref int totalDotsCount, ref int polylinesCount, ref int totalPolylineVerticesCount) {
            if (dots != null) {
                dotsCount += 1;
                totalDotsCount += dots.count;
            }
        }

    }
}

