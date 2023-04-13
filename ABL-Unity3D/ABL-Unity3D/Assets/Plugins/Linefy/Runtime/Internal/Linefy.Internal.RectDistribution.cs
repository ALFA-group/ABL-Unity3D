using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy.Internal {

    public class RectDistribution {
 
        struct Cell {

            public float size;
            public Vector2 position;
            public Vector2 center;
 
            public bool isNull;
            public Vector2 pointPos;
            public int[] adjacents;

            public void Draw(bool drawGrid, bool drawPoint) {
                if (drawGrid) {
                    Vector2 p0 = position;
                    Vector2 p1 = new Vector2(position.x, position.y + size);
                    Vector2 p2 = new Vector2(position.x + size, position.y + size);
                    Vector2 p3 = new Vector2(position.x + size, position.y);

                    Color c = new Color(1, 1, 1, 0.1f);
                    Debug.DrawLine(p0, p1, c);
                    Debug.DrawLine(p1, p2, c);
                    Debug.DrawLine(p2, p3, c);
                    Debug.DrawLine(p3, p0, c);
                }
                if (drawPoint) {
                    if (!isNull) {
                        Utility.DebugDrawPoint(pointPos, 0.1f, Color.red);
                    }  
                }

            }
        }

        int GetCellIdx(int x, int y) {
            int result = x + y * xCellsCount;
            if (result >= cells.Length) {
                result = -1;
            }
            return result;
        }
 
        Cell[] cells;
        Vector2 size;
        Vector2 halfSize;
        float cellSize;
        int xCellsCount;
        int yCellsCount;
        public Vector2[] result;
        List<int> spiral;


        public RectDistribution(Vector2 size, int itemsCount, int relaxPasses) {
            this.size = size;
            //halfSize = size / 2;
            float totalArea = size.x * size.y;
            float areaPerItem = totalArea / itemsCount;
            cellSize = Mathf.Sqrt(areaPerItem);
            float halfCellSize = cellSize / 2;
            xCellsCount = Mathf.CeilToInt(size.x / cellSize);
            yCellsCount = Mathf.CeilToInt(size.y / cellSize);

            float xGridBegin = -xCellsCount * cellSize * 0.5f;
            float yGridBegin = -yCellsCount * cellSize * 0.5f;

            cells = new Cell[xCellsCount * yCellsCount];
            for (int y = 0; y < yCellsCount; y++) {
                for (int x = 0; x < xCellsCount; x++) {
                    int arrId = GetCellIdx(x, y);
                    cells[arrId].size = cellSize;
                    cells[arrId].position = new Vector2(xGridBegin + x * cellSize, yGridBegin + y * cellSize);
                    cells[arrId].center = cells[arrId].position + new Vector2(cellSize, cellSize) / 2;
                    Vector2 rp = cells[arrId].center + Random.insideUnitCircle * halfCellSize;
                    cells[arrId].pointPos = rp;

                    int[] adjacents = new int[8];
                    adjacents[0] = GetCellIdx(x - 1, y - 1);
                    adjacents[1] = GetCellIdx(x - 1, y);
                    adjacents[2] = GetCellIdx(x - 1, y + 1);
                    adjacents[3] = GetCellIdx(x, y + 1);
                    adjacents[4] = GetCellIdx(x + 1, y + 1);
                    adjacents[5] = GetCellIdx(x + 1, y);
                    adjacents[6] = GetCellIdx(x + 1, y - 1);
                    adjacents[7] = GetCellIdx(x, y - 1);
                    cells[arrId].adjacents = adjacents;
                }
            }

            spiral = SpiralIndices();
 
            int nullItemsLength = cells.Length - itemsCount;
            int borderIndicesCount = xCellsCount * 2 + yCellsCount * 2 - 4; 

            float nullStep = borderIndicesCount /  (float)(nullItemsLength ) ;
            float nullsCounter = 0;
            for (int i = 0; i< nullItemsLength; i++) {
                int nullIdx = spiral[(int)(nullsCounter)];
                cells[nullIdx].isNull = true;
                nullsCounter += nullStep;
            }
 
            for (int i = 0; i<relaxPasses; i++) {
                Relax(1f/ relaxPasses);
            }

            result = new Vector2[itemsCount];
             
            int counter = 0;

            for (int i = spiral.Count-1; i>=0; i--) {
                int cellIdx = spiral[i];
                if (!cells[cellIdx].isNull) {
                    result[counter] = cells[cellIdx].pointPos;
                    counter++;
                    if (counter >= result.Length) {
                        break;
                    }
                }
            }
        }

        void Relax(float power) {
            for (int s = 0; s<spiral.Count; s++) {
                if (cells[s].isNull) {
                    continue;
                }
                Cell ccell = cells[s];
                for (int a = 0; a < ccell.adjacents.Length; a++) {
                    if (ccell.adjacents[a] < 0) {
                        continue;
                    }
                    Cell acell = cells[ccell.adjacents[a]];

                    if (acell.isNull) {
                        continue;
                    }

                    Vector2 dir = acell.pointPos - ccell.pointPos;
                    float magnitude = dir.magnitude;
                    dir /= magnitude;

                    if (magnitude < cellSize) {
                        float pv = (cellSize - magnitude) / 2f * power;
                        acell.pointPos += dir * pv;
                        ccell.pointPos -= dir * pv;
                        cells[ccell.adjacents[a]] = acell;
                    }
                }

                cells[s] = ccell;
            }
        }

        public void DrawDebug(bool drawGrid) {
            if (drawGrid) {
                halfSize = size / 2f;
                Vector2 p0 = new Vector2(-halfSize.x, -halfSize.y);
                Vector2 p1 = new Vector2(-halfSize.x, halfSize.y);
                Vector2 p2 = new Vector2(halfSize.x, halfSize.y);
                Vector2 p3 = new Vector2(halfSize.x, -halfSize.y);

                Debug.DrawLine(p0, p1, Color.yellow);
                Debug.DrawLine(p1, p2, Color.yellow);
                Debug.DrawLine(p2, p3, Color.yellow);
                Debug.DrawLine(p3, p0, Color.yellow);
            }

            for (int i = 0; i<cells.Length; i++) {
                cells[i].Draw(drawGrid, true);
            }
        }
  
        List<int> SpiralIndices() {
            List<int> result = new List<int>();
            int x = 0;
            int y = 0;

            int xDir = 0;
            int yDir = 1;

            int leftBound = 1;
            int rightBound = xCellsCount-1;
            int topBound = yCellsCount-1;
            int bottomBound = 0;

            for (int i = 0; i<cells.Length; i++) {
                int cellIdx = GetCellIdx(x, y);
                result.Add(cellIdx);
                x += xDir;
                y += yDir;

                if (yDir == 1 && y == topBound) {
                    xDir = 1;
                    yDir = 0;
                    topBound--;
                }
                if (xDir == 1 && x == rightBound) {
                    xDir = 0;
                    yDir = -1;
                    rightBound--;
                }
                if (yDir ==-1 && y == bottomBound) {
                    xDir = -1;
                    yDir = 0;
                    bottomBound++;
                }
                if (xDir == -1 && x == leftBound) {
                    xDir = 0;
                    yDir = 1;
                    leftBound++;
                }

            }
            return result;
        }
    }
}
