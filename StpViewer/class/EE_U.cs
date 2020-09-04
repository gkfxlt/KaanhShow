using System.Collections;
using System.Collections.Generic;


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

    //public void StoreInitialConfiguration()
    //{
    //    GameObject eeObj = GameObject.Find(eeObj_Name);
    //    eePQ_initial.Add(eeObj.transform.position.x);
    //    eePQ_initial.Add(eeObj.transform.position.y);
    //    eePQ_initial.Add(eeObj.transform.position.z);
    //    eePQ_initial.Add(eeObj.transform.rotation.x);
    //    eePQ_initial.Add(eeObj.transform.rotation.y);
    //    eePQ_initial.Add(eeObj.transform.rotation.z);
    //    eePQ_initial.Add(eeObj.transform.rotation.w);
    //}
    //public void StoreConfiguration()
    //{
    //    GameObject EEObj = GameObject.Find(eeObj_Name);
    //    eeTransform_6.Clear();
    //    eeTransform_6.Add(EEObj.transform.position.x);
    //    eeTransform_6.Add(EEObj.transform.position.y);
    //    eeTransform_6.Add(EEObj.transform.position.z);
    //    eeTransform_6.Add(EEObj.transform.eulerAngles.x);
    //    eeTransform_6.Add(EEObj.transform.eulerAngles.y);
    //    eeTransform_6.Add(EEObj.transform.eulerAngles.z);

    //    Matrix4x4 transMatrix = EEObj.transform.localToWorldMatrix;
    //    eeTransform.Clear();
    //    float[] transMatrixData = new float[16];
    //    eeTransform.Add(transMatrix[0, 0]);
    //    eeTransform.Add(transMatrix[0, 1]);
    //    eeTransform.Add(transMatrix[0, 2]);
    //    eeTransform.Add(transMatrix[0, 3]);

    //    eeTransform.Add(transMatrix[1, 0]);
    //    eeTransform.Add(transMatrix[1, 1]);
    //    eeTransform.Add(transMatrix[1, 2]);
    //    eeTransform.Add(transMatrix[1, 3]);

    //    eeTransform.Add(transMatrix[2, 0]);
    //    eeTransform.Add(transMatrix[2, 1]);
    //    eeTransform.Add(transMatrix[2, 2]);
    //    eeTransform.Add(transMatrix[2, 3]);

    //    eeTransform.Add(transMatrix[3, 0]);
    //    eeTransform.Add(transMatrix[3, 1]);
    //    eeTransform.Add(transMatrix[3, 2]);
    //    eeTransform.Add(transMatrix[3, 3]);


    //}

}

