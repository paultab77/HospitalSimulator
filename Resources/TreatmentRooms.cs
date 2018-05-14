using System.Collections.Generic;

namespace Resources
{
	public class TreatmentRoom
	{
		public string Name { get; set; }
		public string TreatmentMachine { get; set; }
	}


	public class TreatmentRoomList
	{
		public List<TreatmentRoom> TreatmentRooms { get; set; }
	}
}