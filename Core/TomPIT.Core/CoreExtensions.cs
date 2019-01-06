using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using TomPIT.Exceptions;
using TomPIT.Security;

namespace TomPIT
{
	public enum DurationPrecision
	{
		Day = 1,
		Hour = 2,
		Minute = 3,
		Second = 4,
		Millisecond = 5
	}

	public static class CoreExtensions
	{
		public const string PreciseDateFormat = "yyyy-MM-dd HH:mm:ss.fffffff";

		public static string WithMilliseconds(this DateTime value)
		{
			return value.ToString(PreciseDateFormat, CultureInfo.InvariantCulture);
		}

		public static string TypeName(this Type type)
		{
			if (type == null)
				return null;

			return string.Format("{0}, {1}", type.FullName, type.Assembly.GetName().Name);
		}

		public static T Argument<T>(this JObject instance, string argument)
		{
			return Argument<T>(instance, argument, true);
		}

		public static T Argument<T>(this JObject instance, string argument, T defaultValue)
		{
			if (!instance.ContainsKey(argument))
				return defaultValue;

			var v = instance[argument].ToString();

			if (Types.TryConvert(v, out T r))
				return r;

			return defaultValue;
		}

		public static T Argument<T>(this JObject instance, string argument, bool mandatory)
		{
			if (!instance.ContainsKey(argument))
			{
				if (mandatory)
					throw new TomPITException(string.Format("{0} ({1}).", SR.ErrServerExpectedArgument, argument));

				return default(T);
			}

			var v = instance[argument].ToString();

			if (Types.TryConvert(v, out T r))
				return r;

			throw new TomPITException(string.Format("{0} ({1}, {2}).", SR.ErrServerInvalidArgumentType, argument, typeof(T)));
		}

		public static string GetDescription(this IAuthenticationResult result)
		{
			switch (result.Reason)
			{
				case AuthenticationResultReason.NotFound:
					return SR.ErrUserNotFound;
				case AuthenticationResultReason.InvalidPassword:
					return SR.ErrAuthenticationFailed;
				case AuthenticationResultReason.Inactive:
					return SR.ErrUserInactive;
				case AuthenticationResultReason.Locked:
					return SR.ErrUserLocked;
				case AuthenticationResultReason.NoPassword:
					return SR.ErrNoPassword;
				case AuthenticationResultReason.PasswordExpired:
					return SR.ErrPasswordExpired;
				default:
					return null;
			}
		}
	}
}
