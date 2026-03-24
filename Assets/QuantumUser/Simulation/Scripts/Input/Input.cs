using System;
using Photon.Deterministic;

namespace Quantum
{
	public partial struct Input
	{
        
        //PLAYER
        public bool PlayerJump { get { return Flags.IsBitSet(0); } set { Flags.SetBit(0, value); } }
        public bool PlayerAttack { get { return Flags.IsBitSet(1); } set { Flags.SetBit(1, value); } }
        
        public bool PlayerSprint { get { return Flags.IsBitSet(2); } set { Flags.SetBit(2, value); } }
        
        public bool PlayerDodge { get { return Flags.IsBitSet(3); } set { Flags.SetBit(3, value); } }
        
        public bool PlayerBlock { get { return Flags.IsBitSet(4); } set { Flags.SetBit(4, value); } }
        
        public bool FinisherAttack { get { return Flags.IsBitSet(5); } set { Flags.SetBit(5, value); } }
        
        public bool PlayerKick { get { return Flags.IsBitSet(6); } set { Flags.SetBit(6, value); } }
        
        public bool PlayerAttackHard { get { return Flags.IsBitSet(7); } set { Flags.SetBit(7, value); } }
        
        public bool PlayerKickHard { get { return Flags.IsBitSet(8); } set { Flags.SetBit(8, value); } }
        
        public bool PlayerDodgeForward { get { return Flags.IsBitSet(9); } set { Flags.SetBit(9, value); } }
        
        public bool PlayerDodgeBack { get { return Flags.IsBitSet(10); } set { Flags.SetBit(10, value); } }
        
        public bool ConnectionWarning { get { return Flags.IsBitSet(11); } set { Flags.SetBit(11, value); } }
        
        public bool ConnectionBad { get { return Flags.IsBitSet(12); } set { Flags.SetBit(12, value); } }
        
        public bool SweepKick { get { return Flags.IsBitSet(13); } set { Flags.SetBit(13, value); } }
        
        // The interpolation alpha is encoded to a single byte.
        public FP InterpolationAlpha
        {
            get => ((FP)InterpolationAlphaEncoded) / 255;
            set
            {
                FP clamped = FPMath.Clamp(value * 255, 0, 255);
                InterpolationAlphaEncoded = (byte)clamped.AsInt;
            }
        }
        
        public FPVector2 MovementDirection
        {
            get
            {
                return DecodeDirection(EncodedMovementDirection);
            }
            set
            {
                EncodedMovementDirection = EncodeDirection(value);
            }
        }

        public FPVector2 LookDirection
        {
            get
            {
                return DecodeDirection(EncodedLookDirection);
            }
            set
            {
                EncodedLookDirection = EncodeDirection(value);
            }
        }
        
        
        private FPVector2 DecodeDirection(byte encodedDirection)
        {
            if (encodedDirection == default) return default;
            Int32 angle = ((Int32)encodedDirection - 1) * 2;
            return FPVector2.Rotate(FPVector2.Up, angle * FP.Deg2Rad);
        }

        private byte EncodeDirection(FPVector2 value)
        {
            if (value == default)
            {
                return default;
            }
            var angle = FPVector2.RadiansSigned(FPVector2.Up, value) * FP.Rad2Deg;
            angle = (((angle + 360) % 360) / 2) + 1;
            return (Byte)(angle.AsInt);
        }
	}
}