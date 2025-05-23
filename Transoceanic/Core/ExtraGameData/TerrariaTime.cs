using System;
using System.Text;
using Terraria;
using Terraria.Enums;
using Transoceanic.Core.ExtraMathData;
using Transoceanic.Core.Localization;
using Transoceanic.Core.MathHelp;

namespace Transoceanic.Core.ExtraGameData;

public readonly struct TerrariaTime : IEquatable<TerrariaTime>
{
    /// <summary>
    /// 小时数。
    /// <br/>范围为 [0, 24)。
    /// </summary>
    public byte Hour { get; } = 0;

    /// <summary>
    /// 分钟数。
    /// <br/>范围为 [0, 60)。
    /// </summary>
    public byte Minute { get; } = 0;

    /// <summary>
    /// 秒数。
    /// <br/>范围为 [0, 60)。
    /// </summary>
    public byte Second { get; } = 0;

    /// <summary>
    /// 月相。
    /// </summary>
    public MoonPhase MoonPhase { get; } = MoonPhase.Full;

    public float Time => Hour + Minute / 60f + Second / 3600f;

    public TerrariaTime(int hour, int minute, int millisecond, MoonPhase moonPhase = MoonPhase.Full)
    {
        try
        {
            StringBuilder stringBuilder = new();
            bool argumentInvalid = false;

            if (hour is < 0 or >= 24)
            {
                stringBuilder.Append($"{nameof(hour)}: {hour}");
                argumentInvalid = true;
            }
            if (minute is < 0 or >= 60)
            {
                stringBuilder.Append((argumentInvalid ? ", " : "") + $"{nameof(minute)} : {minute}");
                argumentInvalid = true;
            }
            if (millisecond is < 0 or >= 60)
            {
                stringBuilder.Append((argumentInvalid ? ", " : "") + $"{nameof(millisecond)} : {millisecond}");
                argumentInvalid = true;
            }

            if (argumentInvalid)
                throw new ArgumentException(stringBuilder.ToString());
            else
            {
                Hour = (byte)hour;
                Minute = (byte)minute;
                Second = (byte)millisecond;
            }
        }
        catch (ArgumentException e)
        {
            TOLocalizationUtils.ChatDebugErrorMessage("TerrariaTime", Main.LocalPlayer, e.Message);
        }

        MoonPhase = moonPhase;
    }

    public TerrariaTime(DateTime dateTime, MoonPhase moonPhase = MoonPhase.Full) : this(dateTime.Hour, dateTime.Minute, dateTime.Second, moonPhase) { }

    public TerrariaTime(double time, MoonPhase moonPhase = MoonPhase.Full)
    {
        double time24Hour = Main.dayTime ? 4.5f + (time / 54000.0) * 15 : 19.5 + (time / 32400.0) * 9;
        try
        {
            if (time24Hour is < 0f or >= 24f)
                throw new ArgumentException($"{nameof(time24Hour)} : {time24Hour}");

            byte hour = (byte)time24Hour;
            double minutes = (time24Hour - hour) * 60;
            byte minute = (byte)minutes;
            byte second = (byte)((minutes - minute) * 60f);

            Hour = hour;
            Minute = minute;
            Second = second;
        }
        catch (ArgumentException e)
        {
            TOLocalizationUtils.ChatDebugErrorMessage("TerrariaTime", Main.LocalPlayer, e.Message);
        }

        MoonPhase = moonPhase;
    }

    public void Deconstruct(out float hours, out MoonPhase moonPhase)
    {
        hours = Time;
        moonPhase = MoonPhase;
    }

    public void DeConstruct(out byte hour, out byte minute, out byte second, out MoonPhase moonPhase)
    {
        hour = Hour;
        minute = Minute;
        second = Second;
        moonPhase = MoonPhase;
    }

    public override int GetHashCode() => HashCode.Combine(Hour, Minute, Second, MoonPhase);

    public override bool Equals(object obj) => obj is TerrariaTime other && Equals(other);

    public bool Equals(TerrariaTime other) => Hour == other.Hour && Minute == other.Minute && Second == other.Second && MoonPhase == other.MoonPhase;

    public static bool operator ==(TerrariaTime left, TerrariaTime right) => left.Equals(right);

    public static bool operator !=(TerrariaTime left, TerrariaTime right) => !(left == right);

    public override string ToString() => $"{Hour} : {Minute} : {Second}, {MoonPhase}";


    public PolarVector2 HourHand => PolarVector2.UnitClocks[12].RotatedBy(TOMathHelper.PiOver6 * Time);

    public PolarVector2 MinuteHand => PolarVector2.UnitClocks[12].RotatedBy(TOMathHelper.PiOver30 * (Minute + Second / 60000f));

    public PolarVector2 SecondHand => PolarVector2.UnitClocks[12].RotatedBy(TOMathHelper.PiOver30 * Second / 1000f);


    public static TerrariaTime NowReal => new(DateTime.Now);
}
