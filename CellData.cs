namespace TowerDefense.Models
{
	internal class CellData
	{
		public SerializableVector2I Position { get ; set ; }
		public int CellId { get ; set; }
		public SerializableVector2I AtlasCoords {get ;set; }
		public int  Layer{ get ; set ; }
		
		
		
	}
	
	
	
}
