using FtDSharp;
using static FtDSharp.Logging;
using static FtDSharp.Game;

public class MyScript : IFtDSharp
{
    public MyScript()
    {
        Log("Script initialized.");
    }

    public void Update(float deltaTime)
    {
        // Your logic here — runs every game tick (~40 Hz at 1x speed)
    }
}
