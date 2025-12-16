using System;
using OP_Engine.Characters;

namespace DoS1
{
    public class CharacterKilledEventArgs : EventArgs
    {
        public Character Character { get; private set; }

        public CharacterKilledEventArgs(Character character)
        {
            Character = character;
        }
    }
}
