using Terraria.ModLoader;

namespace Transoceanic.Commands;

public record CommandCallInfo(CommandCaller Caller, CommandType CommandType, string Command, string[] Args);