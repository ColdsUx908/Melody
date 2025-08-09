namespace CalamityAnomalies.ModCompatibility;

public static class CAModCall
{
    public static object Call(params object[] args)
    {
        if (args.Length == 0 || args[0] is not string option)
            return null;

        switch (option.ToLower())
        {
            case "world":
                if (args.Length == 1 || args[1] is not string variable)
                    return null;

                return variable.ToLower() switch
                {
                    "anomaly" => CAWorld.Anomaly,
                    "anomalyultra" => CAWorld.AnomalyUltramundane,
                    _ => null,
                };
        }

        return null;
    }
}
