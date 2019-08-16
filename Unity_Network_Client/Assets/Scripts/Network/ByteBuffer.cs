using System;
using System.Collections.Generic;
using System.Text;
//using UnityEngine;

public class ByteBuffer : IDisposable
{
    private List<byte> Buff;
    private byte[] readBuff;
    private int readPos;
    private bool buffUpdate = false;

    public ByteBuffer()
    {
        Buff = new List<byte>();
        readPos = 0;
    }

    public int GetReadPos()
    {
        return readPos;
    }

    public byte[] ToArray()
    {
        return Buff.ToArray();
    }

    public int Count()
    {
        return Buff.Count;
    }

    public int Length()
    {
        return Count() - readPos;
    }

    public void Clear()
    {
        Buff.Clear();
        readPos = 0;
    }

    #region"Write Data"
    public void WriteByte(byte Input)
    {
        Buff.Add(Input);
        buffUpdate = true;
    }

    public void WriteBytes(byte[] Input)
    {
        Buff.AddRange(Input);
        buffUpdate = true;
    }

    public void WriteShort(short Input)
    {
        Buff.AddRange(BitConverter.GetBytes(Input));
        buffUpdate = true;
    }

    public void WriteInteger(int Input)
    {
        Buff.AddRange(BitConverter.GetBytes(Input));
        buffUpdate = true;
    }

    public void WriteLong(long Input)
    {
        Buff.AddRange(BitConverter.GetBytes(Input));
        buffUpdate = true;
    }

    public void WriteFloat(float Input)
    {
        Buff.AddRange(BitConverter.GetBytes(Input));
        buffUpdate = true;
    }

    public void WriteString(string Input)
    {
        Buff.AddRange(BitConverter.GetBytes(Input.Length));
        Buff.AddRange(Encoding.ASCII.GetBytes(Input));
        buffUpdate = true;
    }
    /*
    public void WriteVector3(Vector3 Input)
    {
        byte[] vectorArray = new byte[sizeof(float) * 3];

        Buffer.BlockCopy(BitConverter.GetBytes(Input.x), 0, vectorArray, 0 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(Input.y), 0, vectorArray, 1 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(Input.z), 0, vectorArray, 2 * sizeof(float), sizeof(float));

        Buff.AddRange(vectorArray);
        buffUpdate = true;
    }

    public void WriteQuaternion(Quaternion Input)
    {
        byte[] quaternionArray = new byte[sizeof(float) * 4];

        Buffer.BlockCopy(BitConverter.GetBytes(Input.x), 0, quaternionArray, 0 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(Input.y), 0, quaternionArray, 1 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(Input.z), 0, quaternionArray, 2 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(Input.w), 0, quaternionArray, 3 * sizeof(float), sizeof(float));

        Buff.AddRange(quaternionArray);
        buffUpdate = true;
    }*/
    #endregion

    #region"Read Data"
    public byte ReadByte(bool Peek = true)
    {
        if (Buff.Count > readPos)
        {
            if (buffUpdate)
            {
                readBuff = Buff.ToArray();
                buffUpdate = false;
            }

            byte ret = readBuff[readPos];
            if (Peek & Buff.Count > readPos)
            {
                readPos += 1;
            }
            return ret;
        }
        else
            throw new Exception("ByteBuffer [BYTE] is past its limit!");
    }

    public byte[] ReadBytes(int Length, bool Peek = true)
    {
        if (Buff.Count > readPos)
        {
            if (buffUpdate)
            {
                readBuff = Buff.ToArray();
                buffUpdate = false;
            }

            byte[] ret = Buff.GetRange(readPos, Length).ToArray();
            if (Peek & Buff.Count > readPos)
            {
                readPos += Length;
            }
            return ret;
        }
        else
            throw new Exception("ByteBuffer [BYTE[]] is past its limit!");
    }

    public short ReadShort(bool Peek = true)
    {
        if (Buff.Count > readPos)
        {
            if (buffUpdate)
            {
                readBuff = Buff.ToArray();
                buffUpdate = false;
            }

            short ret = BitConverter.ToInt16(readBuff, readPos);
            if (Peek & Buff.Count > readPos)
            {
                readPos += 2;
            }
            return ret;
        }
        else
            throw new Exception("ByteBuffer [SHORT] is past its limit!");
    }

    public int ReadInteger(bool Peek = true)
    {
        if (Buff.Count > readPos)
        {
            if (buffUpdate)
            {
                readBuff = Buff.ToArray();
                buffUpdate = false;
            }

            int ret = BitConverter.ToInt32(readBuff, readPos);
            if (Peek & Buff.Count > readPos)
            {
                readPos += 4;
            }
            return ret;
        }
        else
            throw new Exception("ByteBuffer [INT] is past its limit!");
    }

    public long ReadLong(bool Peek = true)
    {
        if (Buff.Count > readPos)
        {
            if (buffUpdate)
            {
                readBuff = Buff.ToArray();
                buffUpdate = false;
            }

            long ret = BitConverter.ToInt64(readBuff, readPos);
            if (Peek & Buff.Count > readPos)
            {
                readPos += 8;
            }
            return ret;
        }
        else
            throw new Exception("ByteBuffer [LONG] is past its limit!");
    }

    public float ReadFloat(bool Peek = true)
    {
        if (Buff.Count > readPos)
        {
            if (buffUpdate)
            {
                readBuff = Buff.ToArray();
                buffUpdate = false;
            }

            float ret = BitConverter.ToSingle(readBuff, readPos);
            if (Peek & Buff.Count > readPos)
            {
                readPos += 4;
            }
            return ret;
        }
        else
            throw new Exception("ByteBuffer [FLOAT] is past its limit!");
    }

    public string ReadString(bool Peek = true)
    {
        int length = ReadInteger(true);

        if (buffUpdate)
        {
            readBuff = Buff.ToArray();
            buffUpdate = false;
        }

        string ret = Encoding.ASCII.GetString(readBuff, readPos, length);
        if (Peek & Buff.Count > readPos)
        {
            readPos += length;
        }
        return ret;
    }
    /*
    public Vector3 ReadVector3(bool Peek = true)
    {
        if (buffUpdate)
        {
            readBuff = Buff.ToArray();
            buffUpdate = false;
        }

        byte[] ret = Buff.GetRange(readPos, sizeof(float) * 3).ToArray();
        Vector3 vector3;
        vector3.x = BitConverter.ToSingle(ret, 0 * sizeof(float));
        vector3.y = BitConverter.ToSingle(ret, 1 * sizeof(float));
        vector3.z = BitConverter.ToSingle(ret, 2 * sizeof(float));

        if (Peek)
        {
            readPos += sizeof(float) * 3;
        }
        return vector3;
    }

    public Quaternion ReadQuaternion(bool Peek = true)
    {
        if (buffUpdate)
        {
            readBuff = Buff.ToArray();
            buffUpdate = false;
        }

        byte[] ret = Buff.GetRange(readPos, sizeof(float) * 4).ToArray();
        Quaternion quaternion;
        quaternion.x = BitConverter.ToSingle(ret, 0 * sizeof(float));
        quaternion.y = BitConverter.ToSingle(ret, 1 * sizeof(float));
        quaternion.z = BitConverter.ToSingle(ret, 2 * sizeof(float));
        quaternion.w = BitConverter.ToSingle(ret, 3 * sizeof(float));

        if (Peek)
        {
            readPos += sizeof(float) * 4;
        }
        return quaternion;
    }*/

    #endregion

    private bool disposedValue = false;

    //IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            if (disposing)
            {
                Buff.Clear();
                readPos = 0;
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
