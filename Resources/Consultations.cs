using System;
using System.Collections.Generic;

namespace Resources
{
	public class Consultation : IComparable
	{
		public string Patient { get; set; }
		public string PatientCondition { get; set; }
		public string PatientTopography { get; set; }
		public string Doctor { get; set; }
		public string DoctorRole { get; set; }
		public string TreatmentRoom { get; set; }
		public string MachineName { get; set; }
		public string MachineCapability { get; set; }
		public string ConsultationDate { get; set; }

		public DateTime ConsultationDateAsDateTime()
		{
				return new DateTime(Convert.ToInt32(ConsultationDate.Substring(0, 4)),
						Convert.ToInt32(ConsultationDate.Substring(4, 2)), Convert.ToInt32(ConsultationDate.Substring(6, 2)));
		}

		public int CompareTo(object obj)
		{
			if (obj == null) return 1;

			var otherConsultation = obj as Consultation;
			if (otherConsultation != null)
				return string.Compare(ConsultationDate, otherConsultation.ConsultationDate, StringComparison.Ordinal);
			else
				throw new ArgumentException("Object is not a Consultation");
		}
	}


	public class ConsultationList
	{
		public List<Consultation> Consultations { get; set; }
	}


	//public class ConsultationQuery
	//{
	//	public IEnumerable<Consultation> Consultations { get; set; }
	//}
}