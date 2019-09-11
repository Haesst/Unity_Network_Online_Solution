using System;
using System.Collections.Generic;
using System.Text;

public class ByteBuffer : IDisposable
{
    private List<byte> buffer;
    private byte[] readBuffer;
    private int readPos;
    private bool bufferUpdate = false;

    public ByteBuffer()
    {
        buffer = new List<byte>(); // First thing first, create a new list with bytes
        readPos = 0; // Set the readPos to 0
    }

    public int GetReadPos()
    {
        return readPos;
    }

    public byte[] ToArray()
    {
        return buffer.ToArray();
    }

    public int Count()
    {
        return buffer.Count;
    }

    public int Length()
    {
        return buffer.Count - readPos;
    }

    public void Clear()
    {
        buffer.Clear(); // Clear the buffer.
        readPos = 0; // Reset readPos
    }

    #region Write Data

    #region Numbers
    public void WriteByte(byte input)
    {
        buffer.Add(input); // Since this is a byte list we can just enter the input
        bufferUpdate = true; // Set update to true
    }

    public void WriteBytes(byte[] input)
    {
        buffer.AddRange(input); // Since we're adding an array of bytes we need to use AddRange
        bufferUpdate = true; // Set update to true
    }

    public void WriteShort(short input)
    {
        buffer.AddRange(BitConverter.GetBytes(input)); // Convert the input with BitConverter and then add the range to the buffer(BitConverter converts the input to an array)
        bufferUpdate = true; // Set update to true
    }

    public void WriteInteger(int input)
    {
        buffer.AddRange(BitConverter.GetBytes(input)); // Convert the input with BitConverter and then add the range to the buffer(BitConverter converts the input to an array)
        bufferUpdate = true; // Set update to true
    }

    public void WriteLong(long input)
    {
        buffer.AddRange(BitConverter.GetBytes(input)); // Convert the input with BitConverter and then add the range to the buffer(BitConverter converts the input to an array)
        bufferUpdate = true; // Set update to true
    }

    public void WriteFloat(float input)
    {
        buffer.AddRange(BitConverter.GetBytes(input)); // Convert the input with BitConverter and then add the range to the buffer(BitConverter converts the input to an array)
        bufferUpdate = true; // Set update to true
    }
    #endregion Numbers

    public void WriteString(string input)
    {
        buffer.AddRange(BitConverter.GetBytes(input.Length)); // Add the string length to the buffer
        buffer.AddRange(Encoding.ASCII.GetBytes(input)); // Convert the input string to bytes with Encoding.ASCII and then add it to the buffer
        bufferUpdate = true;
    }

    #endregion Write Data
    #region Read Data

    public byte ReadByte(bool peek = true)
    {
        if (buffer.Count > readPos)
        {
            if (bufferUpdate)
            {
                readBuffer = buffer.ToArray();
                bufferUpdate = false;
            }

            byte ret = readBuffer[readPos];
            if (peek && buffer.Count > readPos)
            {
                readPos++;
            }
            return ret;
        }
        else
        {
            throw new Exception($"ByteBuffer [BYTE] is past it's limit!\nBuffer size is: {buffer.Count}, readPos is: {readPos}");
        }
    }

    public byte[] ReadBytes(int length, bool peek = true)
    {
        if (buffer.Count > readPos)
        {
            if (bufferUpdate)
            {
                readBuffer = buffer.ToArray();
                bufferUpdate = false;
            }

            byte[] ret = buffer.GetRange(readPos, length).ToArray();

            if (peek && buffer.Count > readPos)
            {
                readPos += length;
            }
            return ret;
        }
        else
        {
            throw new Exception($"ByteBuffer [BYTE[]] is past it's limit!\nBuffer size is: {buffer.Count}, readPos is: {readPos}");
        }
    }

    public short ReadShort(bool peek = true)
    {
        if (buffer.Count > readPos)
        {
            if (bufferUpdate)
            {
                readBuffer = buffer.ToArray();
                bufferUpdate = false;
            }

            short ret = BitConverter.ToInt16(readBuffer, readPos);
            if (peek && buffer.Count > readPos)
            {
                //int returnBitSize = BitConverter.GetBytes(ret).Length;
                //readPos += returnBitSize; // Testing

                readPos += 2;

            }
            return ret;
        }
        else
        {
            throw new Exception($"ByteBuffer [SHORT] is past it's limit!\nBuffer size is: {buffer.Count}, readPos is: {readPos}");
        }
    }

    public int ReadInteger(bool peek = true)
    {
        if (buffer.Count > readPos)
        {
            if (bufferUpdate)
            {
                readBuffer = buffer.ToArray();
                bufferUpdate = false;
            }

            int ret = BitConverter.ToInt32(readBuffer, readPos);
            if (peek && buffer.Count > readPos)
            {
                //int returnBitSize = BitConverter.GetBytes(ret).Length;
                //readPos += returnBitSize; //Testing

                readPos += 4;

            }
            return ret;
        }
        else
        {
            throw new Exception($"ByteBuffer [INTEGER] is past it's limit!\nBuffer size is: {buffer.Count}, readPos is: {readPos}");
        }
    }

    public long ReadLong(bool peek = true)
    {
        if (buffer.Count > readPos)
        {
            if (bufferUpdate)
            {
                readBuffer = buffer.ToArray();
                bufferUpdate = false;
            }

            long ret = BitConverter.ToInt64(readBuffer, readPos);

            if (peek && buffer.Count > readPos)
            {
                readPos += 8;
            }
            return ret;
        }
        else
        {
            throw new Exception($"ByteBuffer [LONG] is past it's limit!\nBuffer size is: {buffer.Count}, readPos is: {readPos}");
        }
    }

    public float ReadFloat(bool peek = true)
    {
        if (buffer.Count >= readPos)
        {
            if (bufferUpdate)
            {
                readBuffer = buffer.ToArray();
                bufferUpdate = false;
            }

            float ret = BitConverter.ToSingle(readBuffer, readPos);

            if (peek && buffer.Count > readPos)
            {
                readPos += 4;
            }
            return ret;
        }
        else
        {
            throw new Exception($"ByteBuffer [FLOAT] is past it's limit!\nBuffer size is: {buffer.Count}, readPos is: {readPos}");
        }
    }

    public string ReadString(bool peek = true)
    {
        if (buffer.Count > readPos)
        {
            int length = ReadInteger();

            if (bufferUpdate)
            {
                readBuffer = buffer.ToArray();
                bufferUpdate = false;
            }

            string ret = Encoding.ASCII.GetString(readBuffer, readPos, length);

            if (peek && buffer.Count > readPos)
            {
                readPos += length;
            }
            return ret;
        }
        else
        {
            throw new Exception($"ByteBuffer [STRING] is past it's limit!\nBuffer size is: {buffer.Count}, readPos is: {readPos}");
        }
    }

    #endregion Read Data

    private bool disposed = false;
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                buffer.Clear();
                readPos = 0;
            }
            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}