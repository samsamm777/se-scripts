List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
List<IMyTextSurface> surfaces = new List<IMyTextSurface>(); 

public Program()
{
    // Configure this program to run the Main method every 100 update ticks
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Main(string argument, UpdateType updateType)
{
    // If the update source has this update flag, it means
    // that it's run from the frequency system, and we should
    // update our continuous logic.
    if ((updateType & UpdateType.Update100) != 0)
    {
        FindLCDS();
    }

    // If the update source is from a trigger or a terminal,
    // this is an interactive command.
    if ((updateType & (UpdateType.Trigger | UpdateType.Terminal)) != 0)
    {
        switch (argument)
        {
            case "refuel":
                Refuel();
                break;
            case "shutdown":
                PowerDown();
                break;
            case "poweron":
                PowerOn();
                break;
            case "release":
                LifeSupportDetach();
                break;
            case "launch":
                ClearLog();
                LogLine("Launch Sequence Starting...");
                Launch();
                break;
            case "prelaunch":
                ClearLog();
                LogLine("PRELAUNCH");
                PreLaunch();
                break;
            case "reset":
                ResetSilo();
                break;
            default:
                LogErrorLine("No argument supplied. Noop.");
                break;
        }
    }
}

/// <summary>
/// Finds all the lcds with our tag in the name
/// </summary>
private void FindLCDS()
{
    List<IMyTerminalBlock> tmpBlocks = new List<IMyTerminalBlock>();

    try
    {
        GridTerminalSystem.SearchBlocksOfName("[LOG]", tmpBlocks, block =>
        {
            return block is IMyTextSurfaceProvider;
        });
    }
    catch (Exception e)
    {
        // Dump the exception content to the 
        Echo("An error occurred during script execution.");
        Echo($"Exception: {e}\n---");

        // Rethrow the exception to make the programmable block halt execution properly
        throw;
    }

    foreach (IMyTerminalBlock b in tmpBlocks)
    {
        if (blocks.Find(x => x.EntityId == b.EntityId) == null)
        {
            blocks.Add(b);
            InitNewScreen((IMyTextSurfaceProvider)b);
        }
    }
}

private void InitNewScreen(IMyTextSurfaceProvider block)
{
    IMyTextSurface surface = block.GetSurface(0);
    surfaces.Add(surface);

    surface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
    surface.WriteText("Init New Screen Complete\n", false);
}

private void Abort()
{
    LogLine("!ABORTING!");
}

private void ReadyTheTanks()
{
    // Retrieve a named group
    IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName("[RM] Tanks");
    // Create a list containing all interior lights that are currently on
    List<IMyGasTank> tanks = new List<IMyGasTank>();
    // Fetch the blocks from that group - if they're enabled. Again, the type
    // is implicit.
    group.GetBlocksOfType<IMyGasTank>(tanks);
    foreach (IMyGasTank t in tanks)
    {
        t.Stockpile = false;
        t.Enabled = true;
    }
}

private void Refuel()
{
    ClearLog();
    LogLine("Refueling...");

    // Retrieve a named group
    IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName("[RM] Tanks");
    // Create a list containing all interior lights that are currently on
    List<IMyGasTank> tanks = new List<IMyGasTank>();
    // Fetch the blocks from that group - if they're enabled. Again, the type
    // is implicit.
    group.GetBlocksOfType<IMyGasTank>(tanks);
    foreach (IMyGasTank t in tanks)
    {
        t.Stockpile = true;
        LogLine("RM Stockpile tank on");
    }

    // Retrieve a named group
    group = GridTerminalSystem.GetBlockGroupWithName("Hydro Tanks Storage");
    // Create a list containing all interior lights that are currently on
    List<IMyGasTank> reserveTanks = new List<IMyGasTank>();
    // Fetch the blocks from that group - if they're enabled. Again, the type
    // is implicit.
    group.GetBlocksOfType<IMyGasTank>(reserveTanks);
    foreach (IMyGasTank t in reserveTanks)
    {
        t.Stockpile = false;
        LogLine("Reserve Tank Stockpile off");
    }
}

private void PowerDown()
{
    ClearLog();
    LogLine("Power down Sequence Starting...");

    LogLine("1. Batteries Power Down");
    // Retrieve a named group
    IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName("[RM] Batteries");
    // Create a list containing all interior lights that are currently on
    List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
    // Fetch the blocks from that group - if they're enabled. Again, the type
    // is implicit.
    group.GetBlocksOfType(batteries, battery => battery.Enabled);
    foreach (IMyBatteryBlock b in batteries)
    {
        b.Enabled = false;
    }
    LogLine("# Batteries Offline");

    LogLine("Shutdown Complete");
}

private void PowerOn()
{
    ClearLog();
    LogLine("Power Sequence Starting...");

    LogLine("1. Batteries Power Up");
    // Retrieve a named group
    IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName("[RM] Batteries");
    // Create a list containing all interior lights that are currently on
    List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
    // Fetch the blocks from that group - if they're enabled. Again, the type
    // is implicit.
    group.GetBlocksOfType(batteries, battery => !battery.Enabled);
    foreach (IMyBatteryBlock b in batteries)
    {
        b.Enabled = true;
    }
    LogLine("# Batteries On");

    LogLine("2. Checking Battery Levels");
    group.GetBlocksOfType(batteries);
    foreach (IMyBatteryBlock b in batteries)
    {
        float chargePercent = b.CurrentStoredPower / b.MaxStoredPower;
        if (chargePercent <= 0.50)
        {
            LogErrorLine("Batteries low stored power");
            return;
        }
    }
    LogLine("# Batteries OK");

    LogLine("Missile has sufficient on board power.");
}

private void ClearLog()
{
    foreach (IMyTextSurface surface in surfaces)
    {
        surface.WriteText(" ", false);
    }
}

private void LogLine(string text)
{
    foreach (IMyTextSurface surface in surfaces)
    {
        surface.WriteText(text + "\n", true);
    }
}

private void LogErrorLine(string text)
{
    foreach (IMyTextSurface surface in surfaces)
    {
        surface.WriteText("ERROR: " + text + "\n", true);
    }
}

private void LifeSupportDetach(float speed = -0.2f)
{
    IMyShipConnector lifeSupportConnector = GridTerminalSystem.GetBlockWithName("SiloLifeSupportConnector") as IMyShipConnector;
    IMyPistonBase lifeSupportPiston = GridTerminalSystem.GetBlockWithName("SiloLifeSupportPiston") as IMyPistonBase;

    // release the life support connector
    lifeSupportConnector.Disconnect();

    // retract the piston
    lifeSupportPiston.Velocity = -0.2f;
    lifeSupportPiston.Retract();
}

private void ResetSilo()
{
    IMyPistonBase landingGearPiston = GridTerminalSystem.GetBlockWithName("MissleLandingGearPiston") as IMyPistonBase;
    IMyPistonBase lifeSupportPiston = GridTerminalSystem.GetBlockWithName("SiloLifeSupportPiston") as IMyPistonBase;

    // extend the piston
    lifeSupportPiston.Velocity = 1f;
    lifeSupportPiston.Extend();

    // extend the landing gear
    landingGearPiston.Extend();
}

private void PreLaunch()
{
    IMyShipConnector umbilicalCordConnector = GridTerminalSystem.GetBlockWithName("SiloLifeSupportConnector") as IMyShipConnector;
    IMyLandingGear landingGear = GridTerminalSystem.GetBlockWithName("MissileLandingGear") as IMyLandingGear;
    IMyThrust thruster = GridTerminalSystem.GetBlockWithName("[RM] Thruster") as IMyThrust;
    IMyPistonBase landingGearPiston = GridTerminalSystem.GetBlockWithName("MissleLandingGearPiston") as IMyPistonBase;

    ReadyTheTanks();

    thruster.Enabled = true;
    SetThrusterPercent(thruster, 0.01f);
}

private void SetThrusterPercent(IMyThrust thruster, float factor)
{
    var min = thruster.GetMinimum<float>("Override");
    var max = thruster.GetMaximum<float>("Override");
    thruster.SetValueFloat("Override", min + (max - min) * factor);
}

private void Launch()
{
    IMyShipConnector umbilicalCordConnector = GridTerminalSystem.GetBlockWithName("SiloLifeSupportConnector") as IMyShipConnector;
    IMyLandingGear landingGear = GridTerminalSystem.GetBlockWithName("MissileLandingGear") as IMyLandingGear;
    IMyThrust thruster = GridTerminalSystem.GetBlockWithName("[RM] Thruster") as IMyThrust;
    IMyPistonBase landingGearPiston = GridTerminalSystem.GetBlockWithName("MissleLandingGearPiston") as IMyPistonBase;

    ReadyTheTanks();

    thruster.Enabled = true;
    SetThrusterPercent(thruster, 1.0f);

    // release the landing gear holding the missile
    landingGear.Unlock();

    //// retract the piston fast!
    landingGearPiston.Velocity = -1.0f;
    landingGearPiston.Retract();

    //fast detatch life support
    LifeSupportDetach(-1.0f);
}
