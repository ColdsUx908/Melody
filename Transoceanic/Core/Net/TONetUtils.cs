using System.Collections.Generic;
using System.IO;

namespace Transoceanic.Core.Net;

public static class TONetUtils
{
    public static void SendAI(float[] ai, bool[] aiChanged, BinaryWriter binaryWriter)
    {
        Dictionary<byte, float> values = [];
        for (int i = 0; i < ai.Length; i++)
        {
            if (aiChanged[i])
            {
                values[(byte)i] = ai[i];
                aiChanged[i] = false;
            }
        }
        binaryWriter.Write((byte)values.Count);
        foreach ((byte index, float value) in values)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
    }

    public static void ReceiveAI(float[] ai, BinaryReader binaryReader)
    {
        for (int i = 0; i < binaryReader.ReadByte(); i++)
            ai[binaryReader.ReadByte()] = binaryReader.ReadSingle();
    }
}
