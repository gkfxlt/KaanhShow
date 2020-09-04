using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotView
{
    public class Path_U
    {
        public static void Interpolation(double[] pos1,double[] pos2,double lineStep,ref List<double[]> vecArray)
        {
            Vector3_U pos1_ = new RobotView.Vector3_U(pos1[0], pos1[1], pos1[2]);
            Vector3_U pos2_ = new Vector3_U(pos2[0], pos2[1], pos2[2]);
            List<Vector3_U> vecArray_ = new List<RobotView.Vector3_U>();
            Interpolation(pos1_, pos2_, lineStep, ref vecArray_);
            for (int i = 0; i < vecArray_.Count; i++)
            {
                vecArray.Add(new double[] { vecArray_[i].x, vecArray_[i].y, vecArray_[i].z });
            }
        }
        public static void Interpolation(Vector3_U pos1, Vector3_U pos2, double lineStep, ref List<Vector3_U> vecArray)//直线插值
        {
            double dis = lineStep;//插值间距
            Vector3_U direction = new Vector3_U(pos2.x - pos1.x, pos2.y - pos1.y, pos2.z - pos1.z);
            direction.Normalize();
            //vecArray.Add(pos1);
            //Vector3 dif = new Vector3();
            int i = 0;
            double dif;
            do
            {
                Vector3_U midPos = new Vector3_U(pos1.x + dis * i * direction.x, pos1.y + i * dis * direction.y, pos1.z + i * dis * direction.z);
                vecArray.Add(midPos);
                dif = Vector3_U.Distance(midPos, pos2);
                i++;
            } while (dif > dis);
            vecArray.Add(pos2);
        }

        public void InterpolationC(Vector3_U center, double radius, double startDeg, double endDeg, ref List<Vector3_U> vecArray)//0-360，圆形插值
        {
            double stepDeg = startDeg;
            double stepAdd = 10f;
            do
            {
                double theta =(double) (stepDeg / 180.0f * Math.PI);
                Vector3_U vec1 = new Vector3_U(center.x + radius * Math.Cos(theta), center.y + radius * Math.Sin(theta), center.z);
                vecArray.Add(vec1);
                stepDeg = stepDeg + stepAdd;
            } while (stepDeg < endDeg);

        }

    }

    public class Vector3_U
    {
        public double x, y, z;

        public Vector3_U(double x_,double y_,double z_)
        {
            x = x_;
            y = y_;
            z = z_;
        }

        public double Length()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }
        public static double Distance(Vector3_U v1,Vector3_U v2)
        {
            return Math.Sqrt(Math.Pow((v1.x - v2.x), 2) + Math.Pow((v1.y - v2.y), 2) + Math.Pow((v1.z - v2.z), 2));
        }
        public  Vector3_U Normalize()
        {
            return new Vector3_U(x / Length(), y / Length(), z / Length());
        }
    }

}
