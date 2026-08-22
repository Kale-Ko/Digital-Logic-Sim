namespace DLS.Simulation
{
	// Helper class for dealing with pin state.
	// Pin state is stored as a uint32, with format:
	// Tristate flags (most significant 32 bits) | Bit states (least significant 32 bits)
	public static class PinState
	{
		// Each bit has three possible states (tri-state logic):
		public const uint LogicLow = 0;
		public const uint LogicHigh = 1;
		public const uint LogicDisconnected = 2;

		// Mask for single bit value (bit state, and tristate flag)
		public const ulong SingleBitMask = 1 | (1 << 32);

		public static uint GetBitStates(ulong state) => (uint)state;
		public static uint GetTristateFlags(ulong state) => (uint)(state >> 32);

		public static void Set(ref ulong state, uint bitStates, uint tristateFlags)
		{
			state = ((ulong)bitStates | ((ulong) tristateFlags << 32));
		}

		public static void Set(ref ulong state, ulong other) => state = other;

		public static uint GetBitTristatedValue(ulong state, int bitIndex)
		{
			uint bitState = (uint)((GetBitStates(state) >> bitIndex) & 1);
			uint tri = (uint)((GetTristateFlags(state) >> bitIndex) & 1);
			return (uint)(bitState | (tri << 1)); // Combine to form tri-stated value: 0 = LOW, 1 = HIGH, 2 = DISCONNECTED
		}

		public static bool FirstBitHigh(ulong state) => (state & 1) == LogicHigh;

		public static void Set4BitFrom8BitSource(ref ulong state, ulong source8bit, bool firstNibble)
		{
			uint sourceBitStates = GetBitStates(source8bit);
			uint sourceTristateFlags = GetTristateFlags(source8bit);

			if (firstNibble)
			{
				const uint mask = 0b1111;
				Set(ref state, (uint)(sourceBitStates & mask), (uint)(sourceTristateFlags & mask));
			}
			else
			{
				const ulong mask = 0b11110000;
				Set(ref state, (uint)((sourceBitStates & mask) >> 4), (uint)((sourceTristateFlags & mask) >> 4));
			}
		}

		public static void Set8BitFrom4BitSources(ref ulong state, ulong a, ulong b)
		{
			uint bitStates = (uint)(GetBitStates(a) | (GetBitStates(b) << 4));
			uint tristateFlags = (uint)((GetTristateFlags(a) & 0b1111) | ((GetTristateFlags(b) & 0b1111) << 4));
			Set(ref state, bitStates, tristateFlags);
		}


		public static void Toggle(ref ulong state, int bitIndex)
		{
			uint bitStates = GetBitStates(state);
			bitStates ^= (uint)(1u << bitIndex);

			// Clear tristate flags (can't be disconnected if toggling as only input dev pins are allowed)
			Set(ref state, bitStates, 0);
		}

		public static void SetAllDisconnected(ref ulong state) => Set(ref state, 0, uint.MaxValue);
	}
}