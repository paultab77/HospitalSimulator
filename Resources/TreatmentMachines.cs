using System.Collections.Generic;

namespace Resources
{
	public class TreatmentMachine
	{
		public string Name { get; set; }
		public string Capability { get; set; }
	}

	public class TreatmentMachineList
	{
		public List<TreatmentMachine> TreatmentMachines { get; set; }
	}
}