using System.Collections.Generic;

namespace Resources
{
	public class Doctor
	{
		public string Name { get; set; }
		public List<string> Roles { get; set; }
	}

	public class DoctorList
	{
		public List<Doctor> Doctors { get; set; }
	}
}