using Shared;

namespace ClientSideWASM;
public class GameSnapshot {
    public long Tick;
    public byte[] Data;
    public double ClientArrivalTime; // Use your _intervalTimer
}

