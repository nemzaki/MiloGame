namespace Quantum
{
	using System.Runtime.CompilerServices;
	using Photon.Deterministic;

	public static unsafe class FPMathUtility
	{
		// PUBLIC METHODS

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FP ClampBetween(FP value, FP a, FP b)
		{
			return a < b ? FPMath.Clamp(value, a, b) : FPMath.Clamp(value, b, a);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe FP ClampAngleTo180(FP angle)
		{
			return angle > 180 ? angle - 360 : angle;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FPVector2 RandomInsideUnitCircle(Frame frame)
		{
			FP radius = FPMath.Sqrt(frame.RNG->Next());
			FP angle = frame.RNG->Next() * 2 * FP.Pi;

			return new FPVector2(radius * FPMath.Cos(angle), radius * FPMath.Sin(angle));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FPVector2 RandomInsideUnitCircleNonUniform(Frame frame)
		{
			FP radius = frame.RNG->Next();
			FP angle = frame.RNG->Next() * 2 * FP.Pi;

			return new FPVector2(radius * FPMath.Cos(angle), radius * FPMath.Sin(angle));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FPVector2 RandomOnUnitCircle(Frame frame)
		{
			FP angle = frame.RNG->Next() * 2 * FP.Pi;
			return new FPVector2(FPMath.Cos(angle), FPMath.Sin(angle));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FP Map(FP inMin, FP inMax, FP outMin, FP outMax, FP value)
		{
			if (value <= inMin)
				return outMin;

			if (value >= inMax)
				return outMax;

			return (outMax - outMin) * ((value - inMin) / (inMax - inMin)) + outMin;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FP Map(FPVector2 inRange, FPVector2 outRange, FP value)
		{
			return Map(inRange.X, inRange.Y, outRange.X, outRange.Y, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasSameSign(FP a, FP b)
		{
			return (a >= FP._0 && b >= FP._0) || (a < FP._0 && b < FP._0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FP ExpApproximate(FP x)
		{
			x = 1 + x / 256;
			x *= x; x *= x; x *= x; x *= x;
			x *= x; x *= x; x *= x; x *= x;
			return x;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FPVector3 ComponentMin(FPVector3 a, FPVector3 b)
		{
			FPVector3 r = default;

			r.X = a.X < b.X ? a.X : b.X;
			r.Y = a.Y < b.Y ? a.Y : b.Y;
			r.Z = a.Z < b.Z ? a.Z : b.Z;

			return r;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FPVector3 ComponentMax(FPVector3 a, FPVector3 b)
		{
			FPVector3 r = default;

			r.X = a.X > b.X ? a.X : b.X;
			r.Y = a.Y > b.Y ? a.Y : b.Y;
			r.Z = a.Z > b.Z ? a.Z : b.Z;

			return r;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FP MoveTowards(FP current, FP target, FP maxDelta)
		{
			if (FPMath.Abs(target - current) <= maxDelta)
			{
				return target;
			}

			return current + FPMath.Sign(target - current) * maxDelta;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FPVector3 ClosestPointOnLine(FPVector3 position, FPVector3 lineStart, FPVector3 lineEnd)
		{
			FPVector3 line = lineEnd - lineStart;
			FPVector3 toPosition = position - lineStart;

			FP sqrLineLength = line.SqrMagnitude;

			if (sqrLineLength == 0)
				return lineStart;

			FP t = FPMath.Clamp01(FPVector3.Dot(toPosition, line) / sqrLineLength);
			return lineStart + line * t;
		}
	}
}