using System;

namespace TomPIT.Data
{
   internal struct Version : IComparable, IEquatable<Version>, IComparable<Version>
   {
      public static readonly Version Zero = default;

      private readonly ulong Value;

      private Version(ulong value)
      {
         Value = value;
      }

      public static Version Parse(object value)
      {
         if (!Types.TryConvert(value, out string v))
            return Zero;

         if (string.IsNullOrWhiteSpace(v))
            return Zero;

         return new Version(Convert.ToUInt64(v, 16));
      }

      public static implicit operator Version(ulong value)
      {
         return new Version(value);
      }
      public static implicit operator Version(long value)
      {
         return new Version(unchecked((ulong)value));
      }
      public static explicit operator Version(byte[] value)
      {
         return new Version(((ulong)value[0] << 56) | ((ulong)value[1] << 48) | ((ulong)value[2] << 40) | ((ulong)value[3] << 32) | ((ulong)value[4] << 24) | ((ulong)value[5] << 16) | ((ulong)value[6] << 8) | value[7]);
      }
      public static explicit operator Version?(byte[] value)
      {
         if (value == null) return null;
         return new Version(((ulong)value[0] << 56) | ((ulong)value[1] << 48) | ((ulong)value[2] << 40) | ((ulong)value[3] << 32) | ((ulong)value[4] << 24) | ((ulong)value[5] << 16) | ((ulong)value[6] << 8) | value[7]);
      }
      public static implicit operator byte[](Version timestamp)
      {
         var r = new byte[8];
         r[0] = (byte)(timestamp.Value >> 56);
         r[1] = (byte)(timestamp.Value >> 48);
         r[2] = (byte)(timestamp.Value >> 40);
         r[3] = (byte)(timestamp.Value >> 32);
         r[4] = (byte)(timestamp.Value >> 24);
         r[5] = (byte)(timestamp.Value >> 16);
         r[6] = (byte)(timestamp.Value >> 8);
         r[7] = (byte)timestamp.Value;
         return r;
      }

      public override bool Equals(object obj)
      {
         return obj is Version && Equals((Version)obj);
      }

      public override int GetHashCode()
      {
         return Value.GetHashCode();
      }

      public bool Equals(Version other)
      {
         return other.Value == Value;
      }

      int IComparable.CompareTo(object obj)
      {
         return obj == null ? 1 : CompareTo((Version)obj);
      }

      public int CompareTo(Version other)
      {
         return Value == other.Value ? 0 : Value < other.Value ? -1 : 1;
      }

      public static bool operator ==(Version comparand1, Version comparand2)
      {
         return comparand1.Equals(comparand2);
      }
      public static bool operator !=(Version comparand1, Version comparand2)
      {
         return !comparand1.Equals(comparand2);
      }
      public static bool operator >(Version comparand1, Version comparand2)
      {
         return comparand1.CompareTo(comparand2) > 0;
      }
      public static bool operator >=(Version comparand1, Version comparand2)
      {
         return comparand1.CompareTo(comparand2) >= 0;
      }
      public static bool operator <(Version comparand1, Version comparand2)
      {
         return comparand1.CompareTo(comparand2) < 0;
      }
      public static bool operator <=(Version comparand1, Version comparand2)
      {
         return comparand1.CompareTo(comparand2) <= 0;
      }

      public override string ToString()
      {
         return Value.ToString("x16");
      }

      public static Version Max(Version comparand1, Version comparand2)
      {
         return comparand1.Value < comparand2.Value ? comparand2 : comparand1;
      }
   }
}