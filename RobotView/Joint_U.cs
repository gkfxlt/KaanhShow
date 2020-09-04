using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotView
{
    public class Joint_U
    {
        public int jointID;
        public string prefabPath;
        public int jointType;//0: R; 1: T;
        public int partID_1;
        public int partID_2;
        public List<float> jointTransform = new List<float>();
        public string jointObjName;
        public List<float> jointAxisPos = new List<float>();
        public List<float> jointAxisDir = new List<float>();
        public string axisObjName;//体现轴线位置和方向的
        public int axisObjType;//承载轴线属性的obj的轴线类型[1:x/right; 2:y/up; 3:z/forward] 
        public Joint_U()
        {
            jointID = -1;
            partID_1 = -1;
            partID_2 = -1;
            jointType = -1;
        }
    }
}
