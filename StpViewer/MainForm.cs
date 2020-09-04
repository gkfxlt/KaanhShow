using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AnyCAD.Platform;
using AnyCAD.Exchange;
using AnyCAD.Presentation;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using WebSocketSharp;
using RobotView;
using System.IO;


namespace StpViewer
{
    public partial class MainForm : Form
    {
       
        private AnyCAD.Presentation.RenderWindow3d renderView = null;
        private AnyRobot_U any_robot = new AnyRobot_U();
        private bool motionFinished = true;
        private bool ifUpDateSimModelFromWebSocket = false;
        private WebData _wabDataFroBotServerTransfer;
        private List<float> step = new List<float>();
        private MessageFromBotServer mess = new MessageFromBotServer();
        private string webSocketSerAddress = "127.0.0.1";
        //private string webSocketSerAddress = "192.168.1.55";

        private string webSocketServerPort = "5866";
        GroupSceneNode robot_node = new GroupSceneNode();
        List<GroupSceneNode> partNodeList = new List<GroupSceneNode>();
        List<SceneNode> geoNodeList = new List<SceneNode>();
        public bool robotLoadCompleted = true;
        public string path = "";

        public MainForm()
        {
            InitializeComponent();

            this.renderView = new AnyCAD.Presentation.RenderWindow3d();
            this.renderView.Location = new System.Drawing.Point(0, 0);
            this.renderView.Size = this.Size;
            this.renderView.TabIndex = 1;
            this.splitContainer1.Panel2.Controls.Add(this.renderView);

            this.renderView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnRenderWindow_MouseClick);

            GlobalInstance.EventListener.OnChangeCursorEvent += OnChangeCursor;
            GlobalInstance.EventListener.OnSelectElementEvent += OnSelectElement;
            GlobalInstance.EventListener.OnSelectElementEvent += OnSelectionChanged;

            System.Timers.Timer t = new System.Timers.Timer(100);//实例化Timer类，设置时间间隔
            t.Elapsed += new System.Timers.ElapsedEventHandler(Update);//到达时间的时候执行事件
            t.AutoReset = true;//设置是执行一次（false）还是一直执行(true)
            t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件
            path= System.IO.Directory.GetCurrentDirectory();

            //webBrowser1.Navigate("http://120.27.231.59:809/Configure.html");
        }

        private void OnSelectElement(SelectionChangeArgs args)
        {
            if (!args.IsHighlightMode())
            {
                SelectedShapeQuery query = new SelectedShapeQuery();
                renderView.QuerySelection(query);
                var shape = query.GetGeometry();
                if (shape != null)
                {
                    GeomCurve curve = new GeomCurve();
                    if (curve.Initialize(shape))
                    {
                        TopoShapeProperty property = new TopoShapeProperty();
                        property.SetShape(shape);
                        Console.WriteLine("Edge Length {0}", property.EdgeLength());
                    }
                }
            }
        }

        private bool m_PickPoint = false;

        private void OnRenderWindow_MouseClick(object sender, MouseEventArgs e)
        {
            if (!m_PickPoint)
                return;

            AnyCAD.Platform.PickHelper pickHelper = renderView.PickShape(e.X, e.Y);
            if (pickHelper != null)
            {
                // add a ball
                //Platform.TopoShape shape = GlobalInstance.BrepTools.MakeSphere(pt, 2);
                //renderView.ShowGeometry(shape, 100);
            }
            // Try the grid
            Vector3 pt = renderView.HitPointOnGrid(e.X, e.Y);
            if (pt != null)
            {
                //Platform.TopoShape shape = GlobalInstance.BrepTools.MakeSphere(pt, 2);
                //renderView.ShowGeometry(shape, 100);
            }
        }

        private void OnChangeCursor(String commandId, String cursorHint)
        {

            if (cursorHint == "Pan")
            {
                this.renderView.Cursor = System.Windows.Forms.Cursors.SizeAll;
            }
            else if (cursorHint == "Orbit")
            {
                this.renderView.Cursor = System.Windows.Forms.Cursors.Hand;
            }
            else if (cursorHint == "Cross")
            {
                this.renderView.Cursor = System.Windows.Forms.Cursors.Cross;
            }
            else
            {
                if (commandId == "Pick")
                {
                    this.renderView.Cursor = System.Windows.Forms.Cursors.Arrow;
                }
                else
                {
                    this.renderView.Cursor = System.Windows.Forms.Cursors.Default;
                }
            }

        }

        /// <summary>
        /// 相当于Unity的Update
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        ///
        void Update(object source, System.Timers.ElapsedEventArgs e)
        {
            if (motionFinished)//是否对碰撞做出反应，是否停止运动
            {
                motionFinished = false;
                if (ifUpDateSimModelFromWebSocket)
                {

                    if (_wabDataFroBotServerTransfer._webSocket!=null)
                    {

                        if (_wabDataFroBotServerTransfer.msgOutQueue.Count > 0)
                        {
                            StartUpdateSimModel();
                            _wabDataFroBotServerTransfer._webSocket.Send(OperateBotCmd(_wabDataFroBotServerTransfer.msgOutQueue.Dequeue()));
                            
                        }
                        
                        _wabDataFroBotServerTransfer._webSocket.Send(OperateBotCmd("1&1&0&0&0&get_part_pq"));
                        //Console.WriteLine("send Read");
                    }
                    
                    while (_wabDataFroBotServerTransfer.BinQueue.Count > 0)
                    {
                        mess.byteFromBotServer.Enqueue(_wabDataFroBotServerTransfer.BinQueue.Dequeue());
                    }

                    while (mess.byteFromBotServer.Count > 0)
                    {
                        if (step == null)
                        {
                            step = new List<float>();
                        }
                        // List<float> step = new List<float>();
                        //step.Clear();
                        List<float> tempList = mess.ProcessBiteQue(any_robot.partList.Count);
                        if (tempList!=null)
                        {
                            step = step.Concat(tempList).ToList();
                        }
                        
                        //Debug.Log(step)
                        //Debug.Log();
                    }
                    if (step != null && step.Count > any_robot.partList.Count*7-1)
                    {
                        
                        float[] stepData = new float[any_robot.partList.Count * 7];
                        for (int data_i = 0; data_i < stepData.Length; data_i++)
                        {
                            stepData[data_i] = step[step.Count-stepData.Length+ data_i];
                        }
                        MotionWithPQ(stepData);
                        //step.RemoveRange(0, any_robot.partList.Count * 7);
                        step.Clear();
                    }
                    renderView.RequestDraw();
                }
                motionFinished = true;
            }
            
        }

        private static byte[] OperateBotCmd(string str)
        {
            Console.WriteLine("bot cmd:" + str);
            string[] strArray = str.Split(new char[] { '&' });
            if (strArray.Length > 2)
            {
                try
                {
                    int cmd_id = int.Parse(strArray[0]);
                    long cmd_option = long.Parse(strArray[1]);
                    long reserved_1 = long.Parse(strArray[2]);
                    long reserved_2 = long.Parse(strArray[3]);
                    long reserved_3 = long.Parse(strArray[4]);
                    byte[] cmdByte = Encoding.Default.GetBytes(strArray[5]);

                    List<byte> packData = new List<byte>();

                    byte[] byte_cmd_length = BitConverter.GetBytes(cmdByte.Length);
                    byte[] byte_cmd_id = BitConverter.GetBytes(cmd_id);
                    byte[] byte_cmd_option = BitConverter.GetBytes(cmd_option);
                    byte[] byte_cmd_res1 = BitConverter.GetBytes(reserved_1);
                    byte[] byte_cmd_res2 = BitConverter.GetBytes(reserved_2);
                    byte[] byte_cmd_res3 = BitConverter.GetBytes(reserved_3);
                    packData.AddRange(byte_cmd_length);
                    packData.AddRange(byte_cmd_id);
                    packData.AddRange(byte_cmd_option);
                    packData.AddRange(byte_cmd_res1);
                    packData.AddRange(byte_cmd_res2);
                    packData.AddRange(byte_cmd_res3);
                    packData.AddRange(cmdByte);

                    return packData.ToArray();
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.ToString());

                    return null;
                }

            }
            else
            {
                return null;
            }

        }

        private void StartSim(object sender, EventArgs e)
        {
            StartUpdateSimModel();
        }

        public void StartUpdateSimModel()
        {
            
            if (_wabDataFroBotServerTransfer == null)
            {
                BuildWS();
            }
            else
            {
                _wabDataFroBotServerTransfer._webSocket.Connect();
            }
        }

        public void BuildWS()
        {
            ifUpDateSimModelFromWebSocket = true;
            string url = "ws://" + webSocketSerAddress + ":" + webSocketServerPort;
            _wabDataFroBotServerTransfer = new WebData(url);
            _wabDataFroBotServerTransfer.OpenWebSocket();
        }

        public void MotionWithPQ(List<float> pqList)
        {
            if (pqList.Count > 6)
            {
                for (int part_i = 1; part_i < any_robot.partList.Count; part_i++)
                {
                    float[] pqa = new float[7];
                    for (int i = 0; i < 7; i++)
                    {
                        pqa[i] = pqList[i + part_i * 7];
                    }
                    partNodeList[part_i].SetTransform(QuaternionToTransform(pqa));
                }
            }
            //motionFinished = true;
        }

        public void MotionWithPQ(float[] pqList)
        {
            if (robotLoadCompleted)
            {
                if (pqList.Length > 6)
                {
                    for (int part_i = 1; part_i < any_robot.partList.Count; part_i++)
                    {
                        float[] pqa = new float[7];
                        for (int i = 0; i < 7; i++)
                        {
                            pqa[i] = pqList[i + part_i * 7];
                        }
                        partNodeList[part_i].SetTransform(QuaternionToTransform(pqa));
                    }
                }
                motionFinished = true;
            }
           
        }

        void OnSelectionChanged(SelectionChangeArgs args)
        {

        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (renderView != null)
                renderView.Size = this.splitContainer1.Panel2.Size;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            renderView.ExecuteCommand("ShadeWithEdgeMode");
            renderView.ShowCoordinateAxis(true);
            renderView.SetPickMode((int)EnumPickMode.RF_Face);
            this.renderView.RequestDraw();
        }

        async Task Delay()
        {
            await Task.Delay(1000);
            Console.Write(11);
        }

        SceneNode node1;
        private void OpenRobotXml(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "XML File(*.xml;)|*.xml;|All Files(*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {

                any_robot.LoadRobot_Aris(dlg.FileName, true);
                
            }
            robotLoadCompleted = false;
            ShowAnyRobot();
        }

        private void ShowAnyRobot()
        {

            //this.treeViewStp.Nodes.Clear();
            //this.renderView.ClearScene();
            for (int part_i = 0; part_i < any_robot.partList.Count; part_i++)
            {
                GroupSceneNode onePartNode = new GroupSceneNode();
                //Console.Write(onePartNode.GetTransform().GetTranslation().ToString());
                for (int geo_i = 0; geo_i < any_robot.partList[part_i].geometryPathList.Count; geo_i++)
                {
                    string relPath = any_robot.partList[part_i].geometryPathList[geo_i];
                    string absPath = relPath;
                    if (relPath.Contains(":"))
                    {
                        absPath = relPath;
                    }
                    else
                    {
                        absPath = path + relPath;
                    }
                    TopoShape geo = GlobalInstance.BrepTools.LoadFile(new AnyCAD.Platform.Path(absPath));
                    SceneNode oneGeoNode = renderView.ShowGeometry(geo, part_i);
                    float[] oneGeoPq = new float[7];
                    for (int i = 0; i < 7; i++)
                    {
                        oneGeoPq[i] = any_robot.partList[part_i].geometryPQ_List[i + geo_i * 7];
                        // oneGeoPq[i] = any_robot.partList[part_i].partPQ_initial[i];
                    }
                    oneGeoNode.SetTransform(QuaternionToTransform(oneGeoPq));
                    renderView.RequestDraw();
                    geoNodeList.Add(oneGeoNode);
                    onePartNode.AddNode(oneGeoNode);
                    renderView.SceneManager.RemoveNode(oneGeoNode);
                }
                float[] onePartPq = new float[7];
                for (int j = 0; j < 7; j++)
                {
                    onePartPq[j] = any_robot.partList[part_i].partPQ_initial[j];
                }
                onePartNode.SetTransform(QuaternionToTransform(onePartPq));
                renderView.SceneManager.AddNode(onePartNode);
                partNodeList.Add(onePartNode);
            }
            renderView.SceneManager.AddNode(robot_node);
            robot_node.SetPickable(false);
            renderView.RequestDraw();
            robotLoadCompleted = true;
            for (int k = 0; k < partNodeList.Count; k++)
            {
                Console.WriteLine("part" + k + partNodeList[k].GetTransform().GetTranslation().ToString());
            }
            for (int k = 0; k < geoNodeList.Count; k++)
            {
                Console.WriteLine("geo" + k + geoNodeList[k].GetTransform().GetTranslation().ToString());
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "STEP File(*.stp;*.step)|*.stp;*.step|All Files(*.*)|*.*";
            if (DialogResult.OK == dlg.ShowDialog())
            {
                TopoShape a = GlobalInstance.BrepTools.LoadFile(new AnyCAD.Platform.Path(dlg.FileName));
                node1 = renderView.ShowGeometry(a, 0);
                node1.SetPickable(true);
                //float[] oneGeoPq = new float[7] { 0.03f, 0.43f, 0.07f, 0, 0, 0, 1 };
                float[] oneGeoPq = new float[7] { 0.03f, 0.46f, 0.018f, 0, 0, 0, 1 };
                MatrixBuilder mb = new MatrixBuilder();
                Matrix4 mat1 = mb.Multiply(QuaternionToTransform(oneGeoPq),mb.MakeRotation(-90,new Vector3(1,0,0)));
                node1.SetTransform(mat1);
            }
            renderView.SetPickMode((int)(EnumPickMode.RF_Default));


            ////renderView.SetPickMode((int)(EnumPickMode.RF_Vertex));
            //OpenFileDialog dlg = new OpenFileDialog();
            //dlg.Filter = "STEP (*.stp;*.step)|*.stp;*.step|All Files(*.*)|*.*";

            //if (DialogResult.OK != dlg.ShowDialog())

            //    return;

            //AnyCAD.Exchange.ShowShapeReaderContext context = new AnyCAD.Exchange.ShowShapeReaderContext(renderView.SceneManager);
            ////context.NextShapeId = ++ shapeId;
            //AnyCAD.Exchange.StepReader reader = new AnyCAD.Exchange.StepReader();
            //reader.Read(dlg.FileName, context);

            ////shapeId = context.NextShapeId + 1;
            //renderView.RequestDraw(EnumRenderHint.RH_LoadScene);
        }

        private void pickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool res = renderView.ExecuteCommand("Pick");
        }

        List<double[]> pathPtList = new List<double[]>();
        List<double[]> pathPqList = new List<double[]>();
        private void queryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectedShapeQuery context = new SelectedShapeQuery();
            renderView.QuerySelection(context);
            
            TopoShape subShape = context.GetSubGeometry();
            SceneNode topoNode = context.GetSubNode();


            if (subShape != null)
            {
                Console.WriteLine(subShape.GetShapeType());
            }

            Matrix4 shapeTransform = topoNode.GetTransform();
            //surface
            GeomSurface surface = new GeomSurface();
            if (surface.Initialize(subShape))
            {
                List<Vector3> ptVecList = new List<Vector3>();
                List<Vector3> norVecList = new List<Vector3>();
                Console.Write("surface");
                //double firstU = surface.FirstUParameter();
                //double lastU = surface.LastUParameter();
                //double firstV = surface.FirstVParameter();
                //double lastV = surface.LastVParameter();
                double firstU = surface.FirstUParameter();
                double lastU = surface.LastUParameter();
                double firstV = surface.FirstVParameter();
                double lastV = surface.LastVParameter();
                double offSetU = lastU - firstU;
                double offSetV = lastV - firstV;

                double stepU = 0.10;
                double stepV = 10;
                int stepNoU = (int)(offSetU / stepU);
                int stepNoV = (int)(offSetV / stepV);
                for (int v_i = 3; v_i < stepNoV-5; v_i++)
                {
                    for (int u_i = 0; u_i < stepNoU; u_i++)
                    {
                        double tempV = firstV + stepV * v_i;
                        double tempU = firstU + stepU * (v_i % 2 == 0 ? u_i : stepNoU - u_i);
                        //double tempV = firstV + stepV * (u_i % 2 == 0 ? v_i : stepNoV - v_i);
                        Vector3 ptVec_1 = surface.Value(tempU, tempV);
                        Vector3 ptVec = shapeTransform.Transform(ptVec_1);
                        Vector3 normalVec_1 = surface.GetNormal(tempU, tempV);
                        //Vector3 normalVec =shapeTransform.Transform(normalVec_1);//matrix3  3*3
                        Vector3 normalVec = RotateDirVector(shapeTransform, normalVec_1);
                        ptVecList.Add(ptVec);
                        norVecList.Add(normalVec);
                        pathPqList.Add(QuaternionFromTo(new Vector3(-1, 0, 0), normalVec, ptVec));
                        ShowStatusMessage("path pts No: " + pathPqList.Count);

                        //LineNode tempLineNode = new LineNode();
                        //LineStyle lineStyle = new LineStyle();
                        //lineStyle.SetPatternStyle((int)EnumLinePattern.LP_DashedLine);
                        //lineStyle.SetColor(100, 0, 100);
                        //tempLineNode.SetLineStyle(lineStyle);
                        //tempLineNode.Set(ptVec, ptVec + normalVec);
                        //tempLineNode.SetVisible(true);
                        //renderView.SceneManager.AddNode(tempLineNode);
                        //renderView.RequestDraw();
                    }
                }

                //for (int u_i = 0; u_i < stepNoU; u_i++)
                //{
                //    for (int v_i = 0; v_i < stepNoV-0; v_i++)
                //    {

                //        double tempU = firstU + stepU * u_i;
                //        double tempV = firstV + stepV * (u_i % 2 == 0 ? v_i : stepNoV - v_i);

                //        Vector3 ptVec =shapeTransform.Transform(surface.Value(tempU,tempV ));
                //        Vector3 normalVec = surface.GetNormal(tempU,tempV);
                //        ptVecList.Add(ptVec);
                //        norVecList.Add(normalVec);
                //        pathPqList.Add(QuaternionFromTo(new Vector3(-1, 0, 0), normalVec, ptVec));
                //    }
                //}
                int a = 0;
            }
            //curve
            GeomCurve curve = new GeomCurve();
            if (curve.Initialize(subShape))
            {
                Vector3 startPt = shapeTransform.Transform(curve.D0(curve.FirstParameter()));
                //Vector3 startPt_ = shapeTransform.Transform(startPt);
                Vector3 pt1 = curve.GetStartPoint();
                Vector3 endPt = shapeTransform.Transform(curve.D0(curve.LastParameter()));
                Vector3 pt2 = curve.GetEndPoint();
                switch ((EnumCurveType)curve.GetCurveType())
                {
                    case EnumCurveType.CurveType_OtherCurve:
                        Console.Write("other");
                        break;
                    case EnumCurveType.CurveType_BSplineCurve:
                        break;
                    case EnumCurveType.CurveType_BezierCurve:
                        break;
                    case EnumCurveType.CurveType_Parabola:
                        break;
                    case EnumCurveType.CurveType_Hyperbola:
                        break;
                    case EnumCurveType.CurveType_Ellipse:
                        break;
                    case EnumCurveType.CurveType_Circle:
                        Console.Write("Circle");
                        break;
                    case EnumCurveType.CurveType_Line:
                        Console.Write("Line");

                        //path
                        double[] startPt_ = new double[3] { startPt.X, startPt.Y, startPt.Z };
                        double[] endPt_ = new double[3] { endPt.X, endPt.Y, endPt.Z };
                        Path_U.Interpolation(startPt_, endPt_, 0.01, ref pathPtList);
                        //show pick result
                        LineNode tempLineNode = new LineNode();
                        LineStyle lineStyle = new LineStyle();
                        lineStyle.SetPatternStyle((int)EnumLinePattern.LP_DashedLine);
                        lineStyle.SetColor(100, 0, 100);
                        tempLineNode.SetLineStyle(lineStyle);
                        tempLineNode.Set(new Vector3(startPt.X + 0.1, startPt.Y + 10, startPt.Z + 0.1), endPt);
                        tempLineNode.SetVisible(true);
                        renderView.SceneManager.AddNode(tempLineNode);
                        renderView.RequestDraw();
                        break;
                    default:
                        break;
                }



                ElementId id = context.GetNodeId();
                MessageBox.Show(id.AsInt().ToString());
                //...
            }

        }

        private void CMD_textBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void CMD_textBox_Leave(object sender, EventArgs e)
        {
            
        }

        private void EnqueueMessage(string mess)
        {
            if (_wabDataFroBotServerTransfer == null)
            {
                toolStripStatusLabel1.Text = "Server Not Connected!";
                statusStrip1.Refresh();
            }
            else
            {
                _wabDataFroBotServerTransfer.msgOutQueue.Enqueue(mess);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //_wabDataFroBotServerTransfer._webSocket.Send(OperateBotCmd("0&1&0&0&0&" + "moveJI --pq={3.656989e-001,-2.955130e-002,3.860146e-001,3.814761e-001,-2.151443e-002,-8.130945e-002,9.205443e-001} -v={1,1,1,1,1,1} -a={1,1,1,1,1,1} -d={1,1,1,1,1,1} --not_check_vel"));
            //_wabDataFroBotServerTransfer._webSocket.Send(OperateBotCmd("0&1&0&0&0&" + "moveJI --pq={2.217722e-001,-2.313713e-001,2.584152e-001,3.534221e-001,-1.462628e-001,-3.536169e-001,8.536130e-001} -v={1,1,1,1,1,1} -a={1,1,1,1,1,1} -d={1,1,1,1,1,1} --not_check_vel"));
            //_wabDataFroBotServerTransfer._webSocket.Send(OperateBotCmd("0&1&0&0&0&" + "moveJI --pq={2.255298e-001,-2.351491e-001,2.251460e-001,3.535753e-001,-1.464193e-001,-3.535308e-001,8.535584e-001}  --not_check_vel"));
            //_wabDataFroBotServerTransfer._webSocket.Send(OperateBotCmd("0&1&0&0&0&" + "moveJI --pq={2.806871e-001,-2.902937e-001,2.251375e-001,3.535740e-001,-1.464201e-001,-3.535329e-001,8.535579e-001}  --not_check_vel"));
            //_wabDataFroBotServerTransfer._webSocket.Send(OperateBotCmd("0&1&0&0&0&" + "moveJI --pq={3.656989e-001,-2.955130e-002,3.860146e-001,3.814761e-001,-2.151443e-002,-8.130945e-002,9.205443e-001} -v={1,1,1,1,1,1} -a={1,1,1,1,1,1} -d={1,1,1,1,1,1}  --not_check_vel"));


            //foreach (double[] pt in pathPtList)
            //{

            //    double[] newPQ = new double[7] { pt[0] / 1000, pt[1] / 1000, pt[2] / 1000, 0, 0, 0, 1 };
            //    string botCmd = "0&1&0&0&0&" + "moveJI --pq={" + newPQ[0].ToString("e") + "," + newPQ[1].ToString("e") + "," + newPQ[2].ToString("e") + "," + newPQ[3].ToString("e") + "," + newPQ[4].ToString("e") + "," + newPQ[5].ToString("e") + "," + newPQ[6].ToString("e") + "}";
            //    EnqueueMessage(botCmd);
            //    //_wabDataFroBotServerTransfer.msgOutQueue.Enqueue(botCmd);

            //}

            //foreach (double[] newPQ in pathPqList)
            for (int pq_i = 0; pq_i < pathPqList.Count; pq_i++)
            {
                double[] newPQ = pathPqList[pq_i];
                string botCmd = "0&1&0&0&0&" + "moveJI --pq={" + newPQ[0].ToString("e") + "," + newPQ[1].ToString("e") + "," + (newPQ[2]+0.02).ToString("e") + "," + newPQ[3].ToString("e") + "," + newPQ[4].ToString("e") + "," + newPQ[5].ToString("e") + "," + newPQ[6].ToString("e") + "} --vel=0.05";
                object syncObj = new object();
                lock (syncObj)
                {
                    EnqueueMessage(botCmd);
                }
            }

            //StringBuilder cmdStringBuilder = new StringBuilder();
            //foreach (double[] newPQ in pathPqList)
            //{
            //    cmdStringBuilder.Append(newPQ[0].ToString("e") + "," + newPQ[1].ToString("e") + "," + newPQ[2].ToString("e") + "," + newPQ[3].ToString("e") + "," + newPQ[4].ToString("e") + "," + newPQ[5].ToString("e") + "," + newPQ[6].ToString("e") + ";");
            //}
            //string cmdStr = "0&1&0&0&0&" + "FMovePath --pq={" + cmdStringBuilder.ToString() + "} --runtime=8";
            //EnqueueMessage(cmdStr);

            //for (int pq_i = 0; pq_i < 1; pq_i++)
            //{
            //    double[] newPQ = pathPqList[pq_i];
            //    string botCmd = "0&1&0&0&0&" + "movePQB --pqt={" + newPQ[0].ToString("e") + "," + newPQ[1].ToString("e") + "," + (newPQ[2]).ToString("e") + "," + newPQ[3].ToString("e") + "," + newPQ[4].ToString("e") + "," + newPQ[5].ToString("e") + "," + newPQ[6].ToString("e") + "}";
            //    object syncObj = new object();
            //    lock (syncObj)
            //    {
            //        EnqueueMessage(botCmd);
            //    }
            //}
            //EnqueueMessage("0&1&0&0&0&" + "moveSPQ --which_func=7");
        }

        void ShowStatusMessage(string mess)
        {
            toolStripStatusLabel1.Text = mess;
            statusStrip1.Refresh();
        }

        private void moveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            renderView.ExecuteCommand("MoveNode");
        }

        /// <summary>
        /// 对方向向量进行变换
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        Vector3 RotateDirVector(Matrix4 matrix, Vector3 direction)
        {
            Vector3 newDir = new Vector3();
            newDir.X = matrix.m[0, 0] * direction.X + matrix.m[0, 1] * direction.Y + matrix.m[0, 2] * direction.Z;
            newDir.Y = matrix.m[1, 0] * direction.X + matrix.m[1, 1] * direction.Y + matrix.m[1, 2] * direction.Z;
            newDir.Z = matrix.m[2, 0] * direction.X + matrix.m[2, 1] * direction.Y + matrix.m[2, 2] * direction.Z;
            return newDir;
        }

        /// <summary>
        /// 位姿矩阵的逆
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        Matrix4 TransformReverse(Matrix4 mat)
        {
            Matrix4 newMat = new Matrix4();
            newMat.m[0, 0] = mat.m[0, 0];
            newMat.m[0, 1] = mat.m[1, 0];
            newMat.m[0, 2] = mat.m[2, 0];
            newMat.m[1, 0] = mat.m[0, 1];
            newMat.m[1, 1] = mat.m[1, 1];
            newMat.m[1, 2] = mat.m[2, 1];
            newMat.m[2, 0] = mat.m[0, 2];
            newMat.m[2, 1] = mat.m[1, 2];
            newMat.m[2, 2] = mat.m[2, 2];

            newMat.m[3, 0] = 0;
            newMat.m[3, 1] = 0;
            newMat.m[3, 2] = 0;
            newMat.m[3, 3] = 1;

            newMat.m[0, 3] = (mat.m[0, 3] * mat.m[0, 0] + mat.m[1, 3] * mat.m[1, 0] + mat.m[2, 3] * mat.m[2, 0]) * -1;
            newMat.m[1, 3] = (mat.m[0, 3] * mat.m[0, 1] + mat.m[1, 3] * mat.m[1, 1] + mat.m[2, 3] * mat.m[2, 1]) * -1;
            newMat.m[2, 3] = (mat.m[0, 3] * mat.m[0, 2] + mat.m[1, 3] * mat.m[1, 2] + mat.m[2, 3] * mat.m[2, 2]) * -1;

            return newMat;

        }

        /// <summary>
        /// 三个不共线的点构成的坐标系
        /// </summary>
        /// <param name="pt1">origin</param>
        /// <param name="pt2">x</param>
        /// <param name="pt3"></param>
        /// <returns></returns>
        Matrix4 MatrixByTriPoints(Vector3 pt1,Vector3 pt2,Vector3 pt3)
        {
            Vector3 x_dir = pt2 - pt1;
            x_dir.Normalize();
            Vector3 dir_1 = pt3 - pt1;
            dir_1.Normalize();
            Vector3 z_dir = x_dir.CrossProduct(dir_1);
            Vector3 y_dir = x_dir.CrossProduct(z_dir);
            MatrixBuilder builder = new MatrixBuilder();
            return builder.ToWorldMatrix(new Coordinate3(pt1, x_dir, y_dir, z_dir));
        }

        /// <summary>
        /// 通过三个点的原始位置和校准位置对工件坐标系进行标定
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="pt3"></param>
        /// <param name="pt1_cor"></param>
        /// <param name="pt2_cor"></param>
        /// <param name="pt3_cor"></param>
        /// <returns></returns>
        Matrix4 TransformCorrect(Vector3 pt1,Vector3 pt2,Vector3 pt3,Vector3 pt1_cor,Vector3 pt2_cor,Vector3 pt3_cor)
        {
            Matrix4 mat_1 = MatrixByTriPoints(pt1, pt2, pt3);
            Matrix4 mat_2 = MatrixByTriPoints(pt1_cor, pt2_cor, pt3_cor);
            Matrix4 mat_1_rev = TransformReverse(mat_1);
            MatrixBuilder builder = new MatrixBuilder();
            return builder.Multiply(mat_1_rev, mat_2);
        }

        /// <summary>
        /// vecFrom rotate to vecTo 
        /// </summary>
        /// <param name="vecFrom"></param>
        /// <param name="vecTo"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        double[] QuaternionFromTo(Vector3 vecFrom, Vector3 vecTo,Vector3 pt)
        {
            double length_1 = vecFrom.Normalize();
            double length_2 = vecTo.Normalize();
            Vector3 half = (vecFrom + vecTo);
            half.Normalize();
            if ((vecFrom+vecTo).Length()==0)
            {
                return new double[7] {pt.X,pt.Y,pt.Z, 0, 0, 0, 1 };
            }
            else
            {
                double q1 = vecFrom.X * half.X + vecFrom.Y * half.Y + vecFrom.Z * half.Z;
                Vector3 cross = vecFrom.CrossProduct(half);
                return new double[7] { pt.X/1000, pt.Y/1000, pt.Z/1000, cross.X, cross.Y, cross.Z, q1 };
            }
        }

        float[] TransformToQuaternion(float[] transform)
        {
            return null;
        }

        Matrix4 QuaternionToTransform(float[] pq)
        {

            //float q0 = pq[3];
            //float q1 = pq[4] * 1;
            //float q2 = pq[5] * 1;
            //float q3 = pq[6];
            float q0 = pq[6];
            float q1 = pq[3] * 1;
            float q2 = pq[4] * 1;
            float q3 = pq[5];
            float[] rot = new float[16];
            rot[0] = 1 - 2 * q2 * q2 - 2 * q3 * q3;
            rot[1] = 2 * q1 * q2 - 2 * q0 * q3;
            rot[2] = 2 * q1 * q3 + 2 * q0 * q2;
            rot[4] = 2 * q1 * q2 + 2 * q0 * q3;
            rot[5] = 1 - 2 * q1 * q1 - 2 * q3 * q3;
            rot[6] = 2 * q2 * q3 - 2 * q0 * q1;
            rot[8] = 2 * q1 * q3 - 2 * q0 * q2;
            rot[9] = 2 * q2 * q3 + 2 * q0 * q1;
            rot[10] = 1 - 2 * q1 * q1 - 2 * q2 * q2;

            rot[3] = pq[0] * 1000;
            rot[7] = pq[1] * 1000;
            rot[11] = pq[2] * 1000;
            rot[12] = 0;
            rot[13] = 0;
            rot[14] = 0;
            rot[15] = 1;
            
            Matrix4 trf = new Matrix4();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    trf.m[i, j] = rot[i * 4 + j];
                }
            }
            //Matrix4 trf_1 = GlobalInstance.MatrixBuilder.MakeTranslate(new Vector3(pq[0], pq[1], pq[2]));
            //return GlobalInstance.MatrixBuilder.Multiply(trf, trf_1);
            return trf;
        }

        void UpdateModel(object source, System.Timers.ElapsedEventArgs e)
        {
            Matrix4 tr = robot_node.GetTransform();
            Matrix4 tr_1 = GlobalInstance.MatrixBuilder.MakeTranslate(0.1, 0.1, 0.1);
            Matrix4 tr_2 = GlobalInstance.MatrixBuilder.Multiply(tr, tr_1);
            robot_node.SetTransform(tr_2);
            //Console.WriteLine(robot_node.GetTransform().GetTranslation().ToString());

            renderView.RequestDraw();

        }

        void test()
        {
            System.Timers.Timer t = new System.Timers.Timer(100);//实例化Timer类，设置时间间隔
            t.Elapsed += new System.Timers.ElapsedEventHandler(Method2);//到达时间的时候执行事件
            t.AutoReset = true;//设置是执行一次（false）还是一直执行(true)
            t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件
        }

        void Method2(object source, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine(DateTime.Now.ToString() + "_" + Thread.CurrentThread.ManagedThreadId.ToString());
        }

        //private void openToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    OpenFileDialog dlg = new OpenFileDialog();
        //    dlg.Filter = "STEP File(*.stp;*.step)|*.stp;*.step|All Files(*.*)|*.*";

        //    if (DialogResult.OK == dlg.ShowDialog())
        //    {
        //        this.treeViewStp.Nodes.Clear();
        //        this.renderView.ClearScene();

        //        CADBrower browser = new CADBrower(this.treeViewStp, this.renderView);
        //        AnyCAD.Exchange.StepReader reader = new AnyCAD.Exchange.StepReader();
        //        reader.Read(dlg.FileName, browser);
        //    }
        //    renderView.FitAll();
        //}

        private void treeViewStp_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void openIGESToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "IGES File(*.iges;*.igs)|*.igs;*.igesp|All Files(*.*)|*.*";

            if (DialogResult.OK == dlg.ShowDialog())
            {
                this.treeViewStp.Nodes.Clear();
                this.renderView.ClearScene();

                CADBrower browser = new CADBrower(this.treeViewStp, this.renderView);
                AnyCAD.Exchange.IgesReader reader = new AnyCAD.Exchange.IgesReader();
                reader.Read(dlg.FileName, browser);
            }

            renderView.View3d.FitAll();
        }

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Image File(*.png;*.jpg)|*.png;*.jpg|All Files(*.*)|*.*";

            if (DialogResult.OK == dlg.ShowDialog())
            {
                renderView.CaptureImage(dlg.FileName);
            }
        }

        private void MainForm_Load_1(object sender, EventArgs e)
        {

        }

        private void ShowPick()
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            webSocketSerAddress = textBox4.Text;
            ShowStatusMessage("IP: " + webSocketSerAddress);
            BuildWS();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string cmdStr = this.CMD_textBox.Text;
            EnqueueMessage("0&1&0&0&0&" + cmdStr + "\n");

        }
    }


}
