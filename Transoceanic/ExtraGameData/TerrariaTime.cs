using System;
using System.Text;
using Terraria;
using Terraria.Enums;
using Transoceanic.ExtraMathData;
using Transoceanic.Localization;
using Transoceanic.MathHelp;

namespace Transoceanic.ExtraGameData;

public readonly struct TerrariaTime : IEquatable<TerrariaTime>
{
    /// <summary>
    /// 时间。
    /// </summary>
    public double Time { get; } = 0.0;

    /// <summary>
    /// 月相。
    /// </summary>
    public MoonPhase MoonPhase { get; } = MoonPhase.Full;

    /// <summary>
    /// 小时数。
    /// <br/>范围为 [0, 24)。
    /// </summary>
    public int Hour => (int)Time;

    /// <summary>
    /// 分钟数。
    /// <br/>范围为 [0, 60)。
    /// </summary>
    public int Minute => (int)((Time - Hour) * 60.0);

    /// <summary>
    /// 秒数。
    /// <br/>范围为 [0, 60)。
    /// </summary>
    public int Second => (int)(((Time - Minute) * 60.0 - Minute) * 60.0);

    public TerrariaTime(double time, MoonPhase moonPhase = MoonPhase.Full)
    {
        Time = time;
        MoonPhase = moonPhase;
    }

    public TerrariaTime(int hour, int minute, int second, MoonPhase moonPhase = MoonPhase.Full)
    {
        try
        {
            StringBuilder builder = new();
            bool argumentInvalid = false;

            if (hour is < 0 or >= 24)
            {
                builder.Append($"{nameof(hour)}: {hour}");
                argumentInvalid = true;
            }
            if (minute is < 0 or >= 60)
            {
                builder.Append((argumentInvalid ? ", " : "") + $"{nameof(minute)} : {minute}");
                argumentInvalid = true;
            }
            if (second is < 0 or >= 60)
            {
                builder.Append((argumentInvalid ? ", " : "") + $"{nameof(second)} : {second}");
                argumentInvalid = true;
            }

            if (argumentInvalid)
                throw new ArgumentException(builder.ToString());
            else
                Time = hour + minute / 60.0 + second / 3600.0;
        }
        catch (ArgumentException e)
        {
            TOLocalizationUtils.ChatDebugErrorMessage("TerrariaTime", Main.LocalPlayer, e.Message);
        }

        MoonPhase = moonPhase;
    }

    public TerrariaTime(DateTime dateTime, MoonPhase moonPhase = MoonPhase.Full) : this(dateTime.Hour, dateTime.Minute, dateTime.Second, moonPhase) { }

    public void Deconstruct(out double time, out MoonPhase moonPhase)
    {
        time = Time;
        moonPhase = MoonPhase;
    }

    public void DeConstruct(out int hour, out int minute, out int second, out MoonPhase moonPhase)
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

    public string TimeString => $"{Hour} : {Minute} : {Second}";

    public override string ToString() => TimeString + $", {MoonPhase}";


    public PolarVector2 HourHand => PolarVector2.UnitClocks[12].RotatedBy(TOMathHelper.PiOver6 * (float)Time);

    public PolarVector2 MinuteHand => PolarVector2.UnitClocks[12].RotatedBy(TOMathHelper.PiOver30 * (float)(Time - Hour));

    public PolarVector2 SecondHand => PolarVector2.UnitClocks[12].RotatedBy(TOMathHelper.PiOver30 * Second / 1000f);


    public static TerrariaTime RealTime => new(DateTime.Now);
}
