using System;
using System.Collections.Generic;

namespace Resources
{
	public class Consultation : IComparable
	{
		public Patient Patient { get; set; }
		public Doctor Doctor { get; set; }
		public TreatmentRoom Room { get; set; }
		public string ConsultationDate { get; set; }

		public DateTime ConsultationDateAsDateTime => new DateTime(Convert.ToInt32(ConsultationDate.Substring(0, 4)), Convert.ToInt32(ConsultationDate.Substring(4, 2)), Convert.ToInt32(ConsultationDate.Substring(6, 2)));

		public int CompareTo(object obj)
		{
			if (obj == null) return 1;

			Consultation otherConsultation = obj as Consultation;
			if (otherConsultation != null)
				return this.ConsultationDate.CompareTo(otherConsultation.ConsultationDate);
			else
				throw new ArgumentException("Object is not a Temperature");
		}
	}


	public class ConsultationList
	{
		public IEnumerable<Consultation> Consultations { get; set; }
	}
}