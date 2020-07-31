using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Reflection;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //xyz坐标
         struct Point3D
        {
          public float x;
          public  float y;
          public float z;
        };
        //rgb颜色数据
         struct rgb
        {
            public float r;
            public float g;
            public float b;
        };

        const float PI = 3.141592543f;
        //存储坐标点的集合
        List<Point3D> ls_pts=new List<Point3D>();
        //存储颜色集合
        List<rgb> ls_cols=new List<rgb>();
        float fWidth, fHeight;
        //视点z坐标
        float eyeZ;
        //物体中心点
        float centerX, centerY, centerZ;
        //物体长度和宽度
        float halfWidth, halfHeight;
        //平移的距离
        float translateX, translateY;
        //旋转的角度
        float rotX, rotY;
        //鼠标按下的位置
        float downPtX, downPtY;
        BeginMode terrainRenderStyle = BeginMode.Points;

        float tempEyeZ;
        int arb;
        int pt3DNum;
    
        private void glControl1_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.SteelBlue);	// Set color
            string filepath = "F:\\OpenTK\\OpenTK_test\\OpenTK Test\\OpenTK Test\\obj\\Debug\\view1445922.ply";
            LoadHeightMap(filepath);
        }
        //刷新加载
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 lookat = Matrix4.LookAt(0, 0, 6 * eyeZ, 0, 0, 0, 0, 1, 0);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);
            GL.Translate(translateX, translateY, 0); //键盘平移
            GL.Rotate(rotX, 1, 0, 0);//x轴旋转
            GL.Rotate(rotY, 0, 1, 0);//y轴旋转
          //  GL.Scale(scal1, scal1, scal1);
            GL.Translate(-centerX, -centerY, -centerZ);
            // Render heightmap
            this.RenderHeightmap();

            glControl1.SwapBuffers();

        }
        //控件改变后刷新
        private void glControl1_Resize(object sender, EventArgs e)
        {

            fWidth = (float)Width;
            fHeight = (float)Height;
            GL.Viewport(0, 0, Width, Height);

            float aspect_ratio = Width / (float)Height;
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect_ratio, 1f, 50000f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perpective);
            return;

        }
        private void LoadHeightMap(string filepath)
        {
            float minX = 0.0f, maxX = 0.0f, minY = 0.0f, maxY = 0.0f, minZ = 0.0f, maxZ = 0.0f;
            if (File.Exists(filepath))										// File exists
            {
                String[] vs = File.ReadAllLines(filepath);
                for (int i = 11; i < vs.Length; i++)
                {
                    Point3D pt;
                    rgb col;
                    String[] ss = vs[i].Split(' ');
                    pt.x = float.Parse(ss[0]);
                    pt.y = float.Parse(ss[1]);
                    pt.z = float.Parse(ss[2]);
                    col.r = float.Parse(ss[3]);
                    col.g = float.Parse(ss[4]);
                    col.b = float.Parse(ss[5]);
                    if (eyeZ < System.Math.Abs(pt.z))
                        eyeZ = System.Math.Abs(pt.z);
                    if (pt.x < minX)
                        minX = pt.x;
                    if (pt.x > maxX)
                        maxX = pt.x;
                    if (pt.y < minY)
                        minY = pt.y;
                    if (pt.y > maxY)
                        maxY = pt.y;
                    minZ += pt.z;
                    ls_pts.Add(pt);
                    ls_cols.Add(col);
                }
            }
            centerX = (maxX + minX) / 2;
            centerY = (maxY + minY) / 2;
            centerZ = minZ / (ls_pts.Count);

            halfWidth = System.Math.Abs(maxX) > System.Math.Abs(minX) ? System.Math.Abs(maxX) : System.Math.Abs(minX);
            halfHeight = System.Math.Abs(maxY) > System.Math.Abs(minY) ? System.Math.Abs(maxY) : System.Math.Abs(minY);

            pt3DNum = ls_pts.Count;
        }
        private void RenderHeightmap()
        {
            // Set the selected BeginMode
            GL.Begin(terrainRenderStyle);
            for (int i = 0; i < ls_pts.Count; i ++)
            {
                //GL.Normal3((ls_cols[i].r) / 255.0f, (ls_cols[i].g) / 255.0f, (ls_cols[i].b) / 255.0f);
                GL.Color4((ls_cols[i].r) / 255.0f, (ls_cols[i].g) / 255.0f, (ls_cols[i].b) / 255.0f, 1.0f);
                GL.Vertex3(ls_pts[i].x, ls_pts[i].y, ls_pts[i].z);                                                     
            }
            GL.End();
        }

        //滚轮改变刷新
        private void glControl1_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            System.Windows.Forms.MouseEventArgs ev = (e as System.Windows.Forms.MouseEventArgs);
            if (e.Delta > 0)
            {
                eyeZ += eyeZ / 5;
                glControl1.Invalidate();
            }

            // If the mouse wheel delta is negative, move the box down.
            if (e.Delta < 0)
            {
                eyeZ -= eyeZ / 5;
                glControl1.Invalidate();
            }
            
        }
        //鼠标按压
        private void glControl1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            System.Windows.Forms.MouseEventArgs ev = (e as System.Windows.Forms.MouseEventArgs);
            downPtX = (float)ev.X;
            downPtY = (float)ev.Y;
            tempEyeZ = eyeZ;


        }
        //鼠标移动
        private void glControl1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            System.Windows.Forms.MouseEventArgs ev = (e as System.Windows.Forms.MouseEventArgs);
            if (e.Button == MouseButtons.Left) 
            {
                rotX += PI * 10* (ev.Y - downPtY) / fHeight;
                rotY += PI * 10 * (ev.X - downPtX) / fWidth;
                glControl1.Invalidate();
            }

        }
        //键盘按压
        private void glControl1_KeyDown(object sender, KeyEventArgs  e)
        {
            
            if (e.KeyCode ==Keys.W)
            {
                translateY += halfHeight / 30;
            }
            else if (e.KeyCode == Keys.S) 
            {
                translateY -= halfHeight / 30;
            }
            else if (e.KeyCode == Keys.A)
            {
                translateX -= halfWidth / 30;
            }
            else if (e.KeyCode == Keys.D)
            {
                translateX += halfWidth / 30;
            }
            glControl1.Invalidate();
        }
    }
}
