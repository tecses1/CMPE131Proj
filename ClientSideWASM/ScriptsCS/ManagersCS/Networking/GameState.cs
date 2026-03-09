using ClientSideWASM;

public class GameState
{
    public InputWrapper myInputUsed;
    public InputWrapper[] playerInputsUsed;
    public byte[] gameState;

    public DateTime timeStamp;

    public GameState(InputWrapper inputUsed, InputWrapper[] playerInputsUsed, byte[] gameState)
    {
        this.myInputUsed = inputUsed;
        this.playerInputsUsed = playerInputsUsed;
        this.gameState = gameState;
    }
}