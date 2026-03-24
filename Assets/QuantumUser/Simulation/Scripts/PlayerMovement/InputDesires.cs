using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct InputDesires
    {
        // PUBLIC MEMBERS
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
        
        public void CopyFromInput(Input* input)
        {
            if (input == null)
            {
                Clear();
                return;
            } 
            
            MoveDirection = input->MovementDirection;
            LookDirection = input->LookDirection;
            InterpolationOffset = input->InterpolationOffset;
            InterpolationAlphaEncoded = input->InterpolationAlphaEncoded;
            
            PlayerJump = input->PlayerJump;
            PlayerSprint = input->PlayerSprint;
            PlayerAttack = input->PlayerAttack;
            PlayerDodge = input->PlayerDodge;
            
            DeltaPitch = input->DeltaPitch;
            DeltaYaw = input->DeltaYaw;
            
            CameraPosition = input->CameraPosition;
            
            PlayerBlock = input->PlayerBlock;

            FinisherAttack = input->FinisherAttack;
            
            RootPosition = input->RootPosition;
            RootPositionVictim = input->RootPositionVictim;
            
            PlayerKick = input->PlayerKick;
            PlayerAttackHard = input->PlayerAttackHard;
            PlayerKickHard = input->PlayerKickHard;
            
            VerticalAxis = input->VerticalAxis;
            
            PlayerDodgeForward = input->PlayerDodgeForward;
            PlayerDodgeBack = input->PlayerDodgeBack;

            PlayerPing = input->PlayerPing;
            
            ConnectionWarning = input->ConnectionWarning;
            
            ConnectionBad = input->ConnectionBad;
            
            SweepKick = input->SweepKick;
        }

        public void Clear()
        {
            Flags = 0;
        }
    }
}