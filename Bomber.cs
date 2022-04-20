const float gravity = 9.80665f;


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

public double calculateFallTime(double height)
{
    return Math.Sqrt((height * 2f) / gravity);
}

public void Main(string argument, UpdateType updateType)
{
    // If the update source is from a trigger or a terminal,
    // this is an interactive command.
    if ((updateType & (UpdateType.Trigger | UpdateType.Terminal)) != 0)
    {
        switch (argument)
        {
            case "fire":
                Fire();
                break;
            case "test":
                Test();
                break;
            default:
                break;
        }
    }
}

public void ConfigureBombTime(double timeToDetonate)
{
    IMyTimerBlock timerBlock = GridTerminalSystem.GetBlockWithName("[BMR] Timer Block") as IMyTimerBlock;
    timerBlock.TriggerDelay = (float)timeToDetonate;
    timerBlock.StartCountdown();
}

public void Test()
{

}

public void Fire()
{
    // get the blocks
    IMyShipMergeBlock mergeBlock = GridTerminalSystem.GetBlockWithName("[BMR] Merge Block") as IMyShipMergeBlock;
    IMyCockpit cockpit = GridTerminalSystem.GetBlockWithName("[BMR] Pilot Control Seat") as IMyCockpit;

    // get our altitude
    double altitude;
    bool altitudebool = cockpit.TryGetPlanetElevation(MyPlanetElevation.Surface, out altitude);

    // calculate time to fall
    double fallTime = calculateFallTime(altitude);

    // set the timer to explode
    ConfigureBombTime(fallTime + 1f);

    // Release!
    mergeBlock.Enabled = false;
}
