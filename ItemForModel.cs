namespace WinFormsApp1;

public class ItemForModel
{
    public string szProductID { get; set; }

    //public CameraHandle hCamera;
    //public IntPtr pbyImageBuffer;
    public byte[] pbyImageBuffer{ get; set; }
    public int Width{ get; set; }
    public int Height{ get; set; }
    // public tSdkFrameHead pFrameInfo;
    //-------------------------------------------------------------------------------
    public byte bSaved{ get; set; }         //是否已经被存储过
    public byte bProcessed{ get; set; }     //是否已经被模型处理过
    //-------------------------------------------------------------------------------
    public int nTabImageID{ get; set; }    //帧唯一编号
    public byte[] rangeBuffer{ get; set; }
    public byte[] intensityBuffer{ get; set; }
    public int rows{ get; set; }
    public int cols{ get; set; }
    public float xScale{ get; set; }
    public float xOffset{ get; set; }
    public float yScale{ get; set; }
    public float yOffset{ get; set; }
    public float zScale{ get; set; }
    public float zOffset{ get; set; }
    //-------------------------------------------------------------------------------
    //返回结果
    public string strDetectFileName{ get; set; } // 时效性要求，返回较困难
    public bool strReturn{ get; set; }       // 返回值，错误，结果-无缺陷false，有缺陷true
    public string errMsg { get; set; }       // Exception异常信息, 如果抛出异常，异常信息则会存储在此
    public int nDetectNum{ get; set; }       // 缺陷数量, 无缺陷false时，该值为空
    public List<TDDetect> defects{ get; set; } // 缺陷信息, 无缺陷false时，该值为空
}