namespace DTDanmaku
{
	using DTLib;
	using System.Collections.Generic;
	using AgateLib.InputLib;

	public class AgateLibKeyboard : IKeyboard
	{
		private Dictionary<Key, KeyCode> mapping;

		public AgateLibKeyboard()
		{
			var mapping = new Dictionary<Key, KeyCode>();
			mapping[Key.A] = KeyCode.A;
			mapping[Key.B] = KeyCode.B;
			mapping[Key.C] = KeyCode.C;
			mapping[Key.D] = KeyCode.D;
			mapping[Key.E] = KeyCode.E;
			mapping[Key.F] = KeyCode.F;
			mapping[Key.G] = KeyCode.G;
			mapping[Key.H] = KeyCode.H;
			mapping[Key.I] = KeyCode.I;
			mapping[Key.J] = KeyCode.J;
			mapping[Key.K] = KeyCode.K;
			mapping[Key.L] = KeyCode.L;
			mapping[Key.M] = KeyCode.M;
			mapping[Key.N] = KeyCode.N;
			mapping[Key.O] = KeyCode.O;
			mapping[Key.P] = KeyCode.P;
			mapping[Key.Q] = KeyCode.Q;
			mapping[Key.R] = KeyCode.R;
			mapping[Key.S] = KeyCode.S;
			mapping[Key.T] = KeyCode.T;
			mapping[Key.U] = KeyCode.U;
			mapping[Key.V] = KeyCode.V;
			mapping[Key.W] = KeyCode.W;
			mapping[Key.X] = KeyCode.X;
			mapping[Key.Y] = KeyCode.Y;
			mapping[Key.Z] = KeyCode.Z;
			mapping[Key.Zero] = KeyCode.D0;
			mapping[Key.One] = KeyCode.D1;
			mapping[Key.Two] = KeyCode.D2;
			mapping[Key.Three] = KeyCode.D3;
			mapping[Key.Four] = KeyCode.D4;
			mapping[Key.Five] = KeyCode.D5;
			mapping[Key.Six] = KeyCode.D6;
			mapping[Key.Seven] = KeyCode.D7;
			mapping[Key.Eight] = KeyCode.D8;
			mapping[Key.Nine] = KeyCode.D9;
			mapping[Key.UpArrow] = KeyCode.Up;
			mapping[Key.DownArrow] = KeyCode.Down;
			mapping[Key.LeftArrow] = KeyCode.Left;
			mapping[Key.RightArrow] = KeyCode.Right;
			mapping[Key.Delete] = KeyCode.Delete;
			mapping[Key.Backspace] = KeyCode.BackSpace;
			mapping[Key.Enter] = KeyCode.Enter;
			mapping[Key.Shift] = KeyCode.Shift;
			mapping[Key.Space] = KeyCode.Space;
			mapping[Key.Esc] = KeyCode.Escape;

			this.mapping = mapping;
		}

		public bool IsPressed(Key key)
		{
			return Keyboard.Keys[this.mapping[key]];
		}
	}
}
