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

public struct TerrariaTimer : IEquatable<TerrariaTimer>
{
    public int TotalTicks
    {
        readonly get;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Total ticks must be non-negative.");
            field = value;
        }
    }

    public int Minutes
    {
        readonly get => TotalTicks / 3600;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Minutes must be non-negative.");
            TotalTicks = value * 3600 + TotalTicks % 3600;
        }
    }

    public int Seconds
    {
        readonly get => TotalTicks / 60 % 60;
        set
        {
            if (value is < 0 or >= 60)
                throw new ArgumentOutOfRangeException(nameof(value), "Seconds must be in the range [0, 60).");
            TotalTicks = value * 60 + TotalTicks % 60;
        }
    }

    public int Ticks
    {
        readonly get => TotalTicks % 60;
        set
        {
            if (value is < 0 or >= 60)
                throw new ArgumentOutOfRangeException(nameof(value), "Ticks must be in the range [0, 60).");
            TotalTicks = value + TotalTicks / 60 * 60;
        }
    }

    public TerrariaTimer() : this(0) { }

    public TerrariaTimer(int totalTicks) => TotalTicks = totalTicks;

    public TerrariaTimer(int minutes = 0, int seconds = 0, int ticks = 0)
    {
        Minutes = minutes;
        Seconds = seconds;
        Ticks = ticks;
    }

    public TerrariaTimer(string timeString)
    {
        if (string.IsNullOrWhiteSpace(timeString))
            throw new ArgumentException("Time string cannot be null or empty.", nameof(timeString));

        string[] parts = timeString.Split(':');

        if (parts.Length != 3)
            throw new FormatException("Time string must be in the format 'MM:SS:TT'.");
        if (!int.TryParse(parts[0], out int minutes) || !int.TryParse(parts[1], out int seconds) || !int.TryParse(parts[2], out int ticks))
            throw new FormatException("Time string must contain valid integers.");

        Minutes = minutes;
        Seconds = seconds;
        Ticks = ticks;
    }

    public readonly void Deconstruct(out int totalTicks, out int minutes, out int seconds, out int ticks)
    {
        totalTicks = TotalTicks;
        minutes = Minutes;
        seconds = Seconds;
        ticks = Ticks;
    }

    public override readonly int GetHashCode() => TotalTicks.GetHashCode();

    public override readonly bool Equals(object obj) => obj is TerrariaTimer other && Equals(other);

    public readonly bool Equals(TerrariaTimer other) => TotalTicks == other.TotalTicks;

    public static bool operator ==(TerrariaTimer left, TerrariaTimer right) => left.Equals(right);

    public static bool operator !=(TerrariaTimer left, TerrariaTimer right) => !(left == right);

    public static bool operator >(TerrariaTimer left, TerrariaTimer right) => left.TotalTicks > right.TotalTicks;

    public static bool operator <(TerrariaTimer left, TerrariaTimer right) => left.TotalTicks < right.TotalTicks;

    public static bool operator >=(TerrariaTimer left, TerrariaTimer right) => left.TotalTicks >= right.TotalTicks;

    public static bool operator <=(TerrariaTimer left, TerrariaTimer right) => left.TotalTicks <= right.TotalTicks;

    public static bool operator >(TerrariaTimer left, int right) => left.TotalTicks > right;

    public static bool operator <(TerrariaTimer left, int right) => left.TotalTicks < right;

    public static bool operator >=(TerrariaTimer left, int right) => left.TotalTicks >= right;

    public static bool operator <=(TerrariaTimer left, int right) => left.TotalTicks <= right;

    public static TerrariaTimer operator ++(TerrariaTimer timer) => new(++timer.TotalTicks);

    public static TerrariaTimer operator --(TerrariaTimer timer) => new(--timer.TotalTicks);

    public static TerrariaTimer operator +(TerrariaTimer timer, int ticks) => new(timer.TotalTicks + ticks);

    public static TerrariaTimer operator -(TerrariaTimer timer, int ticks) => new(timer.TotalTicks - ticks);

    public static TerrariaTimer operator +(TerrariaTimer left, TerrariaTimer right) => new(left.TotalTicks + right.TotalTicks);

    public static TerrariaTimer operator -(TerrariaTimer left, TerrariaTimer right) => new(left.TotalTicks - right.TotalTicks);

    public static implicit operator TerrariaTimer(int totalTicks) => new(totalTicks);

    public static implicit operator TerrariaTimer((int minutes, int seconds) time) => new(time.minutes, time.seconds, 0);

    public static implicit operator TerrariaTimer((int minutes, int seconds, int ticks) time) => new(time.minutes, time.seconds, time.ticks);

    public override readonly string ToString() => $"{Minutes}:{Seconds:D2}:{Ticks:D2}";
}