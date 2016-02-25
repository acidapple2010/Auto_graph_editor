using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DrawingEngine.Drawing3D;
using DrawingEngine.Common;
using System.Drawing;

namespace DrawingEngine.Objects
{
    class BezierPatch : SimpleObject
    {
        public Point3D[,] controlPoints { get; set; }
        private int detalizationDegree;
        Matrix M, Mt, Px, Py, Pz;

        public BezierPatch(Point3D[,] controlPoints, int detalizationDegree)
        {
            this.controlPoints = controlPoints;
            this.detalizationDegree = detalizationDegree;

            // Инициализируем матрицы функции изгиба
            // Для сплайнов Безье матрица изгиба при транспонировании дает себя же
            double[,] MArray = new double[4, 4] { {-1, 3, -3, 1}, 
                                                  {3, -6, 3, 0}, 
                                                  {-3, 3, 0, 0}, 
                                                  {1, 0, 0, 0} };
            M = new Matrix(MArray);
            Mt = new Matrix(MArray);
            Mt.Transpose();

            // Инициализируем матрицы для координат точек по координатам X, Y и Z
            double[,] PxArray = new double[4, 4] 
                    { {controlPoints[0, 0].X, controlPoints[0, 1].X, controlPoints[0, 2].X, controlPoints[0, 3].X}, 
                      {controlPoints[1, 0].X, controlPoints[1, 1].X, controlPoints[1, 2].X, controlPoints[1, 3].X}, 
                      {controlPoints[2, 0].X, controlPoints[2, 1].X, controlPoints[2, 2].X, controlPoints[2, 3].X}, 
                      {controlPoints[3, 0].X, controlPoints[3, 1].X, controlPoints[3, 2].X, controlPoints[3, 3].X} };
            Px = new Matrix(PxArray);
            // По оси Y
            double[,] PyArray = new double[4, 4] 
                    { {controlPoints[0, 0].Y, controlPoints[0, 1].Y, controlPoints[0, 2].Y, controlPoints[0, 3].Y}, 
                      {controlPoints[1, 0].Y, controlPoints[1, 1].Y, controlPoints[1, 2].Y, controlPoints[1, 3].Y}, 
                      {controlPoints[2, 0].Y, controlPoints[2, 1].Y, controlPoints[2, 2].Y, controlPoints[2, 3].Y}, 
                      {controlPoints[3, 0].Y, controlPoints[3, 1].Y, controlPoints[3, 2].Y, controlPoints[3, 3].Y} };
            Py = new Matrix(PyArray);
            // По оси Z
            double[,] PzArray = new double[4, 4] 
                    { {controlPoints[0, 0].Z, controlPoints[0, 1].Z, controlPoints[0, 2].Z, controlPoints[0, 3].Z}, 
                      {controlPoints[1, 0].Z, controlPoints[1, 1].Z, controlPoints[1, 2].Z, controlPoints[1, 3].Z}, 
                      {controlPoints[2, 0].Z, controlPoints[2, 1].Z, controlPoints[2, 2].Z, controlPoints[2, 3].Z}, 
                      {controlPoints[3, 0].Z, controlPoints[3, 1].Z, controlPoints[3, 2].Z, controlPoints[3, 3].Z} };
            Pz = new Matrix(PzArray);

            fillPoints();
            fillAxis();
            this.currentPointList = sourcePointsList.ToList();
            updateEdges();
            updateFaces();
        }

        private Point3D getBezierPatchPoint(double u, double v)
        {
            double[,] UArray = new double[4, 4] { {u * u * u, 0, 0, 0}, 
                                                  {u * u, 0, 0, 0}, 
                                                  {u, 0, 0, 0}, 
                                                  {1, 0, 0, 0} };
            double[,] VArray = new double[4, 4] { {v * v * v, v * v, v, 1}, 
                                                  {0, 0, 0, 0}, 
                                                  {0, 0, 0, 0}, 
                                                  {0, 0, 0, 0} };
            Matrix U = new Matrix(UArray);
            Matrix V = new Matrix(VArray);

            // Нужная нам точка
            Point3D ans = new Point3D();
            // Считаем ее координаты по осям по формуле ans = (U * M * P * Mt * V)
            Matrix temp = Matrix.Multiply(Matrix.Multiply(Matrix.Multiply(Matrix.Multiply(V, Mt), Px), M), U);
            ans.X = temp[0, 0];
            temp = Matrix.Multiply(Matrix.Multiply(Matrix.Multiply(Matrix.Multiply(V, Mt), Py), M), U);
            ans.Y = temp[0, 0];
            temp = Matrix.Multiply(Matrix.Multiply(Matrix.Multiply(Matrix.Multiply(V, Mt), Pz), M), U);
            ans.Z = temp[0, 0];

            return ans;
        }

        private void fillPoints()
        {
            // Формируем список точек поверхности Безье
            this.sourcePointsList.Clear();
            // Их число зависит только от степени детализации и равно квадрату степени детализации
            // А проставляются они равномерно на всем участке сплайна
            for (int i = 0; i < detalizationDegree; i++)
            {
                for (int j = 0; j < detalizationDegree; j++)
                {
                    this.sourcePointsList.Add(getBezierPatchPoint
                        ((double)i/(detalizationDegree - 1),(double)j/(detalizationDegree - 1))
                        );
                }
            }
        }

        private void fillAxis()
        {
            // Делаем вид, будто они безрамерны, чтобы нам не ебали мозги их пересечения
            this.firstPointAxisX = new Point3D(0, 0, 0);
            this.lastPointAxisX = new Point3D(0, 0, 0);

            this.firstPointAxisY = new Point3D(0, 0, 0);
            this.lastPointAxisY = new Point3D(0, 0, 0);

            this.firstPointAxisZ = new Point3D(0, 0, 0);
            this.lastPointAxisZ = new Point3D(0, 0, 0);
        }

        protected override void updateEdges()
        {
            edgesList.Clear();

            // Формируем список ребер
            for (int i = 0; i < detalizationDegree - 1; i++)
            {
                for (int j = 0; j < detalizationDegree - 1; j++)
                {
                    this.edgesList.Add(
                        new Edge(
                            currentPointList[i * detalizationDegree + j],
                            currentPointList[i * detalizationDegree + (j + 1)]
                            )
                        );
                    this.edgesList.Add(
                        new Edge(
                            currentPointList[i * detalizationDegree + j],
                            currentPointList[(i + 1) * detalizationDegree + j])
                        );
                    this.edgesList.Add(
                        new Edge(
                            currentPointList[i * detalizationDegree + j],
                            currentPointList[(i + 1) * detalizationDegree + (j + 1)]
                            )
                        );
                }
            }
            // После этого остаются непроставлены только ребра i = 3 и j = 3
            for (int t = 0; t < detalizationDegree - 1; t++)
            {
                this.edgesList.Add(
                    new Edge(
                        currentPointList[t * detalizationDegree + 3],
                        currentPointList[(t + 1) * detalizationDegree + 3]
                        )
                    );
                this.edgesList.Add(
                    new Edge(
                        currentPointList[3 * detalizationDegree + t],
                        currentPointList[3 * detalizationDegree + (t + 1)]
                        )
                    );
            }
        }

        protected override void updateFaces()
        {
            this.faceList.Clear();

            // Проставляем грани поверхности
            // Их число так же завит только от степени детализации
            for (int i = 0; i < detalizationDegree - 1; i++)
            {
                for (int j = 0; j < detalizationDegree - 1; j++)
                {
                    // Проставляем грани с обеих сторон
                    // С одной стороны
                    this.faceList.Add(
                        new Face(
                            currentPointList[i * detalizationDegree + j],
                            currentPointList[i * detalizationDegree + (j + 1)],
                            currentPointList[(i + 1) * detalizationDegree + (j + 1)]
                            )
                        );
                    this.faceList.Add(
                        new Face(
                            currentPointList[i * detalizationDegree + j],
                            currentPointList[(i + 1) * detalizationDegree + (j + 1)],
                            currentPointList[(i + 1) * detalizationDegree + j]
                            )
                        );
                    // С другой стороны
                    this.faceList.Add(
                        new Face(
                            currentPointList[i * detalizationDegree + j],
                            currentPointList[(i + 1) * detalizationDegree + (j + 1)],
                            currentPointList[i * detalizationDegree + (j + 1)]
                            )
                        );
                    this.faceList.Add(
                        new Face(
                            currentPointList[i * detalizationDegree + j],
                            currentPointList[(i + 1) * detalizationDegree + j],
                            currentPointList[(i + 1) * detalizationDegree + (j + 1)]
                            )
                        );
                }
            }
        }

        public Point3D getCenter()
        {
            Point3D result = new Point3D();

            for (int i = 0; i < detalizationDegree * detalizationDegree; i++)
            {
                result.X += currentPointList[i].X;
                result.Y += currentPointList[i].Y;
                result.Z += currentPointList[i].Z;
            }
            result.X /= detalizationDegree * detalizationDegree;
            result.Y /= detalizationDegree * detalizationDegree;
            result.Z /= detalizationDegree * detalizationDegree;

            return result;
        }


        #region Gets & Sets methods

        public override List<Face> FaceList
        {
            get
            {
                return this.faceList;
            }
            set
            {
                this.faceList = value;
            }
        }

        // TODO: Deprecated
        public override List<Point3D> PointList
        {
            get
            {
                return this.currentPointList;
            }
            set
            {
                this.currentPointList = value;
            }
        }

        public override List<Edge> EdgesList
        {
            get
            {
                return this.edgesList;
            }
            set
            {
                this.edgesList = value;
            }
        }

        #endregion
    }
}
