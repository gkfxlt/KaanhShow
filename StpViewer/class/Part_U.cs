using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 
/// </summary>
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

    //public void ResetToInitial()
    //{
    //    GameObject partObj = GameObject.Find(partObjName);
    //    partObj.transform.position = new Vector3(partTransform_6_initial[0], partTransform_6_initial[1], partTransform_6_initial[2]);
    //    partObj.transform.eulerAngles = new Vector3(partTransform_6_initial[3], partTransform_6_initial[4], partTransform_6_initial[5]);
    //}
    //public void StoreConfiguration()
    //{
    //    GameObject partObj = GameObject.Find(partObjName);
    //    partTransform_6.Clear();
    //    partTransform_6.Add(partObj.transform.position.x);
    //    partTransform_6.Add(partObj.transform.position.y);
    //    partTransform_6.Add(partObj.transform.position.z);
    //    partTransform_6.Add(partObj.transform.eulerAngles.x);
    //    partTransform_6.Add(partObj.transform.eulerAngles.y);
    //    partTransform_6.Add(partObj.transform.eulerAngles.z);

    //    Matrix4x4 transMatrix = partObj.transform.localToWorldMatrix;
    //    float[] transMatrixData = new float[16];
    //    partTransform.Clear();
    //    partTransform.Add(transMatrix[0, 0]);
    //    partTransform.Add(transMatrix[0, 1]);
    //    partTransform.Add(transMatrix[0, 2]);
    //    partTransform.Add(transMatrix[0, 3]);

    //    partTransform.Add(transMatrix[1, 0]);
    //    partTransform.Add(transMatrix[1, 1]);
    //    partTransform.Add(transMatrix[1, 2]);
    //    partTransform.Add(transMatrix[1, 3]);

    //    partTransform.Add(transMatrix[2, 0]);
    //    partTransform.Add(transMatrix[2, 1]);
    //    partTransform.Add(transMatrix[2, 2]);
    //    partTransform.Add(transMatrix[2, 3]);

    //    partTransform.Add(transMatrix[3, 0]);
    //    partTransform.Add(transMatrix[3, 1]);
    //    partTransform.Add(transMatrix[3, 2]);
    //    partTransform.Add(transMatrix[3, 3]);
    //    //partTransform[0] = transMatrix[0, 0];
    //    //partTransform[1] = transMatrix[0, 1];
    //    //partTransform[2] = transMatrix[0, 2];
    //    //partTransform[3] = transMatrix[0, 3];

    //    //partTransform[4] = transMatrix[1, 0];
    //    //partTransform[5] = transMatrix[1, 1];
    //    //partTransform[6] = transMatrix[1, 2];
    //    //partTransform[7] = transMatrix[1, 3];

    //    //partTransform[8] = transMatrix[2, 0];
    //    //partTransform[9] = transMatrix[2, 1];
    //    //partTransform[10] = transMatrix[2, 2];
    //    //partTransform[11] = transMatrix[2, 3];

    //    //partTransform[12] = transMatrix[3, 0];
    //    //partTransform[13] = transMatrix[3, 1];
    //    //partTransform[14] = transMatrix[3, 2];
    //    //partTransform[15] = transMatrix[3, 3];

    //}
    //public void StoreInitialConfiguration()
    //{
    //    GameObject partObj = GameObject.Find(partObjName);
    //    partTransform_6_initial.Clear();
    //    partTransform_6_initial.Add(partObj.transform.position.x);
    //    partTransform_6_initial.Add(partObj.transform.position.y);
    //    partTransform_6_initial.Add(partObj.transform.position.z);
    //    partTransform_6_initial.Add(partObj.transform.eulerAngles.x);
    //    partTransform_6_initial.Add(partObj.transform.eulerAngles.y);
    //    partTransform_6_initial.Add(partObj.transform.eulerAngles.z);

    //    partPQ_initial.Clear();
    //    partPQ_initial.Add(partObj.transform.position.x);
    //    partPQ_initial.Add(partObj.transform.position.y);
    //    partPQ_initial.Add(partObj.transform.position.z);
    //    partPQ_initial.Add(partObj.transform.rotation.x);
    //    partPQ_initial.Add(partObj.transform.rotation.y);
    //    partPQ_initial.Add(partObj.transform.rotation.z);
    //    partPQ_initial.Add(partObj.transform.rotation.w);
    //    Debug.Log(partPQ_initial.ToString());

    //}

}

