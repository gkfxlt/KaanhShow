using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RobotView
{
    public class AnyRobot_U
    {
        public string brand;
        public string model;
        public string xmlPath;

        //所有ID 从0开始，ground ID 为0；
        public string robotName;
        public int jointNo;
        public List<Part_U> partList;
        public List<Joint_U> jointList;
        public List<EE_U> endEList;
        public bool useOneModel = true;//使用一个装配体文件

        public string mainModelPath;//装配体文件路径
        public List<string> otherModelPaths = new List<string>();//附属文件路径

        public List<float> transformList = new List<float>();
        public int linkNo;
        //public int baseNo;
        public int jointsNo;
        public int endNo;

        public string robotXml_filePath;
        public string targetPathXml_FilePath;
        public string wifiCmdXml_FilePath;

        public List<string> linkNameList = new List<string>();
        private int assembleSteps;
        public bool ifFormatted = false;


        //for motion
        public List<List<float[]>> partMotionTransList = new List<List<float[]>>();
        public List<List<float[]>> partMotionPQList = new List<List<float[]>>();
        public List<List<float[]>> partMotionTransformList = new List<List<float[]>>();
        public List<List<float>> jointAngle_rel = new List<List<float>>();//步进值
        public List<List<float>> jointAngle_abs = new List<List<float>>();//绝对值
        //List<Transform> objToRotateList = new List<Transform>();

        public List<float[]> partRealTransList = new List<float[]>();//实时更新杆件的位姿

        public List<string> cmdList = new List<string>();
        public string cmdListStr;
        public float[] workStateTheta;//到达工作状态需转动角度
        public float[] initialCorrectTheta;//补偿装配时的零位误差

        public bool jointAngleGot = false;
        public List<int> motorDirCorrect = new List<int>();//电机方向的纠正值

        public bool inReading = false;//是否在读取关节角度


        //for motion
        public void ClearRobotData()//新的计算时，清空除初始化信息外的其他数据
        {
            transformList.Clear();
            partMotionTransList.Clear();
            partMotionPQList.Clear();
            partMotionTransformList.Clear();
            jointAngle_abs.Clear();
            jointAngle_rel.Clear();
            cmdList.Clear();
            jointAngleGot = false;


        }
        public AnyRobot_U()
        {
            partList = new List<Part_U>();
            jointList = new List<Joint_U>();
            endEList = new List<EE_U>();
        }
        public AnyRobot_U(int axisNum)
        {
            linkNo = 0;
            //baseNo = 0;
            jointsNo = 0;
            assembleSteps = 0;
            endNo = 0;
            partList = new List<Part_U>();
            jointList = new List<Joint_U>();
            endEList = new List<EE_U>();
            workStateTheta = new float[axisNum];
            initialCorrectTheta = new float[axisNum];
        }

        /// <summary>
        /// 从服务器xml中定义到robot
        /// </summary>
        public void LoadRobot(string strXml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(strXml);


            XmlNode rootNode = doc.DocumentElement;
            robotName = ((XmlElement)rootNode).GetAttribute("Name");
            brand = ((XmlElement)rootNode).GetAttribute("Brand");
            model = ((XmlElement)rootNode).GetAttribute("Model");

            XmlNode modelPathNode = rootNode.SelectSingleNode("ModelPath");
            mainModelPath = modelPathNode.SelectSingleNode("MainModel").InnerText;
            XmlNodeList othersList = modelPathNode.SelectNodes("OtherModel");
            foreach (XmlNode other in othersList)
            {
                otherModelPaths.Add(other.InnerText);
            }

            XmlNode listNode = rootNode.SelectSingleNode("PartList");
            XmlNodeList partNode = listNode.SelectNodes("PartObj");
            foreach (XmlNode node in partNode)
            {
                Part_U onePart = new Part_U();
                XmlElement robotEle = (XmlElement)node;
                onePart.partObjName = robotEle.GetAttribute("Name");
                string partIDStr = robotEle.GetAttribute("ID");
                if (!String.IsNullOrEmpty(partIDStr))
                {
                    try
                    {
                        onePart.partID = int.Parse(partIDStr);
                    }
                    catch (Exception e)
                    {

                    }
                }

                if (false)
                {
                    onePart.modelPath = node.SelectSingleNode("ModelPath").InnerText;
                }

                string partPQstr = node.SelectSingleNode("PQ").InnerText;
                if (!String.IsNullOrEmpty(partPQstr))
                {
                    try
                    {
                        partPQstr = partPQstr.Replace(" ", "");
                        string[] strList = partPQstr.Split(new char[] { ',' });
                        for (int i = 0; i < strList.Length; i++)
                        {
                            onePart.partPQ_initial.Add(float.Parse(strList[i]));
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
                partList.Add(onePart);
            }

            XmlNode jointNode = rootNode.SelectSingleNode("JointList");
            XmlNodeList jointNodeList = jointNode.SelectNodes("Axis");
            foreach (XmlNode joint in jointNodeList)
            {
                Joint_U oneJoint = new Joint_U();
                try
                {
                    oneJoint.jointID = int.Parse(((XmlElement)joint).GetAttribute("ID"));
                    oneJoint.partID_1 = int.Parse(((XmlElement)joint).GetAttribute("PartID1"));
                    oneJoint.partID_2 = int.Parse(((XmlElement)joint).GetAttribute("PartID2"));
                    oneJoint.jointType = int.Parse(((XmlElement)joint).GetAttribute("Type"));
                    string posStr = joint.SelectSingleNode("Pos").InnerText;
                    posStr = posStr.Replace(" ", "");
                    string[] posStrList = posStr.Split(new char[] { ',' });
                    string dirStr = joint.SelectSingleNode("Dir").InnerText;
                    dirStr = dirStr.Replace(" ", "");
                    string[] dirStrList = dirStr.Split(new char[] { ',' });
                    for (int i = 0; i < 3; i++)
                    {
                        oneJoint.jointAxisPos.Add(float.Parse(posStrList[i]));
                        oneJoint.jointAxisDir.Add(float.Parse(dirStrList[i]));
                    }
                }
                catch (Exception e)
                {

                }
                jointList.Add(oneJoint);
            }

            XmlNode eeNode = rootNode.SelectSingleNode("EndEffectorList");
            XmlNodeList eeList = eeNode.SelectNodes("EE");
            foreach (XmlNode ee in eeList)
            {
                EE_U oneEE = new EE_U();
                try
                {
                    oneEE.eeID = int.Parse(((XmlElement)ee).GetAttribute("ID"));
                    oneEE.eePartID = int.Parse(((XmlElement)ee).GetAttribute("PartID"));
                    oneEE.eeObj_Name = ((XmlElement)ee).GetAttribute("ObjName");
                    string pqStr = ee.SelectSingleNode("PQ").InnerText;
                    pqStr = pqStr.Replace(" ", "");
                    string[] strList = pqStr.Split(new char[] { ',' });
                    for (int i = 0; i < strList.Length; i++)
                    {
                        oneEE.eePQ_initial.Add(float.Parse(strList[i]));
                    }
                }
                catch (Exception e)
                {

                }
                endEList.Add(oneEE);
            }

        }

        /// <summary>
        /// 从服务器xml中定义到robot, aris格式
        /// </summary>
        public void LoadRobot_Aris(string strXml, bool isPath)
        {
            XmlDocument doc = new XmlDocument();
            if (isPath)
            {
                doc.Load(strXml);
            }
            else
            {
                doc.LoadXml(strXml);
            }


            XmlNode rootNode = doc.DocumentElement;
            robotName = "AnyRobot";
            brand = "AnyBrand";
            model = "AnyModel";
            //brand = ((XmlElement)rootNode).GetAttribute("Brand");
            //model = ((XmlElement)rootNode).GetAttribute("Model");

            XmlNode modelNode = rootNode.SelectSingleNode("model");
            XmlNode part_pool_node = modelNode.SelectSingleNode("part_pool");
            XmlNodeList part_list_node = part_pool_node.ChildNodes;
            foreach (XmlNode oneNode in part_list_node)
            {
                Part_U onePart = new Part_U();
                onePart.partObjName = oneNode.Name;
                XmlElement oneElement = (XmlElement)oneNode;
                string peStr = oneElement.GetAttribute("pq");
                //Debug.Log(peStr);
                peStr = peStr.Replace("{", "");
                peStr = peStr.Replace("}", "");
                peStr = peStr.Replace(" ", "");
                //Debug.Log(peStr);
                string[] peStr_List = peStr.Split(new char[] { ',' });
                for (int str_i = 0; str_i < peStr_List.Length; str_i++)
                {
                    if (!String.IsNullOrEmpty(peStr_List[str_i]))
                    {
                        try
                        {
                            onePart.partPQ_initial.Add(float.Parse(peStr_List[str_i]));
                        }
                        catch (Exception e)
                        {
                            //Debug.Log(e.ToString());
                            Console.WriteLine(e.ToString());
                        }

                    }
                }

                XmlNode geometry_pool_node = oneNode.SelectSingleNode("geometry_pool");
                XmlNodeList geo_list_node = geometry_pool_node.ChildNodes;
                foreach (XmlNode oneGeoNode in geo_list_node)
                {
                    XmlElement oneGeoElement = (XmlElement)oneGeoNode;
                    Console.WriteLine(oneGeoElement.GetAttribute("graphic_file_path"));
                    onePart.geometryPathList.Add(oneGeoElement.GetAttribute("graphic_file_path"));
                    string geoPeStr = oneGeoElement.GetAttribute("pq");
                    geoPeStr = geoPeStr.Replace("{", "");
                    geoPeStr = geoPeStr.Replace("}", "");
                    geoPeStr = geoPeStr.Replace(" ", "");
                    foreach (string str in geoPeStr.Split(new char[] { ',' }))
                    {
                        try
                        {
                            onePart.geometryPQ_List.Add(float.Parse(str));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            //throw;
                        }
                    }
                    //string[] geoPeStr_list = geoPeStr.Split(new char[] { ',' });

                }
                partList.Add(onePart);
            }

        }


        /// <summary>
        /// 模型下载加载完成后，重新命名标识part，避免重复
        /// </summary>
        //public void ReNameParts()
        //{
        //    string dateTimeNow = DateTime.Now.ToString("yyyyMMddHHmm");
        //    for (int part_i = 0; part_i < partList.Count; part_i++)
        //    {
        //        GameObject partObj = GameObject.Find(partList[part_i].partObjName);
        //        string newName = robotName + "_" + partList[part_i].partObjName + "_" + dateTimeNow;
        //        partList[part_i].partObjName = newName;
        //        partObj.name = newName;
        //    }

        //    for (int ee_i = 0; ee_i < endEList.Count; ee_i++)
        //    {
        //        GameObject partObj = GameObject.Find(endEList[ee_i].eeObj_Name);
        //        string newName = robotName + "_" + endEList[ee_i].eeObj_Name + "_" + dateTimeNow;
        //        endEList[ee_i].eeObj_Name = newName;
        //        partObj.name = newName;
        //    }

        //}
        //public void StoreInitialConfiguration()
        //{
        //    for (int part_i = 0; part_i < partList.Count; part_i++)
        //    {
        //        partList[part_i].StoreInitialConfiguration();
        //    }
        //    for (int joint_i = 0; joint_i < jointList.Count; joint_i++)
        //    {
        //        //jointList[joint_i].StoreConfiguration();
        //        jointList[joint_i].FindAxis();
        //    }
        //    for (int ee_i = 0; ee_i < endEList.Count; ee_i++)
        //    {
        //        endEList[ee_i].StoreInitialConfiguration();
        //    }

        //}
        public void ExportModel(string filePath)//save robot user defined to aris model in xml
        {
            string dateTimeNow = DateTime.Now.ToString("yyyyMMdd HH:mm");
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "us-ascii", null);
            doc.AppendChild(dec);
            //创建一个根节点（一级）
            XmlElement root = doc.CreateElement("Robot");
            root.SetAttribute("Name", robotName);
            root.SetAttribute("Brand", brand);
            root.SetAttribute("Model", model);
            root.SetAttribute("Modify", dateTimeNow);
            root.SetAttribute("Create", dateTimeNow);
            doc.AppendChild(root);

            XmlElement ModelList = doc.CreateElement("ModelPath");
            ModelList.SetAttribute("MainModel", useOneModel.ToString());
            XmlElement mainModelEle = doc.CreateElement("MainModel");
            mainModelEle.InnerText = mainModelPath;
            ModelList.AppendChild(mainModelEle);

            for (int path_i = 0; path_i < otherModelPaths.Count; path_i++)
            {
                XmlElement othersEle = doc.CreateElement("OtherModel");
                othersEle.InnerText = otherModelPaths[path_i];
                ModelList.AppendChild(othersEle);
            }
            root.AppendChild(ModelList);

            XmlElement partListEle = doc.CreateElement("PartList");
            for (int part_i = 0; part_i < partList.Count; part_i++)
            {
                XmlElement partObjEle = doc.CreateElement("PartObj");
                partObjEle.SetAttribute("Name", partList[part_i].partObjName);
                partObjEle.SetAttribute("ID", partList[part_i].partID.ToString());
                if (!String.IsNullOrEmpty(partList[part_i].modelPath))
                {
                    XmlElement modelPathEle = doc.CreateElement("ModelPath");
                    modelPathEle.InnerText = partList[part_i].modelPath;
                    partObjEle.AppendChild(modelPathEle);
                }
                if (false)
                {
                    XmlElement partTransformEle = doc.CreateElement("Transform");
                    //for (int i = 0; i < 16; i++)
                    //{
                    //    partTransform.SetAttribute("a" + i.ToString(), partList[part_i].partTransform[i].ToString());
                    //}

                    partTransformEle.InnerText = ListToString(partList[part_i].partTransform);
                    partObjEle.AppendChild(partTransformEle);
                }
                if (true)
                {
                    XmlElement partPQEle = doc.CreateElement("PQ");
                    //for (int i = 0; i < 16; i++)
                    //{
                    //    partTransform.SetAttribute("a" + i.ToString(), partList[part_i].partTransform[i].ToString());
                    //}

                    partPQEle.InnerText = ListToString(partList[part_i].partPQ_initial);

                    partObjEle.AppendChild(partPQEle);
                }

                partListEle.AppendChild(partObjEle);
            }
            root.AppendChild(partListEle);

            XmlElement jointListEle = doc.CreateElement("JointList");
            for (int joint_i = 0; joint_i < jointList.Count; joint_i++)
            {
                XmlElement axisEle = doc.CreateElement("Axis");
                axisEle.SetAttribute("ID", jointList[joint_i].jointID.ToString());
                axisEle.SetAttribute("Type", jointList[joint_i].jointType.ToString());
                axisEle.SetAttribute("PartID1", jointList[joint_i].partID_1.ToString());
                axisEle.SetAttribute("PartID2", jointList[joint_i].partID_2.ToString());

                XmlElement posEle = doc.CreateElement("Pos");
                //posEle.SetAttribute("x", jointList[joint_i].jointAxisPos[0].ToString());
                //posEle.SetAttribute("y", jointList[joint_i].jointAxisPos[1].ToString());
                //posEle.SetAttribute("z", jointList[joint_i].jointAxisPos[2].ToString());

                posEle.InnerText = ListToString(jointList[joint_i].jointAxisPos);
                axisEle.AppendChild(posEle);

                XmlElement dirEle = doc.CreateElement("Dir");
                //dirEle.SetAttribute("x", jointList[joint_i].jointAxisDir[0].ToString());
                //dirEle.SetAttribute("y", jointList[joint_i].jointAxisDir[1].ToString());
                //dirEle.SetAttribute("z", jointList[joint_i].jointAxisDir[2].ToString());
                dirEle.InnerText = ListToString(jointList[joint_i].jointAxisDir);
                axisEle.AppendChild(dirEle);

                jointListEle.AppendChild(axisEle);
            }
            root.AppendChild(jointListEle);

            XmlElement EEList = doc.CreateElement("EndEffectorList");
            for (int ee_i = 0; ee_i < endEList.Count; ee_i++)
            {

                XmlElement EEele = doc.CreateElement("EE");
                EEele.SetAttribute("ID", ee_i.ToString());
                EEele.SetAttribute("PartID", endEList[ee_i].eePartID.ToString());

                if (false)
                {
                    XmlElement ptEle = doc.CreateElement("Transform");
                    for (int i = 0; i < 16; i++)
                    {
                        ptEle.SetAttribute("a" + i.ToString(), endEList[ee_i].eeTransform[i].ToString());
                    }
                    EEele.AppendChild(ptEle);
                }
                if (true)
                {
                    XmlElement eePQEle = doc.CreateElement("PQ");
                    eePQEle.InnerText = ListToString(endEList[ee_i].eePQ_initial);
                    EEele.AppendChild(eePQEle);
                }

                EEList.AppendChild(EEele);
            }

            root.AppendChild(EEList);

            doc.Save(filePath);

        }
        //public void MotionWithPQ(List<float> pqList)
        //{
        //    if (pqList.Count > 6)
        //    {
        //        for (int part_i = 1; part_i < partList.Count; part_i++)
        //        {
        //            //float[] partPosEuler = partMotionPQList[part_i][moveStep];
        //            GameObject partObj = GameObject.Find(partList[part_i].partObjName);
        //            //Debug.Log(partList[part_i].partObjName);

        //            if (partObj != null)
        //            {

        //                partObj.transform.position = new Vector3(pqList[0 + part_i * 7] * -1, pqList[1 + part_i * 7], pqList[2 + part_i * 7]);
        //                partObj.transform.rotation = new Quaternion(pqList[3 + part_i * 7], pqList[4 + part_i * 7] * (-1), pqList[5 + part_i * 7] * (-1), pqList[6 + part_i * 7]);

        //            }

        //        }
        //    }
        //}
        public string ListToString(List<float> listStr)
        {
            StringBuilder strBuild = new StringBuilder();
            for (int i = 0; i < listStr.Count; i++)
            {
                strBuild.Append(listStr[i].ToString("e"));
                if (i < listStr.Count - 1)
                {
                    strBuild.Append(",");
                }
            }
            return strBuild.ToString();
        }
        public string ListToString(float[] listStr)
        {
            StringBuilder strBuild = new StringBuilder();
            for (int i = 0; i < listStr.Length; i++)
            {
                strBuild.Append(listStr[i].ToString("e"));
                if (i < listStr.Length - 1)
                {
                    strBuild.Append(",");
                }
            }
            return strBuild.ToString();
        }
    }

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
