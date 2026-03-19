using System;

public class StateManager
{
    private readonly (byte[] Data, long ArrivalTime)[] _buffer;
    private readonly byte[] _accessBuffer; // The "one" array used for reading
    private readonly int _capacity;
    public readonly int _packetSize;
    
    private int _head = 0;
    private int _tail = 0;
    private int _count = 0;
    public int lastSize = -1;

    private readonly object _lock = new();

    public StateManager(int capacity, int packetSize)
    {
        _capacity = capacity;
        _packetSize = packetSize;
        _buffer = new (byte[] Data, long ArrivalTime)[capacity];
        _accessBuffer = new byte[packetSize];
        for (int i = 0; i < capacity; i++)
            _buffer[i] = (new byte[packetSize], -1);
    }


    // This is your new "Clean" API
    public byte[] TryPopState(out long arrivalTime)
    {
        lock (_lock)
        {
            if (_count == 0)
            {
                arrivalTime = -1;
                return null;
            }

            // Get the reference to the internal pre-allocated array
            var entry = _buffer[_head];
            arrivalTime = entry.ArrivalTime;
            Array.Copy(entry.Data, _accessBuffer, _packetSize);
            // Advance the pointers
            _head = (_head + 1) % _capacity;
            _count--;

            // Return a "Window" into that specific array
            return _accessBuffer;
        }
    }

    public void PushState(ReadOnlySpan<byte> newState, long arrivalTime)
    {
        lock (_lock)
        {
            if (_count == _capacity)
            {
                // BUFFER FULL: "Drop" the oldest by moving the head forward.
                _head = (_head + 1) % _capacity;
            }
            else
            {
                _count++;
            }
            lastSize = newState.Length;
            // Write the new data into the tail's pre-allocated slot
            newState.CopyTo(_buffer[_tail].Data);
            _buffer[_tail].ArrivalTime = arrivalTime;

            // Move the tail forward
            _tail = (_tail + 1) % _capacity;
        }
    }

    public long PeekArrivalTime()
    {
        lock (_lock)
        {
            return _count > 0 ? _buffer[_head].ArrivalTime : -1;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _head = 0;
            _tail = 0;
            _count = 0;
        }
    }

    public int Count => _count;
}