using System.Collections;
using System.Collections.Generic;

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
    //public void StoreConfiguration()
    //{
    //    jointTransform.Clear();
    //    Transform transs = GameObject.Find(jointObjName).transform;
    //    jointTransform.Add(transs.position.x);
    //    jointTransform.Add(transs.position.y);
    //    jointTransform.Add(transs.position.z);
    //    jointTransform.Add(transs.eulerAngles.x);
    //    jointTransform.Add(transs.eulerAngles.y);
    //    jointTransform.Add(transs.eulerAngles.z);
    //}
    //public void FindAxis()
    //{
    //    Transform axisObjTransform = GameObject.Find(axisObjName).transform;
    //    jointAxisPos.Clear();
    //    jointAxisPos.Add(axisObjTransform.position.x);
    //    jointAxisPos.Add(axisObjTransform.position.y);
    //    jointAxisPos.Add(axisObjTransform.position.z);

    //    jointAxisDir.Clear();
    //    switch (axisObjType)
    //    {
    //        case 1:
    //            jointAxisDir.Add(axisObjTransform.right.x);
    //            jointAxisDir.Add(axisObjTransform.right.y);
    //            jointAxisDir.Add(axisObjTransform.right.z);
    //            break;
    //        case 2:
    //            jointAxisDir.Add(axisObjTransform.up.x);
    //            jointAxisDir.Add(axisObjTransform.up.y);
    //            jointAxisDir.Add(axisObjTransform.up.z);
    //            break;
    //        case 3:
    //            jointAxisDir.Add(axisObjTransform.forward.x);
    //            jointAxisDir.Add(axisObjTransform.forward.y);
    //            jointAxisDir.Add(axisObjTransform.forward.z);
    //            break;
    //        default:
    //            break;
    //    }


    //}

}


