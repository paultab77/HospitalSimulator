using System.Collections.Generic;

namespace Resources
{
	public class Condition
	{
		public string Diagnosis { get; set; }
		public string Topography { get; set; }
	}


	public class Patient
	{
		public string Name { get; set; }
		public string RegistrationDate { get; set; }
		public Condition Condition { get; set; }
	}


	public class PatientList
	{
		public List<Patient> Patients { get; set; }
	}
}
