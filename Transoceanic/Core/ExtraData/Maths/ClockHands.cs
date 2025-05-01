using System;
using System.Text;
using Terraria;
using Transoceanic.Core.Localization;
using Transoceanic.Core.MathHelp;

namespace Transoceanic.Core.ExtraData.Maths;

public readonly struct ClockHands : IEquatable<ClockHands>
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
    /// 毫秒数。
    /// <br/>范围为 [0, 60000)。
    /// </summary>
    public ushort Millisecond { get; } = 0;

    public float Time => Hour + Minute / 60f + Millisecond / 3600000f;

    public ClockHands(int hour, int minute, int millisecond)
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
            if (millisecond is < 0 or >= 60000)
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
                Millisecond = (ushort)millisecond;
            }
        }
        catch (ArgumentException e)
        {
            TOLocalizationUtils.ChatDebugErrorMessage("ClockHands", Main.LocalPlayer, e.Message);
        }
    }

    public ClockHands(DateTime dateTime) : this(dateTime.Hour, dateTime.Minute, dateTime.Second * 60 + dateTime.Millisecond) { }

    public ClockHands(float hours)
    {
        try
        {
            if (hours is < 0f or >= 24f)
                throw new ArgumentException($"{nameof(hours)} : {hours}");

            byte hour = (byte)hours;
            float minutes = (hours - hour) * 60f;
            byte minute = (byte)minutes;
            ushort millisecond = (ushort)((minutes - minute) * 60000f);

            Hour = hour;
            Minute = minute;
            Millisecond = millisecond;
        }
        catch (ArgumentException e)
        {
            TOLocalizationUtils.ChatDebugErrorMessage("ClockHands", Main.LocalPlayer, e.Message);
        }
    }

    public void Deconstruct(out float hours) => hours = Time;

    public void DeConstruct(out byte hour, out byte minute, out ushort millisecond) => (hour, minute, millisecond) = (Hour, Minute, Millisecond);

    public override int GetHashCode() => HashCode.Combine(Hour, Minute, Millisecond);

    public override bool Equals(object obj) => obj is ClockHands other && Equals(other);

    public bool Equals(ClockHands other) => Hour == other.Hour && Minute == other.Minute && Millisecond == other.Millisecond;

    public static bool operator ==(ClockHands left, ClockHands right) => left.Equals(right);

    public static bool operator !=(ClockHands left, ClockHands right) => !(left == right);

    public override string ToString() => $"{Hour} : {Minute} : {Millisecond / 1000} : {Millisecond % 1000}";


    public PolarVector2 HourHand => PolarVector2.UnitClocks[12].RotatedBy(TOMathHelper.PiOver6 * Time);

    public PolarVector2 MinuteHand => PolarVector2.UnitClocks[12].RotatedBy(TOMathHelper.PiOver30 * (Minute + Millisecond / 60000f));

    public PolarVector2 SecondHand => PolarVector2.UnitClocks[12].RotatedBy(TOMathHelper.PiOver30 * Millisecond / 1000f);


    public static ClockHands NowReal => new(DateTime.Now);

    public static ClockHands NowGame => new(Utils.GetDayTimeAs24FloatStartingFromMidnight());
}
