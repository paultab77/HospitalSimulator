using Newtonsoft.Json;
using Resources;



namespace DataAccessLayer
{
	/// <summary>
	/// TODO:  Put the interface in a separate file.
	/// </summary>
	public interface IDataAccess
	{
		DoctorList LoadDoctors();

		TreatmentMachineList LoadTreatmentMachines();

		TreatmentRoomList LoadTreatmentRooms();

		PatientList LoadPatients();

		ConsultationList LoadConsultations();
	}

	public class DataAccess: IDataAccess
	{
		public DoctorList LoadDoctors()
		{
			var doctorsJson = System.IO.File.ReadAllText(@"JSON\Doctors.json");
			return JsonConvert.DeserializeObject<DoctorList>(doctorsJson);
		}


		public TreatmentMachineList LoadTreatmentMachines()
		{
			var treatmentMachinesJson = System.IO.File.ReadAllText(@"JSON\TreatmentMachines.json");
			return JsonConvert.DeserializeObject<TreatmentMachineList>(treatmentMachinesJson);
		}


		public TreatmentRoomList LoadTreatmentRooms()
		{
			var treatmentRoomsJson = System.IO.File.ReadAllText(@"JSON\TreatmentRooms.json");
			return JsonConvert.DeserializeObject<TreatmentRoomList>(treatmentRoomsJson);
		}


		public PatientList LoadPatients()
		{
			var patientsJson = System.IO.File.ReadAllText(@"JSON\Patients.json");
			return JsonConvert.DeserializeObject<PatientList>(patientsJson);
		}


		public ConsultationList LoadConsultations()
		{
			var consultationsJson = System.IO.File.ReadAllText(@"JSON\Consultations.json");
			return JsonConvert.DeserializeObject<ConsultationList>(consultationsJson);
		}


	}
}
