using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotView
{
    public class EE_U
    {
        public int eeID;
        public int eePartID;//末端执行器所附着的part id
        public string eeObj_Name;//末端执行器的name，比如 pen
        public List<float> eeTransform = new List<float>();//末端执行器的初始Transform
        public List<float> eeTransform_6 = new List<float>();//末端执行器的初始Transform
        public List<float> eePQ_initial = new List<float>();
        public List<float[]> ee_pathTransList = new List<float[]>();//末端执行器的目标路径
        public int ee_pathTransListIndex;//目标路径list对应的ee id
        public EE_U()
        {
            eePartID = -1;
            eeID = -1;
        }
    }
}
