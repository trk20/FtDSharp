public class MyScript
{
    [OnStart]
    public void Initialize()
    {
        Log("Script initialized.");
    }

    [OnPhysicsTick]
    public void Update()
    {
        // Your logic here — runs every game tick (~40 Hz at 1x speed)
    }
}
