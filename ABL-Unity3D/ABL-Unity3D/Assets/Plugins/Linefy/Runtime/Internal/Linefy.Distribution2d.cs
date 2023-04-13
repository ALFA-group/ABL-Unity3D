using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy {

    public class Distribution2d  {

        class Cell {
            public int x;
            public int y;

            public float xFrom;
            public float xTo;
            public float yFrom;
            public float yTo;
            public List<Sample> samples = new List<Sample>();

            public Cell(int x, int y, float xFrom, float xTo, float yFrom, float yTo) {
                this.x = x;
                this.y = y;
                this.xFrom = xFrom;
                this.xTo = xTo;
                this.yFrom = yFrom;
                this.yTo = yTo;
            }

            public override string ToString() {
                return string.Format("x:{0},{1} y:{2},{3}", xFrom, xTo, yFrom, yTo);
            }

            public Vector2 GetRandomPos() {
                float x = Mathf.Lerp(xFrom, xTo, Random.value);
                float y = Mathf.Lerp(yFrom, yTo, Random.value);
                return new Vector2(x, y);
            }
        }

        class Sample {
            public int index;
            public Cell cell;
            public Vector2 position;
            public float distance;

            public Sample(int index) {
                this.index = index;
            }
        }

        int _samplesCount;
        public int samplesCount { 
            get {
                return _samplesCount;
            }
        }

        Vector2 _size;
        public Vector2 size {
            get {
                return _size;
            }
        }

        int cellsXCount;
        int cellsYCount;
        Vector2 cellSizeStep;
        float area;

        Sample[] samples;

        int _quality;
        public int quality { 
            get {
                return _quality;
            }
        }

        Cell[] cells;
        Cell[] shuffledCells;
        float cellSize;

        public Distribution2d(int samplesCount, Vector2 size, int quality) {
            this._quality = quality;
            this._samplesCount = Mathf.Max(samplesCount, 1);
            this._size = new Vector2( Mathf.Max( size.x, 0), Mathf.Max(size.y, 0));
            area = _size.x * _size.y;
            float cellArea = area / samplesCount;
            cellSize = Mathf.Sqrt(cellArea);
            cellsXCount = Mathf.CeilToInt(_size.x / cellSize);
            cellsYCount = Mathf.CeilToInt(_size.y / cellSize);
 
            cellSizeStep = new Vector2(_size.x / cellsXCount, _size.y / cellsYCount);

            cells = new Cell[cellsXCount * cellsYCount];
            shuffledCells = new Cell[cells.Length];
    

            for (int y = 0; y<cellsYCount; y++) {
                for (int x = 0; x < cellsXCount; x++) {
                    float xFrom = x * cellSizeStep.x;
                    float xTo = xFrom + cellSizeStep.x;
                    float yFrom = y * cellSizeStep.y;
                    float yTo = yFrom + cellSizeStep.y;
                    this[x, y] = new Cell(x, y, xFrom, xTo, yFrom, yTo);
                }
            }

            cells.CopyTo(shuffledCells, 0);
 
            for (int i = 0; i<shuffledCells.Length/2; i++ ) {
                int idxA = Random.Range(0, shuffledCells.Length) ;
                int idxB = Random.Range(0, shuffledCells.Length);
                Cell ca = shuffledCells[idxA];
                Cell cb = shuffledCells[idxB];
                shuffledCells[idxA] = cb;
                shuffledCells[idxB] = ca;
            }

            samples = new Sample[samplesCount];
 
            for (int i = 0; i<samples.Length; i++) {
                samples[i] = new Sample(i);
                samples[i].position = shuffledCells[i].GetRandomPos();
            }

            FillSamplesAndCells();
 
            for (int i = 0; i<_quality; i++) {
                Relax();
            }
        }

        Cell this[int x, int y] {
            get {
                if (x < 0 || x>=cellsXCount) {
                    return null;
                }
                if (y < 0 || y >= cellsYCount) {
                    return null;
                }

                int idx = x + y * cellsXCount;
                if (idx >= cells.Length || idx<0) {
                    Debug.LogErrorFormat("out of range idx:{0} x:{1} y:{2} cells.Length:{3}", idx, x, y, cells.Length);
                    return null;
                }

                return cells[ idx];
            }

            set {
                if (x < 0 || x >= cellsXCount) {
                    return;
                }
                if (y < 0 || y >= cellsYCount) {
                    return;
                }
                int idx = x + y * cellsXCount;
                if (idx >= cells.Length || idx < 0) {
                    Debug.LogErrorFormat("out of range idx:{0} x:{1} y:{2} cells.Length:{3}", idx, x, y, cells.Length);
                    return;
                }
                cells[idx] = value;
            }
        }

        Cell[] adjacentsCells = new Cell[9];

        void FillAdjacentCell(Cell cell) {
            int adressX =  cell.x;
            int adressY =  cell.y;
            adjacentsCells[0] = this[adressX - 1, adressY - 1];
            adjacentsCells[1] = this[adressX - 1, adressY];
            adjacentsCells[2] = this[adressX - 1, adressY + 1];
            adjacentsCells[3] = this[adressX, adressY + 1];
            adjacentsCells[4] = this[adressX + 1, adressY + 1];
            adjacentsCells[5] = this[adressX + 1, adressY];
            adjacentsCells[6] = this[adressX + 1, adressY - 1];
            adjacentsCells[7] = this[adressX, adressY - 1];
            adjacentsCells[8] = cell;
        }

        void Relax() {
            for (int i = 0; i<samples.Length; i++) {
                Sample isample = samples[i];
                FillAdjacentCell(isample.cell);
                foreach (Cell a in adjacentsCells) {
                    if (a != null) {
                        foreach (Sample _as in a.samples) {
                           SpreadTwoSample(isample, _as, cellSize);
                        }
                    }
                }
            }

            FillSamplesAndCells();
        }

        void SpreadTwoSample( Sample a, Sample b, float minDist ) {
            float dist = Vector2.Distance(a.position, b.position);
            Vector2 dir = (b.position - a.position);
            dist = dir.magnitude;
            if (dist < minDist) {
                 dir = (b.position - a.position).normalized;
                float spreadValue = (minDist - dist) / 2;
                a.position = a.position - dir * spreadValue;
                b.position  = b.position + dir * spreadValue;
            }
        }

        void FillSamplesAndCells() {
            for (int i = 0; i < cells.Length; i++) {
                cells[i].samples.Clear();
            }
            for (int i = 0; i < samples.Length; i++) {
                Vector2 pos = samples[i].position;
                int cellX = Mathf.FloorToInt(pos.x / cellSizeStep.x);
                cellX = Mathf.Clamp(cellX, 0, cellsXCount - 1);
                int cellY = Mathf.FloorToInt(pos.y / cellSizeStep.y);
                cellY = Mathf.Clamp(cellY, 0, cellsYCount - 1);
                Cell c = this[cellX, cellY];
                if (c == null) {
                    Debug.LogFormat("cell for sample {0} == null");
                }

                Sample s = samples[i];
                c.samples.Add(s);
                s.cell = c;
            }
        }

        List<Sample> adjacentSamples = new List<Sample>();

        public void GetAdjacentSamples(List<int> result,  int sampleIndex) {
            result.Clear();
            adjacentSamples.Clear();
            Sample sample = samples[sampleIndex];
            FillAdjacentCell(sample.cell);
            foreach (Cell c in adjacentsCells) {
                if (c != null) {
                    foreach (Sample s in c.samples) {
                        if (s != sample) {
                            s.distance = Vector2.Distance(s.position, sample.position);
                            adjacentSamples.Add(s);
                        }
                    }
                }
            }

            adjacentSamples.Sort(distanceSorter);
            foreach (Sample s in adjacentSamples ) {
                result.Add(s.index);
            }
 
        }

        int distanceSorter(Sample a, Sample b) {
            return (int)Mathf.Sign(a.distance - b.distance);
        }

       Lines cellsWireframe;
       Dots samplesDots;

       public Vector2 this [int sampleIndex] { 
            get {
                if (sampleIndex < 0 || sampleIndex >= _samplesCount) {
                    Debug.LogWarningFormat("out of range sampleIndex {0} samples count:{1}", sampleIndex,_samplesCount);
                }
                return samples[sampleIndex].position;
            }
       }

       public void DrawDebug(Matrix4x4 matrix, float wireTransparency, float dotSampleSize) {
            if (cellsWireframe == null) {
                cellsWireframe = new Lines(cellsXCount + 1 + cellsYCount + 1, true, 1, 2, Color.white);
                int linesCounter = 0;
                for (int x = 0; x <= cellsXCount; x++) {
                    Vector2 a = new Vector2(x * cellSizeStep.x, 0);
                    Vector2 b = new Vector2(a.x, _size.y);
                    cellsWireframe.SetPosition(linesCounter, a, b);
                    linesCounter++;
                }

                for (int y = 0; y <= cellsYCount; y++) {
                    Vector2 a = new Vector2(0, y * cellSizeStep.y);
                    Vector2 b = new Vector2(_size.x, a.y);
                    cellsWireframe.SetPosition(linesCounter, a, b);
                    linesCounter++;
                }
            }
            if (samplesDots == null) {
                samplesDots = new Dots(samplesCount, true);
                for (int i = 0; i<samplesCount; i++) {
                    samplesDots[i] = new Dot(samples[i].position, 8, 0, Color.red);
                }
            }
            cellsWireframe.colorMultiplier = new Color(1,1,1, wireTransparency);

            cellsWireframe.Draw(matrix);
            samplesDots.widthMultiplier = dotSampleSize;
            samplesDots.Draw(matrix);
        }

    }
}
