IMyMotorStator rotor;
IMyTextSurface mesurface0;
IMyFunctionalBlock refBlock;

public Program()
{
    // The constructor, called only once every session and
    // always before any other method is called. Use it to
    // initialize your script. 
    //     
    // The constructor is optional and can be removed if not
    // needed.
    // 
    // It's recommended to set RuntimeInfo.UpdateFrequency 
    // here, which will allow your script to run itself without a 
    // timer block.

    // Configure this program to run the Main method every 10 update ticks
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    rotor = GridTerminalSystem.GetBlockWithName("SiloDoorRotor2") as IMyMotorStator;
    refBlock = GridTerminalSystem.GetBlockWithName("SiloDoorLevelProgram") as IMyFunctionalBlock;
}

void RunContinuousLogic()
{
    Vector3D rotorDown = rotor.WorldMatrix.Down;
    Vector3D refDown = refBlock.WorldMatrix.Down;

    // the Normalize method normalizes the axis and returns the length it had before
    double angle = AngleBetween(rotorDown, refDown);

    Echo(rotorDown.ToString());
}

/// <summary>
/// Computes angle between 2 vectors in radians.
/// </summary>
public double AngleBetween(Vector3D a, Vector3D b)
{
    if (Vector3D.IsZero(a) || Vector3D.IsZero(b))
        return 0;
    else
        return Math.Acos(MathHelper.Clamp(a.Dot(b) / Math.Sqrt(a.LengthSquared() * b.LengthSquared()), -1, 1));
}

public void Save()
{
    // Called when the program needs to save its state. Use
    // this method to save your state to the Storage field
    // or some other means. 
    // 
    // This method is optional and can be removed if not
    // needed.
}

public void Main(string argument, UpdateType updateSource)
{
    mesurface0 = Me.GetSurface(0);
    mesurface0.ContentType = ContentType.TEXT_AND_IMAGE;
    mesurface0.FontSize = 2;
    mesurface0.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
    mesurface0.WriteText("Silo Door v1");


    // If the update source is from a trigger or a terminal,
    // this is an interactive command.
    if ((updateSource & (UpdateType.Trigger | UpdateType.Terminal)) != 0)
    {
        //RunCommand(argument);
    }

    // If the update source has this update flag, it means
    // that it's run from the frequency system, and we should
    // update our continuous logic.
    if ((updateSource & UpdateType.Update10) != 0)
    {
        RunContinuousLogic();
    }
}
