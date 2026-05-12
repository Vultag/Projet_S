using UnityEngine;

public class RingBuffer<T>
{
    T[] buffer;
    short bufferSize;
    public short head;

    public RingBuffer(short bufferSize)
    {
        this.bufferSize = bufferSize;
        buffer = new T[bufferSize];
        head = -1;
        if ((bufferSize & (bufferSize - 1)) != 0) Debug.LogError("bufferSize NOT POWER OF TWO");
    }
    public void Write(T item)
    {
        buffer[((++head)%bufferSize)] = item;
    }
    public T Read(short idxFromHead)
    {
        return buffer[((head + idxFromHead) & (PlayerNet.PayloadRBufferSize - 1))];
        //return buffer[(bufferSize + ((head + idxFromHead)% bufferSize)) % bufferSize];
    }
    public void SlideHead(short offset) 
    {
        //head = (short)((PlayerNet.PayloadRBufferSize+((head + offset) % PlayerNet.PayloadRBufferSize)) % PlayerNet.PayloadRBufferSize);
        head = (short)((head + offset) & (PlayerNet.PayloadRBufferSize - 1));
        if (head < 0) Debug.LogError("PB");
    }
}