using Godot;

namespace TowerDefense.Models
{
	internal class SerializableVector2I
	{
		public int X { get ; set; }
		public int Y { get ; set; }
		public SerializableVector2I() { }
		public SerializableVector2I(Vector2I vector)
		{
			X = vector.X;
			Y = vector.Y;
		}
		
		public Vector2 ToVector2()
		{
			return new Vector2(X, Y);
		}
		public Vector2I ToVector2I()
		{
			return new Vector2I(X, Y);
		}
		 
		 
		
	}
}
