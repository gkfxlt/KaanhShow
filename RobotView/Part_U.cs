using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotView
{
    public class Part_U
    {
        public int partID;
        public string partObjName;
        public string modelPath;
        //public List<Link_Module> linkList = new List<Link_Module>();
        //public List<float> linkTransformList = new List<float>();
        //public List<string> linkPrefabPath = new List<string>();
        //public List<string> linkObjNameList = new List<string>();
        public List<float> partTransform_6 = new List<float>();
        public List<float> partTransform_6_initial = new List<float>();
        public List<float> partTransform_initial = new List<float>();
        public List<float> partPQ_initial = new List<float>();
        //public List<float> partPQ_initial = new List<float>();
        public List<float> partTransform = new List<float>();
        public List<string> jointNameList = new List<string>();//两个joint直接相连时
        public bool isEE;
        public int eeID;
        public List<string> geometryPathList = new List<string>();
        public List<float> geometryPQ_List = new List<float>();


        public Part_U()
        {
            partID = -1;
            isEE = false;
        }
    }
}
