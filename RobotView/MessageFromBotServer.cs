using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotView
{
    public  class MessageFromBotServer
    {
        public Queue<byte[]> byteFromBotServer = new Queue<byte[]>();
        int bufferLength = 0;
        byte[] bufferBetween = new byte[1024];
        //List<byte[]> buffer = new List<byte[]>();

        public string ProcessMessQue()//处理bot server传过来的数据
        {

            byte[] tempByte = byteFromBotServer.Dequeue();//提取处理队列
            byte[] processByte = new byte[bufferLength + tempByte.Length];
            if (bufferLength > 0)
            {
                Array.Copy(bufferBetween, 0, processByte, 0, bufferLength);//将未处理的数据拷入
            }

            Array.Copy(tempByte, 0, processByte, bufferLength, tempByte.Length);//将新数据拷入
            if (processByte.Length <= 40)//如果长度小于40，直接存入buffer
            {
                bufferLength = processByte.Length;
                Array.Copy(processByte, 0, bufferBetween, 0, processByte.Length);
                return null;
            }
            else
            {
                int length = BitConverter.ToInt32(processByte, 0);
                if (processByte.Length < length + 40)//长度不够，数据不完整，存入buffer
                {
                    bufferLength = processByte.Length;
                    Array.Copy(processByte, 0, bufferBetween, 0, processByte.Length);
                    return null;
                }
                else //长度足够，处理数据
                {
                    string output = Encoding.Default.GetString(processByte, 40, length);
                    if (processByte.Length == length + 40)//长度刚好，buffer 置零
                    {
                        bufferLength = 0;
                    }
                    else
                    {
                        bufferLength = processByte.Length - (length + 40);
                        Array.Copy(processByte, (length + 40), bufferBetween, 0, bufferLength);
                    }
                    return output;
                }

            }
        }

        public List<float> ProcessBiteQue()//处理bot server传过来的二进制数据
        {

            byte[] tempByte = byteFromBotServer.Dequeue();//提取处理队列
            byte[] processByte = new byte[bufferLength + tempByte.Length];
            if (bufferLength > 0)
            {
                Array.Copy(bufferBetween, 0, processByte, 0, bufferLength);//将未处理的数据拷入
            }

            Array.Copy(tempByte, 0, processByte, bufferLength, tempByte.Length);//将新数据拷入
            if (processByte.Length <= 40)//如果长度小于40，直接存入buffer
            {
                bufferLength = processByte.Length;
                Array.Copy(processByte, 0, bufferBetween, 0, processByte.Length);
                return null;
            }
            else
            {
                int length = BitConverter.ToInt32(processByte, 0);
                //Debug.Log("length:" + length);
                if (processByte.Length < length + 40)//长度不够，数据不完整，存入buffer
                {
                    bufferLength = processByte.Length;
                    Array.Copy(processByte, 0, bufferBetween, 0, processByte.Length);
                    return null;
                }
                else //长度足够，处理数据
                {
                    //string output = Encoding.Default.GetString(processByte, 40, length);

                    if (processByte.Length == length + 40)//长度刚好，buffer 置零
                    {
                        bufferLength = 0;
                    }
                    else
                    {
                        bufferLength = processByte.Length - (length + 40);
                        Array.Copy(processByte, (length + 40), bufferBetween, 0, bufferLength);
                    }
                    if (length > 0)
                    {
                        List<float> output = new List<float>();
                        for (int i = 0; i < 49; i++)
                        {
                            double data = BitConverter.ToDouble(processByte, 40 + i * 8);
                            output.Add((float)data);
                        }
                        return output;
                    }
                    else
                    {
                        return null;
                    }

                }


            }

        }

        public List<float> ProcessBiteQue(int partNum)//处理bot server传过来的二进制数据
        {

            byte[] tempByte = byteFromBotServer.Dequeue();//提取处理队列
            if (tempByte == null)
            {
                return null;
            }
            byte[] processByte = new byte[bufferLength + tempByte.Length];
            if (bufferLength > 0)
            {
                Array.Copy(bufferBetween, 0, processByte, 0, bufferLength);//将未处理的数据拷入
            }

            Array.Copy(tempByte, 0, processByte, bufferLength, tempByte.Length);//将新数据拷入
            if (processByte.Length <= 40)//如果长度小于40，直接存入buffer
            {
                bufferLength = processByte.Length;
                Array.Copy(processByte, 0, bufferBetween, 0, processByte.Length);
                return null;
            }
            else
            {
                int length = BitConverter.ToInt32(processByte, 0);
                //Debug.Log("length:" + length);
                if (processByte.Length < length + 40)//长度不够，数据不完整，存入buffer
                {
                    bufferLength = processByte.Length;
                    Array.Copy(processByte, 0, bufferBetween, 0, processByte.Length);
                    return null;
                }
                else //长度足够，处理数据
                {
                    //string output = Encoding.Default.GetString(processByte, 40, length);

                    if (processByte.Length == length + 40)//长度刚好，buffer 置零
                    {
                        bufferLength = 0;
                    }
                    else
                    {
                        bufferLength = processByte.Length - (length + 40);
                        Array.Copy(processByte, (length + 40), bufferBetween, 0, bufferLength);
                    }
                    if (length > 0)
                    {
                        List<float> output = new List<float>();
                        int dataCount = partNum * 7;
                        for (int i = 0; i < dataCount; i++)
                        {
                            double data = BitConverter.ToDouble(processByte, 40 + i * 8);
                            output.Add((float)data);
                            Console.WriteLine(data + ",");
                        }
                        Console.WriteLine("\n");
                        return output;
                    }
                    else
                    {
                        return null;
                    }

                }


            }

        }

    }
}
