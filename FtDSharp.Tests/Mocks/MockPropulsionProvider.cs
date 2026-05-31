namespace FtDSharp.Tests.Mocks;

public class MockPropulsionProvider : IPropulsionProvider
{
    public IPropulsion Propulsion { get; set; } = new MockPropulsion();
}

public class MockPropulsion : IPropulsion
{
    public float Forwards { get; set; }
    public float Strafe { get; set; }
    public float Hover { get; set; }
    public float Yaw { get; set; }
    public float Pitch { get; set; }
    public float Roll { get; set; }
    public float A { get; set; }
    public float B { get; set; }
    public float C { get; set; }
    public float D { get; set; }
    public float E { get; set; }
    public float MainDrive { get; set; }
    public float SecondaryDrive { get; set; }
    public float TertiaryDrive { get; set; }
}
