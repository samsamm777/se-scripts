


public void Main(string argument, UpdateType updateSource)
{
    List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocks(allBlocks);

    for (int i = 0; i < allBlocks.Count; i++)
    {
        string name = allBlocks[i].CustomName;

        if (name.Length < 5)
        {
            AppendTagPrefixToBlock(allBlocks[i]);
            continue;
        } else
        {
            string sub = name.Substring(0, 5);
            if (sub != "[BMR]")
            {
                AppendTagPrefixToBlock(allBlocks[i]);
            }
        }


    }

    Echo("DONE");
}

public void AppendTagPrefixToBlock(IMyTerminalBlock block)
{
    block.CustomName = "[BMR] " + block.CustomName;
}
