using System;
using System.Drawing;
using System.Windows.Forms;
using DrawingEngine.Drawing2D;
using DrawingEngine.Objects;
using DrawingEngine.Drawing3D;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using DrawingEngine.Common;

namespace DrawingEngine
{
    enum ButtonPressed { Zp, Xm, Zm, Xp, Yp, Ym, UP, DOWN, LEFT, RIGHT, NONE }

    public partial class BezierRedactor : Form
    {

        // Движок рендеринга
        Render engine;

        // Список объектов
        List<List<SimpleObject>> bezierPatches = new List<List<SimpleObject>>();

        // Необходимо перерисовать picturebox
        bool needToReDraw = false;

        // Панорамирование
        bool panorama = false;

        // Строка, в которой записаны ошибки
        string errors = "";

        // Зажатая в настоящий момент кнопка
        ButtonPressed currentButton;


        ErrorProvider provider = new ErrorProvider();

        public BezierRedactor()
        {
            InitializeComponent();

            cbObjects.Items.Add("Все объекты");
            cbObjects.SelectedIndex = 0;

            // Создали движок
            engine = new Render(picture.Height, picture.Width);

            // Задаем движку отображение граней, и вид с перспективой
            engine.SetProecType(Render.ProecType.Parallel);
            engine.SetViewType(Render.ViewType.Edges);

            // Заставляем обновиться камере
            /*btnSetCamera_Click(null, null);*/
            lblErrors.Text = errors;

            currentButton = ButtonPressed.NONE; 

            UpdateImg();
        }

        private void FileMenuExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Желаете выйти?", "Выход", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void redrawTimer_Tick(object sender, EventArgs e)
        {
            if (needToReDraw)
            {
                DateTime beforeTime = DateTime.Now; 
                
                // Перемещаем или поворачиваем камеру, в зависимости от того, какая кнопка нажата
                switch (currentButton)
                {
                    case ButtonPressed.Zp:
                        {
                            engine.moveCamera(0, 0, 15);
                        }
                        break;
                    case ButtonPressed.Zm:
                        {
                            engine.moveCamera(0, 0, -15);
                        }
                        break;
                    case ButtonPressed.Xm:
                        {
                            engine.moveCamera(-15, 0, 0);
                        }
                        break;
                    case ButtonPressed.Xp:
                        {
                            engine.moveCamera(15, 0, 0);
                        }
                        break;
                    case ButtonPressed.Ym:
                        {
                            engine.moveCamera(0, -15, 0);
                        }
                        break;
                    case ButtonPressed.Yp:
                        {
                            engine.moveCamera(0, 15, 0);
                        }
                        break;
                    case ButtonPressed.UP:
                        {
                            engine.rotateCamera(-0.010, 0, 0);
                        }
                        break;
                    case ButtonPressed.DOWN:
                        {
                            engine.rotateCamera(0.010, 0, 0);
                        }
                        break;
                    case ButtonPressed.LEFT:
                        {
                            engine.rotateCamera(0, -0.010, 0);
                        }
                        break;
                    case ButtonPressed.RIGHT:
                        {
                            engine.rotateCamera(0, 0.010, 0);
                        }
                        break;
                }

                this.picture.Image = new Bitmap(this.picture.Width, this.picture.Height);
                engine.SetGraphic(Graphics.FromImage(this.picture.Image));
                engine.ReDraw();

                // Очищаем текст сообщения об ошибке
                lblErrors.Text = "";
                needToReDraw = false;

                DateTime afterTime = DateTime.Now;

                TimeSpan diff = afterTime - beforeTime;

                if (diff.TotalMilliseconds > 40)
                {
                    redrawTimer.Interval = Math.Max(15 - (int)diff.TotalMilliseconds, 1);
                }

                if (currentButton != ButtonPressed.NONE)
                {
                    needToReDraw = true;
                }
            }
        }

        private void UpdateImg()
        {
            // Удаляем текущие выбранные объекты
            engine.Clear();

            // Обновляем список объектов в движке, добавляем туда тупо все объекты, что есть
            foreach (List<SimpleObject> patch in bezierPatches)
            {
                engine.addObjects(patch);
            }

            needToReDraw = true;
        }

        private void ParallelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            engine.SetProecType(Render.ProecType.Parallel);
            needToReDraw = true;
        }

        private void CentralToolStripMenuItem_Click(object sender, EventArgs e)
        {
            engine.SetProecType(Render.ProecType.Central);
            needToReDraw = true;
        }

        private void edgesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            engine.SetViewType(Render.ViewType.Edges);
            needToReDraw = true;
        }

        private void facesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            engine.SetViewType(Render.ViewType.Faces);
            needToReDraw = true;
        }

        private void txtMoveX_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Double.Parse(txtMoveX.Text);
                provider.SetError(txtMoveX, String.Empty);
            }
            catch
            {
                provider.SetError(txtMoveX, "Неверное значение величины переноса  по X!");
                e.Cancel = true;
            }
        }

        private void txtMoveY_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Double.Parse(txtMoveY.Text);
                provider.SetError(txtMoveY, String.Empty);
            }
            catch
            {
                provider.SetError(txtMoveY, "Неверное значение величины переноса  по Y!");
                e.Cancel = true;
            }
        }

        private void txtMoveZ_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Double.Parse(txtMoveZ.Text);
                provider.SetError(txtMoveZ, String.Empty);
            }
            catch
            {
                provider.SetError(txtMoveZ, "Неверное значение величины переноса  по Z!");
                e.Cancel = true;
            }
        }

        // Счетчик объектов. Необходим для именования.
        private int objectCounter = 1;
        private int tempObjectNumber = -1;

        private Point3D[,] getBezierPatchPoinsFrom9Points(Point3D[] ninePoints)
        {
            Point3D[,] result = new Point3D[4, 4];
            Point3D bossPoint = ninePoints[8];

            result[0, 0] = ninePoints[0];
            result[0, 1] = ninePoints[1];
            result[0, 2] = ninePoints[2];
            result[0, 3] = ninePoints[3];

            result[3, 0] = ninePoints[4];
            result[3, 1] = ninePoints[5];
            result[3, 2] = ninePoints[6];
            result[3, 3] = ninePoints[7];

            Point3D temp;

            for (int i = 0; i < 4; i++)
            {
                temp = new Point3D();
                temp.X = (bossPoint.X + ninePoints[i].X) / 2;
                temp.Y = (bossPoint.Y + ninePoints[i].Y) / 2;
                temp.Z = (bossPoint.Z + ninePoints[i].Z) / 2;

                result[1, i] = temp;
            }
            for (int i = 0; i < 4; i++)
            {
                temp = new Point3D();
                temp.X = (bossPoint.X + ninePoints[4 + i].X) / 2;
                temp.Y = (bossPoint.Y + ninePoints[4 + i].Y) / 2;
                temp.Z = (bossPoint.Z + ninePoints[4 + i].Z) / 2;

                result[2, i] = temp;
            }
            
            return result;
        }

        private void btnAddNewObject_Click(object sender, EventArgs e)
        {
            Point3D[,] patchControlPoints;
            Point3D[] ninePoints = new Point3D[9];

#region Создаем девять точек
            ninePoints[0] = new Point3D(
                Convert.ToDouble(txtObjPoint1X.Text), 
                Convert.ToDouble(txtObjPoint1Y.Text), 
                Convert.ToDouble(txtObjPoint1Z.Text)
                );
            ninePoints[1] = new Point3D(
                Convert.ToDouble(txtObjPoint2X.Text), 
                Convert.ToDouble(txtObjPoint2Y.Text), 
                Convert.ToDouble(txtObjPoint2Z.Text)
                );
            ninePoints[2] = new Point3D(
                Convert.ToDouble(txtObjPoint3X.Text), 
                Convert.ToDouble(txtObjPoint3Y.Text), 
                Convert.ToDouble(txtObjPoint3Z.Text)
                );
            ninePoints[3] = new Point3D(
                Convert.ToDouble(txtObjPoint4X.Text), 
                Convert.ToDouble(txtObjPoint4Y.Text), 
                Convert.ToDouble(txtObjPoint4Z.Text)
                );
            ninePoints[4] = new Point3D(
                Convert.ToDouble(txtObjPoint5X.Text), 
                Convert.ToDouble(txtObjPoint5Y.Text), 
                Convert.ToDouble(txtObjPoint5Z.Text)
                );
            ninePoints[5] = new Point3D(
                Convert.ToDouble(txtObjPoint6X.Text), 
                Convert.ToDouble(txtObjPoint6Y.Text), 
                Convert.ToDouble(txtObjPoint6Z.Text)
                );
            ninePoints[6] = new Point3D(
                Convert.ToDouble(txtObjPoint7X.Text), 
                Convert.ToDouble(txtObjPoint7Y.Text), 
                Convert.ToDouble(txtObjPoint7Z.Text)
                );
            ninePoints[7] = new Point3D(
                Convert.ToDouble(txtObjPoint8X.Text), 
                Convert.ToDouble(txtObjPoint8Y.Text), 
                Convert.ToDouble(txtObjPoint8Z.Text)
                );
            ninePoints[8] = new Point3D(
                Convert.ToDouble(txtObjShapeControlPointX.Text), 
                Convert.ToDouble(txtObjShapeControlPointY.Text), 
                Convert.ToDouble(txtObjShapeControlPointZ.Text)
                );
#endregion
            
            patchControlPoints = getBezierPatchPoinsFrom9Points(ninePoints);

            // Создаем наш объект
            // Тут должны задаваться и передаваться параметры объекта
            int detalizationDegree = Convert.ToInt32(txtObjDetalization.Text);
            BezierPatch bezierPatch = new BezierPatch(patchControlPoints, detalizationDegree);

            //bezierPatch.setID(objectCounter);
            // Устанавливаем id для фигуры и ее составляющих
            if (tempObjectNumber != -1)
            {
                bezierPatch.setID(tempObjectNumber);

                // Добавляем объект в комбобокс
                cbObjects.Items.Add("object_" + tempObjectNumber++);

                tempObjectNumber = -1;
            }
            else
            {
                bezierPatch.setID(objectCounter);

                // Добавляем объект в комбобокс
                cbObjects.Items.Add("object_" + objectCounter++);
            }

            // Добавили объект в список
            List<SimpleObject> lToAdd = new List<SimpleObject>();
            lToAdd.Add(bezierPatch);
            bezierPatches.Add(lToAdd);

            // Добавляем объекты в движок
            engine.addObject(bezierPatch);


            // Перерисовываем изображение
            needToReDraw = true;
        }

        private void btnRemoveObject_Click(object sender, EventArgs e)
        {
            if (cbObjects.SelectedIndex == 0)
            {
                lblErrors.Text = "Выберете объект для удаления!";
                return;
            }

            // Удаляем объект
            bezierPatches.RemoveAt(cbObjects.SelectedIndex - 1);

            // Удаляем из комбобокса
            int old_selected = cbObjects.SelectedIndex;
            cbObjects.Items.RemoveAt(old_selected);
            cbObjects.SelectedIndex = old_selected - 1;

            // Обновляем картинку и список
            UpdateImg();
        }

        private void btnMove_Click(object sender, EventArgs e)
        {
            UpdateEngine();
            engine.setMove(Convert.ToDouble(txtMoveX.Text), Convert.ToDouble(txtMoveY.Text), Convert.ToDouble(txtMoveZ.Text));
            
            UpdateImg();
        }

        private void UpdateEngine()
        {
            engine.Clear();
            if (cbObjects.SelectedIndex == 0)
            {
                // Обновляем список объектов в движке
                foreach (List<SimpleObject> patch in bezierPatches)
                {
                    engine.addObjects(patch);
                }
            }
            else
            {
                // Добавляем выбранный объект
                engine.addObjects(bezierPatches[cbObjects.SelectedIndex - 1]);
            }
        }

        // Запуск панорамирования
        private void bntPlay_Click(object sender, EventArgs e)
        {
            panorama = true;
        }

        // Остановка панорамирования
        private void btnStop_Click(object sender, EventArgs e)
        {
            panorama = false;
            UpdateImg();
            needToReDraw = true;
        }

        // Таймер панорамирования
        private void panoramaTimer_Tick(object sender, EventArgs e)
        {
            Point3D position = engine.getCameraPosition();
            Point3D target = engine.getCameraTarget();

            txtPositionX.Text = "" + position.X;
            txtPositionY.Text = "" + position.Y;
            txtPositionZ.Text = "" + position.Z;

            txtTargetX.Text = "" + target.X;
            txtTargetY.Text = "" + target.Y;
            txtTargetZ.Text = "" + target.Z;

        }

        private void btnRotUp_MouseDown(object sender, MouseEventArgs e)
        {
            needToReDraw = true;
            currentButton = ButtonPressed.UP;
        }

        private void btnRotLeft_MouseDown(object sender, MouseEventArgs e)
        {
            needToReDraw = true;
            currentButton = ButtonPressed.LEFT;
        }

        private void btnRotDown_MouseDown(object sender, MouseEventArgs e)
        {
            needToReDraw = true;
            currentButton = ButtonPressed.DOWN;
        }

        private void btnRotRight_MouseDown(object sender, MouseEventArgs e)
        {
            needToReDraw = true;
            currentButton = ButtonPressed.RIGHT;
        }

        private void btn_MouseUp(object sender, MouseEventArgs e)
        {
            needToReDraw = true;
            currentButton = ButtonPressed.NONE;
        }

        private void btnRotate_Click(object sender, EventArgs e)
        {
            UpdateEngine();
            if (rbRotateTypeGlobal.Checked)
                engine.SetRotateType(Render.RotateType.Global);
            // RotateTypeOwn не работает!
            if (rbRotateTypeOwn.Checked)
                engine.SetRotateType(Render.RotateType.Own);
            engine.setRotate(Convert.ToDouble(txtRotateX.Text), Convert.ToDouble(txtRotateY.Text), Convert.ToDouble(txtRotateZ.Text));
            
            UpdateImg();
        }

        private void btnScale_Click(object sender, EventArgs e)
        {
            UpdateEngine();
            engine.setScale(Convert.ToDouble(txtScaleX.Text), Convert.ToDouble(txtScaleY.Text), Convert.ToDouble(txtScaleZ.Text));
            
            UpdateImg();
        }

        private void btnZp_MouseDown(object sender, MouseEventArgs e)
        {
            needToReDraw = true;
            currentButton = ButtonPressed.Zp;
        }

        private void btnZm_MouseDown(object sender, MouseEventArgs e)
        {
            needToReDraw = true;
            currentButton = ButtonPressed.Zm;
        }

        private void btnXp_MouseDown(object sender, MouseEventArgs e)
        {
            needToReDraw = true;
            currentButton = ButtonPressed.Xp;
        }

        private void btnXm_MouseDown(object sender, MouseEventArgs e)
        {
            needToReDraw = true;
            currentButton = ButtonPressed.Xm;
        }

        private void btnYp_MouseDown(object sender, MouseEventArgs e)
        {
            needToReDraw = true;
            currentButton = ButtonPressed.Yp;
        }

        private void btnYm_MouseDown(object sender, MouseEventArgs e)
        {
            needToReDraw = true;
            currentButton = ButtonPressed.Ym;
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            if (cbObjects.SelectedIndex == 0 || bezierPatches[cbObjects.SelectedIndex - 1].Count != 1)
            {
                lblErrors.Text = "Выберете объект! Не группу.";
                return;
            }
            tempObjectNumber = cbObjects.SelectedIndex;
            Point3D newPoint = new Point3D();
            Amend form2 = new Amend(ref newPoint);
            form2.ShowDialog();

            txtObjPoint1X.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[0, 0].X;
            txtObjPoint1Y.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[0, 0].Y;
            txtObjPoint1Z.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[0, 0].Z;
                                                                                              
            txtObjPoint2X.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[0, 1].X;
            txtObjPoint2Y.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[0, 1].Y;
            txtObjPoint2Z.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[0, 1].Z;
                                                                                              
            txtObjPoint3X.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[0, 2].X;
            txtObjPoint3Y.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[0, 2].Y;
            txtObjPoint3Z.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[0, 2].Z;
                                                                                              
            txtObjPoint4X.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[0, 3].X;
            txtObjPoint4Y.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[0, 3].Y;
            txtObjPoint4Z.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[0, 3].Z;
                                                                                              
            txtObjPoint5X.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[3, 0].X;
            txtObjPoint5Y.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[3, 0].Y;
            txtObjPoint5Z.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[3, 0].Z;
                                                                                              
            txtObjPoint6X.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[3, 1].X;
            txtObjPoint6Y.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[3, 1].Y;
            txtObjPoint6Z.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[3, 1].Z;
                                                                                              
            txtObjPoint7X.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[3, 2].X;
            txtObjPoint7Y.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[3, 2].Y;
            txtObjPoint7Z.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[3, 2].Z;
                                                                                              
            txtObjPoint8X.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[3, 3].X;
            txtObjPoint8Y.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[3, 3].Y;
            txtObjPoint8Z.Text = "" + ((BezierPatch)bezierPatches[cbObjects.SelectedIndex - 1][0]).controlPoints[3, 3].Z;

            txtObjShapeControlPointX.Text = "" + newPoint.X;
            txtObjShapeControlPointY.Text = "" + newPoint.Y;
            txtObjShapeControlPointZ.Text = "" + newPoint.Z;


            // Удаляем объект
            bezierPatches.RemoveAt(cbObjects.SelectedIndex - 1);

            // Удаляем из комбобокса
            int old_selected = cbObjects.SelectedIndex;
            cbObjects.Items.RemoveAt(old_selected);
            cbObjects.SelectedIndex = old_selected - 1;

            btnAddNewObject_Click(null, null);

            // Обновляем картинку и список
            UpdateImg();
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {
            if (cbObjects.SelectedIndex == 0)
            {
                lblErrors.Text = "Выберете объект!";
                return;
            }

            IntHolder toMerge = new IntHolder();
            toMerge.value = -1;
            Grouping form3 = new Grouping(ref toMerge, cbObjects.Items);
            form3.ShowDialog();

            if (toMerge.value != -1 && cbObjects.SelectedIndex != toMerge.value)
            {
                bezierPatches[(int)toMerge.value - 1].AddRange(
                    bezierPatches[(int)cbObjects.SelectedIndex - 1]
                    );
                btnRemoveObject_Click(null, null);
            }
        }

    }
}
